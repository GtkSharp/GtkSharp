using System;
using System.Collections.Generic;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// Text the caller supplied must not be treated as a <c>printf</c> format
    /// string.
    /// </summary>
    /// <remarks>
    /// Two hand-written wrappers handed a message the application composed to a
    /// C parameter that is a <c>printf</c> format, and they had opposite fates.
    ///
    /// <c>MessageDialog</c> was safe: it composed through
    /// <c>Marshaller.StringFormat</c>, which doubles every per cent sign so that
    /// printf renders one. <c>Log.WriteLog</c> used plain
    /// <c>String.Format</c> and called <c>g_logv</c> through a delegate missing
    /// its <c>va_list</c> parameter, so a message containing <c>%s</c>
    /// dereferenced whatever was in the next register. Logging "100% complete"
    /// was undefined; logging "%s" killed the process.
    ///
    /// Every existing test of these APIs used a message with no <c>%</c> in it,
    /// which is why the suite was green, and "100% complete" is an ordinary thing
    /// for an application to log.
    ///
    /// Both now pass the text as an *argument* behind a literal <c>"%s"</c>,
    /// which cannot be got wrong by a conversion the escaping did not anticipate.
    /// </remarks>
    public class FormatStringTests : GtkTestBase
    {
        public FormatStringTests(GtkFixture fixture) : base(fixture) { }

        /// <summary>Captures what GLib delivers for one log domain.</summary>
        static List<string> CaptureLog(string domain, GLib.LogLevelFlags level, Action write)
        {
            var messages = new List<string>();

            uint id = GLib.Log.SetLogHandler(domain, level,
                (d, l, message) => messages.Add(message));

            try
            {
                write();
            }
            finally
            {
                GLib.Log.RemoveLogHandler(domain, id);
            }

            return messages;
        }

        // ------------------------------------------------------------- logging

        [Fact]
        public void A_logged_message_containing_a_per_cent_sign_arrives_intact()
        {
            // g_logv's fourth parameter is a va_list; the delegate declared three
            // parameters, so GLib read the argument list from whatever was in the
            // register. With no conversion in the message nothing was consumed and
            // it worked, which is exactly why this went unnoticed.
            Run(() =>
            {
                var messages = CaptureLog("gtksharp-format", GLib.LogLevelFlags.Info,
                    () => GLib.Log.WriteLog("gtksharp-format", GLib.LogLevelFlags.Info,
                                            "100% complete"));

                Assert.Equal(new[] { "100% complete" }, messages);
            });
        }

        [Fact]
        public void A_logged_message_containing_a_string_conversion_arrives_intact()
        {
            // The dangerous one: %s makes the callee dereference the next
            // argument as a char*.
            Run(() =>
            {
                var messages = CaptureLog("gtksharp-format-s", GLib.LogLevelFlags.Info,
                    () => GLib.Log.WriteLog("gtksharp-format-s", GLib.LogLevelFlags.Info,
                                            "literally %s and %d"));

                Assert.Equal(new[] { "literally %s and %d" }, messages);
            });
        }

        [Fact]
        public void The_managed_format_arguments_are_still_applied_first()
        {
            // WriteLog takes a format and args of its own and applies them with
            // String.Format before the string ever reaches GLib. That has to keep
            // working -- the fix is about what happens *after* that point.
            Run(() =>
            {
                var messages = CaptureLog("gtksharp-format-args", GLib.LogLevelFlags.Info,
                    () => GLib.Log.WriteLog("gtksharp-format-args", GLib.LogLevelFlags.Info,
                                            "{0} of {1}", 3, 7));

                Assert.Equal(new[] { "3 of 7" }, messages);
            });
        }

        [Fact]
        public void A_message_that_is_only_conversions_still_arrives_intact()
        {
            Run(() =>
            {
                var messages = CaptureLog("gtksharp-format-only", GLib.LogLevelFlags.Info,
                    () => GLib.Log.WriteLog("gtksharp-format-only", GLib.LogLevelFlags.Info,
                                            "%s%s%s"));

                Assert.Equal(new[] { "%s%s%s" }, messages);
            });
        }

        // ------------------------------------------------------ message dialogs

        [Fact]
        public void A_message_dialog_shows_a_per_cent_sign_as_typed()
        {
            // This one was never broken -- Marshaller.StringFormat doubled the
            // per cent signs on the way in, and printf turned them back. The test
            // is here because the escaping had to be *removed* when the call
            // started passing "%s" with the message behind it, and forgetting
            // that gave "100%% complete".
            Run(() =>
            {
                using var dialog = new Gtk.MessageDialog(
                    null, Gtk.DialogFlags.Modal, Gtk.MessageType.Info,
                    Gtk.ButtonsType.Ok, false, "100% complete");

                Assert.Equal("100% complete", (string) dialog.GetProperty("text").Val);
            });
        }

        [Fact]
        public void A_markup_message_dialog_shows_a_per_cent_sign_as_typed()
        {
            Run(() =>
            {
                using var dialog = new Gtk.MessageDialog(
                    null, Gtk.DialogFlags.Modal, Gtk.MessageType.Info,
                    Gtk.ButtonsType.Ok, true, "<b>100%</b> complete");

                // Gtk stores the markup variant's "text" property in its own
                // escaped form, so the raw string is not what comes back and
                // asserting equality would be asserting Gtk's representation.
                // What this test is about is the per cent sign: exactly one,
                // neither doubled by the escaping that used to be needed nor
                // eaten by a conversion.
                var text = (string) dialog.GetProperty("text").Val;

                Assert.Contains("100%", text);
                Assert.DoesNotContain("100%%", text);
            });
        }

        [Fact]
        public void A_message_dialogs_own_format_arguments_are_still_applied()
        {
            Run(() =>
            {
                using var dialog = new Gtk.MessageDialog(
                    null, Gtk.DialogFlags.Modal, Gtk.MessageType.Info,
                    Gtk.ButtonsType.Ok, false, "{0} of {1}", 3, 7);

                Assert.Equal("3 of 7", (string) dialog.GetProperty("text").Val);
            });
        }

        [Fact]
        public void A_message_dialog_with_no_message_at_all_is_allowed()
        {
            // The null-format path skips the format entirely, and has to keep
            // doing so rather than passing "%s" with nothing behind it.
            Run(() =>
            {
                using var dialog = new Gtk.MessageDialog(
                    null, Gtk.DialogFlags.Modal, Gtk.MessageType.Info,
                    Gtk.ButtonsType.Ok, false, null);

                Assert.NotNull(dialog);
            });
        }
    }
}
