using System;
using System.IO;
using System.Reflection;
using System.Text;
using Xunit;
using UI = Gtk.Builder.ObjectAttribute;

namespace GtkSharp.Tests
{
    /// <summary>
    /// <c>Gtk.Builder.Autoconnect</c> — binding <c>[UI]</c> fields to objects in a
    /// <c>.ui</c> document, and what happens when the document asks for signal
    /// handlers that Gtk 4 cannot deliver.
    /// </summary>
    /// <remarks>
    /// This is the workflow the templates generate and the one
    /// <c>Docs/getting-started.md</c> teaches, and none of it was covered:
    /// <c>Builder.cs</c>, <c>BuilderXml.cs</c> and <c>BindingAttribute.cs</c> are
    /// entirely hand-written, so nothing about them is checked by compiling.
    ///
    /// The oracle throughout is a field this file declares and a document this
    /// file wrote — reflection is what is being tested, so nothing is asked of
    /// the library that the test cannot already answer itself.
    /// </remarks>
    public class BuilderBindingTests : GtkTestBase
    {
        public BuilderBindingTests(GtkFixture fixture) : base(fixture) { }

        // ------------------------------------------------------------ documents

        const string TwoWidgets = @"<?xml version='1.0' encoding='UTF-8'?>
<interface>
  <object class='GtkBox' id='root'>
    <child>
      <object class='GtkLabel' id='caption'>
        <property name='label'>bound</property>
      </object>
    </child>
  </object>
</interface>";

        const string WithSignal = @"<?xml version='1.0' encoding='UTF-8'?>
<interface>
  <object class='GtkButton' id='action'>
    <property name='label'>Press</property>
    <signal name='clicked' handler='OnClicked'/>
  </object>
</interface>";

        // The word "signal" in a comment is the case BuilderXml exists to get
        // right: it parses rather than searching the text, because a document
        // that says it has no signals says so using the word.
        const string SignalOnlyInAComment = @"<?xml version='1.0' encoding='UTF-8'?>
<interface>
  <!-- No <signal> elements here: handlers are connected in code. -->
  <object class='GtkButton' id='action'>
    <property name='label'>Press</property>
  </object>
</interface>";

        static Gtk.Builder Loaded(string xml)
        {
            var builder = new Gtk.Builder();
            Assert.True(builder.AddFromString(xml));
            return builder;
        }

        // ------------------------------------------------------ binding targets

        class Target
        {
            [UI] public Gtk.Box root = null;
            [UI] public Gtk.Label caption = null;

            // Named explicitly: the field is called something else on purpose.
            [UI("caption")] public Gtk.Label theSameLabelAgain = null;

            // No attribute, so Autoconnect must leave it exactly as it is.
            public Gtk.Label untouched = null;

            [UI] public Gtk.Label absent = null;
        }

        class Base
        {
            [UI] public Gtk.Box root = null;
        }

        class Derived : Base
        {
            [UI] public Gtk.Label caption = null;
        }

        class OnlyMissing
        {
            [UI] public Gtk.Label absent = null;
        }

        static class StaticTarget
        {
            [UI] public static Gtk.Label caption = null;
        }

        class PrivateTarget
        {
            [UI] Gtk.Label caption = null;

            public Gtk.Label Caption => caption;
        }

        // ---------------------------------------------------------- field binding

        [Fact]
        public void Autoconnect_binds_a_field_to_the_object_of_the_same_name()
        {
            Run(() =>
            {
                var builder = Loaded(TwoWidgets);
                var target = new Target();

                builder.Autoconnect(target);

                Assert.Same(builder.GetObject("root"), target.root);
                Assert.Same(builder.GetObject("caption"), target.caption);
                Assert.Equal("bound", target.caption.Text);
            });
        }

        [Fact]
        public void A_named_attribute_binds_to_that_name_rather_than_the_field_name()
        {
            Run(() =>
            {
                var builder = Loaded(TwoWidgets);
                var target = new Target();

                builder.Autoconnect(target);

                // Nothing in the document is called "theSameLabelAgain".
                Assert.Same(builder.GetObject("caption"), target.theSameLabelAgain);
            });
        }

        [Fact]
        public void A_field_without_the_attribute_is_left_alone()
        {
            Run(() =>
            {
                var builder = Loaded(TwoWidgets);
                var target = new Target { untouched = new Gtk.Label("mine") };

                builder.Autoconnect(target);

                Assert.Equal("mine", target.untouched.Text);
            });
        }

        [Fact]
        public void A_private_field_is_bound_too()
        {
            // The templates declare their fields private, so this is the shape
            // that actually ships.
            Run(() =>
            {
                var builder = Loaded(TwoWidgets);
                var target = new PrivateTarget();

                builder.Autoconnect(target);

                Assert.Same(builder.GetObject("caption"), target.Caption);
            });
        }

