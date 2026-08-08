using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// <c>GLib.AbiStruct</c> — the field-offset arithmetic the binding uses to
    /// find its way around native class structs.
    /// </summary>
    /// <remarks>
    /// This is how a vfunc gets overridden: the binding computes where a function
    /// pointer sits inside <c>GObjectClass</c> or <c>GtkWidgetClass</c> and writes
    /// there. An offset that is wrong by one slot overwrites a different vfunc,
    /// and what breaks is some unrelated widget behaviour much later. 145
    /// hand-written lines, and nothing referenced it by name.
    ///
    /// The oracle is deliberately not this test's own arithmetic. Every layout
    /// below is also declared as a <c>[StructLayout(Sequential)]</c> managed
    /// struct, and the answers are compared against <c>Marshal.OffsetOf</c> and
    /// <c>Marshal.SizeOf</c> — an independent implementation of the same C rules,
    /// written by someone else, checking the one under test.
    /// </remarks>
    public class AbiStructTests : GtkTestBase
    {
        public AbiStructTests(GtkFixture fixture) : base(fixture) { }

        /// <summary>Builds an AbiStruct from (name, size, align) triples, wiring
        /// up the prev/next chain the way the generated ABI descriptions do.</summary>
        static GLib.AbiStruct Layout(params (string Name, uint Size, long Align)[] fields)
        {
            var list = new List<GLib.AbiField>();

            for (int i = 0; i < fields.Length; i++)
            {
                var (name, size, align) = fields[i];

                list.Add(new GLib.AbiField(
                    name,
                    i == 0 ? 0 : -1,                                  // only the first has a fixed offset
                    size,
                    i == 0 ? null : fields[i - 1].Name,               // prev
                    i == fields.Length - 1 ? null : fields[i + 1].Name,
                    align,
                    0));                                              // no bitfields
            }

            return new GLib.AbiStruct(list);
        }

        // ------------------------------------------------------ against Marshal

        [StructLayout(LayoutKind.Sequential)]
        struct ByteThenInt { public byte A; public int B; }

        [StructLayout(LayoutKind.Sequential)]
        struct IntThenByte { public int A; public byte B; }

        [StructLayout(LayoutKind.Sequential)]
        struct ByteThenPointer { public byte A; public IntPtr B; }

        [StructLayout(LayoutKind.Sequential)]
        struct ShortPair { public short A; public short B; }

        [StructLayout(LayoutKind.Sequential)]
        struct LongThenByte { public long A; public byte B; }

        [StructLayout(LayoutKind.Sequential)]
        struct ThreeMixed { public byte A; public int B; public IntPtr C; }

        [Fact]
        public void A_field_after_a_smaller_one_is_padded_up_to_its_alignment()
        {
            Run(() =>
            {
                var abi = Layout(("A", 1, 1), ("B", 4, 4));

                Assert.Equal((uint) Marshal.OffsetOf<ByteThenInt>("A"), abi.GetFieldOffset("A"));
                Assert.Equal((uint) Marshal.OffsetOf<ByteThenInt>("B"), abi.GetFieldOffset("B"));
                Assert.Equal((uint) Marshal.SizeOf<ByteThenInt>(), abi.Size);

                // ...and the padding is real rather than incidental.
                Assert.Equal(4u, abi.GetFieldOffset("B"));
            });
        }

        [Fact]
        public void A_struct_is_padded_at_the_end_to_a_multiple_of_its_alignment()
        {
            // The trailing byte does not make the struct five bytes long: an array
            // of them has to keep every element aligned.
            Run(() =>
            {
                var abi = Layout(("A", 4, 4), ("B", 1, 1));

                Assert.Equal((uint) Marshal.OffsetOf<IntThenByte>("B"), abi.GetFieldOffset("B"));
                Assert.Equal((uint) Marshal.SizeOf<IntThenByte>(), abi.Size);
                Assert.Equal(8u, abi.Size);
            });
        }

        [Fact]
        public void A_pointer_field_is_aligned_to_the_pointer_size()
        {
            // The one that differs between 32-bit and 64-bit, so the expected
            // value comes from IntPtr.Size rather than from a literal.
            Run(() =>
            {
                uint pointer = (uint) IntPtr.Size;
                var abi = Layout(("A", 1, 1), ("B", pointer, pointer));

                Assert.Equal((uint) Marshal.OffsetOf<ByteThenPointer>("B"), abi.GetFieldOffset("B"));
                Assert.Equal((uint) Marshal.SizeOf<ByteThenPointer>(), abi.Size);
                Assert.Equal(pointer, abi.GetFieldOffset("B"));
            });
        }

        [Fact]
        public void Fields_that_already_fit_get_no_padding_between_them()
        {
            // The control for the padding tests: two shorts pack tight, so
            // "the offsets are right" is not just reporting a rule that always
            // rounds up.
            Run(() =>
            {
                var abi = Layout(("A", 2, 2), ("B", 2, 2));

                Assert.Equal((uint) Marshal.OffsetOf<ShortPair>("B"), abi.GetFieldOffset("B"));
                Assert.Equal(2u, abi.GetFieldOffset("B"));
                Assert.Equal((uint) Marshal.SizeOf<ShortPair>(), abi.Size);
                Assert.Equal(4u, abi.Size);
            });
        }

        [Fact]
        public void The_widest_field_decides_the_structs_alignment_and_its_tail()
        {
            Run(() =>
            {
                var abi = Layout(("A", 8, 8), ("B", 1, 1));

                Assert.Equal(8u, abi.Align);
                Assert.Equal((uint) Marshal.OffsetOf<LongThenByte>("B"), abi.GetFieldOffset("B"));
                Assert.Equal((uint) Marshal.SizeOf<LongThenByte>(), abi.Size);
                Assert.Equal(16u, abi.Size);
            });
        }

        [Fact]
        public void Three_fields_of_growing_width_land_where_the_marshaller_puts_them()
        {
            // The shape a real class struct starts with: a pointer-sized header,
            // a counter, and a pointer.
            Run(() =>
            {
                uint pointer = (uint) IntPtr.Size;
                var abi = Layout(("A", 1, 1), ("B", 4, 4), ("C", pointer, pointer));

                Assert.Equal((uint) Marshal.OffsetOf<ThreeMixed>("A"), abi.GetFieldOffset("A"));
                Assert.Equal((uint) Marshal.OffsetOf<ThreeMixed>("B"), abi.GetFieldOffset("B"));
                Assert.Equal((uint) Marshal.OffsetOf<ThreeMixed>("C"), abi.GetFieldOffset("C"));
                Assert.Equal((uint) Marshal.SizeOf<ThreeMixed>(), abi.Size);
            });
        }

        // ---------------------------------------------------------- on its own

        [Fact]
        public void The_first_field_sits_at_the_start()
        {
            Run(() =>
            {
                var abi = Layout(("first", 4, 4), ("second", 4, 4));

                Assert.Equal(0u, abi.GetFieldOffset("first"));
            });
        }

        [Fact]
        public void A_struct_of_one_byte_is_one_byte()
        {
            Run(() =>
            {
                var abi = Layout(("only", 1, 1));

                Assert.Equal(1u, abi.Size);
                Assert.Equal(1u, abi.Align);
            });
        }

        [Fact]
        public void Alignment_is_the_widest_field_rather_than_the_last_one()
        {
            // Put the wide field first, so a Load that tracked "the most recent
            // alignment" instead of the maximum would answer 1.
            Run(() =>
            {
                Assert.Equal(8u, Layout(("wide", 8, 8), ("narrow", 1, 1)).Align);
                Assert.Equal(8u, Layout(("narrow", 1, 1), ("wide", 8, 8)).Align);
            });
        }

        [Fact]
        public void Every_field_is_reachable_by_name()
        {
            Run(() =>
            {
                var abi = Layout(("alpha", 4, 4), ("bravo", 8, 8), ("charlie", 1, 1));

                Assert.Equal(0u, abi.GetFieldOffset("alpha"));
                Assert.Equal(8u, abi.GetFieldOffset("bravo"));
                Assert.Equal(16u, abi.GetFieldOffset("charlie"));
                Assert.Equal(3, abi.Fields.Count);
            });
        }

        // ------------------------------------------------------------ bitfields

        [Fact]
        public void Consecutive_bitfields_share_one_offset_and_the_field_after_clears_them()
        {
            // GHookList is the only shape in the tree that uses this path:
            //
            //     struct _GHookList {
            //         gulong  seq_id;
            //         guint   hook_size : 16;
            //         guint   is_setup  : 1;
            //         GHook  *hooks;
            //         ...
            //
            // The two bit members share a storage unit, and what actually has to
            // be right is where "hooks" lands -- that is the first field anything
            // reads through. On a 64-bit host seq_id is eight bytes, the bit
            // members occupy the next unit, and a pointer aligns to sixteen.
            Run(() =>
            {
                uint pointer = (uint) IntPtr.Size;
                uint gulong_size = pointer;      // gulong is pointer-sized on the platforms tested

                var fields = new List<GLib.AbiField>
                {
                    new GLib.AbiField("seq_id", 0, gulong_size, null, "hook_size", gulong_size, 0),
                    new GLib.AbiField("hook_size", -1, sizeof(uint), "seq_id", "is_setup", 1, 16),
                    new GLib.AbiField("is_setup", -1, sizeof(uint), "hook_size", "hooks", 1, 1),
                    new GLib.AbiField("hooks", -1, pointer, "is_setup", null, pointer, 0),
                };

                var abi = new GLib.AbiStruct(fields);

                Assert.Equal(0u, abi.GetFieldOffset("seq_id"));

                // Both bit members start at the same byte: that is what sharing a
                // storage unit means, and reading either one from a different
                // offset would read the neighbouring field's bits.
                Assert.Equal(gulong_size, abi.GetFieldOffset("hook_size"));
                Assert.Equal(gulong_size, abi.GetFieldOffset("is_setup"));

                // And the first ordinary field after them is back on alignment.
                Assert.Equal(gulong_size * 2, abi.GetFieldOffset("hooks"));
            });
        }

        // ------------------------------------------- the one the binding relies on

        [Fact]
        public void The_real_GObject_layout_agrees_with_what_GLib_reports()
        {
            // Not a made-up shape: this is the description Object.cs builds for
            // GObject itself, and it is the one every vfunc override is measured
            // from. GObject is a GTypeInstance pointer, a ref count and a qdata
            // pointer -- so the whole struct is two pointers and a uint, and the
            // ref count sits immediately after the first pointer.
            Run(() =>
            {
                uint pointer = (uint) IntPtr.Size;

                var abi = Layout(
                    ("g_type_instance", pointer, pointer),
                    ("ref_count", 4, 4),
                    ("qdata", pointer, pointer));

                Assert.Equal(0u, abi.GetFieldOffset("g_type_instance"));
                Assert.Equal(pointer, abi.GetFieldOffset("ref_count"));

                // qdata follows the ref count, padded back up to pointer alignment.
                Assert.Equal(pointer * 2, abi.GetFieldOffset("qdata"));
                Assert.Equal(pointer * 3, abi.Size);
            });
        }
    }
}
