using System.Text;
using Content.Server.Speech.Components;
using Content.Shared.StatusEffect;
using Robust.Shared.Random;

namespace Content.Server.Speech.EntitySystems;

public sealed class StonerAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<StonerAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, StonerAccentComponent component, AccentGetEvent args)
    {
        var scale = 4f;
        args.Message = Accentuate(args.Message, scale);
    }

    private string Accentuate(string message, float scale)
    {
        var sb = new StringBuilder();

        // How many characters through the message we are
        var position = 0;
        foreach (var character in message)
        {
            position += 1;

            // Additions at the end of words and sentences.
            if (_random.Prob(scale / 20f))
            {
                // End of word
                if (character == ' ')
                {
                    sb.Append(Loc.GetString("stoner-accent-confused-1"));
                }
                // End of sentence
                else if (character == '.')
                {
                    sb.Append(' ');
                    sb.Append(Loc.GetString("stoner-accent-confused-2"));
                }
                // Start of message
                else if (position == 1)
                {
                    sb.Append(Loc.GetString("stoner-accent-confused-2"));
                    sb.Append(' ');
                }
            }

            // Chance to trail off and cut the message short
            if (_random.NextGaussian(-2D + ((double) position / message.Length)) > 0)
            {
                if (character == ' ')
                {
                    sb.Append("...");
                    break;
                }
            }

            sb.Append(character);
        }

        return sb.ToString();
    }
}
