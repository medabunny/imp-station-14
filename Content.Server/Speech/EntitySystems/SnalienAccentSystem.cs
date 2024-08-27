using System.Text.RegularExpressions;
using Content.Server.Speech.Components;

namespace Content.Server.Speech.EntitySystems;

public sealed class SnalienAccentSystem : EntitySystem
{
    private static readonly Regex RegexLowerslurw = new Regex("w{1,3}");
    private static readonly Regex RegexUpperslurw = new Regex("W{1,3}");
    private static readonly Regex RegexLowerslure = new Regex("e{1,3}");
    private static readonly Regex RegexUpperslure = new Regex("E{1,3}");
	private static readonly Regex RegexLowerslurr = new Regex("r{1,3}");
    private static readonly Regex RegexUpperslurr = new Regex("R{1,3}");
    private static readonly Regex RegexLowerslury = new Regex("y{1,3}");
    private static readonly Regex RegexUpperslury = new Regex("Y{1,3}");
	private static readonly Regex RegexLowersluru = new Regex("u{1,3}");
    private static readonly Regex RegexUppersluru = new Regex("U{1,3}");
    private static readonly Regex RegexLowersluri = new Regex("i{1,3}");
    private static readonly Regex RegexUppersluri = new Regex("I{1,3}");
	private static readonly Regex RegexLowersluro = new Regex("o{1,3}");
    private static readonly Regex RegexUppersluro = new Regex("O{1,3}");
    private static readonly Regex RegexLowerslura = new Regex("a{1,3}");
    private static readonly Regex RegexUpperslura = new Regex("A{1,3}");
	private static readonly Regex RegexLowerslurs = new Regex("s{1,3}");
    private static readonly Regex RegexUpperslurs = new Regex("S{1,3}");
    private static readonly Regex RegexLowerslurf = new Regex("f{1,3}");
    private static readonly Regex RegexUpperslurf = new Regex("F{1,3}");
	private static readonly Regex RegexLowerslurh = new Regex("h{1,3}");
    private static readonly Regex RegexUpperslurh = new Regex("H{1,3}");
    private static readonly Regex RegexLowerslurl = new Regex("l{1,3}");
    private static readonly Regex RegexUpperslurl = new Regex("L{1,3}");
	private static readonly Regex RegexLowerslurz = new Regex("z{1,3}");
    private static readonly Regex RegexUpperslurz = new Regex("Z{1,3}");
    private static readonly Regex RegexLowerslurv = new Regex("v{1,3}");
    private static readonly Regex RegexUpperslurv = new Regex("V{1,3}");
	private static readonly Regex RegexLowerslurn = new Regex("n{1,3}");
    private static readonly Regex RegexUpperslurn = new Regex("N{1,3}");
    private static readonly Regex RegexLowerslurm = new Regex("m{1,3}");
    private static readonly Regex RegexUpperslurm = new Regex("M{1,3}");
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SnalienAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, SnalienAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        // I see nor hear no evil
        message = RegexLowerslurw.Replace(message, "ww");
        // Black writings on the wall
        message = RegexUpperslurw.Replace(message, "WW");
        // Unleashed a million faces
        message = RegexLowerslure.Replace(message, "ee");
        // And one by one they fall
        message = RegexUpperslure.Replace(message, "EE");
        // Black hearted evil
        message = RegexLowerslurr.Replace(message, "rr");
        // Black hearted hero
        message = RegexUpperslurr.Replace(message, "RR");
        // I am all
        message = RegexLowerslury.Replace(message, "yy");
        // I am all
        message = RegexUpperslury.Replace(message, "YY");
        // I am
        message = RegexLowersluru.Replace(message, "uu");
        // Here we go, buddy
        message = RegexUppersluru.Replace(message, "UU");
        // Here we go, buddy
        message = RegexLowersluri.Replace(message, "ii");
        // Here we go
        message = RegexUppersluri.Replace(message, "II");
        // Here we go, buddy
        message = RegexLowersluro.Replace(message, "oo");
        // Here we go
        message = RegexUppersluro.Replace(message, "OO");
        // Go ahead and try to see through me
        message = RegexLowerslura.Replace(message, "aa");
        // Do it if you dare
        message = RegexUpperslura.Replace(message, "AA");
        // One step forwards, two steps back
        message = RegexLowerslurs.Replace(message, "ss");
        // I'm here
        message = RegexUpperslurs.Replace(message, "SS");
        // Do it
        message = RegexLowerslurf.Replace(message, "ff");
        // Do it
        message = RegexUpperslurf.Replace(message, "FF");
        // Do it
        message = RegexLowerslurh.Replace(message, "hh");
        // Do it
        message = RegexUpperslurh.Replace(message, "HH");
        // Can you see all of me
        message = RegexLowerslurl.Replace(message, "ll");
        // Walk into my mystery
        message = RegexUpperslurl.Replace(message, "LL");
        // Step inside and hold on for dear life
        message = RegexLowerslurz.Replace(message, "zz");
        // Do you remember me
        message = RegexUpperslurz.Replace(message, "ZZ");
        // Capture you or set you free
        message = RegexLowerslurv.Replace(message, "vv");
        // I am all
        message = RegexUpperslurv.Replace(message, "VV");
        // I am all of me
        message = RegexLowerslurn.Replace(message, "nn");
        // I am
        message = RegexUpperslurn.Replace(message, "NN");
        // I am
        message = RegexLowerslurm.Replace(message, "mm");
        // I am
        message = RegexUpperslurm.Replace(message, "MM");
        args.Message = message;
    }
}
