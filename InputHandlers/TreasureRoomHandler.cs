using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace KeyboardQuickPlay.Handlers;

public partial class TreasureRoomHandler : Node
{
    private readonly NTreasureRoom _room;

    private Node _relicCollection;
    private Button _chestButton;

    private bool _isRelicOpen;
    private bool _hasOpened;

    public TreasureRoomHandler(NTreasureRoom room)
    {
        _room = room;
    }

    public override void _Ready()
    {
        SetProcessInput(true);

        var type = _room.GetType();

        _relicCollection = AccessTools.Field(type, "_relicCollection")?.GetValue(_room) as Node;
        _chestButton     = AccessTools.Field(type, "_chestButton")?.GetValue(_room) as Button;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Helpers.IsTopScreen(_room))
            return;

        if (@event is not InputEventKey key || !key.Pressed || key.Echo)
            return;

        if (!ModConfig.IsMatch(@event) && !ModConfig.IsHeld())
            return;

        UpdateState();

        // SPACE behavior
        if (key.Keycode == Key.Space)
        {
            if (!_hasOpened)
            {
                OpenChest();
                return;
            }

            TrySelectSingleRelic();
            return;
        }

        // Number keys
        int index = Helpers.KeyToIndex(key.Keycode);
        if (index >= 0)
        {
            SelectRelic(index);
        }
    }

    #region State

    private void UpdateState()
    {
        var type = _room.GetType();

        _isRelicOpen = (bool)(AccessTools.Field(type, "_isRelicCollectionOpen")?.GetValue(_room) ?? false);
        _hasOpened   = (bool)(AccessTools.Field(type, "_hasChestBeenOpened")?.GetValue(_room) ?? false);
    }

    #endregion

    #region Chest

    private void OpenChest()
    {
        _room.OnChestButtonReleased(null);
        // var method = _room.GetType().GetMethod("OnChestButtonReleased",
        //     System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        // method?.Invoke(_room, new object[] { null });
    }

    #endregion

    #region Relics

    private void TrySelectSingleRelic()
    {
        if (!_isRelicOpen || _relicCollection == null)
            return;

        var holders = GetHolders();
        if (holders == null || holders.Count != 1)
            return;

        SelectRelic(0);
    }

    private void SelectRelic(int index)
    {
        var holders = GetHolders();
        if (holders == null || index >= holders.Count)
            return;

        // BEST: call game logic directly (no UI timing issues)
        RunManager.Instance.TreasureRoomRelicSynchronizer
            .PickRelicLocally(index);
    }

    private System.Collections.IList GetHolders()
    {
        var field = AccessTools.Field(_relicCollection.GetType(), "_holdersInUse");
        return field?.GetValue(_relicCollection) as System.Collections.IList;
    }

    #endregion
}
