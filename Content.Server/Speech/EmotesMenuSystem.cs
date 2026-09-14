using Content.Server.Chat.Systems;
//imp edit start
using Content.Server.Popups;
using Content.Shared.Chat;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Examine;
//imp edit end
using Robust.Shared.Prototypes;

namespace Content.Server.Speech;

public sealed partial class EmotesMenuSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly PopupSystem _popup = default!; //imp edit
    [Dependency] private readonly ExamineSystemShared _examine = default!; //imp edit

    public override void Initialize()
    {
        base.Initialize();

        SubscribeAllEvent<PlayEmoteMessage>(OnPlayEmote);
    }

    private void OnPlayEmote(PlayEmoteMessage msg, EntitySessionEventArgs args)
    {
        var player = args.SenderSession.AttachedEntity;
        var target = GetEntity(msg.Target); //imp edit

        if (!player.HasValue)
            return;

        if (!_prototypeManager.Resolve(msg.ProtoId, out var proto) || proto.ChatTriggers.Count == 0)
            return;

        //imp edit start: targeted emotes
        if (!_prototypeManager.Resolve<EmotePrototype>(msg.ProtoId, out var emotePrototype))
        {
            return;
        }
        if (emotePrototype.Targeted)
        {
            if (target == null)
            {
                _popup.PopupEntity(Loc.GetString("targeted-emote-no-target"), player.Value); //if the emote needs a target but doesn't find one, cancel and send a popup
                return;
            }
            if (!_examine.InRangeUnOccluded(player.Value, target.Value) && !IsClientSide(target.Value)){
                _popup.PopupEntity(Loc.GetString("targeted-emote-target-blocked"), player.Value);//if target isn't visible, cancel and send a popup. skip this check for clientside stuff, e.g. things in bags
                return;
            }
            else //nothing stopping a targeted emote
            {
                _chat.TryTargetedEmoteWithChat(player.Value, target.Value, msg.ProtoId);
            }
        }
        else // non-targeted emote - do it normal style, throwing out target
        {
            _chat.TryEmoteWithChat(player.Value, msg.ProtoId);
        }
        //imp edit end
    }
}
