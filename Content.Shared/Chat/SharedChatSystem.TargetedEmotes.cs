using Content.Shared.Chat.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared.Chat;

public abstract partial class SharedChatSystem
{

    //IMP EDIT START: add targeted emotes
    /// <summary>
    /// Makes the selected entity emote using the given <see cref="EmotePrototype"/> and sends a message to chat.
    /// </summary>
    /// <param name="source">The entity that is speaking</param>
    /// <param name="target">The entity that is being targeted in a targeted emote.</param>
    /// <param name="emoteId">The id of emote prototype. Should have valid <see cref="EmotePrototype.ChatMessages"/></param>
    /// <param name="hideLog">Whether this message should appear in the adminlog window, or not.</param>
    /// <param name="range">Conceptual range of transmission, if it shows in the chat window, if it shows to far-away ghosts or ghosts at all...</param>
    /// <param name="ignoreActionBlocker">Whether emote action blocking should be ignored or not.</param>
    /// <param name="nameOverride">
    /// The name to use for the speaking entity. Usually this should just be modified via <see cref="TransformSpeakerNameEvent"/>.
    /// If this is set, the event will not get raised.
    /// </param>
    /// <param name="forceEmote">Bypasses whitelist/blacklist/availibility checks for if the entity can use this emote</param>
    /// <returns>True if an emote was performed. False if the emote is unavailable, cancelled, etc.</returns>
    public bool TryTargetedEmoteWithChat(
        EntityUid source,
        EntityUid target,
        string emoteId,
        ChatTransmitRange range = ChatTransmitRange.Normal,
        bool hideLog = false,
        string? nameOverride = null,
        bool ignoreActionBlocker = false,
        bool forceEmote = false
    )
    {
        if (!_prototypeManager.Resolve<EmotePrototype>(emoteId, out var proto))
            return false;

        return TryTargetedEmoteWithChat(source, target, proto, range, hideLog: hideLog, nameOverride, ignoreActionBlocker: ignoreActionBlocker, forceEmote: forceEmote);
    }
    //IMP EDIT END

    //IMP EDIT START: add targeted emotes
    /// <summary>
    /// Makes the selected entity emote using the given <see cref="EmotePrototype"/> and sends a message to chat.
    /// </summary>
    /// <param name="source">The entity that is speaking.</param>
    /// <param name="target">The entity that is being targeted in a targeted emote.</param>
    /// <param name="emote">The emote prototype. Should have valid <see cref="EmotePrototype.ChatMessages"/>.</param>
    /// <param name="hideLog">Whether this message should appear in the adminlog window or not.</param>
    /// <param name="ignoreActionBlocker">Whether emote action blocking should be ignored or not.</param>
    /// <param name="range">Conceptual range of transmission, if it shows in the chat window, if it shows to far-away ghosts or ghosts at all...</param>
    /// <param name="nameOverride">
    /// The name to use for the speaking entity. Usually this should just be modified via <see cref="TransformSpeakerNameEvent"/>.
    /// If this is set, the event will not get raised.
    /// </param>
    /// <param name="forceEmote">Bypasses whitelist/blacklist/availibility checks for if the entity can use this emote</param>
    /// <returns>True if an emote was performed. False if the emote is unavailable, cancelled, etc.</returns>
    public bool TryTargetedEmoteWithChat(
        EntityUid source,
        EntityUid target,
        EmotePrototype emote,
        ChatTransmitRange range = ChatTransmitRange.Normal,
        bool hideLog = false,
        string? nameOverride = null,
        bool ignoreActionBlocker = false,
        bool forceEmote = false
    )
    {
        if (!forceEmote && !AllowedToUseEmote(source, emote))
            return false;

        var didEmote = TryEmoteWithoutChat(source, emote, ignoreActionBlocker);

        // check if proto has valid message for chat
        if (didEmote && emote.ChatMessages.Count != 0)
        {
            // not all emotes are loc'd, but for the ones that are we pass in entity
            var action = Loc.GetString(_random.Pick(emote.ChatMessages), ("entity", source), ("target", target));
            SendEntityEmote(source, action, range, nameOverride, hideLog: hideLog, checkEmote: false, ignoreActionBlocker: ignoreActionBlocker);
        }

        return didEmote;
    }
    //IMP EDIT END

}
