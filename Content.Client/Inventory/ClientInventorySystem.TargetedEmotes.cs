using Content.Shared.Chat;

namespace Content.Client.Inventory
{
    public sealed partial class ClientInventorySystem
    {
        public void UIInventoryEmote(string slot, EntityUid uid) //imp edit start
        {
            if (!TryGetSlotEntity(uid, slot, out var item))
                return;

            RaiseLocalEvent(new EmoteInventorySlotEvent(item.Value));
        } //imp edit end

    }
}
