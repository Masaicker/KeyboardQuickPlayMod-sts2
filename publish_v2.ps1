# STS2 MOD 发布脚本 V2（适配新版 mod manifest 结构）
# manifest 外置为 <mod_id>.json，PCK 按需构建
$ErrorActionPreference = "Stop"

$ProjectDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$csproj = Get-ChildItem "$ProjectDir/*.csproj" | Select-Object -First 1
if (-not $csproj) { throw "No .csproj found in $ProjectDir" }
$ModName = $csproj.BaseName

$SteamRoot = (Get-ItemProperty "HKCU:\Software\Valve\Steam" -Name SteamPath).SteamPath
$ModsDir = "$SteamRoot/steamapps/common/Slay the Spire 2/mods"
$ManifestPath = [System.IO.Path]::GetFullPath("$ProjectDir/mod_manifest.json")
$ImageImportPath = [System.IO.Path]::GetFullPath("$ProjectDir/$ModName/mod_image.png.import")

# 读取 manifest
if (!(Test-Path $ManifestPath)) { throw "mod_manifest.json not found" }
$manifest = [System.IO.File]::ReadAllText($ManifestPath, [System.Text.Encoding]::UTF8) | ConvertFrom-Json
$ModId = $manifest.id
if (-not $ModId) { throw "manifest missing 'id' field" }

# ========== 辅助函数：写入一个文件条目到 PCK ==========
function Write-PckFileEntry($bw, [string]$resPath, [byte[]]$data, [long]$offsetFromBase) {
    $pathBytes = [System.Text.Encoding]::UTF8.GetBytes($resPath)
    $pathLen = $pathBytes.Length + 1
    $paddedLen = [Math]::Ceiling($pathLen / 4) * 4
    $bw.Write([int]$paddedLen)
    $bw.Write($pathBytes)
    for ($i = $pathBytes.Length; $i -lt $paddedLen; $i++) { $bw.Write([byte]0) }
    $bw.Write([long]$offsetFromBase)
    $bw.Write([long]$data.Length)
    $md5 = [System.Security.Cryptography.MD5]::Create().ComputeHash($data)
    $bw.Write($md5)
    $bw.Write([int]0)
}

