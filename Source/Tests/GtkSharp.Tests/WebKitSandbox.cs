using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Samples;

namespace GtkSharp.Tests
{
    /// <summary>
    /// Whether WebKitGTK can render a page in this environment, and which sample
    /// sections need it to.
    /// </summary>
    /// <remarks>
    /// WebKit 6 has no unsandboxed mode. It launches its network and web
    /// processes through <c>bwrap</c>, which begins by creating an unprivileged
    /// user namespace, and a container is not guaranteed to be allowed one:
    /// Docker's default seccomp profile refuses <c>clone(CLONE_NEWUSER)</c>, so
    /// every run inside <c>debian:forky</c> without
    /// <c>--security-opt seccomp=unconfined</c> is refused.
    ///
    /// WebKit does not report that as a failed call. It aborts the process --
    /// "bwrap: Creating new namespace failed", then "Failed to fully launch
    /// dbus-proxy" -- which takes the test host down mid-suite and ends the run
    /// under a "Passed!" line carrying a truncated total. Nothing managed can
    /// catch it, so it has to be predicted rather than handled, and the
    /// prediction has to happen before the section that would trigger it is
    /// constructed.
    ///
    /// The prediction is the operation bwrap performs, run against the binary
    /// WebKit itself will use. Its failure text is what named this:
    ///
    ///   bwrap: No permissions to create a new namespace, likely because the
    ///   kernel does not allow non-privileged user namespaces.
    ///
    /// Restoring WEBKIT_DISABLE_SANDBOX_THIS_IS_DANGEROUS is deliberately not
    /// the answer -- see the comment on the "Run tests headless" step in
    /// .github/workflows/main.yml. The skip is self-disabling: give the job's
    /// container the namespace and the probe succeeds, so the sections run
    /// again with WebKit's own sandbox intact and nothing here to remove.
    /// </remarks>
    internal static class WebKitSandbox
    {
        private static readonly Lazy<bool> Probed = new Lazy<bool>(Probe);

        /// <summary>Whether a WebKit page load will work rather than abort the process.</summary>
        public static bool Available => Probed.Value;

        public const string SkipReason =
            "WebKitGTK renders through a bwrap sandbox and this environment forbids the " +
            "unprivileged user namespace it needs, which aborts the test host rather than " +
            "failing a call.";

        /// <summary>
        /// The tree labels of the sections backed by WebKit -- the sample lists a
        /// section under its content type's name, so these are the rows that must
        /// not be selected when the sandbox cannot start.
        /// </summary>
        public static IReadOnlyCollection<string> BackedLabels { get; } =
            new HashSet<string>(
                typeof(SectionAttribute).Assembly
                    .GetTypes()
                    .SelectMany(SectionsOf)
                    .Where(a => NeedsWebKit(a.ContentType))
                    .Select(a => a.ContentType.Name));

        /// <summary>Whether constructing <paramref name="sectionType"/> would reach WebKit.</summary>
        public static bool IsBacked(Type sectionType) =>
            SectionsOf(sectionType).Any(a => NeedsWebKit(a.ContentType));

        /// <summary>
        /// True when the section is safe to mount here: either it does not need
        /// WebKit, or WebKit works.
        /// </summary>
        public static bool CanMount(string label) =>
            Available || !BackedLabels.Contains(label);

        private static IEnumerable<SectionAttribute> SectionsOf(Type type) =>
            type.GetCustomAttributes(typeof(SectionAttribute), true).Cast<SectionAttribute>();

        private static bool NeedsWebKit(Type contentType) =>
            contentType != null &&
            (contentType.Namespace == "WebKit" || contentType.Namespace == "JavaScriptCore");

        private static bool Probe()
        {
            if (!WebKit.Global.IsSupported)
                return false;

            // The escape hatch Docs/testing.md documents for a local container
            // run: with the sandbox off there is no namespace to create.
            if (!string.IsNullOrEmpty(
                    Environment.GetEnvironmentVariable("WEBKIT_DISABLE_SANDBOX_THIS_IS_DANGEROUS")))
                return true;

            // bwrap is a Linux program and the sandbox is a Linux arrangement.
            // Anywhere else, if WebKit loaded at all it can render.
            if (!OperatingSystem.IsLinux())
                return true;

            try
            {
                using var probe = Process.Start(new ProcessStartInfo("bwrap")
                {
                    ArgumentList = { "--unshare-user", "--ro-bind", "/", "/", "/bin/true" },
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                });

                if (probe == null)
                    return false;

                if (!probe.WaitForExit(15000))
                {
                    probe.Kill(true);
                    return false;
                }

                return probe.ExitCode == 0;
            }
            catch (Exception)
            {
                // No bwrap on PATH is not a working sandbox either: it is the
                // program WebKit spawns, and its absence aborts the same way.
                return false;
            }
        }
    }
}