        [Fact]
        public void Fields_declared_on_a_base_class_are_bound_as_well()
        {
            // BindFields walks up BaseType with DeclaredOnly, which is the only
            // reason a field on Base is reached at all.
            Run(() =>
            {
                var builder = Loaded(TwoWidgets);
                var target = new Derived();

                builder.Autoconnect(target);

                Assert.Same(builder.GetObject("root"), target.root);
                Assert.Same(builder.GetObject("caption"), target.caption);
            });
        }

        [Fact]
        public void Autoconnect_of_a_type_binds_static_fields()
        {
            Run(() =>
            {
                StaticTarget.caption = null;

                var builder = Loaded(TwoWidgets);
                builder.Autoconnect(typeof(StaticTarget));

                Assert.Same(builder.GetObject("caption"), StaticTarget.caption);

                StaticTarget.caption = null;
            });
        }

        // ------------------------------------------------------ the missing case

        [Fact]
        public void A_field_naming_nothing_in_the_document_is_left_null_by_default()
        {
            Run(() =>
            {
                var builder = Loaded(TwoWidgets);
                var target = new Target();

                builder.Autoconnect(target);

                Assert.Null(target.absent);
                // ...and the rest still bound, which is the point of the default.
                Assert.NotNull(target.root);
            });
        }

        [Fact]
        public void Asking_to_be_told_about_a_missing_object_raises_and_names_it()
        {
            Run(() =>
            {
                var builder = Loaded(TwoWidgets);

                var error = Assert.ThrowsAny<Exception>(
                    () => builder.Autoconnect(new OnlyMissing(), true));

                Assert.Contains("absent", error.Message);
            });
        }

        // ------------------------------------------------------------- signals

        // A .ui file that declares a handler cannot be loaded by this binding at
        // all -- not "loads with its signals unconnected", which is what
        // Docs/getting-started.md used to promise and what Autoconnect's
        // NotSupportedException was written for. Gtk 4 resolves the handler while
        // *parsing*, through GtkBuilderScope; the default scope looks the name up
        // as an exported C symbol, does not find a managed method, and fails the
        // whole document. Autoconnect is never reached.

        [Fact]
        public void A_document_that_declares_a_handler_cannot_be_loaded_at_all()
        {
            Run(() =>
            {
                var builder = new Gtk.Builder();

                var error = Assert.Throws<NotSupportedException>(
                    () => builder.AddFromString(WithSignal));

                // The message has to name the cause, because GtkBuilder's own
                // does not: "No function named `OnClicked`" reads as a missing
                // native symbol.
                Assert.Contains("GtkBuilderScope", error.Message);
                Assert.Contains("connect the handlers in C#", error.Message);

                // GtkBuilder's own report is kept rather than thrown away.
                var inner = Assert.IsAssignableFrom<GLib.GException>(error.InnerException);
                Assert.Contains("OnClicked", inner.Message);
            });
        }

        [Fact]
        public void The_explanation_is_offered_only_for_documents_that_declared_a_signal()
        {
            // Every other builder error is GtkBuilder's to describe, and it does
            // so better than anything invented here. Turning all of them into
            // NotSupportedException would be worse than the bug being fixed.
            Run(() =>
            {
                var builder = new Gtk.Builder();

                Assert.ThrowsAny<GLib.GException>(
                    () => builder.AddFromString(
                        "<interface><object class='NoSuchWidget' id='x'/></interface>"));
            });
        }

        [Fact]
        public void A_document_that_asks_for_a_handler_is_recorded_as_having_done_so()
        {
            // The flag is set before the native call, so it survives the failure
            // and answers "was that my XML, or is this unsupported?".
            Run(() =>
            {
                var builder = new Gtk.Builder();

                Assert.False(builder.DeclaresSignals);
                Assert.Throws<NotSupportedException>(() => builder.AddFromString(WithSignal));
                Assert.True(builder.DeclaresSignals);
            });
        }

        [Fact]
        public void A_document_without_handlers_is_not_recorded_as_having_them()
        {
            Run(() => Assert.False(Loaded(TwoWidgets).DeclaresSignals));
        }

        [Fact]
        public void One_document_with_signals_is_enough_to_taint_the_builder()
        {
            Run(() =>
            {
                var builder = new Gtk.Builder();

                Assert.Throws<NotSupportedException>(() => builder.AddFromString(WithSignal));
                Assert.True(builder.DeclaresSignals);

                // A later document without signals must not clear it: the
                // handlers the first one asked for are still unconnected.
                Assert.True(builder.AddFromString(TwoWidgets));
                Assert.True(builder.DeclaresSignals);
            });
        }

        [Fact]
        public void The_word_signal_in_a_comment_does_not_count_as_a_signal()
        {
            // BuilderXml parses instead of searching for "<signal" precisely so
            // that a comment saying a document has none is not read as declaring
            // one -- which would turn a perfectly good load failure into the
            // wrong explanation, or a good document into a refused one.
            Run(() =>
            {
                var builder = Loaded(SignalOnlyInAComment);

                Assert.False(builder.DeclaresSignals);
                Assert.Equal("Press", ((Gtk.Button) builder.GetObject("action")).Label);

                // ...and Autoconnect gets as far as binding rather than throwing.
                builder.Autoconnect(new object());
            });
        }

