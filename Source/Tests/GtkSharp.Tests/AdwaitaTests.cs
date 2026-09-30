using System;
using Gtk;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// libadwaita, which nothing else in the suite touched at all.
    /// </summary>
    /// <remarks>
    /// AdwaitaSharp is a from-scratch binding added during the Gtk 4 migration
    /// and was the one assembly with zero coverage: it built and packed, and no
    /// line of it had ever run. Since a missing native export here is a null
    /// delegate rather than a link error, "it builds" said nothing at all about
    /// whether it works.
    ///
    /// Adw requires its own initialisation before any of its widgets are used,
    /// which is what the fixture below does once for the class.
    /// </remarks>
    public class AdwaitaTests : GtkTestBase, IDisposable
    {
        private static bool _initialised;

        public AdwaitaTests(GtkFixture fixture) : base(fixture)
        {
            Run(() =>
            {
                if (_initialised)
                    return;

                Adw.Global.Init();
                _initialised = true;
            });
        }

        public void Dispose() { }

        [Fact]
        public void Adwaita_reports_a_version()
        {
            Run(() =>
            {
                Assert.True(Adw.Global.MajorVersion >= 1,
                            $"expected libadwaita 1 or later, got {Adw.Global.MajorVersion}");
            });
        }

        [Fact]
        public void An_action_row_carries_its_title_and_subtitle()
        {
            Run(() =>
            {
                var row = new Adw.ActionRow { Title = "Wi-Fi", Subtitle = "Connected" };

                Assert.Equal("Wi-Fi", row.Title);
                Assert.Equal("Connected", row.Subtitle);
            });
        }

        [Fact]
        public void A_preferences_group_holds_the_rows_added_to_it()
        {
            Run(() =>
            {
                var group = new Adw.PreferencesGroup { Title = "Network" };
                var row = new Adw.ActionRow { Title = "Wi-Fi" };

                group.Add(row);

                Assert.Equal("Network", group.Title);

                // The group wraps its rows in an internal list box, so the row
                // is a descendant rather than a direct child. How deep is Adw's
                // business; that it ended up inside the group is not.
                Assert.True(IsDescendantOf(row, group),
                            "the row should end up somewhere inside the group");
            });
        }

        private static bool IsDescendantOf(Widget child, Widget ancestor)
        {
            for (var parent = child.Parent; parent != null; parent = parent.Parent)
                if (parent.Handle == ancestor.Handle)
                    return true;

            return false;
        }

        [Fact]
        public void A_status_page_carries_its_description()
        {
            Run(() =>
            {
                var page = new Adw.StatusPage
                {
                    Title = "Nothing here",
                    Description = "Add something to get started",
                    IconName = "folder-symbolic",
                };

                Assert.Equal("Nothing here", page.Title);
                Assert.Equal("Add something to get started", page.Description);
                Assert.Equal("folder-symbolic", page.IconName);
            });
        }

        [Fact]
        public void A_banner_reports_whether_it_is_revealed()
        {
            Run(() =>
            {
                var banner = new Adw.Banner("Update available") { ButtonLabel = "Install" };

                Assert.Equal("Update available", banner.Title);
                Assert.Equal("Install", banner.ButtonLabel);

                banner.Revealed = true;
                Assert.True(banner.Revealed);
            });
        }

        [Fact]
        public void An_avatar_keeps_its_size_and_text()
        {
            Run(() =>
            {
                var avatar = new Adw.Avatar(48, "Ada Lovelace", true);

                Assert.Equal(48, avatar.Size);
                Assert.Equal("Ada Lovelace", avatar.Text);
                Assert.True(avatar.ShowInitials);
            });
        }

        [Fact]
        public void A_clamp_limits_the_size_of_its_child()
        {
            Run(() =>
            {
                var clamp = new Adw.Clamp { MaximumSize = 600, Child = new Label("inside") };

                Assert.Equal(600, clamp.MaximumSize);
                Assert.NotNull(clamp.Child);
            });
        }

        [Fact]
        public void A_toast_carries_its_title_and_can_be_sent_to_an_overlay()
        {
            Run(() =>
            {
                var overlay = new Adw.ToastOverlay { Child = new Label("content") };
                var toast = new Adw.Toast("Saved") { Timeout = 1 };

                Assert.Equal("Saved", toast.Title);

                // Adding a toast is the whole point of the overlay; it must not
                // require a mapped window to accept one.
                overlay.AddToast(toast);

                Assert.NotNull(overlay.Child);
            });
        }

        [Fact]
        public void A_button_content_carries_its_label_and_icon()
        {
            Run(() =>
            {
                var content = new Adw.ButtonContent { Label = "Open", IconName = "document-open-symbolic" };

                Assert.Equal("Open", content.Label);
                Assert.Equal("document-open-symbolic", content.IconName);
            });
        }

        [Fact]
        public void A_window_title_carries_both_lines()
        {
            Run(() =>
            {
                var title = new Adw.WindowTitle("Document", "Edited");

                Assert.Equal("Document", title.Title);
                Assert.Equal("Edited", title.Subtitle);
            });
        }

        [Fact]
        public void A_carousel_counts_the_pages_appended_to_it()
        {
            Run(() =>
            {
                var carousel = new Adw.Carousel();

                carousel.Append(new Label("one"));
                carousel.Append(new Label("two"));

                Assert.Equal(2u, carousel.NPages);
            });
        }

        [Fact]
        public void The_style_manager_reports_a_colour_scheme()
        {
            Run(() =>
            {
                var manager = Adw.StyleManager.Default;

                manager.ColorScheme = Adw.ColorScheme.ForceLight;

                Assert.Equal(Adw.ColorScheme.ForceLight, manager.ColorScheme);
            });
        }
    }
}
