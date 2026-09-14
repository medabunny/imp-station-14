using Content.Shared.Chat;

namespace Content.Client.Hands.Systems
{
    public sealed partial class HandsSystem
    {
        public void UIInventoryEmote(string handName) //start imp edit - targeted emotes
        {
            if (!TryGetPlayerHands(out var hands) ||
                !TryGetHeldItem(hands.Value.AsNullable(), handName, out var heldEntity))
            {
                return;
            }

            RaiseLocalEvent(new EmoteInventorySlotEvent(heldEntity.Value));
        } //end imp edit
    }
}
