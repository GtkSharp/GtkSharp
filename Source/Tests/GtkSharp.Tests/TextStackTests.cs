using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// The Gtk text stack: TextBuffer, TextIter, TextMark, TextTag,
    /// TextTagTable, TextChildAnchor and EntryBuffer.
    /// </summary>
    /// <remarks>
    /// Wherever one exists the oracle is outside Gtk: how many Unicode scalars
    /// a string has, how many bytes UTF-8 needs for them, which characters end
    /// a sentence, that a combining mark is not a cursor position. Those hold
    /// whatever the binding does, so the assertion can only be satisfied by the
    /// call working.
    /// </remarks>
    public class TextStackTests : GtkTestBase
    {
        public TextStackTests(GtkFixture fixture) : base(fixture) { }

        /// <summary>A buffer holding exactly <paramref name="text"/>.</summary>
        private static TextBuffer BufferOf(string text)
        {
            var buffer = new TextBuffer(null);
            var end = buffer.EndIter;
            buffer.Insert(ref end, text);
            return buffer;
        }

        // -------------------------------------------------- text and offsets

        [Fact]
        public void Insert_places_text_at_the_iterator_and_leaves_it_past_the_insertion()
        {
            Run(() =>
            {
                var buffer = BufferOf("hello world");

                var at = buffer.GetIterAtOffset(5);
                buffer.Insert(ref at, " brave");

                Assert.Equal("hello brave world", buffer.Text);

                // gtk_text_buffer_insert leaves the iterator pointing at the end
                // of what it just inserted, which is what makes a sequence of
                // Inserts append rather than repeatedly overwrite.
                Assert.Equal(11, at.Offset);
                Assert.Equal(" brave", buffer.GetText(buffer.GetIterAtOffset(5), at, false));
            });
        }

        [Fact]
        public void An_offset_counts_unicode_scalars_where_a_dotnet_string_counts_utf16_units()
        {
            Run(() =>
            {
                // U+1F600 is outside the BMP, so C# stores it as two chars.
                // Gtk counts characters, and an offset that treated the two as
                // separate positions would land inside the surrogate pair.
                var buffer = BufferOf("a\U0001F600b");

                Assert.Equal(4, buffer.Text.Length);
                Assert.Equal(3, buffer.CharCount);

                Assert.Equal("a", buffer.GetIterAtOffset(0).Char);
                Assert.Equal("\U0001F600", buffer.GetIterAtOffset(1).Char);
                Assert.Equal("b", buffer.GetIterAtOffset(2).Char);
            });
        }

        [Fact]
        public void A_child_anchor_occupies_one_character_that_GetText_drops_and_GetSlice_keeps()
        {
            Run(() =>
            {
                var buffer = BufferOf("ab");
                var at = buffer.GetIterAtOffset(1);
                var anchor = buffer.CreateChildAnchor(ref at);

                // The anchor is a real character in the buffer - this is why an
                // offset computed from GetText cannot be used against the buffer.
                Assert.Equal(3, buffer.CharCount);

                var start = buffer.StartIter;
                var end = buffer.EndIter;
                Assert.Equal("ab", buffer.GetText(start, end, false));
                Assert.Equal("a\uFFFCb", buffer.GetSlice(start, end, false));

                Assert.Same(anchor, buffer.GetIterAtOffset(1).ChildAnchor);
                Assert.Null(buffer.GetIterAtOffset(0).ChildAnchor);
            });
        }

        [Fact]
        public void GetText_omits_invisible_text_unless_hidden_characters_are_asked_for()
        {
            Run(() =>
            {
                var buffer = BufferOf("visible[hidden]tail");
                var tag = new TextTag("hide") { Invisible = true };
                buffer.TagTable.Add(tag);
                buffer.ApplyTag(tag, buffer.GetIterAtOffset(7), buffer.GetIterAtOffset(15));

                var start = buffer.StartIter;
                var end = buffer.EndIter;

                Assert.Equal("visibletail", buffer.GetText(start, end, false));
                Assert.Equal("visible[hidden]tail", buffer.GetText(start, end, true));

                // The characters are still there; only the reading of them changed.
                Assert.Equal(19, buffer.CharCount);
            });
        }

        // ---------------------------------------------------------- iteration

        [Fact]
        public void Word_iteration_brackets_each_word_from_both_directions()
        {
            Run(() =>
            {
                var buffer = BufferOf("the quick brown fox");

                var forward = new List<int>();
                var iter = buffer.StartIter;
                while (forward.Count < 4 && iter.ForwardWordEnd())
                    forward.Add(iter.Offset);
                // The last word end is the end of the buffer, which
                // forward_word_end reports as "did not move to a word end".
                if (forward.Count == 3)
                    forward.Add(buffer.CharCount);

                Assert.Equal(new[] { 3, 9, 15, 19 }, forward);

                var backward = new List<int>();
                iter = buffer.EndIter;
                while (iter.BackwardWordStart())
                    backward.Add(iter.Offset);

                Assert.Equal(new[] { 16, 10, 4, 0 }, backward);

                Assert.True(buffer.GetIterAtOffset(0).StartsWord());
                Assert.True(buffer.GetIterAtOffset(3).EndsWord());
                Assert.True(buffer.GetIterAtOffset(1).InsideWord());
                Assert.False(buffer.GetIterAtOffset(3).InsideWord());
            });
        }

        [Fact]
        public void Sentence_iteration_cuts_the_text_at_its_terminating_punctuation()
        {
            Run(() =>
            {
                var buffer = BufferOf("One. Two? Three!");

                var ends = new List<int>();
                var iter = buffer.StartIter;
                while (ends.Count < 3 && iter.ForwardSentenceEnd())
                    ends.Add(iter.Offset);

                // The last sentence ends where the buffer does, and
                // forward_sentence_end reports that as "did not move" rather
                // than as a stop - the same terminal case as forward_word_end.
                // A loop that trusts the return value drops the last sentence.
                Assert.Equal(2, ends.Count);
                ends.Add(buffer.CharCount);

                // Which side of the space between two sentences the boundary
                // falls on is Gtk's business; that the cuts separate the three
                // sentences is not.
                var sentences = new List<string>();
                var from = 0;
                foreach (var to in ends)
                {
                    sentences.Add(buffer.GetText(buffer.GetIterAtOffset(from), buffer.GetIterAtOffset(to), false).Trim());
                    from = to;
                }

                Assert.Equal(new[] { "One.", "Two?", "Three!" }, sentences);

                Assert.True(buffer.GetIterAtOffset(0).StartsSentence());
                Assert.True(buffer.GetIterAtOffset(ends[0]).EndsSentence());
                Assert.True(buffer.GetIterAtOffset(1).InsideSentence());
            });
        }

        [Fact]
        public void A_combining_mark_is_a_character_but_not_a_cursor_position()
        {
            Run(() =>
            {
                // "e" + COMBINING ACUTE ACCENT + "f": three characters, but a
                // caret may not be placed between the "e" and its accent.
                var buffer = BufferOf("e\u0301f");

                Assert.Equal(3, buffer.CharCount);

                var byChar = buffer.StartIter;
                Assert.True(byChar.ForwardChar());
                Assert.Equal(1, byChar.Offset);
                Assert.False(byChar.IsCursorPosition);

                var byCursor = buffer.StartIter;
                Assert.True(byCursor.ForwardCursorPosition());
                Assert.Equal(2, byCursor.Offset);
                Assert.True(byCursor.IsCursorPosition);
            });
        }

        [Fact]
        public void A_lines_character_count_includes_the_newline_that_ends_it()
        {
            Run(() =>
            {
                var buffer = BufferOf("alpha\nbeta\ngamma");

                Assert.Equal(3, buffer.LineCount);

                TextIter second;
                Assert.True(buffer.GetIterAtLine(out second, 1));
                Assert.Equal(6, second.Offset);
                Assert.Equal(1, second.Line);
                Assert.Equal(0, second.LineOffset);
                Assert.True(second.StartsLine());

                // "alpha\n" is six characters: a line's own newline counts
                // towards it, so summing CharsInLine over the lines gives the
                // buffer's character count exactly.
                Assert.Equal(6, buffer.GetIterAtOffset(0).CharsInLine);
                Assert.Equal(5, buffer.GetIterAtOffset(11).CharsInLine);

                var total = 0;
                for (int line = 0; line < buffer.LineCount; line++)
                {
                    TextIter it;
                    buffer.GetIterAtLine(out it, line);
                    total += it.CharsInLine;
                }
                Assert.Equal(buffer.CharCount, total);

                var atEnd = buffer.GetIterAtOffset(5);
                Assert.True(atEnd.EndsLine());
                Assert.True(atEnd.ForwardLine());
                Assert.Equal(6, atEnd.Offset);
            });
        }

        // -------------------------------------------------------------- marks

        [Fact]
        public void Left_gravity_decides_which_side_of_an_insertion_a_mark_ends_up_on()
        {
            Run(() =>
            {
                var buffer = BufferOf("abcdef");

                var stays = buffer.CreateMark("stays", buffer.GetIterAtOffset(3), true);
                var moves = buffer.CreateMark("moves", buffer.GetIterAtOffset(3), false);

                var at = buffer.GetIterAtOffset(3);
                buffer.Insert(ref at, "XY");

                Assert.Equal("abcXYdef", buffer.Text);
                Assert.True(stays.LeftGravity);
                Assert.False(moves.LeftGravity);

                // A left-gravity mark stays to the left of text inserted at it;
                // a right-gravity one is pushed along to the right of it. Naming
                // reads backwards: "left gravity" is the one that does not move.
                Assert.Equal(3, buffer.GetIterAtMark(stays).Offset);
                Assert.Equal(5, buffer.GetIterAtMark(moves).Offset);
            });
        }

        [Fact]
        public void A_mark_inside_a_deleted_range_collapses_to_where_the_range_began()
        {
            Run(() =>
            {
                var buffer = BufferOf("0123456789");
                var inside = buffer.CreateMark("inside", buffer.GetIterAtOffset(5), true);
                var after = buffer.CreateMark("after", buffer.GetIterAtOffset(8), true);

                var start = buffer.GetIterAtOffset(3);
                var end = buffer.GetIterAtOffset(7);
                buffer.Delete(ref start, ref end);

                Assert.Equal("012789", buffer.Text);

                // Deletion does not delete marks: the ones it swallowed are
                // pulled back to the deletion point, and the ones past it shift.
                Assert.Equal(3, buffer.GetIterAtMark(inside).Offset);
                Assert.Equal(4, buffer.GetIterAtMark(after).Offset);

                // Delete leaves both iterators at the deletion point.
                Assert.Equal(3, start.Offset);
                Assert.Equal(3, end.Offset);
            });
        }

        [Fact]
        public void Deleting_a_mark_unregisters_it_and_reports_it_as_deleted()
        {
            Run(() =>
            {
                var buffer = BufferOf("abcdef");
                var mark = buffer.CreateMark("temp", buffer.GetIterAtOffset(2), true);

                Assert.Same(mark, buffer.GetMark("temp"));
                Assert.False(mark.Deleted);

                TextMark reported = null;
                buffer.MarkDeleted += (o, e) => reported = e.Mark;

                buffer.DeleteMark(mark);

                Assert.Same(mark, reported);
                Assert.True(mark.Deleted);
                Assert.Null(buffer.GetMark("temp"));

                // The buffer itself is untouched by removing a mark.
                Assert.Equal("abcdef", buffer.Text);
            });
        }

        [Fact]
        public void An_iterator_lists_every_mark_standing_at_its_position()
        {
            Run(() =>
            {
                var buffer = BufferOf("abcdef");
                buffer.CreateMark("one", buffer.GetIterAtOffset(2), true);
                buffer.CreateMark("two", buffer.GetIterAtOffset(2), false);
                buffer.CreateMark("elsewhere", buffer.GetIterAtOffset(4), true);

                // TextIter.Marks is hand-written: it walks a GSList of GtkTextMark
                // that the generated layer cannot express.
                var names = buffer.GetIterAtOffset(2).Marks.Select(m => m.Name).ToList();

                Assert.Contains("one", names);
                Assert.Contains("two", names);
                Assert.DoesNotContain("elsewhere", names);

                Assert.Empty(buffer.GetIterAtOffset(3).Marks);
            });
        }

        // --------------------------------------------------------------- tags

        [Fact]
        public void A_tag_toggles_exactly_at_the_bounds_it_was_applied_over()
        {
            Run(() =>
            {
                var buffer = BufferOf("0123456789");
                var tag = new TextTag("marked");
                buffer.TagTable.Add(tag);

                buffer.ApplyTag(tag, buffer.GetIterAtOffset(2), buffer.GetIterAtOffset(6));

                Assert.True(buffer.GetIterAtOffset(2).StartsTag(tag));
                Assert.True(buffer.GetIterAtOffset(6).EndsTag(tag));
                Assert.True(buffer.GetIterAtOffset(5).HasTag(tag));
                Assert.False(buffer.GetIterAtOffset(6).HasTag(tag));

                var walk = buffer.StartIter;
                Assert.True(walk.ForwardToTagToggle(tag));
                Assert.Equal(2, walk.Offset);
                Assert.True(walk.ForwardToTagToggle(tag));
                Assert.Equal(6, walk.Offset);

                var back = buffer.EndIter;
                Assert.True(back.BackwardToTagToggle(tag));
                Assert.Equal(6, back.Offset);

                Assert.Equal(new[] { tag }, buffer.GetIterAtOffset(2).GetToggledTags(true));
                Assert.Equal(new[] { tag }, buffer.GetIterAtOffset(6).GetToggledTags(false));
            });
        }

        [Fact]
        public void Removing_a_tag_from_the_middle_of_its_range_splits_it_in_two()
        {
            Run(() =>
            {
                var buffer = BufferOf("0123456789");
                var tag = new TextTag("marked");
                buffer.TagTable.Add(tag);

                buffer.ApplyTag(tag, buffer.GetIterAtOffset(0), buffer.GetIterAtOffset(10));
                buffer.RemoveTag(tag, buffer.GetIterAtOffset(3), buffer.GetIterAtOffset(6));

                var toggles = new List<int>();
                var walk = buffer.StartIter;
                if (walk.StartsTag(tag))
                    toggles.Add(walk.Offset);
                while (walk.ForwardToTagToggle(tag))
                    toggles.Add(walk.Offset);

                Assert.Equal(new[] { 0, 3, 6, 10 }, toggles);
                Assert.True(buffer.GetIterAtOffset(2).HasTag(tag));
                Assert.False(buffer.GetIterAtOffset(4).HasTag(tag));
                Assert.True(buffer.GetIterAtOffset(7).HasTag(tag));
            });
        }

        [Fact]
        public void An_iterator_lists_its_tags_in_ascending_priority()
        {
            Run(() =>
            {
                var buffer = BufferOf("abcdef");
                var first = new TextTag("first");
                var second = new TextTag("second");
                buffer.TagTable.Add(first);
                buffer.TagTable.Add(second);

                buffer.ApplyTag(first, buffer.StartIter, buffer.EndIter);
                buffer.ApplyTag(second, buffer.StartIter, buffer.EndIter);

                // A tag's priority is the order it entered the table, not the
                // order it was applied - the highest priority wins when two tags
                // set the same attribute, and it is last in this list.
                Assert.True(first.Priority < second.Priority);

                // TextIter.Tags is hand-written, over a GSList of GtkTextTag.
                var tags = buffer.GetIterAtOffset(2).Tags;
                Assert.Equal(new[] { "first", "second" }, tags.Select(t => t.Name).ToArray());
            });
        }

        [Fact]
        public void A_tag_table_counts_its_tags_and_announces_additions_and_removals()
        {
            Run(() =>
            {
                var table = new TextTagTable();
                var added = new List<string>();
                var removed = new List<string>();
                table.TagAdded += (o, e) => added.Add(e.Tag.Name);
                table.TagRemoved += (o, e) => removed.Add(e.Tag.Name);

                var bold = new TextTag("bold");
                var italic = new TextTag("italic");

                Assert.True(table.Add(bold));
                Assert.True(table.Add(italic));

                Assert.Equal(2, table.Size);
                Assert.Same(bold, table.Lookup("bold"));
                Assert.Null(table.Lookup("absent"));

                var visited = new List<string>();
                table.Foreach(tag => visited.Add(tag.Name));
                Assert.Equal(new[] { "bold", "italic" }, visited.OrderBy(n => n).ToArray());

                table.Remove(italic);

                Assert.Equal(1, table.Size);
                Assert.Null(table.Lookup("italic"));
                Assert.Equal(new[] { "bold", "italic" }, added);
                Assert.Equal(new[] { "italic" }, removed);
            });
        }

        [Fact]
        public void InsertMarkup_turns_markup_into_tags_rather_than_into_text()
        {
            Run(() =>
            {
                var buffer = new TextBuffer(null);
                var at = buffer.EndIter;
                buffer.InsertMarkup(ref at, "plain <b>bold</b>");

                Assert.Equal("plain bold", buffer.Text);

                Assert.Empty(buffer.GetIterAtOffset(1).Tags);

                // TextTag.Weight is hand-written, because the property is typed
                // as a plain int by Gtk and Pango.Weight is the useful shape.
                var tags = buffer.GetIterAtOffset(7).Tags;
                Assert.Single(tags);
                Assert.Equal(Pango.Weight.Bold, tags[0].Weight);
            });
        }

        [Fact]
        public void InsertRange_carries_the_tags_across_from_the_source_buffer()
        {
            Run(() =>
            {
                var source = BufferOf("copied text");
                var tag = new TextTag("emphasis") { Style = Pango.Style.Italic };
                source.TagTable.Add(tag);
                source.ApplyTag(tag, source.GetIterAtOffset(0), source.GetIterAtOffset(6));

                // The destination shares the tag table, which is what makes a
                // tag applied in one buffer meaningful in the other.
                var target = new TextBuffer(source.TagTable);
                var at = target.EndIter;
                target.Insert(ref at, ">> ");
                target.InsertRange(ref at, source.GetIterAtOffset(0), source.GetIterAtOffset(11));

                Assert.Equal(">> copied text", target.Text);

                // The tag covered "copied" in the source, so it must cover
                // exactly offsets 3..9 here - shifted by the ">> " prefix.
                Assert.True(target.GetIterAtOffset(3).StartsTag(tag));
                Assert.True(target.GetIterAtOffset(8).HasTag(tag));
                Assert.True(target.GetIterAtOffset(9).EndsTag(tag));
                Assert.False(target.GetIterAtOffset(10).HasTag(tag));
            });
        }

        // ------------------------------------------------------------- search

        [Fact]
        public void Search_reports_the_bounds_of_the_match_and_is_case_sensitive_by_default()
        {
            Run(() =>
            {
                var buffer = BufferOf("the cat sat on the mat");

                TextIter matchStart, matchEnd;
                Assert.False(buffer.StartIter.ForwardSearch("CAT", 0, out matchStart, out matchEnd, buffer.EndIter));

                Assert.True(buffer.StartIter.ForwardSearch(
                    "CAT", TextSearchFlags.CaseInsensitive, out matchStart, out matchEnd, buffer.EndIter));
                Assert.Equal(4, matchStart.Offset);
                Assert.Equal(7, matchEnd.Offset);
                Assert.Equal("cat", buffer.GetText(matchStart, matchEnd, false));

                // Backward search from the end finds the *last* "the".
                Assert.True(buffer.EndIter.BackwardSearch(
                    "the", 0, out matchStart, out matchEnd, buffer.StartIter));
                Assert.Equal(15, matchStart.Offset);
            });
        }

        [Fact]
        public void A_child_anchor_hides_a_match_from_search_unless_TextOnly_is_given()
        {
            Run(() =>
            {
                var buffer = BufferOf("abc");
                var at = buffer.GetIterAtOffset(1);
                buffer.CreateChildAnchor(ref at);

                // The buffer now reads "a\uFFFCbc", so a plain search for "abc"
                // no longer matches anything: search works on the slice.
                TextIter matchStart, matchEnd;
                Assert.False(buffer.StartIter.ForwardSearch("abc", 0, out matchStart, out matchEnd, buffer.EndIter));

                Assert.True(buffer.StartIter.ForwardSearch(
                    "abc", TextSearchFlags.TextOnly, out matchStart, out matchEnd, buffer.EndIter));
                Assert.Equal(0, matchStart.Offset);
                Assert.Equal(4, matchEnd.Offset);
            });
        }

        // --------------------------------------------------------------- undo

        [Fact]
        public void Undo_and_redo_walk_the_history_one_user_action_at_a_time()
        {
            Run(() =>
            {
                var buffer = new TextBuffer(null) { EnableUndo = true };
                int undone = 0, redone = 0;
                buffer.Undone += (o, e) => undone++;
                buffer.Redone += (o, e) => redone++;

                buffer.BeginUserAction();
                var at = buffer.EndIter;
                buffer.Insert(ref at, "one");
                buffer.EndUserAction();

                buffer.BeginUserAction();
                at = buffer.EndIter;
                buffer.Insert(ref at, " two");
                buffer.EndUserAction();

                Assert.Equal("one two", buffer.Text);
                Assert.True(buffer.CanUndo);
                Assert.False(buffer.CanRedo);

                buffer.Undo();
                Assert.Equal("one", buffer.Text);
                Assert.True(buffer.CanRedo);

                buffer.Undo();
                Assert.Equal(string.Empty, buffer.Text);
                Assert.False(buffer.CanUndo);

                buffer.Redo();
                Assert.Equal("one", buffer.Text);
                buffer.Redo();
                Assert.Equal("one two", buffer.Text);
                Assert.False(buffer.CanRedo);

                Assert.Equal(2, undone);
                Assert.Equal(2, redone);
            });
        }

        [Fact]
        public void An_irreversible_action_empties_the_undo_history()
        {
            Run(() =>
            {
                var buffer = new TextBuffer(null) { EnableUndo = true };

                var at = buffer.EndIter;
                buffer.Insert(ref at, "undoable");
                Assert.True(buffer.CanUndo);

                buffer.BeginIrreversibleAction();
                at = buffer.EndIter;
                buffer.Insert(ref at, " permanent");
                buffer.EndIrreversibleAction();

                // Everything before the irreversible action goes too: the point
                // is that the buffer can no longer be walked back past it.
                Assert.False(buffer.CanUndo);
                Assert.Equal("undoable permanent", buffer.Text);
            });
        }

        // ---------------------------------------------------------- selection

        [Fact]
        public void Selecting_a_range_backwards_leaves_the_cursor_at_the_anchor_it_was_given()
        {
            Run(() =>
            {
                var buffer = BufferOf("0123456789");
                Assert.False(buffer.HasSelection);

                // SelectRange(insert, bound): the cursor goes where the first
                // argument says even when that is the higher offset, while
                // GetSelectionBounds always answers in ascending order.
                buffer.SelectRange(buffer.GetIterAtOffset(7), buffer.GetIterAtOffset(2));

                Assert.True(buffer.HasSelection);
                Assert.Equal(7, buffer.CursorPosition);
                Assert.Equal(7, buffer.GetIterAtMark(buffer.InsertMark).Offset);
                Assert.Equal(2, buffer.GetIterAtMark(buffer.SelectionBound).Offset);

                TextIter start, end;
                Assert.True(buffer.GetSelectionBounds(out start, out end));
                Assert.Equal(2, start.Offset);
                Assert.Equal(7, end.Offset);
                Assert.Equal("23456", buffer.GetText(start, end, false));

                Assert.True(buffer.DeleteSelection(false, true));
                Assert.Equal("01789", buffer.Text);
                Assert.False(buffer.HasSelection);
            });
        }

        [Fact]
        public void Ordering_a_reversed_pair_of_iterators_swaps_both_of_them()
        {
            Run(() =>
            {
                var buffer = BufferOf("0123456789");

                var first = buffer.GetIterAtOffset(7);
                var second = buffer.GetIterAtOffset(2);

                first.Order(ref second);

                // gtk_text_iter_order writes through *both* pointers. Passing
                // the second by value - which the api.xml alone implies, since
                // the gir does not mark it inout - returned the range collapsed
                // to a single point, silently making every range operation
                // performed on the "ordered" pair a no-op.
                Assert.Equal(2, first.Offset);
                Assert.Equal(7, second.Offset);
                Assert.Equal("23456", buffer.GetText(first, second, false));

                Assert.True(first.Compare(second) < 0);
                Assert.True(buffer.GetIterAtOffset(4).InRange(first, second));
                Assert.False(buffer.GetIterAtOffset(8).InRange(first, second));
            });
        }

        // ------------------------------------------------------------ anchors

        [Fact]
        public void A_widget_added_at_a_child_anchor_is_listed_by_the_anchor()
        {
            Run(() =>
            {
                var view = new TextView();
                var buffer = view.Buffer;
                var end = buffer.EndIter;
                buffer.Insert(ref end, "before after");

                var at = buffer.GetIterAtOffset(6);
                var anchor = buffer.CreateChildAnchor(ref at);
                Assert.Empty(anchor.Widgets);

                var child = new Label("child");
                view.AddChildAtAnchor(child, anchor);

                // Gtk 4 changed this call to return a GtkWidget** plus a count.
                // The Gtk 3 binding passed one argument and read the result as a
                // GList, so the callee wrote the count through a register the
                // caller never set.
                var widgets = anchor.Widgets;
                Assert.Single(widgets);
                Assert.Same(child, widgets[0]);
                Assert.False(anchor.Deleted);

                var start = buffer.GetIterAtOffset(6);
                var stop = buffer.GetIterAtOffset(7);
                buffer.Delete(ref start, ref stop);

                Assert.True(anchor.Deleted);
                Assert.Equal("before after", buffer.Text);
            });
        }

        // ------------------------------------------------------------ signals

        // The signal-ordering pair below records into these, because a
        // [ConnectBefore] handler has to be a named method: the attribute is
        // read off the delegate's MethodInfo, and a lambda cannot carry it.
        private readonly List<string> _order = new List<string>();
        private string _textSeenBefore, _textSeenAfter, _doomedSeenBefore, _doomedSeenAfter;
        private int _offsetSeenBefore = -1;

        [GLib.ConnectBefore]
        private void RecordInsertBefore(object o, InsertTextArgs args)
        {
            _order.Add("insert-text (before)");
            _offsetSeenBefore = args.Location.Offset;
            _textSeenBefore = ((TextBuffer) o).Text;
        }

        [Fact]
        public void An_insert_handler_runs_after_the_insertion_unless_it_is_marked_ConnectBefore()
        {
            Run(() =>
            {
                var buffer = BufferOf("start");

                buffer.InsertText += RecordInsertBefore;
                buffer.InsertText += (o, e) =>
                {
                    _order.Add("insert-text (after)");
                    _textSeenAfter = buffer.Text;
                    Assert.Equal("!", e.Text);
                };
                buffer.Changed += (o, e) => _order.Add("changed");

                var at = buffer.EndIter;
                buffer.Insert(ref at, "!");

                // GtkSharp connects every "+=" handler with after=TRUE unless the
                // method carries [GLib.ConnectBefore]. insert-text is RUN_LAST and
                // its default handler is what performs the insertion and emits
                // changed, so the plain handler sees a buffer that has already
                // been modified - the opposite of what the Gtk documentation for
                // ::insert-text describes, and useless as a veto point.
                Assert.Equal(
                    new[] { "insert-text (before)", "changed", "insert-text (after)" },
                    _order);

                Assert.Equal(5, _offsetSeenBefore);
                Assert.Equal("start", _textSeenBefore);
                Assert.Equal("start!", _textSeenAfter);
            });
        }

        [GLib.ConnectBefore]
        private void RecordDeleteBefore(object o, DeleteRangeArgs args)
        {
            _doomedSeenBefore = ((TextBuffer) o).GetText(args.Start, args.End, false);
        }

        [Fact]
        public void Only_a_ConnectBefore_delete_handler_can_still_read_the_range_being_removed()
        {
            Run(() =>
            {
                var buffer = BufferOf("0123456789");

                buffer.DeleteRange += RecordDeleteBefore;
                buffer.DeleteRange += (o, e) => _doomedSeenAfter = buffer.GetText(e.Start, e.End, false);

                var start = buffer.GetIterAtOffset(2);
                var end = buffer.GetIterAtOffset(5);
                buffer.Delete(ref start, ref end);

                Assert.Equal("0156789", buffer.Text);
                Assert.Equal("234", _doomedSeenBefore);

                // By the time a plain handler runs, the range is gone and both
                // iterators have collapsed onto the deletion point, so reading
                // "what was deleted" from it yields the empty string rather than
                // an error. Silent, and exactly the wrong answer.
                Assert.Equal(string.Empty, _doomedSeenAfter);
            });
        }

        [Fact]
        public void A_tag_applied_signal_carries_the_tag_and_the_range_it_covers()
        {
            Run(() =>
            {
                var buffer = BufferOf("0123456789");
                var tag = new TextTag("marked");
                buffer.TagTable.Add(tag);

                string applied = null, appliedName = null;
                buffer.TagApplied += (o, e) =>
                {
                    appliedName = e.Tag.Name;
                    applied = buffer.GetText(e.Start, e.End, false);
                };

                buffer.ApplyTag(tag, buffer.GetIterAtOffset(3), buffer.GetIterAtOffset(8));

                Assert.Equal("marked", appliedName);
                Assert.Equal("34567", applied);
            });
        }

        [Fact]
        public void A_commit_notify_reports_character_positions_around_each_edit()
        {
            Run(() =>
            {
                var buffer = new TextBuffer(null);
                var seen = new List<string>();

                var id = buffer.AddCommitNotify(
                    TextBufferNotifyFlags.BeforeInsert | TextBufferNotifyFlags.AfterInsert |
                    TextBufferNotifyFlags.BeforeDelete | TextBufferNotifyFlags.AfterDelete,
                    (b, flags, position, length) => seen.Add(flags + ":" + position + ":" + length));

                var at = buffer.EndIter;
                buffer.Insert(ref at, "abcdef");

                var start = buffer.GetIterAtOffset(1);
                var end = buffer.GetIterAtOffset(4);
                buffer.Delete(ref start, ref end);

                Assert.Equal("aef", buffer.Text);

                // Every count here is in characters, and after-delete carries a
                // length of zero: the range is gone, so there is nothing left to
                // describe. A handler that read it as "how much was removed"
                // would see nothing happen.
                Assert.Equal(new[]
                {
                    "BeforeInsert:0:6",
                    "AfterInsert:0:6",
                    "BeforeDelete:1:3",
                    "AfterDelete:1:0",
                }, seen);

                buffer.RemoveCommitNotify(id);
                seen.Clear();
                at = buffer.EndIter;
                buffer.Insert(ref at, "z");
                Assert.Empty(seen);
            });
        }

        // -------------------------------------------------------- EntryBuffer

        [Fact]
        public void EntryBuffer_counts_characters_while_its_byte_count_counts_utf8()
        {
            Run(() =>
            {
                var buffer = new EntryBuffer(null, 0);
                buffer.SetText("h\u00E9llo", -1);

                // U+00E9 is one character and two bytes in UTF-8; a binding that
                // conflated the two would agree on pure ASCII and only here.
                Assert.Equal("h\u00E9llo", buffer.Text);
                Assert.Equal(5u, buffer.Length);
                Assert.Equal(6ul, buffer.Bytes);
            });
        }

        [Fact]
        public void EntryBuffer_announces_an_insertion_with_its_position_and_length()
        {
            Run(() =>
            {
                var buffer = new EntryBuffer("hello", -1);
                uint position = uint.MaxValue, count = uint.MaxValue;
                string chars = null;

                buffer.InsertedText += (o, e) =>
                {
                    position = e.Position;
                    chars = e.Chars;
                    count = e.NChars;
                };

                var inserted = buffer.InsertText(2, "XY", -1);

                Assert.Equal(2u, inserted);
                Assert.Equal("heXYllo", buffer.Text);
                Assert.Equal(2u, position);
                Assert.Equal("XY", chars);
                Assert.Equal(2u, count);
            });
        }

        [Fact]
        public void EntryBuffer_clamps_a_deletion_to_the_text_that_is_actually_there()
        {
            Run(() =>
            {
                var buffer = new EntryBuffer("abcdefg", -1);
                uint position = uint.MaxValue, count = uint.MaxValue;

                buffer.DeletedText += (o, e) =>
                {
                    position = e.Position;
                    count = e.NChars;
                };

                // Asking for more than exists is not an error; the return value
                // and the signal both report what was really removed.
                var removed = buffer.DeleteText(5, 100);

                Assert.Equal(2u, removed);
                Assert.Equal("abcde", buffer.Text);
                Assert.Equal(5u, position);
                Assert.Equal(2u, count);

                // A deletion starting past the end removes nothing at all, and
                // says so rather than throwing.
                Assert.Equal(0u, buffer.DeleteText(99, 1));
                Assert.Equal("abcde", buffer.Text);
            });
        }

        [Fact]
        public void EntryBuffer_max_length_truncates_rather_than_refusing()
        {
            Run(() =>
            {
                var buffer = new EntryBuffer(null, 0) { MaxLength = 4 };

                buffer.SetText("abcdefg", -1);

                Assert.Equal("abcd", buffer.Text);
                Assert.Equal(4u, buffer.Length);

                // Full: a further insert reports zero characters inserted.
                Assert.Equal(0u, buffer.InsertText(4, "z", -1));
                Assert.Equal("abcd", buffer.Text);

                buffer.DeleteText(0, 2);
                Assert.Equal(2u, buffer.InsertText(0, "zz", -1));
                Assert.Equal("zzcd", buffer.Text);
            });
        }

        [Fact]
        public void An_entry_shares_its_buffer_so_edits_are_visible_from_both_sides()
        {
            Run(() =>
            {
                var buffer = new EntryBuffer("shared", -1);
                var entry = new Entry { Buffer = buffer };

                Assert.Equal("shared", entry.Text);

                buffer.InsertText(0, "un", -1);
                Assert.Equal("unshared", entry.Text);

                entry.Text = "written through";
                Assert.Equal("written through", buffer.Text);
                Assert.Equal(15u, buffer.Length);
            });
        }
    }
}
