using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// The parts of the hand-written GLib layer that nothing else reaches:
    /// <c>HookList</c>'s ABI description, the container side of
    /// <c>Variant</c>/<c>VariantType</c>, <c>Bytes</c> slicing and ownership,
    /// the <c>Marshaller</c> helpers below the ones every binding uses, and the
    /// two branches of <c>GLib.Signal</c> that only a returning signal or an
    /// emission hook takes.
    /// </summary>
    public class GLibDeepTests : GtkTestBase
    {
        public GLibDeepTests(GtkFixture fixture) : base(fixture) { }

        // --------------------------------------------------------- GLib.HookList
        //
        // HookList is 118 lines that describe the layout of GHookList and bind
        // nothing, so the only way to test it is against the struct glib itself
        // writes. These reach past the binding to g_hook_* directly; that is the
        // point, since the assertion is precisely that the two agree.

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void d_g_hook_list_init(IntPtr hook_list, uint hook_size);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void d_g_hook_list_clear(IntPtr hook_list);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_g_hook_alloc(IntPtr hook_list);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void d_g_hook_insert_before(IntPtr hook_list, IntPtr sibling, IntPtr hook);

        private static readonly IntPtr GLibModule = LoadGLib();

        private static IntPtr LoadGLib()
        {
            // The same candidate list Source/Libs/Shared/GLibrary.cs carries,
            // ordered {windows, linux, macos}. GLib is already resident by the
            // time any test runs, so this resolves the loaded module.
            foreach (var name in new[] { "glib-2.0-0.dll", "libglib-2.0.so.0", "libglib-2.0.0.dylib", "libglib-2.0-0.dll" })
                if (NativeLibrary.TryLoad(name, out var handle))
                    return handle;

            throw new InvalidOperationException("GLib could not be loaded by any of its known names.");
        }

        private static T Native<T>(string symbol) where T : Delegate =>
            Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(GLibModule, symbol));

        /// <summary>sizeof(GHook) is 64 at most; anything larger is accepted by glib.</summary>
        private const uint HookSize = 256;

        [Fact]
        public void The_generated_HookList_layout_is_the_one_glib_writes_into()
        {
            // g_hook_list_init sets seq_id to 1, hook_size to its argument,
            // is_setup to TRUE and NULLs the rest. Those values are decided by
            // glib and by the argument this test chose, so they hold whatever
            // the binding believes -- which is what makes them an oracle for
            // the field offsets abi_info computes.
            Run(() =>
            {
                var abi = GLib.HookList.abi_info;
                const int scratch = 512;

                IntPtr buffer = Marshal.AllocHGlobal(scratch);
                try
                {
                    for (int i = 0; i < scratch; i++)
                        Marshal.WriteByte(buffer, i, 0xAA);

                    Native<d_g_hook_list_init>("g_hook_list_init")(buffer, HookSize);

                    var seqId = (GLib.AbiField) abi.Fields["seq_id"];
                    ulong written = seqId.GetSize() == 8
                        ? (ulong) Marshal.ReadInt64(buffer, (int) seqId.GetOffset())
                        : (uint) Marshal.ReadInt32(buffer, (int) seqId.GetOffset());
                    Assert.Equal(1UL, written);

                    // hook_size:16 and is_setup:1 share one guint. abi_info
                    // gives them the same offset, which is the container's.
                    Assert.Equal(abi.GetFieldOffset("hook_size"), abi.GetFieldOffset("is_setup"));
                    uint bits = (uint) Marshal.ReadInt32(buffer, (int) abi.GetFieldOffset("hook_size"));
                    Assert.Equal(HookSize, bits & 0xFFFFu);
                    Assert.Equal(1u, (bits >> 16) & 1u);

                    Assert.Equal(IntPtr.Zero, Marshal.ReadIntPtr(buffer, (int) abi.GetFieldOffset("hooks")));
                    Assert.Equal(IntPtr.Zero, Marshal.ReadIntPtr(buffer, (int) abi.GetFieldOffset("dummy3")));

                    // The one field glib does not leave NULL: it installs its
                    // own default_finalize_hook. That makes it a better probe
                    // than a NULL would be, because a wrong offset reads one of
                    // the NULLs on either side of it.
                    Assert.NotEqual(IntPtr.Zero, Marshal.ReadIntPtr(buffer, (int) abi.GetFieldOffset("finalize_hook")));

                    int dummy = (int) abi.GetFieldOffset("dummy");
                    Assert.Equal(IntPtr.Zero, Marshal.ReadIntPtr(buffer, dummy));
                    Assert.Equal(IntPtr.Zero, Marshal.ReadIntPtr(buffer, dummy + IntPtr.Size));

                    // Nothing past the size abi_info reports may have been
                    // touched: a struct larger than the binding thinks is how
                    // a caller-allocated buffer gets overrun.
                    for (int i = (int) abi.Size; i < scratch; i++)
                        Assert.Equal((byte) 0xAA, Marshal.ReadByte(buffer, i));
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            });
        }

        [Fact]
        public void The_hooks_field_holds_the_hook_glib_linked_into_the_list()
        {
            // A second, independent check on the "hooks" offset that needs no
            // knowledge of GHook's own layout: the pointer g_hook_alloc handed
            // back must be readable at that offset once it is linked in, and
            // gone again after the list is cleared.
            Run(() =>
            {
                var abi = GLib.HookList.abi_info;
                IntPtr buffer = Marshal.AllocHGlobal((int) abi.Size + 64);
                try
                {
                    Native<d_g_hook_list_init>("g_hook_list_init")(buffer, HookSize);

                    IntPtr hook = Native<d_g_hook_alloc>("g_hook_alloc")(buffer);
                    Assert.NotEqual(IntPtr.Zero, hook);

                    Native<d_g_hook_insert_before>("g_hook_insert_before")(buffer, IntPtr.Zero, hook);

                    Assert.Equal(hook, Marshal.ReadIntPtr(buffer, (int) abi.GetFieldOffset("hooks")));

                    // Linking a hook consumes one sequence id, so seq_id moved
                    // from 1 to 2 -- at the offset abi_info reports for it.
                    var seqId = (GLib.AbiField) abi.Fields["seq_id"];
                    ulong after = seqId.GetSize() == 8
                        ? (ulong) Marshal.ReadInt64(buffer, (int) seqId.GetOffset())
                        : (uint) Marshal.ReadInt32(buffer, (int) seqId.GetOffset());
                    Assert.Equal(2UL, after);

                    Native<d_g_hook_list_clear>("g_hook_list_clear")(buffer);

                    Assert.Equal(IntPtr.Zero, Marshal.ReadIntPtr(buffer, (int) abi.GetFieldOffset("hooks")));
                    uint bits = (uint) Marshal.ReadInt32(buffer, (int) abi.GetFieldOffset("is_setup"));
                    Assert.Equal(0u, (bits >> 16) & 1u);
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            });
        }

        [Fact]
        public void HookList_binds_no_operation_at_all_and_cannot_be_allocated()
        {
            // Recorded rather than worked around. GLibSharp has no .metadata
            // file, so nothing generates methods for HookList and none were
            // written by hand: the type is its ABI description and nothing
            // else. Like Gsk.RoundedRect (see Docs/testing.md), it is a boxed
            // type with no allocator, so the inherited GLib.Opaque constructor
            // leaves the handle null and there is no way to get a usable one.
            // This test exists so that adding an operation forces a test too.
            Run(() =>
            {
                var declared = typeof(GLib.HookList).GetMethods(
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);

                Assert.Empty(declared);

                using var list = new GLib.HookList();
                Assert.Equal(IntPtr.Zero, list.Handle);
            });
        }

        // ---------------------------------------------------------- GLib.Variant

        [Fact]
        public void A_tuple_variant_keeps_its_children_and_their_order()
        {
            Run(() =>
            {
                var tuple = GLib.Variant.NewTuple(new[]
                {
                    new GLib.Variant(42),
                    new GLib.Variant("text"),
                    new GLib.Variant(true),
                });

                Assert.Equal("(isb)", tuple.Type.ToString());
                Assert.True(tuple.Type.IsTuple);
                Assert.True(tuple.Type.IsContainer);
                Assert.Equal(3, tuple.Type.NItems());

                var children = tuple.ToArray();
                Assert.Equal(42, (int) children[0]);
                Assert.Equal("text", (string) children[1]);
                Assert.True((bool) children[2]);

                // Print is glib's own textual form, so it pins the whole
                // structure at once rather than one child at a time.
                Assert.Equal("(42, 'text', true)", tuple.Print(false));
            });
        }

        [Fact]
        public void A_tuple_with_no_children_is_the_unit_type()
        {
            // NewTuple (null) is not an error: the empty tuple is a real
            // GVariant type, the one D-Bus uses for "no arguments".
            Run(() =>
            {
                var unit = GLib.Variant.NewTuple(null);

                Assert.Equal("()", unit.Type.ToString());
                Assert.True(unit.Type.IsTuple);
                Assert.Equal(0, unit.Type.NItems());
                Assert.Empty(unit.ToArray());
                Assert.Equal(GLib.VariantType.Unit, unit.Type);
            });
        }

        [Fact]
        public void An_array_variant_takes_its_element_type_from_its_children()
        {
            Run(() =>
            {
                var array = GLib.Variant.NewArray(new[]
                {
                    new GLib.Variant(1), new GLib.Variant(2), new GLib.Variant(3),
                });

                Assert.Equal("ai", array.Type.ToString());
                Assert.True(array.Type.IsArray);
                Assert.Equal(GLib.VariantType.Int32, array.Type.Element());

                Assert.Equal(new[] { 1, 2, 3 }, Array.ConvertAll(array.ToArray(), v => (int) v));
                Assert.Equal("[1, 2, 3]", array.Print(false));
            });
        }

        [Fact]
        public void An_empty_array_needs_a_type_because_its_children_cannot_supply_one()
        {
            Run(() =>
            {
                Assert.Throws<ArgumentNullException>(() => GLib.Variant.NewArray(null));
                Assert.Throws<ArgumentException>(() => GLib.Variant.NewArray(null, null));

                var empty = GLib.Variant.NewArray(GLib.VariantType.Int32, null);

                Assert.Equal("ai", empty.Type.ToString());
                Assert.Empty(empty.ToArray());
            });
        }

        [Fact]
        public void An_array_of_bytes_is_the_byte_string_type()
        {
            Run(() =>
            {
                var bytes = GLib.Variant.NewArray(GLib.VariantType.Byte, new[]
                {
                    new GLib.Variant((byte) 0x68), new GLib.Variant((byte) 0x69),
                });

                Assert.Equal(GLib.VariantType.ByteString, bytes.Type);
                Assert.Equal("ay", bytes.Type.ToString());
                Assert.Equal(new byte[] { 0x68, 0x69 }, Array.ConvertAll(bytes.ToArray(), v => (byte) v));
            });
        }

        [Fact]
        public void A_variant_boxed_in_a_variant_has_exactly_one_child()
        {
            Run(() =>
            {
                var boxed = GLib.Variant.NewVariant(new GLib.Variant("inner"));

                Assert.Equal("v", boxed.Type.ToString());
                Assert.True(boxed.Type.IsVariant);

                // ToArray is the only way back out through this binding: a
                // boxed variant's single child is the value it wraps.
                var unwrapped = Assert.Single(boxed.ToArray());
                Assert.Equal("inner", (string) unwrapped);
            });
        }

        [Fact]
        public void A_dictionary_variant_is_an_array_of_string_to_boxed_variant_entries()
        {
            Run(() =>
            {
                var dict = new GLib.Variant(new Dictionary<string, GLib.Variant>
                {
                    ["count"] = new GLib.Variant(5),
                });

                Assert.Equal("a{sv}", dict.Type.ToString());
                Assert.True(dict.Type.IsSubtypeOf(GLib.VariantType.Dictionary));

                var entry = Assert.Single(dict.ToArray());
                Assert.True(entry.Type.IsDictionaryEntry);
                Assert.Equal(GLib.VariantType.String, entry.Type.Key());
                Assert.Equal(GLib.VariantType.Variant, entry.Type.Value());

                // The value half is boxed, so the entry's second child is a
                // variant holding the int rather than the int. ToAsv is what
                // unboxes it, and reading the entry directly does not.
                var pair = entry.ToArray();
                Assert.Equal("count", (string) pair[0]);
                Assert.True(pair[1].Type.IsVariant);
                Assert.Equal(5, (int) dict.ToAsv()["count"]);
            });
        }

        // ------------------------------------------------------ GLib.VariantType

        [Fact]
        public void VariantTypes_built_two_different_ways_are_interchangeable_as_dictionary_keys()
        {
            // Equals goes to g_variant_type_equal and GetHashCode to
            // g_variant_type_hash, so the two cannot drift apart. What a
            // constant hash would still satisfy is the first half of this;
            // the last line is what catches it.
            Run(() =>
            {
                var spelled = new GLib.VariantType("a{sv}");
                var built = GLib.VariantType.NewArray(
                    GLib.VariantType.NewDictionaryEntry(GLib.VariantType.String, GLib.VariantType.Variant));

                Assert.Equal(spelled, built);
                Assert.Equal(spelled.GetHashCode(), built.GetHashCode());

                var table = new Dictionary<GLib.VariantType, string> { [spelled] = "asv" };
                Assert.Equal("asv", table[built]);

                Assert.NotEqual(spelled.GetHashCode(), new GLib.VariantType("as").GetHashCode());
            });
        }

        [Fact]
        public void A_maybe_type_wraps_its_element_and_the_wildcard_maybe_is_indefinite()
        {
            Run(() =>
            {
                var maybeInt = GLib.VariantType.NewMaybe(GLib.VariantType.Int32);

                Assert.Equal("mi", maybeInt.ToString());
                Assert.True(maybeInt.IsMaybe);
                Assert.True(maybeInt.IsContainer);
                Assert.False(maybeInt.IsBasic);
                Assert.True(maybeInt.IsDefinite);
                Assert.Equal(GLib.VariantType.Int32, maybeInt.Element());

                // "m*" is a pattern, not a type any value can have -- which is
                // exactly what IsDefinite distinguishes.
                Assert.False(GLib.VariantType.Maybe.IsDefinite);
                Assert.True(maybeInt.IsSubtypeOf(GLib.VariantType.Maybe));
                Assert.False(GLib.VariantType.Maybe.IsSubtypeOf(maybeInt));
            });
        }

        [Fact]
        public void Subtyping_runs_from_the_definite_type_towards_the_wildcard()
        {
            Run(() =>
            {
                Assert.True(GLib.VariantType.StringArray.IsSubtypeOf(GLib.VariantType.Array));
                Assert.True(GLib.VariantType.String.IsSubtypeOf(GLib.VariantType.Basic));
                Assert.True(GLib.VariantType.String.IsSubtypeOf(GLib.VariantType.Any));
                Assert.True(GLib.VariantType.Int32.IsSubtypeOf(GLib.VariantType.Int32));

                Assert.False(GLib.VariantType.Int32.IsSubtypeOf(GLib.VariantType.String));
                Assert.False(GLib.VariantType.Array.IsSubtypeOf(GLib.VariantType.StringArray));
                Assert.False(GLib.VariantType.Int32.IsSubtypeOf(GLib.VariantType.Array));
            });
        }

        [Fact]
        public void A_type_string_holds_exactly_one_complete_type_or_it_is_invalid()
        {
            Run(() =>
            {
                Assert.True(GLib.VariantType.StringIsValid("a{sv}"));
                Assert.True(GLib.VariantType.StringIsValid("(ii)"));
                Assert.True(GLib.VariantType.StringIsValid("*"));

                Assert.False(GLib.VariantType.StringIsValid("a"));   // no element type
                Assert.False(GLib.VariantType.StringIsValid("(ii")); // unbalanced
                Assert.False(GLib.VariantType.StringIsValid("is"));  // two types, not one
                Assert.False(GLib.VariantType.StringIsValid(""));
            });
        }

        [Fact]
        public void Walking_a_tuple_type_yields_its_items_in_order()
        {
            Run(() =>
            {
                var tuple = new GLib.VariantType("(sib)");
                Assert.Equal(3, tuple.NItems());

                // First and Next are positions inside the tuple's own type
                // string, so they cannot be copies -- a copy's "next" is its
                // own end. This walk returned ["s", "", ""] until First/Next
                // were made to borrow.
                var seen = new List<string>();
                for (var item = tuple.First(); item != null; item = item.Next())
                    seen.Add(item.ToString());

                Assert.Equal(new[] { "s", "i", "b" }, seen);

                // A borrowed position points at the rest of the tuple, so its
                // printed form has to be cut to the item's own length rather
                // than read as a C string: without that the first item prints
                // as "sib)".
                Assert.Equal("s", tuple.First().ToString());
                Assert.Equal(GLib.VariantType.String, tuple.First());
            });
        }

        // ------------------------------------------------------------ GLib.Bytes

        [Fact]
        public void Slicing_a_Bytes_gives_back_exactly_the_range_that_was_asked_for()
        {
            Run(() =>
            {
                var payload = Encoding.ASCII.GetBytes("0123456789");
                var whole = new GLib.Bytes(payload);

                var middle = new GLib.Bytes(whole, 3, 4);

                Assert.Equal(4UL, middle.Size);
                Assert.Equal(Encoding.ASCII.GetBytes("3456"), middle.Data);

                // The slice references the parent rather than copying it, so
                // the parent must be unchanged and still readable.
                Assert.Equal(payload, whole.Data);
            });
        }

        [Fact]
        public void An_empty_Bytes_has_size_zero_but_hands_back_a_null_array()
        {
            // Pinned as what it is. g_bytes_get_data returns NULL for an empty
            // GBytes, and Data turns that into null rather than an empty
            // array -- so foreach over the Data of a perfectly valid Bytes can
            // throw NullReferenceException.
            Run(() =>
            {
                var empty = new GLib.Bytes(new byte[0]);

                Assert.Equal(0UL, empty.Size);
                Assert.Null(empty.Data);
            });
        }

        [Fact]
        public void Bytes_compare_and_hash_by_content()
        {
            Run(() =>
            {
                var ab = new GLib.Bytes(Encoding.ASCII.GetBytes("ab"));
                var abc = new GLib.Bytes(Encoding.ASCII.GetBytes("abc"));
                var abcAgain = new GLib.Bytes(Encoding.ASCII.GetBytes("abc"));

                // memcmp then length: a prefix sorts before what extends it.
                Assert.True(ab.CompareTo(abc) < 0);
                Assert.True(abc.CompareTo(ab) > 0);
                Assert.Equal(0, abc.CompareTo(abcAgain));

                Assert.True(abc.Equals(abcAgain));
                Assert.Equal(abc.GetHash(), abcAgain.GetHash());
                Assert.NotEqual(ab.GetHash(), abc.GetHash());
            });
        }

        [Fact]
        public void NewTake_and_NewStatic_hand_glib_memory_it_is_allowed_to_keep()
        {
            // Both used to pass the marshaller's *pinned managed array*
            // straight to g_bytes_new_take / g_bytes_new_static. A blittable
            // byte[] is pinned for the duration of the call and not copied, so
            // glib was left either g_freeing an interior pointer into the GC
            // heap or holding one across a collection. Disposing is what makes
            // the first of those show up, so this test disposes.
            Run(() =>
            {
                var payload = Encoding.ASCII.GetBytes("taken");

                var taken = GLib.Bytes.NewTake((byte[]) payload.Clone());
                Assert.Equal(payload, taken.Data);
                taken.Dispose();
                Assert.Equal(IntPtr.Zero, taken.Handle);

                var stat = GLib.Bytes.NewStatic((byte[]) payload.Clone());
                Assert.Equal(payload, stat.Data);
                stat.Dispose();
                Assert.Equal(IntPtr.Zero, stat.Handle);
            });
        }

        // ------------------------------------------------------- GLib.Marshaller

        [Fact]
        public void The_pointer_array_string_overloads_drop_the_last_element_unread()
        {
            Run(() =>
            {
                IntPtr[] ptrs = GLib.Marshaller.StringArrayToNullTermPointer(new[] { "one", "two" });

                Assert.Equal(3, ptrs.Length);
                Assert.Equal(IntPtr.Zero, ptrs[2]);

                // Neither overload searches for the terminator; both simply
                // return Length - 1 strings. Hand them an array that is not
                // null-terminated and the last string silently disappears.
                Assert.Equal(new[] { "one", "two" }, GLib.Marshaller.Utf8PtrToString(ptrs));

                // Same read, freeing as it goes -- so it must come second.
                Assert.Equal(new[] { "one", "two" }, GLib.Marshaller.PtrToStringGFree(ptrs));
            });
        }

        [Fact]
        public void A_filename_survives_the_conversion_glib_puts_it_through()
        {
            // Deliberately ASCII: g_filename_from_utf8 goes through the
            // filename charset, which is the C locale's in a bare CI container,
            // and a non-ASCII name would then fail for reasons that are not
            // this binding's.
            Run(() =>
            {
                const string path = "some/directory/file.txt";

                IntPtr native = GLib.Marshaller.StringToFilenamePtr(path);
                Assert.NotEqual(IntPtr.Zero, native);

                Assert.Equal(path, GLib.Marshaller.FilenamePtrToStringGFree(native));
            });
        }

        [Fact]
        public void StringFormat_escapes_every_percent_including_the_ones_the_caller_meant()
        {
            // The result is destined for a printf-style consumer, so a literal
            // % must be doubled -- and there is no way to ask for one that is
            // not, which is the trap: a format string that already contains %%
            // comes back with four.
            Run(() =>
            {
                Assert.Equal("plain text", GLib.Marshaller.StringFormat("{0} text", "plain"));
                Assert.Equal("100%% sure", GLib.Marshaller.StringFormat("100% sure"));
                Assert.Equal("%%%%", GLib.Marshaller.StringFormat("%%"));
            });
        }

        [Fact]
        public void A_native_list_and_a_native_pointer_array_both_copy_out_as_managed_arrays()
        {
            Run(() =>
            {
                using var list = new GLib.List(typeof(string));
                list.Append("alpha");
                list.Append("beta");

                // owned:false on both counts -- the wrapper above still owns
                // the list and its elements, and freeing them here would leave
                // the using block disposing freed memory.
                var fromList = GLib.Marshaller.ListPtrToArray(list.Handle, typeof(GLib.List), false, false, typeof(string));
                Assert.Equal(new[] { "alpha", "beta" }, Assert.IsType<string[]>(fromList));

                using var array = new GLib.PtrArray(typeof(string), true, true);
                array.Add(GLib.Marshaller.StringToPtrGStrdup("x"));
                array.Add(GLib.Marshaller.StringToPtrGStrdup("y"));

                Assert.Equal(new[] { "x", "y" }, GLib.Marshaller.PtrArrayToArray<string>(array.Handle, false, false));
            });
        }

        [Fact]
        public void The_untyped_array_marshaller_handles_bytes_and_refuses_everything_else()
        {
            Run(() =>
            {
                var bytes = new byte[] { 9, 8, 7, 0, 255 };

                IntPtr native = GLib.Marshaller.ArrayToArrayPtr(bytes);
                var read = GLib.Marshaller.ArrayPtrToArray(native, typeof(byte), bytes.Length, true);

                Assert.Equal(bytes, Assert.IsType<byte[]>(read));

                // Any other element type is rejected rather than marshalled at
                // the wrong width, which is the only safe answer here.
                Assert.Throws<InvalidOperationException>(
                    () => GLib.Marshaller.ArrayPtrToArray(IntPtr.Zero, typeof(int), 1, false));
                Assert.Throws<InvalidOperationException>(
                    () => GLib.Marshaller.ArrayPtrToArray<int>(IntPtr.Zero, 1, false));
            });
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Pair
        {
            public int First;
            public double Second;
        }

        [Fact]
        public void A_struct_array_survives_the_trip_out_to_a_null_terminated_pointer_array_and_back()
        {
            // The two halves are inverses, and neither had ever been called.
            // The writer returned its cursor rather than the base of the block
            // it allocated -- so what came back was a pointer to the NULL
            // terminator, an empty array, with the real block unreachable. The
            // reader walked the base pointer by sizeof(T) and stopped when the
            // *pointer* went null, which it never does.
            Run(() =>
            {
                var input = new[]
                {
                    new Pair { First = 1, Second = 0.5 },
                    new Pair { First = -7, Second = 1e10 },
                };

                IntPtr native = GLib.Marshaller.StructArrayToNullTerminatedStructArrayIntPtr(input);
                try
                {
                    Assert.NotEqual(IntPtr.Zero, Marshal.ReadIntPtr(native, 0));
                    Assert.Equal(IntPtr.Zero, Marshal.ReadIntPtr(native, 2 * IntPtr.Size));

                    var read = GLib.Marshaller.StructArrayFromNullTerminatedIntPtr<Pair>(native);

                    Assert.Equal(2, read.Length);
                    Assert.Equal(1, read[0].First);
                    Assert.Equal(0.5, read[0].Second);
                    Assert.Equal(-7, read[1].First);
                    Assert.Equal(1e10, read[1].Second);
                }
                finally
                {
                    for (int i = 0; i < input.Length; i++)
                        Marshal.FreeHGlobal(Marshal.ReadIntPtr(native, i * IntPtr.Size));
                    Marshal.FreeHGlobal(native);
                }

                Assert.Empty(GLib.Marshaller.StructArrayFromNullTerminatedIntPtr<Pair>(IntPtr.Zero));
            });
        }

        // ----------------------------------------------------------- GLib.Signal

        [Fact]
        public void Emitting_a_signal_that_returns_a_value_hands_the_handlers_answer_back()
        {
            // Signal.Emit has two branches: one that queries the signal, emits
            // into a GLib.Value and unboxes the result, and one that discards
            // it. Only a signal with a non-void return reaches the first, and
            // nothing else in the suite emits one. close-request is RUN_LAST
            // with the boolean-handled accumulator and a default handler that
            // returns FALSE, so the answer is whatever a handler decided.
            Run(() =>
            {
                var window = new Gtk.Window();

                Assert.False((bool) GLib.Signal.Emit(window, "close-request"));

                void Veto(object o, Gtk.CloseRequestArgs args) => args.RetVal = true;

                window.CloseRequest += Veto;
                Assert.True((bool) GLib.Signal.Emit(window, "close-request"));

                window.CloseRequest -= Veto;
                Assert.False((bool) GLib.Signal.Emit(window, "close-request"));
            });
        }

        [Fact]
        public void Emitting_a_signal_no_one_declared_names_it_rather_than_emitting_nothing()
        {
            Run(() =>
            {
                var label = new Gtk.Label("x");

                var bad = Assert.Throws<ArgumentException>(() => GLib.Signal.Emit(label, "no-such-signal"));
                Assert.Contains("no-such-signal", bad.Message);

                // A detail with no signal in front of it is malformed, and is
                // rejected while parsing rather than looked up as "".
                Assert.Throws<FormatException>(() => GLib.Signal.Emit(label, "::detail"));
            });
        }

        [Fact]
        public void An_emission_hook_sees_the_instance_and_unhooks_itself_when_it_returns_false()
        {
            // AddEmissionHook and the whole EmissionHookMarshaler had nothing
            // calling them. The hook is per-GType rather than per-instance, so
            // returning false matters twice over: it is the assertion, and it
            // is what keeps the hook from firing on every later Button in the
            // run.
            Run(() =>
            {
                var button = new Gtk.Button();
                object seen = null;
                uint signalId = 0;
                int calls = 0;

                GLib.Signal.AddEmissionHook("clicked", Gtk.Button.GType, (hint, values) =>
                {
                    calls++;
                    signalId = hint.signal_id;
                    seen = values[0];
                    return false;
                });

                GLib.Signal.Emit(button, "clicked");

                Assert.Equal(1, calls);
                Assert.Same(button, seen);
                Assert.NotEqual(0u, signalId);

                GLib.Signal.Emit(button, "clicked");
                Assert.Equal(1, calls);
            });
        }
    }
}
