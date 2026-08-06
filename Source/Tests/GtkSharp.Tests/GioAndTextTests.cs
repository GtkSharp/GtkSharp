using System;
using System.Text;
using Gtk;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// Gio, GtkBuilder, the text stack and GtkSourceView.
    /// </summary>
    public class GioAndTextTests : GtkTestBase
    {
        public GioAndTextTests(GtkFixture fixture) : base(fixture) { }

        // ------------------------------------------------------------------ Gio

        [Fact]
        public void A_simple_action_runs_its_handler_when_activated()
        {
            Run(() =>
            {
                var action = new GLib.SimpleAction("do-it", null);
                int calls = 0;
                action.Activated += (o, e) => calls++;

                action.Activate(null);

                Assert.Equal(1, calls);
                Assert.Equal("do-it", action.Name);
            });
        }

        [Fact]
        public void A_stateful_action_reports_and_changes_its_state()
        {
            Run(() =>
            {
                var action = new GLib.SimpleAction("toggle", null, new GLib.Variant(false));

                Assert.False((bool) action.State);

                action.ChangeState(new GLib.Variant(true));

                Assert.True((bool) action.State);
            });
        }

        [Fact]
        public void An_action_map_returns_the_action_that_was_added()
        {
            Run(() =>
            {
                var group = new GLib.SimpleActionGroup();
                group.AddAction(new GLib.SimpleAction("named", null));

                Assert.True(group.HasAction("named"));
                Assert.Equal("named", group.LookupAction("named").Name);
            });
        }

        [Fact]
        public void A_disabled_action_reports_itself_as_disabled()
        {
            Run(() =>
            {
                var action = new GLib.SimpleAction("do-it", null) { Enabled = false };

                Assert.False(action.Enabled);
            });
        }

        [Fact]
        public void A_memory_stream_reads_back_what_was_put_in_it()
        {
            Run(() =>
            {
                var payload = Encoding.UTF8.GetBytes("stream contents");
                var stream = new GLib.MemoryInputStream(new GLib.Bytes(payload));

                var buffer = new byte[payload.Length];
                var read = stream.Read(buffer, (ulong) buffer.Length, null);

                Assert.Equal(payload.Length, (int) read);
                Assert.Equal("stream contents", Encoding.UTF8.GetString(buffer));
            });
        }

        [Fact]
        public void A_GFile_reports_the_path_it_was_built_from()
        {
            Run(() =>
            {
                var file = GLib.FileFactory.NewForPath("/tmp/example.txt");

                Assert.Equal("example.txt", file.Basename);
            });
        }

        [Fact]
        public void A_menu_model_counts_the_items_appended_to_it()
        {
            Run(() =>
            {
                var menu = new GLib.Menu();
                menu.AppendItem(new GLib.MenuItem("One", "app.one"));
                menu.AppendItem(new GLib.MenuItem("Two", "app.two"));

                Assert.Equal(2, menu.NItems);
            });
        }

        // -------------------------------------------------------------- Builder

        [Fact]
        public void Builder_constructs_the_objects_its_xml_describes()
        {
            Run(() =>
            {
                const string ui = @"<?xml version='1.0' encoding='UTF-8'?>
<interface>
  <requires lib='gtk' version='4.0'/>
  <object class='GtkBox' id='root'>
    <property name='orientation'>vertical</property>
    <child>
      <object class='GtkLabel' id='caption'>
        <property name='label'>from the builder</property>
      </object>
    </child>
  </object>
</interface>";

                var builder = new Builder();
                builder.AddFromString(ui);

                var root = builder.GetObject("root") as Box;
                var caption = builder.GetObject("caption") as Label;

                Assert.NotNull(root);
                Assert.Equal(Orientation.Vertical, root.Orientation);
                Assert.Equal("from the builder", caption.Text);
            });
        }

        // ----------------------------------------------------------------- Text

        [Fact]
        public void TextBuffer_reports_the_text_between_two_iterators()
        {
            Run(() =>
            {
                var buffer = new TextBuffer(null);
                var end = buffer.EndIter;
                buffer.Insert(ref end, "hello world");

                Assert.Equal(11, buffer.CharCount);

                var start = buffer.GetIterAtOffset(0);
                var middle = buffer.GetIterAtOffset(5);

                Assert.Equal("hello", buffer.GetText(start, middle, false));
            });
        }

        [Fact]
        public void A_text_mark_follows_the_position_it_was_set_at()
        {
            Run(() =>
            {
                var buffer = new TextBuffer(null);
                var end = buffer.EndIter;
                buffer.Insert(ref end, "abcdef");

                var mark = buffer.CreateMark("here", buffer.GetIterAtOffset(3), false);

                Assert.Equal(3, buffer.GetIterAtMark(mark).Offset);
            });
        }

        [Fact]
        public void A_text_tag_can_be_applied_and_found_again()
        {
            Run(() =>
            {
                var buffer = new TextBuffer(null);
                var tag = buffer.TagTable.Lookup("bold") ?? new TextTag("bold");
                buffer.TagTable.Add(tag);

                var end = buffer.EndIter;
                buffer.Insert(ref end, "bold text");

                buffer.ApplyTag(tag, buffer.GetIterAtOffset(0), buffer.GetIterAtOffset(4));

                Assert.True(buffer.GetIterAtOffset(1).HasTag(tag));
                Assert.False(buffer.GetIterAtOffset(6).HasTag(tag));
            });
        }

        // ----------------------------------------------------------- GtkSource

        [Fact]
        public void The_language_manager_knows_c_sharp()
        {
            Run(() =>
            {
                var language = new GtkSource.LanguageManager().GetLanguage("c-sharp");

                Assert.NotNull(language);
                Assert.Equal("c-sharp", language.Id);
            });
        }

        [Fact]
        public void A_source_buffer_keeps_the_language_it_is_given()
        {
            Run(() =>
            {
                var language = new GtkSource.LanguageManager().GetLanguage("c-sharp");
                var buffer = new GtkSource.Buffer((Gtk.TextTagTable) null) { Language = language };

                Assert.Equal("c-sharp", buffer.Language.Id);
            });
        }
    }
}
