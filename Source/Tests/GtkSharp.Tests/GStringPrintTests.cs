using System;
using System.Linq;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// <c>GLib.GString</c>, and the ten calls across gdk, gio, gsk and gtk that
    /// print into one.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Every one of these is shaped <c>void thing_print (Thing *, GString *out)</c>:
    /// the caller allocates the buffer, the callee appends to it, the caller
    /// reads it back. The binding took a C# <c>string</c>, built a fresh GString
    /// out of it, let the callee append, and then dropped the buffer — so the
    /// only thing these methods exist to do could not be observed at all, and a
    /// GString leaked on every call. <c>Gdk.RGBA.Print</c> was worse: it returns
    /// the buffer, and the return was decoded by reading the <c>GString*</c>
    /// itself as UTF-8 rather than the <c>str</c> field inside it.
    /// </para>
    /// <para>
    /// The oracle for each of these is fourfold, and no part of it can pass
    /// against a binding that throws the buffer away: the prefix already in the
    /// buffer has to survive (so it must be *our* buffer), printing twice has to
    /// append twice (arithmetic the test does itself), the text has to agree with
    /// the object's independent <c>to_string</c> sibling, and a different object
    /// has to print something different.
    /// </para>
    /// </remarks>
    public class GStringPrintTests : GtkTestBase
    {
        public GStringPrintTests(GtkFixture fixture) : base(fixture) { }

        // ---------------------------------------------------------- the buffer

        [Fact]
        public void A_buffer_hands_back_the_text_it_holds_and_its_length_in_utf8_bytes()
        {
            Run(() =>
            {
                // "naïve" is five characters and six utf-8 bytes, so a length
                // taken from glib cannot be confused with the C# string's.
                const string text = "naïve";

                using var buffer = new GLib.GString(text);

                Assert.Equal(text, buffer.Str);
                Assert.Equal(text, buffer.ToString());
                Assert.Equal(System.Text.Encoding.UTF8.GetByteCount(text), buffer.Length);
                Assert.NotEqual(text.Length, buffer.Length);

                using var empty = new GLib.GString();
                Assert.Equal(string.Empty, empty.Str);
                Assert.Equal(0, empty.Length);
            });
        }

        [Fact]
        public void Reading_a_GString_pointer_reads_the_string_inside_it_not_the_struct()
        {
            // GString is { gchar *str; gsize len; gsize allocated_len; }, so
            // decoding the GString* as utf-8 decodes the bytes of a heap
            // pointer. PtrToString did exactly that and returned whatever those
            // bytes happened to spell.
            Run(() =>
            {
                using var buffer = new GLib.GString("mark");

                Assert.Equal("mark", GLib.GString.PtrToString(buffer.Handle));
                Assert.Null(GLib.GString.PtrToString(IntPtr.Zero));
            });
        }

        [Fact]
        public void Appending_and_truncating_change_the_buffer_without_replacing_it()
        {
            Run(() =>
            {
                using var buffer = new GLib.GString("one");
                var handle = buffer.Handle;

                buffer.Append(" two").Append(" three");

                Assert.Equal("one two three", buffer.Str);
                Assert.Equal(13, buffer.Length);
                Assert.Equal(handle, buffer.Handle);

                buffer.Truncate(3);
                Assert.Equal("one", buffer.Str);
                Assert.Equal(handle, buffer.Handle);

                buffer.Truncate(0);
                Assert.Equal(string.Empty, buffer.Str);
                Assert.Equal(0, buffer.Length);
            });
        }

        // ------------------------------------------------- gtk: shortcut triggers

        static Gtk.KeyvalTrigger Keys(Gdk.Key key, Gdk.ModifierType modifiers)
        {
            return new Gtk.KeyvalTrigger((uint) key, modifiers);
        }

        [Fact]
        public void A_shortcut_trigger_prints_into_the_buffer_it_was_given_and_appends_to_what_is_there()
        {
            Run(() =>
            {
                var ctrlA = Keys(Gdk.Key.a, Gdk.ModifierType.ControlMask);
                var ctrlB = Keys(Gdk.Key.b, Gdk.ModifierType.ControlMask);

                // The accelerator syntax gtk documents, and the same text
                // gtk_accelerator_name produces for the pair -- a second entry
                // point into gtk that has to agree.
                Assert.Equal("<Control>a", ctrlA.ToString());
                Assert.Equal(Gtk.Accelerator.Name((uint) Gdk.Key.a, Gdk.ModifierType.ControlMask),
                             ctrlA.ToString());

                using var buffer = new GLib.GString("keys: ");
                ctrlA.Print(buffer);

                Assert.Equal("keys: <Control>a", buffer.Str);

                buffer.Append(" and ");
                ctrlB.Print(buffer);

                // The control is built in: had ctrlB printed the same thing as
                // ctrlA, or nothing, this could not match.
                Assert.Equal("keys: <Control>a and <Control>b", buffer.Str);
            });
        }

        [Fact]
        public void A_trigger_printed_into_a_buffer_parses_back_to_an_equal_trigger()
        {
            // A round trip through gtk's own parser, which never sees the
            // managed object -- only the text that came out of the buffer.
            Run(() =>
            {
                var trigger = Keys(Gdk.Key.F5, Gdk.ModifierType.ControlMask | Gdk.ModifierType.ShiftMask);
                var other = Keys(Gdk.Key.F5, Gdk.ModifierType.ControlMask);

                using var buffer = new GLib.GString();
                trigger.Print(buffer);

                var parsed = new Gtk.ShortcutTrigger(buffer.Str);

                Assert.NotEqual(IntPtr.Zero, parsed.Handle);
                Assert.True(parsed.Equal(trigger.Handle), buffer.Str + " should parse back equal");
                Assert.False(parsed.Equal(other.Handle), "shift is part of the trigger");
                Assert.Equal(trigger.Hash(), parsed.Hash());
            });
        }

        [Fact]
        public void A_string_that_names_no_key_produces_no_trigger()
        {
            // The control for the round trip above: parsing is what decides
            // whether the printed text meant anything, and it can say no.
            Run(() =>
            {
                var nothing = new Gtk.ShortcutTrigger("<NotAModifier>notakey");

                Assert.Equal(IntPtr.Zero, nothing.Handle);
            });
        }

        [Fact]
        public void A_trigger_label_goes_into_the_buffer_the_caller_supplied()
        {
            // print_label is the display-aware sibling of print: same shape,
            // separate binding, and it returns whether anything was written.
            Run(() =>
            {
                var display = Gdk.Display.Default;
                Assert.NotNull(display);

                var ctrlA = Keys(Gdk.Key.a, Gdk.ModifierType.ControlMask);
                var ctrlB = Keys(Gdk.Key.b, Gdk.ModifierType.ControlMask);

                var label = ctrlA.ToLabel(display);
                Assert.False(string.IsNullOrEmpty(label));
                Assert.NotEqual(label, ctrlB.ToLabel(display));

                using var buffer = new GLib.GString("press ");
                Assert.True(ctrlA.PrintLabel(display, buffer));

                Assert.Equal("press " + label, buffer.Str);
            });
        }

        // -------------------------------------------------- gtk: shortcut actions

        [Fact]
        public void A_shortcut_action_prints_into_the_buffer_and_parses_back()
        {
            Run(() =>
            {
                var named = new Gtk.NamedAction("win.close");
                var nothing = Gtk.NothingAction.Get();

                using var buffer = new GLib.GString("do: ");
                named.Print(buffer);

                Assert.Equal("do: action(win.close)", buffer.Str);

                buffer.Append(", else ");
                nothing.Print(buffer);
                Assert.Equal("do: action(win.close), else nothing", buffer.Str);

                // Round trip: gtk parses the text back into an action of the
                // same kind, holding the same action name.
                //
                // The wrapper is typed Gtk.ShortcutAction and stays that way --
                // gtk_shortcut_action_parse_string is bound as a constructor on
                // the base class, so the wrapper registers itself under that
                // type before anything can look at what gtk actually built, and
                // an IsType<NamedAction> here fails. The expectation was mine,
                // not the library's, and the *native* object is what the round
                // trip is about: its GType and its action-name property both say
                // GtkNamedAction, read straight out of GObject.
                var parsed = new Gtk.ShortcutAction("action(win.close)");
                Assert.NotEqual(IntPtr.Zero, parsed.Handle);
                Assert.Equal("action(win.close)", parsed.ToString());
                Assert.Equal(Gtk.NamedAction.GType, parsed.NativeType);
                Assert.Equal("win.close", parsed.GetProperty("action-name").Val);
            });
        }

        // ----------------------------------------------------------- gdk: RGBA

        [Fact]
        public void Printing_an_RGBA_fills_the_callers_buffer_and_hands_it_straight_back()
        {
            Run(() =>
            {
                var red = new Gdk.RGBA { Red = 1f, Green = 0f, Blue = 0f, Alpha = 1f };
                var blue = new Gdk.RGBA { Red = 0f, Green = 0f, Blue = 1f, Alpha = 1f };

                Assert.Equal("rgb(255,0,0)", red.ToString());

                using var buffer = new GLib.GString("color: ");
                var returned = red.Print(buffer);

                Assert.Equal("color: rgb(255,0,0)", buffer.Str);

                // gdk_rgba_print returns the buffer it was handed. That is why
                // the binding must wrap the return non-owning: an owning wrapper
                // over it would free what the caller still holds.
                Assert.NotNull(returned);
                Assert.Equal(buffer.Handle, returned.Handle);
                Assert.Equal(buffer.Str, returned.Str);

                buffer.Append("; ");
                blue.Print(buffer);
                Assert.Equal("color: rgb(255,0,0); rgb(0,0,255)", buffer.Str);
            });
        }

        // ------------------------------------------------ gdk: content formats

        [Fact]
        public void Content_formats_print_into_a_buffer_and_parse_back()
        {
            Run(() =>
            {
                using var formats = new Gdk.ContentFormats(GLib.GType.String);

                using var buffer = new GLib.GString("accepts ");
                formats.Print(buffer);

                Assert.StartsWith("accepts ", buffer.Str);
                var printed = buffer.Str.Substring("accepts ".Length);
                Assert.Equal(formats.ToString(), printed);

                using var again = Gdk.ContentFormats.Parse(printed);
                Assert.NotNull(again);
                Assert.True(again.ContainGtype(GLib.GType.String));
                Assert.False(again.ContainGtype(Gdk.Texture.GType));
            });
        }

        // -------------------------------------------------------- gsk: geometry

        [Fact]
        public void A_path_printed_into_a_buffer_parses_back_to_the_same_path()
        {
            Run(() =>
            {
                var triangle = Gsk.Path.Parse("M 0 0 L 10 0 L 10 10 Z");
                var line = Gsk.Path.Parse("M 0 0 L 20 0");
                Assert.NotNull(triangle);
                Assert.NotNull(line);
                Assert.False(triangle.Equal(line));

                using var buffer = new GLib.GString("d=");
                triangle.Print(buffer);

                Assert.StartsWith("d=", buffer.Str);
                var printed = buffer.Str.Substring(2);
                Assert.Equal(triangle.ToString(), printed);

                // gsk parses the text back and reports the result equal to the
                // path it never saw.
                var again = Gsk.Path.Parse(printed);
                Assert.NotNull(again);
                Assert.True(triangle.Equal(again));
                Assert.False(line.Equal(again));
            });
        }

        [Fact]
        public void A_transform_prints_into_the_buffer_the_caller_keeps()
        {
            Run(() =>
            {
                Assert.True(Gsk.Transform.Parse("translate(10, 20)", out var moved));
                Assert.True(Gsk.Transform.Parse("scale(2)", out var scaled));
                Assert.NotEqual(moved.ToString(), scaled.ToString());

                using var buffer = new GLib.GString();
                moved.Print(buffer);
                var first = buffer.Str;

                Assert.Equal(moved.ToString(), first);
                Assert.Contains("translate", first);

                // Printing again appends rather than replacing: the callee is
                // writing into the buffer the test holds, not into one of its own.
                moved.Print(buffer);
                Assert.Equal(first + first, buffer.Str);
            });
        }

        // ------------------------------------------------------ gtk: css section

        [Fact]
        public void A_css_section_prints_into_the_buffer_it_is_given()
        {
            Run(() =>
            {
                Gtk.CssSection section = null;
                var provider = new Gtk.CssProvider();
                provider.ParsingError += (o, args) => { section ??= args.Section; };
                provider.LoadFromString("button { this-is-not-a-property: 3; }");

                Assert.NotNull(section);

                using var buffer = new GLib.GString("at ");
                section.Print(buffer);
                var first = buffer.Str;

                Assert.StartsWith("at ", first);
                Assert.Equal("at " + section.ToString(), first);

                section.Print(buffer);
                Assert.Equal(first + section.ToString(), buffer.Str);
            });
        }

        // -------------------------------------------------------- gio: dbus xml

        const string NodeXml =
            "<node>" +
            "  <interface name='org.gtksharp.Tests.Thing'>" +
            "    <method name='Ping'><arg name='payload' type='s' direction='in'/></method>" +
            "    <property name='Count' type='u' access='read'/>" +
            "  </interface>" +
            "</node>";

        [Fact]
        public void Generated_dbus_xml_lands_in_the_buffer_and_parses_back_to_the_same_interface()
        {
            // A round trip through a serialisation this binding did not write:
            // gio generates the XML, gio parses it again, and the interface has
            // to survive with its member names intact.
            Run(() =>
            {
                var info = new GLib.DBusNodeInfo(NodeXml);

                using var buffer = new GLib.GString();
                info.GenerateXml(0, buffer);

                Assert.Contains("org.gtksharp.Tests.Thing", buffer.Str);

                var again = new GLib.DBusNodeInfo(buffer.Str);

                var iface = again.LookupInterface("org.gtksharp.Tests.Thing");
                Assert.NotNull(iface);
                Assert.NotNull(iface.LookupMethod("Ping"));
                Assert.NotNull(iface.LookupProperty("Count"));

                // Control: the same lookup for something that was never in the
                // document has to come back empty, or "not null" means nothing.
                Assert.Null(again.LookupInterface("org.gtksharp.Tests.Absent"));
                Assert.Null(iface.LookupMethod("Pong"));
            });
        }

        [Fact]
        public void The_indent_argument_shifts_the_generated_xml_by_that_many_spaces()
        {
            // The indent is the one thing about GenerateXml the caller chooses,
            // and comparing the two forms line by line is arithmetic the test
            // does itself rather than anything gio reports.
            Run(() =>
            {
                var info = new GLib.DBusNodeInfo(NodeXml);

                using var flat = new GLib.GString();
                using var indented = new GLib.GString();
                info.GenerateXml(0, flat);
                info.GenerateXml(4, indented);

                Assert.StartsWith("<node", flat.Str);
                Assert.StartsWith("    <node", indented.Str);

                var flatLines = flat.Str.Split('\n');
                var indentedLines = indented.Str.Split('\n');

                Assert.True(flatLines.Length > 3, "the document should be more than one line");
                Assert.Equal(flatLines.Length, indentedLines.Length);

                for (var i = 0; i < flatLines.Length; i++)
                    Assert.Equal(flatLines[i].Length == 0 ? string.Empty : "    " + flatLines[i],
                                 indentedLines[i]);
            });
        }

        [Fact]
        public void An_interface_generates_only_its_own_xml()
        {
            // GenerateXml is bound separately on GDBusInterfaceInfo, and its
            // output is a strict part of the node's -- so the node's document
            // has to contain every line of it, indented one step further.
            Run(() =>
            {
                var info = new GLib.DBusNodeInfo(NodeXml);
                var iface = info.LookupInterface("org.gtksharp.Tests.Thing");
                Assert.NotNull(iface);

                using var ifaceXml = new GLib.GString();
                iface.GenerateXml(2, ifaceXml);

                using var nodeXml = new GLib.GString();
                info.GenerateXml(0, nodeXml);

                Assert.StartsWith("  <interface", ifaceXml.Str);

                foreach (var line in ifaceXml.Str.Split('\n').Where(l => l.Length > 0))
                    Assert.Contains(line, nodeXml.Str);

                Assert.DoesNotContain("<node", ifaceXml.Str);
            });
        }
    }
}
