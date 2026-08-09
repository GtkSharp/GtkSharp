using System;

namespace GtkSharp.Tests
{
    /// <summary>
    /// Facts about the machine the suite is running on that decide whether a
    /// test can run at all, as opposed to whether it passes.
    /// </summary>
    public static class TestEnvironment
    {
        /// <summary>The Gtk actually loaded, as "major.minor.micro".</summary>
        public static string GtkVersion { get; } =
            $"{Gtk.Global.MajorVersion}.{Gtk.Global.MinorVersion}.{Gtk.Global.MicroVersion}";

        /// <summary>
        /// Whether the Gtk in use is at least <paramref name="major"/>.<paramref name="minor"/>.
        /// </summary>
        /// <remarks>
        /// The api.xml describes Gtk 4.22, so a wrapper generated from it can name
        /// a function an older Gtk does not export. That is a null delegate rather
        /// than a link error, and it surfaces as a NullReferenceException from
        /// inside the wrapper with nothing naming the symbol.
        ///
        /// A test that needs such a function guards on this, so that an older Gtk
        /// reports "this Gtk is too old" instead of a failure that reads like a
        /// defect. Debian trixie ships 4.18 and is the case this exists for; the
        /// reference environment is a forky container at 4.22.
        ///
        /// Guard on the *version the symbol appeared in*, taken from the gir's
        /// version attribute, and name the symbol in the reason. Never guard a
        /// test merely because it fails somewhere.
        /// </remarks>
        public static bool GtkAtLeast(uint major, uint minor)
            => Gtk.Global.MajorVersion > major
               || (Gtk.Global.MajorVersion == major && Gtk.Global.MinorVersion >= minor);

        /// <summary>A skip reason naming the symbol and the Gtk that lacks it.</summary>
        public static string NeedsGtk(uint major, uint minor, string symbol)
            => $"{symbol} arrived in Gtk {major}.{minor}; this is {GtkVersion}.";

        /// <summary>
        /// Whether a <c>WebKit.WebView</c> can be constructed here.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>WebKit.Global.IsSupported</c> only answers whether the library
        /// loads. It is true in a Debian container, and constructing a WebView
        /// there still kills the process: WebKit spawns its network and web
        /// processes into a bwrap sandbox, that needs a user namespace, and a
        /// container is not generally permitted to create one. It does not fail
        /// the call — it aborts, so nothing can catch it:
        /// </para>
        /// <code>
        /// bwrap: Creating new namespace failed: Operation not permitted
        /// ** (testhost): ERROR **: Failed to fully launch dbus-proxy
        /// </code>
        /// <para>
        /// The run then ends mid-suite with a truncated total under a "Passed!"
        /// line, which is indistinguishable at the console from the finalizer
        /// crashes documented in <c>Docs/testing.md</c>.
        /// </para>
        /// <para>
        /// There is a documented escape — <c>WEBKIT_DISABLE_SANDBOX_THIS_IS_DANGEROUS</c>
        /// — and CI deliberately does not take it: that job holds a token with
        /// <c>packages:write</c>, and turning off process isolation to make a
        /// test suite run is a poor trade. CI sets
        /// <c>GTKSHARP_TESTS_SKIP_WEBKIT=1</c> instead and loses two tests plus
        /// one sample section, which Windows loses anyway because gvsbuild ships
        /// no WebKit.
        /// </para>
        /// <para>
        /// Set it yourself when running the suite in a container. On a normal
        /// desktop, leave it unset — the sandbox starts and WebKit is covered.
        /// </para>
        /// </remarks>
        public static bool WebKitUsable { get; } =
            WebKit.Global.IsSupported && !IsTruthy(Environment.GetEnvironmentVariable("GTKSHARP_TESTS_SKIP_WEBKIT"));

        /// <summary>The reason <see cref="WebKitUsable"/> is false, for a skip
        /// message that says which of the two causes applies.</summary>
        public static string WebKitSkipReason =>
            WebKit.Global.IsSupported
                ? "GTKSHARP_TESTS_SKIP_WEBKIT is set: WebKit's sandbox cannot start here."
                : "WebKit is not installed (gvsbuild ships no webkitgtk).";

        /// <summary>
        /// Whether a sample section that builds a WebView has to be left out of
        /// the section theories altogether.
        /// </summary>
        /// <remarks>
        /// Narrower than <see cref="WebKitUsable"/>, and deliberately so. Where
        /// WebKit is simply absent — Windows, since gvsbuild ships none — the
        /// section detects that itself and shows a label instead, so it
        /// constructs safely and the tree still lists its row. The only case
        /// that has to be excluded is the library loading and then being unable
        /// to sandbox itself, because that aborts the host.
        /// </remarks>
        public static bool SkipWebKitSections { get; } =
            WebKit.Global.IsSupported
            && IsTruthy(Environment.GetEnvironmentVariable("GTKSHARP_TESTS_SKIP_WEBKIT"));

        /// <summary>True for a type whose construction reaches WebKit.</summary>
        public static bool NeedsWebKit(Type sectionType)
            => sectionType != null && sectionType.Name == "WebviewSection";

        /// <summary>
        /// Whether a section theory case has to be skipped rather than run.
        /// </summary>
        /// <remarks>
        /// The section enumerations stay complete on purpose: they describe the
        /// application, and <c>The_tree_offers_a_row_for_every_section</c>
        /// compares one against the live tree. Filtering the enumeration made
        /// that guard compare a filtered list against an unfiltered tree and
        /// fail — the guard was right and the filter was wrong. So the skip
        /// belongs here, where the WebView would actually be built.
        /// </remarks>
        public static bool SkipWebKitSectionNamed(string typeName)
            => SkipWebKitSections
               && typeName != null
               && typeName.EndsWith("WebviewSection", StringComparison.Ordinal);

        /// <summary>
        /// Whether a section may be mounted by the label the sample's tree shows
        /// for it, rather than by type name.
        /// </summary>
        /// <remarks>
        /// The browsing tests reach sections the way the application does —
        /// through the tree, by label — so <see cref="SkipWebKitSectionNamed"/>
        /// does not reach them. Two of them pick a section positionally rather
        /// than from the theory data (the first row, the first five rows), and
        /// the tree lists a section under its content type's name, so the row to
        /// step over is "WebView".
        /// </remarks>
        public static bool CanMountSectionLabelled(string label)
            => !SkipWebKitSections || label != "WebView";

        static bool IsTruthy(string value)
            => !string.IsNullOrEmpty(value)
               && value != "0"
               && !value.Equals("false", StringComparison.OrdinalIgnoreCase);
    }
}
