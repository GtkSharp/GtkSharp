using System;
using System.Collections;
using System.Linq;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// <c>GLib.PtrArray</c> and <c>GLib.Argv</c> — the other two hand-written
    /// containers the binding marshals through.
    /// </summary>
    /// <remarks>
    /// <c>PtrArray</c> is <c>ListBase</c>'s sibling: same <c>DataMarshal</c>, same
    /// <c>ICollection</c> surface, same enumerator shape, written separately. It
    /// was recorded as unexamined at the end of the <c>ListBase</c> pass, so this
    /// is that examination — and it turned out to share two of the five defects
    /// found there, plus one of its own.
    ///
    /// Element pointers here are either fabricated *and* typed <c>IntPtr</c>, so
    /// nothing dereferences them, or real strings allocated for the test. Handing
    /// GLib an invented address in a context that reads through it crashes the
    /// host rather than failing; see Docs/testing.md.
    /// </remarks>
    public class GLibContainerTests : GtkTestBase
    {
        public GLibContainerTests(GtkFixture fixture) : base(fixture) { }

        /// <summary>A pointer array of fabricated addresses. Typed as IntPtr, so
        /// they are only ever compared, never followed.</summary>
        static GLib.PtrArray Pointers(params int[] values)
        {
            var array = new GLib.PtrArray(typeof(IntPtr), true, false);
            foreach (int value in values)
                array.Add(new IntPtr(value));
            return array;
        }

        // ------------------------------------------------------------ PtrArray

        [Fact]
        public void A_pointer_array_holds_what_was_added_in_order()
        {
            Run(() =>
            {
                using var array = Pointers(0x10, 0x20, 0x30);

                Assert.Equal(3, array.Count);
                Assert.Equal(new IntPtr(0x10), array[0]);
                Assert.Equal(new IntPtr(0x30), array[2]);
            });
        }

        [Fact]
        public void Index_enumerator_and_CopyTo_agree_about_a_pointer_array()
        {
            Run(() =>
            {
                using var array = Pointers(0x10, 0x20, 0x30);

                var byIndex = new object[array.Count];
                for (int i = 0; i < array.Count; i++)
                    byIndex[i] = array[i];

                var byEnumerator = array.Cast<object>().ToArray();

                var byCopyTo = new object[array.Count];
                array.CopyTo(byCopyTo, 0);

                Assert.Equal(byIndex, byEnumerator);
                Assert.Equal(byIndex, byCopyTo);
            });
        }

        [Fact]
        public void Removing_a_pointer_shortens_the_array()
        {
            Run(() =>
            {
                using var array = Pointers(0x10, 0x20, 0x30);

                array.Remove(new IntPtr(0x20));

                Assert.Equal(2, array.Count);
                Assert.Equal(new object[] { new IntPtr(0x10), new IntPtr(0x30) },
                             array.Cast<object>().ToArray());
            });
        }

        [Fact]
        public void A_pointer_arrays_count_follows_what_is_in_it()
        {
            // PtrArray reads len out of the native struct on every call rather
            // than caching it, which is what ListBase got wrong. Pinned so a
            // future "optimisation" has to come past this test.
            Run(() =>
            {
                using var array = Pointers(0x10);

                Assert.Equal(1, array.Count);

                array.Add(new IntPtr(0x20));
                Assert.Equal(2, array.Count);

                array.Remove(new IntPtr(0x10));
                Assert.Equal(1, array.Count);
            });
        }

        [Fact]
        public void A_pointer_array_of_strings_marshals_them_back()
        {
            Run(() =>
            {
                var array = new GLib.PtrArray(typeof(string), true, true);

                array.Add(GLib.Marshaller.StringToPtrGStrdup("alpha"));
                array.Add(GLib.Marshaller.StringToPtrGStrdup("bravo"));

                try
                {
                    Assert.Equal(2, array.Count);
                    Assert.Equal("alpha", array[0]);
                    Assert.Equal("bravo", array[1]);
                }
                finally
                {
                    array.Dispose();     // frees the strings, because elements_owned
                }
            });
        }

        [Fact]
        public void A_pointer_arrays_enumerator_stops_at_the_end_and_stays_stopped()
        {
            // Same defect ListBase had: running off the end reset the cursor, so
            // the next MoveNext started over and answered true again. A loop that
            // kept asking never terminated.
            Run(() =>
            {
                using var array = Pointers(0x10, 0x20);

                var enumerator = array.GetEnumerator();

                Assert.True(enumerator.MoveNext());
                Assert.True(enumerator.MoveNext());

                Assert.False(enumerator.MoveNext());
                Assert.False(enumerator.MoveNext());
            });
        }

        [Fact]
        public void A_pointer_arrays_enumerator_can_be_reset()
        {
            Run(() =>
            {
                using var array = Pointers(0x10, 0x20);

                var enumerator = array.GetEnumerator();

                Assert.True(enumerator.MoveNext());
                Assert.Equal(new IntPtr(0x10), enumerator.Current);

                enumerator.Reset();

                Assert.True(enumerator.MoveNext());
                Assert.Equal(new IntPtr(0x10), enumerator.Current);
            });
        }

        [Fact]
        public void A_pointer_array_offers_a_lock_object_like_any_other_collection()
        {
            // ICollection.SyncRoot is documented as something a caller can lock,
            // and null is not. ListBase had the same hole.
            Run(() =>
            {
                using var array = Pointers(0x10);

                ICollection collection = array;

                Assert.NotNull(collection.SyncRoot);
                Assert.False(collection.IsSynchronized);

                lock (collection.SyncRoot)
                    Assert.Equal(1, collection.Count);
            });
        }

        [Fact]
        public void A_cloned_pointer_array_has_the_same_contents_and_its_own_storage()
        {
            Run(() =>
            {
                using var original = Pointers(0x10, 0x20);
                var clone = (GLib.PtrArray) original.Clone();

                try
                {
                    Assert.Equal(2, clone.Count);
                    Assert.Equal(original.Cast<object>().ToArray(), clone.Cast<object>().ToArray());
                    Assert.NotEqual(original.Handle, clone.Handle);

                    // Adding to the clone must not touch the original.
                    clone.Add(new IntPtr(0x30));

                    Assert.Equal(3, clone.Count);
                    Assert.Equal(2, original.Count);
                }
                finally
                {
                    clone.Dispose();
                }
            });
        }

        // ---------------------------------------------------------------- Argv

        [Fact]
        public void Argv_hands_back_the_arguments_it_was_given()
        {
            // The bridge every "string[] args" call crosses: a managed array into
            // a native char** and back. The round trip is the whole contract.
            Run(() =>
            {
                var argv = new GLib.Argv(new[] { "first", "second", "third" });

                Assert.NotEqual(IntPtr.Zero, argv.Handle);
                Assert.Equal(new[] { "first", "second", "third" }, argv.GetArgs(3));
            });
        }

        [Fact]
        public void Argv_can_put_the_program_name_in_front_and_takes_it_off_again()
        {
            // add_program_name prepends argv[0] on the way in, and GetArgs skips
            // it on the way out -- so what the caller passed is what the caller
            // gets, with the extra element visible only to the C function in
            // between. Off-by-one here would silently drop the first argument.
            Run(() =>
            {
                var plain = new GLib.Argv(new[] { "one", "two" }, false);
                var named = new GLib.Argv(new[] { "one", "two" }, true);

                Assert.Equal(new[] { "one", "two" }, plain.GetArgs(2));

                // Three pointers went in; two arguments come back out.
                Assert.Equal(new[] { "one", "two" }, named.GetArgs(3));

                // ...and the one in front is the program, which the test can name
                // independently of the binding.
                var programName = Environment.GetCommandLineArgs()[0];
                var raw = System.Runtime.InteropServices.Marshal.ReadIntPtr(named.Handle, 0);
                Assert.Equal(programName, GLib.Marshaller.Utf8PtrToString(raw));
            });
        }

        [Fact]
        public void Argv_asked_for_fewer_arguments_than_it_holds_answers_with_that_many()
        {
            // A C function may consume some of argv and report a smaller argc.
            Run(() =>
            {
                var argv = new GLib.Argv(new[] { "one", "two", "three" });

                Assert.Equal(new[] { "one" }, argv.GetArgs(1));
                Assert.Equal(new[] { "one", "two" }, argv.GetArgs(2));
            });
        }

        [Fact]
        public void An_empty_argv_is_allowed()
        {
            Run(() =>
            {
                var argv = new GLib.Argv(new string[0]);

                Assert.Empty(argv.GetArgs(0));
            });
        }

        [Fact]
        public void Argv_keeps_non_ascii_arguments_intact()
        {
            // The pointers are g_strdup'd UTF-8, so anything that treated them as
            // ANSI would come back mangled rather than failing.
            Run(() =>
            {
                var argv = new GLib.Argv(new[] { "café", "日本語" });

                Assert.Equal(new[] { "café", "日本語" }, argv.GetArgs(2));
            });
        }
    }
}
