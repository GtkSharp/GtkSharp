using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using Samples;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// Constructs every sample section. The samples are the widest exercise of
    /// the bindings in this repository, so this is the broadest regression net
    /// available -- it is what caught gtk_button_new_from_stock and the
    /// template-instance crash.
    /// </summary>
    /// <remarks>
    /// One test case per section, so a failure names the section rather than
    /// reporting "something in the samples broke". Each case asserts that the
    /// section produced a live widget, not merely that construction did not
    /// throw: a section that silently yields nothing is just as broken.
    /// </remarks>
    public class SampleSectionTests : GtkTestBase
    {
        public SampleSectionTests(GtkFixture gtk) : base(gtk) { }

        public static IEnumerable<object[]> Sections()
        {
            return typeof(SectionAttribute).Assembly
                .GetTypes()
                .Where(t => t.GetCustomAttributes(typeof(SectionAttribute), true).Length > 0)
                .OrderBy(t => t.Name)
                .Select(t => new object[] { t.FullName });
        }

        [Fact]
        public void There_are_sections_to_test()
        {
            // Without this, an empty section list would make every theory below
            // pass vacuously.
            Assert.NotEmpty(Sections());
        }

        [SkippableTheory]
        [MemberData(nameof(Sections))]
        public void Section_constructs_and_produces_a_live_widget(string typeName)
        {
            Skip.If(TestEnvironment.SkipWebKitSectionNamed(typeName), TestEnvironment.WebKitSkipReason);

            var type = typeof(SectionAttribute).Assembly.GetType(typeName);
            Assert.NotNull(type);

            var widget = Run(() =>
            {
                // Several sections add actions to the application, so they need
                // the same bootstrap the real program gives them.
                Program.EnsureApplication();

                return Activator.CreateInstance(type) as Widget;
            });

            Assert.NotNull(widget);
            Assert.NotEqual(IntPtr.Zero, widget.Handle);
        }
    }
}