try {
    Write-Host "=== $ModName Publish (V2) ===" -ForegroundColor Cyan

    # 询问是否包含封面图片（决定是否构建 PCK）
    $includePck = $false
    $ctexFile = $null
    if (Test-Path $ImageImportPath) {
        $ctexFile = Get-ChildItem "$ProjectDir/.godot/imported" -Filter "mod_image.png-*.ctex" | Select-Object -First 1
        if ($ctexFile) {
            Write-Host ""
            Write-Host "  Include mod cover image? (requires PCK)" -ForegroundColor White
            Write-Host "  [Y] Yes  [N] No (default)" -ForegroundColor DarkGray
            $key = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown").Character
            if ($key -eq 'y' -or $key -eq 'Y') { $includePck = $true }
            Write-Host ""
        }
    }

    $hasDll = [bool]$manifest.has_dll
    $totalSteps = 1  # json 复制必做
    if ($hasDll) { $totalSteps++ }
    if ($includePck) { $totalSteps++ }
    $step = 0

    # 1. 编译 DLL
    if ($hasDll) {
        $step++
        Write-Host "[$step/$totalSteps] Publishing DLL..." -ForegroundColor Yellow
        dotnet publish $csproj.FullName -c Release --no-restore -o "$ProjectDir/bin/publish"
        if ($LASTEXITCODE -ne 0) { throw "DLL publish failed!" }
    }

    # 2. 构建 PCK（仅当需要时）
    $pckSize = 0
    if ($includePck) {
        $step++
        Write-Host "[$step/$totalSteps] Building PCK..." -ForegroundColor Yellow

        $PckPath = [System.IO.Path]::GetFullPath("$ProjectDir/$ModName.pck")
        $files = @()
        $files += ,@("res://$ModName/mod_image.png.import", [System.IO.File]::ReadAllBytes($ImageImportPath))
        $files += ,@("res://.godot/imported/$($ctexFile.Name)", [System.IO.File]::ReadAllBytes($ctexFile.FullName))
        Write-Host "  + mod_image.png (.import + .ctex)" -ForegroundColor Gray

        $ms = New-Object System.IO.MemoryStream
        $bw = New-Object System.IO.BinaryWriter($ms)

        # Header
        $bw.Write([byte[]]@(0x47, 0x44, 0x50, 0x43))  # Magic: GDPC
        $bw.Write([int]2)                                # Pack version
        $bw.Write([int]4); $bw.Write([int]5); $bw.Write([int]1)  # Engine 4.5.1
        $bw.Write([int]0)                                # Flags
        $fileBaseOffsetPos = $ms.Position
        $bw.Write([long]0)                               # File base offset (fill later)
        for ($i = 0; $i -lt 16; $i++) { $bw.Write([int]0) }  # Reserved

        # Directory
        $bw.Write([int]$files.Count)
        $dataOffset = [long]0
        $offsets = @()
        foreach ($f in $files) {
            $offsets += $dataOffset
            $dataLen = $f[1].Length
            $dataPadding = (4 - ($dataLen % 4)) % 4
            $dataOffset += $dataLen + $dataPadding
        }
        for ($i = 0; $i -lt $files.Count; $i++) {
            Write-PckFileEntry $bw $files[$i][0] $files[$i][1] $offsets[$i]
        }

        # 写入文件数据
        $fileBasePos = $ms.Position
        foreach ($f in $files) {
            $bw.Write($f[1])
            $dataPadding = (4 - ($f[1].Length % 4)) % 4
            for ($j = 0; $j -lt $dataPadding; $j++) { $bw.Write([byte]0) }
        }

        # 回填 file base offset
        $ms.Position = $fileBaseOffsetPos
        $bw.Write([long]$fileBasePos)

        $bw.Flush()
        [System.IO.File]::WriteAllBytes($PckPath, $ms.ToArray())
        $bw.Close(); $ms.Close()

        $pckSize = (Get-Item $PckPath).Length
        Write-Host "  PCK built: $pckSize bytes" -ForegroundColor Gray
    }

    # 3. 生成输出 manifest 并复制文件到 mods 目录
    $step++
    Write-Host "[$step/$totalSteps] Copying to mods folder..." -ForegroundColor Yellow
    if (!(Test-Path $ModsDir)) { New-Item -ItemType Directory -Path $ModsDir | Out-Null }

    # 动态生成 manifest：读取原始 JSON，替换 has_pck，去掉旧字段
    $jsonText = [System.IO.File]::ReadAllText($ManifestPath, [System.Text.Encoding]::UTF8)
    $jsonText = $jsonText -replace '"pck_name"\s*:\s*"[^"]*"\s*,?\s*\n?', ''
    $hasPckStr = if ($includePck) { 'true' } else { 'false' }
    $jsonText = $jsonText -replace '"has_pck"\s*:\s*(true|false)', "`"has_pck`": $hasPckStr"
    [System.IO.File]::WriteAllText("$ModsDir/$ModId.json", $jsonText.Trim() + "`n", [System.Text.Encoding]::UTF8)

    if ($hasDll) {
        Copy-Item "$ProjectDir/bin/publish/$ModName.dll" "$ModsDir/$ModName.dll" -Force
    }
    if ($includePck) {
        Copy-Item $PckPath "$ModsDir/$ModName.pck" -Force
        Remove-Item $PckPath -Force -ErrorAction SilentlyContinue
    }

    # 结果
    Write-Host ""
    Write-Host "=== Publish OK ===" -ForegroundColor Green
    Write-Host "  JSON -> $ModsDir/$ModId.json"
    if ($hasDll) { Write-Host "  DLL  -> $ModsDir/$ModName.dll" }
    if ($includePck) { Write-Host "  PCK  -> $ModsDir/$ModName.pck ($pckSize bytes)" }
}
catch {
    Write-Host ""
    Write-Host "=== FAILED ===" -ForegroundColor Red
    Write-Host "  $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor DarkGray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
