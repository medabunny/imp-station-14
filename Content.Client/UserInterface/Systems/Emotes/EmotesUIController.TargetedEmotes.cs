

using Content.Shared.Chat;
using Content.Shared.Chat.Prototypes;
using Robust.Shared.Input.Binding;

namespace Content.Client.UserInterface.Systems.Emotes;

public sealed partial class EmotesUIController
{
    struct EmoteInfo //imp edit start - pass targets into emote events for targeted emotes
    {
        public EmotePrototype prototype;
        public NetEntity? emoteTarget;
    } //imp edit end

    public override void Initialize() //imp edit start: subscribe to clientside emote events, for stuff in bags, pockets, etc
    {
        base.Initialize();

        SubscribeLocalEvent<EmoteInventorySlotEvent>(HandleClientsideEmote);
    } //end imp edit

    private bool HandleEmote(in PointerInputCmdHandler.PointerInputCmdArgs args)
    { //imp edit start - handles emote events for clientside entities
        ToggleEmotesMenu(false, args.EntityUid);
        return true;
    }//imp edit end

    private void HandleClientsideEmote(EmoteInventorySlotEvent args) //imp edit start - for emoting at clientside items, e.g. in an inventory slot
    {
        ToggleEmotesMenu(false, args.TargetUid);
    }//imp edit end

    public void OpenEmotesMenu(bool centered, EntityUid? emoteTarget = null) //for emoting at clientside items inside other UIs, e.g. rightclick dropdown
    {
        ToggleEmotesMenu(centered, emoteTarget);
    }//imp edit end

}