        // ----------------------------------------------------- the load paths

        [Fact]
        public void A_document_loaded_from_a_file_is_explained_the_same_way()
        {
            // AddFromFile reads the file itself to look for signals, so the
            // explanation has to reach this path too -- it is the one the
            // templates use.
            Run(() =>
            {
                var path = Path.Combine(Path.GetTempPath(),
                                        "gtksharp-builder-" + Guid.NewGuid().ToString("N") + ".ui");
                try
                {
                    File.WriteAllText(path, WithSignal);

                    var error = Assert.Throws<NotSupportedException>(
                        () => new Gtk.Builder().AddFromFile(path));

                    Assert.Contains("GtkBuilderScope", error.Message);
                }
                finally
                {
                    File.Delete(path);
                }
            });
        }

        [Fact]
        public void A_document_loaded_from_a_file_without_signals_still_builds()
        {
            Run(() =>
            {
                var path = Path.Combine(Path.GetTempPath(),
                                        "gtksharp-builder-" + Guid.NewGuid().ToString("N") + ".ui");
                try
                {
                    File.WriteAllText(path, TwoWidgets);

                    var builder = new Gtk.Builder();
                    Assert.True(builder.AddFromFile(path));

                    Assert.False(builder.DeclaresSignals);
                    Assert.Equal("bound", ((Gtk.Label) builder.GetObject("caption")).Text);
                }
                finally
                {
                    File.Delete(path);
                }
            });
        }

        [Fact]
        public void A_file_that_is_not_there_is_still_GtkBuilders_error_to_report()
        {
            // AddFromFile reads the file itself to look for signals. That read
            // must not turn a missing file into a different exception than the
            // one GtkBuilder raises, with the path in it.
            Run(() =>
            {
                var missing = Path.Combine(Path.GetTempPath(), "gtksharp-no-such-" + Guid.NewGuid().ToString("N") + ".ui");

                var error = Assert.ThrowsAny<GLib.GException>(
                    () => new Gtk.Builder().AddFromFile(missing));

                Assert.Contains("gtksharp-no-such-", error.Message);
            });
        }

        [Fact]
        public void A_builder_reads_a_ui_file_out_of_an_assembly_resource()
        {
            // The constructor the templates use. Nothing else in the suite loads
            // a manifest resource, so this is also the only check that the
            // stream path handles a document written by a tool rather than by a
            // string literal.
            Run(() =>
            {
                var builder = new Gtk.Builder(
                    Assembly.GetExecutingAssembly(), "GtkSharp.Tests.embedded-window.ui", null);

                var greeting = builder.GetObject("greeting") as Gtk.Label;

                Assert.NotNull(greeting);
                Assert.Equal("from an embedded resource", greeting.Text);

                // Its only mention of a signal is inside a comment.
                Assert.False(builder.DeclaresSignals);
            });
        }

        [Fact]
        public void Naming_a_resource_that_does_not_exist_says_so()
        {
            Run(() =>
            {
                var error = Assert.Throws<ArgumentException>(
                    () => new Gtk.Builder(Assembly.GetExecutingAssembly(), "no.such.resource", null));

                Assert.Contains("no.such.resource", error.Message);
            });
        }

        [Fact]
        public void A_byte_order_mark_in_front_of_the_document_is_dropped()
        {
            // GtkBuilder rejects a document whose first character is a BOM, so
            // AddFromStream strips it. A .ui file saved by a Windows editor
            // routinely has one.
            Run(() =>
            {
                var bytes = new byte[] { 0xEF, 0xBB, 0xBF };
                using var stream = new MemoryStream(
                    Concat(bytes, Encoding.UTF8.GetBytes(TwoWidgets)));

                var builder = new Gtk.Builder(stream);

                Assert.Equal("bound", ((Gtk.Label) builder.GetObject("caption")).Text);
            });
        }

        [Fact]
        public void A_document_from_a_stream_is_explained_the_same_way()
        {
            Run(() =>
            {
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(WithSignal));

                var error = Assert.Throws<NotSupportedException>(() => new Gtk.Builder(stream));

                Assert.Contains("GtkBuilderScope", error.Message);
            });
        }

        [Fact]
        public void A_null_stream_is_refused_before_anything_native_happens()
        {
            Run(() => Assert.Throws<ArgumentNullException>(() => new Gtk.Builder((Stream) null)));
        }

        static byte[] Concat(byte[] first, byte[] second)
        {
            var joined = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, joined, 0, first.Length);
            Buffer.BlockCopy(second, 0, joined, first.Length, second.Length);
            return joined;
        }
    }
}
