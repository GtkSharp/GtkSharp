using System;
using System.Collections;
using System.Linq;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// <c>GLib.List</c>, <c>GLib.SList</c> and the <c>ListBase</c> underneath
    /// them — the marshalling every list-returning call in the binding goes
    /// through.
    /// </summary>
    /// <remarks>
    /// 290 hand-written lines that nothing referenced by name, and the place two
    /// separate defects found in this sweep actually lived: a <c>GSList</c> built
    /// without an element type hands back a list of nulls, silently, because
    /// <c>DataMarshal</c> falls through to "is this a GObject?" and a
    /// PangoAttribute is not. That is pinned here rather than left to be
    /// rediscovered from the next crash.
    ///
    /// The oracle throughout is data the test put in: a managed array goes into a
    /// native list and has to come back out unchanged, by index, by enumerator and
    /// by <c>CopyTo</c>, all agreeing.
    /// </remarks>
    public class GLibListTests : GtkTestBase
    {
        public GLibListTests(GtkFixture fixture) : base(fixture) { }

        static readonly string[] Three = { "alpha", "bravo", "charlie" };

        // ------------------------------------------------------- round-tripping

        [Fact]
        public void A_list_of_strings_comes_back_as_the_strings_that_went_in()
        {
            Run(() =>
            {
                using var list = new GLib.List(Three, typeof(string), true, true);

                Assert.Equal(3, list.Count);
                Assert.Equal("alpha", list[0]);
                Assert.Equal("charlie", list[2]);
            });
        }

        [Fact]
        public void A_singly_linked_list_behaves_the_same_as_a_doubly_linked_one()
        {
            // GList and GSList differ only in the node layout, and ListBase reads
            // both through the same two offsets. If that assumption were wrong,
            // one of these would walk off into nothing.
            Run(() =>
            {
                using var list = new GLib.SList(Three, typeof(string), true, true);

                Assert.Equal(3, list.Count);
                Assert.Equal(new object[] { "alpha", "bravo", "charlie" }, list.Cast<object>().ToArray());
            });
        }

        [Fact]
        public void Index_enumerator_and_CopyTo_all_report_the_same_contents()
        {
            // Three separate walks over the same native chain. They are written
            // independently in ListBase, so agreeing is not a foregone conclusion.
            Run(() =>
            {
                using var list = new GLib.List(Three, typeof(string), true, true);

                var byIndex = new object[list.Count];
                for (int i = 0; i < list.Count; i++)
                    byIndex[i] = list[i];

                var byEnumerator = list.Cast<object>().ToArray();

                var byCopyTo = new object[list.Count];
                list.CopyTo(byCopyTo, 0);

                Assert.Equal(byIndex, byEnumerator);
                Assert.Equal(byIndex, byCopyTo);
            });
        }

        [Fact]
        public void CopyTo_honours_the_offset_it_is_given()
        {
            Run(() =>
            {
                using var list = new GLib.List(Three, typeof(string), true, true);

                var destination = new object[5];
                list.CopyTo(destination, 2);

                Assert.Null(destination[0]);
                Assert.Null(destination[1]);
                Assert.Equal("alpha", destination[2]);
                Assert.Equal("charlie", destination[4]);
            });
        }

        [Fact]
        public void An_empty_list_has_no_elements_and_survives_being_walked()
        {
            Run(() =>
            {
                using var list = new GLib.List(typeof(string));

                Assert.Equal(0, list.Count);
                Assert.Empty(list.Cast<object>());

                var destination = new object[0];
                list.CopyTo(destination, 0);      // must not walk off the end
            });
        }

        // ------------------------------------------------------------- ordering

        [Fact]
        public void Append_adds_at_the_end_and_Prepend_at_the_front()
        {
            Run(() =>
            {
                using var list = new GLib.List(typeof(string));

                list.Append("middle");
                list.Append("last");
                list.Prepend("first");

                Assert.Equal(new object[] { "first", "middle", "last" },
                             list.Cast<object>().ToArray());

                // Prepend(string) and Prepend(object) did not exist: Append had
                // taken both since the mono era and Prepend only ever took the
                // raw pointer, so building a list front-to-back meant marshalling
                // every element by hand.
                list.Prepend((object) "zeroth");

                Assert.Equal("zeroth", list[0]);

                // Count is cached, and used to be dropped only when the list was
                // emptied -- so reading it once (as the enumeration above does,
                // via ICollection.Count) left it answering 3 forever.
                Assert.Equal(4, list.Count);
            });
        }

        // -------------------------------------------------------- element types

        [Fact]
        public void An_element_type_of_IntPtr_hands_the_raw_pointers_straight_back()
        {
            // The escape hatch every hand-written list walk in this repository
            // uses when the elements are neither GObjects nor strings -- Pango
            // attributes, Gsk render nodes.
            //
            // The addresses are invented, which is safe *here* precisely because
            // typeof(IntPtr) means nothing dereferences them. Do not copy this
            // into a test with no element type: that path asks GLib whether the
            // pointer is a GObject, which reads through it.
            Run(() =>
            {
                using var list = new GLib.SList(typeof(IntPtr));

                // Append(IntPtr), not Append(object): the object overload copies
                // the value into fresh native memory and stores *that* address,
                // which is right for a struct and wrong for a pointer.
                list.Append(new IntPtr(0x1234));
                list.Append(new IntPtr(0x5678));

                Assert.Equal(new IntPtr(0x1234), list[0]);
                Assert.Equal(new IntPtr(0x5678), list[1]);
            });
        }

        [Fact]
        public void A_list_of_GObjects_comes_back_as_wrappers_for_the_same_objects()
        {
            Run(() =>
            {
                var first = new Gtk.Label("first");
                var second = new Gtk.Label("second");

                using var list = new GLib.List(new object[] { first, second },
                                               typeof(Gtk.Label), false, false);

                Assert.Same(first, list[0]);
                Assert.Same(second, list[1]);
                Assert.Equal("second", ((Gtk.Label) list[1]).Text);
            });
        }

        [Fact]
        public void Without_an_element_type_anything_that_is_not_a_GObject_comes_back_null()
        {
            // This is the trap, and it is worth a test of its own because it is
            // silent: DataMarshal has no element type to work from, asks GLib
            // whether the pointer is a GObject, and answers null when it is not.
            //
            // Both defects found in this sweep were exactly this --
            // Pango.AttrList.Attributes and, before it, the generated
            // AttrIterator.Attrs -- and in both the symptom was a
            // NullReferenceException a long way from the cause.
            //
            // A null pointer is the element, deliberately. Asking GLib whether an
            // *invented* address is a GObject reads through it, and the first
            // draft of this test crashed the host doing exactly that -- which is
            // worse than a failure, because an aborted run still prints "Passed!"
            // with a smaller total.
            Run(() =>
            {
                using var typed = new GLib.SList(typeof(IntPtr));
                using var untyped = new GLib.SList((Type) null);

                typed.Append(IntPtr.Zero);
                untyped.Append(IntPtr.Zero);

                Assert.Equal(IntPtr.Zero, typed[0]);
                Assert.Null(untyped[0]);
            });
        }

        [Fact]
        public void Without_an_element_type_a_GObject_is_still_recognised()
        {
            // The other half, and the reason the fallback exists at all: for a
            // list that really does hold GObjects, no element type is needed.
            Run(() =>
            {
                var label = new Gtk.Label("recognised");

                using var list = new GLib.List(new object[] { label }, null, false, false);

                Assert.Same(label, list[0]);
            });
        }

        // ------------------------------------------------------------ ownership

        [Fact]
        public void Emptying_a_list_the_binding_owns_leaves_it_with_nothing_in_it()
        {
            Run(() =>
            {
                var list = new GLib.List(Three, typeof(string), true, true);

                Assert.Equal(3, list.Count);

                list.Empty();

                Assert.Equal(0, list.Count);
                Assert.Equal(IntPtr.Zero, list.Handle);
            });
        }

        [Fact]
        public void A_list_the_binding_does_not_own_keeps_its_nodes_when_emptied()
        {
            // Empty() frees the chain only when the list is managed. A list handed
            // over by a C function that still owns it must be left alone --
            // freeing it would leave the owner holding freed nodes.
            Run(() =>
            {
                using var owned = new GLib.List(Three, typeof(string), true, false);
                IntPtr chain = owned.Handle;

                // A second wrapper over the same chain, this one not owning it.
                var borrowed = new GLib.List(chain, typeof(string), false, false);

                Assert.Equal(3, borrowed.Count);

                borrowed.Empty();

                // The chain is still there, and the owner can still read it.
                Assert.Equal(3, owned.Count);
                Assert.Equal("alpha", owned[0]);
            });
        }

        [Fact]
        public void A_cloned_list_has_the_same_contents_and_its_own_chain()
        {
            // Clone was "new List (g_list_copy (Handle))" -- the element type did
            // not come across, so every element of the clone went through the
            // is-this-a-GObject fallthrough. Cloning a list of strings therefore
            // dereferenced a char* as a GTypeInstance and took the process down.
            // Reading the clone's contents back is what pins it.
            Run(() =>
            {
                using var original = new GLib.List(Three, typeof(string), true, true);
                var clone = (GLib.List) original.Clone();

                try
                {
                    Assert.Equal(3, clone.Count);
                    Assert.Equal(original.Cast<object>().ToArray(), clone.Cast<object>().ToArray());
                    Assert.NotEqual(original.Handle, clone.Handle);
                }
                finally
                {
                    clone.Dispose();
                }
            });
        }

        // ----------------------------------------------------------- the shape

        [Fact]
        public void A_list_presents_itself_as_an_ordinary_collection()
        {
            // ICollection is what makes a returned list usable with LINQ and
            // foreach without the caller knowing it is native.
            Run(() =>
            {
                using var list = new GLib.List(Three, typeof(string), true, true);

                ICollection collection = list;

                Assert.Equal(3, collection.Count);
                Assert.False(collection.IsSynchronized);
                Assert.NotNull(collection.SyncRoot);

                Assert.Equal(new object[] { "alpha", "bravo", "charlie" },
                             collection.Cast<object>().ToArray());
            });
        }

        [Fact]
        public void An_enumerator_can_be_reset_and_walked_again()
        {
            Run(() =>
            {
                using var list = new GLib.List(Three, typeof(string), true, true);

                var enumerator = list.GetEnumerator();

                Assert.True(enumerator.MoveNext());
                Assert.Equal("alpha", enumerator.Current);
                Assert.True(enumerator.MoveNext());
                Assert.Equal("bravo", enumerator.Current);

                enumerator.Reset();

                Assert.True(enumerator.MoveNext());
                Assert.Equal("alpha", enumerator.Current);
            });
        }

        [Fact]
        public void Walking_past_the_end_stops_rather_than_running_on()
        {
            Run(() =>
            {
                using var list = new GLib.List(Three, typeof(string), true, true);

                var enumerator = list.GetEnumerator();
                for (int i = 0; i < 3; i++)
                    Assert.True(enumerator.MoveNext(), $"element {i} should be there");

                Assert.False(enumerator.MoveNext());
                Assert.False(enumerator.MoveNext());      // and stays stopped
            });
        }
    }
}
