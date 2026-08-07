using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// The controls an application is actually built out of -- entry,
    /// adjustment, spin button, scale, level bar, calendar, notebook, stack,
    /// expander, popover, drop-down, scrolled window, search entry and search
    /// bar -- and the two subsystems Gtk 4 rebuilt from nothing: the clipboard
    /// and drag-and-drop.
    /// </summary>
    /// <remarks>
    /// The oracles here are arithmetic the test does itself (an adjustment's
    /// real maximum is upper minus page size; a step increment is a number the
    /// test chose), facts about the calendar (29 February 2024 was a Thursday),
    /// facts about UTF-8 (how many bytes a character needs), and the bytes the
    /// test put on the clipboard.
    ///
    /// The transfer half had never been called: Gdk.Clipboard, Gtk.DragSource
    /// and Gtk.DropTarget are wholly new in Gtk 4, and a drag cannot be
    /// performed from a test. What can be done is what Gtk itself does with
    /// them -- ask a content provider for its formats and its value, emit a
    /// drop target's signals and read the answers back -- and that is enough to
    /// have found four defects.
    /// </remarks>
    public class ControlsAndTransferTests : GtkTestBase
    {
        public ControlsAndTransferTests(GtkFixture fixture) : base(fixture) { }

        // ------------------------------------------------------------ helpers

        /// <summary>Runs the main loop until <paramref name="until"/> holds, or
        /// gives up. Returns whether it held, so a caller asserts it rather
        /// than quietly proceeding on a callback that never arrived.</summary>
        private static bool Pump(Func<bool> until, int milliseconds = 4000)
        {
            var deadline = Stopwatch.StartNew();
            while (!until() && deadline.ElapsedMilliseconds < milliseconds)
                Gtk.Application.RunIteration(false);
            return until();
        }

        private static void Settle(Gtk.Window window)
        {
            window.Present();
            for (int i = 0; i < 400 && Gtk.Application.EventsPending(); i++)
                Gtk.Application.RunIteration(false);
        }

        /// <summary>A drop target whose vfuncs answer from managed code instead
        /// of chaining. Chaining is not an option in a test: every default
        /// handler dereferences the GdkDrop of a drag that is in progress, and
        /// no drag can be started without a pointer device.</summary>
        private sealed class RecordingDropTarget : Gtk.DropTarget
        {
            public RecordingDropTarget(GLib.GType type, Gdk.DragAction actions) : base(type, actions) { }
            public RecordingDropTarget(IntPtr raw) : base(raw) { }

            public int Accepts, Motions, Leaves, Drops;
            public bool AcceptAnswer = true;
            public bool AcceptSawNullDrop;
            public string Dropped;
            public double DropX = double.NaN, DropY = double.NaN;
            public double MotionX = double.NaN, MotionY = double.NaN;

            protected override bool OnAccept(Gdk.Drop drop)
            {
                Accepts++;
                AcceptSawNullDrop = drop == null;
                return AcceptAnswer;
            }

            protected override Gdk.DragAction OnMotion(double x, double y)
            {
                Motions++;
                MotionX = x;
                MotionY = y;
                return Gdk.DragAction.Move;
            }

            protected override void OnLeave() => Leaves++;

            protected override bool OnDropped(GLib.Value value, double x, double y)
            {
                Drops++;
                Dropped = value.Val as string;
                DropX = x;
                DropY = y;
                return true;
            }
        }

        // ------------------------------------------- Entry / buffer / editable

        [Fact]
        public void An_entry_and_a_second_view_of_one_buffer_are_the_same_string()
        {
            // GtkEntryBuffer is the model and GtkEntry the view, which is the
            // whole reason the type exists in Gtk 4 -- and the only way to see
            // that the split is real is to attach two views to one buffer and
            // write through each of them in turn.
            Run(() =>
            {
                var buffer = new Gtk.EntryBuffer("shared", -1);
                var one = new Gtk.Entry(buffer);
                var two = new Gtk.Entry(buffer);

                Assert.Equal("shared", one.Text);
                Assert.Equal("shared", two.Text);

                one.Text = "written through the first";
                Assert.Equal("written through the first", two.Text);
                Assert.Equal("written through the first", buffer.Text);

                buffer.SetText("written through the buffer", -1);
                Assert.Equal("written through the buffer", one.Text);
                Assert.Equal("written through the buffer", two.Text);

                // An entry with no buffer of its own still has one.
                var alone = new Gtk.Entry();
                Assert.NotNull(alone.Buffer);
                Assert.Equal(0u, alone.Buffer.Length);
            });
        }

        [Fact]
        public void Editable_positions_count_characters_while_the_buffer_counts_utf8_bytes()
        {
            // gtk_editable_insert_text takes a LENGTH IN BYTES and a POSITION IN
            // CHARACTERS. The binding hides the length and computes it with
            // Encoding.UTF8.GetByteCount; passing text.Length instead would
            // truncate every non-ASCII insertion, and agree on ASCII, which is
            // what every other test in this repository types.
            //
            // The oracle is UTF-8 itself: "na\u00EFve caf\u00E9" is ten
            // characters and twelve bytes, and "\u00E9\u00E8" adds two
            // characters and four bytes.
            Run(() =>
            {
                var entry = new Gtk.Entry();
                var editable = (Gtk.IEditable) entry;

                entry.Text = "na\u00EFve caf\u00E9";
                Assert.Equal(10, entry.TextLength);
                Assert.Equal(10u, entry.Buffer.Length);
                Assert.Equal(12ul, entry.Buffer.Bytes);

                int position = 5;
                editable.InsertText("\u00E9\u00E8", ref position);

                Assert.Equal("na\u00EFve\u00E9\u00E8 caf\u00E9", entry.Text);
                Assert.Equal(7, position);              // advanced by two CHARACTERS
                Assert.Equal(16ul, entry.Buffer.Bytes); // grew by four BYTES

                // Delete is in characters at both ends, so it is the inverse.
                editable.DeleteText(5, 7);
                Assert.Equal("na\u00EFve caf\u00E9", entry.Text);

                // And so is every other index into the editable: -1 is "to the
                // end", which is the one value that is not a character count.
                Assert.Equal("na\u00EFve", editable.GetChars(0, 5));
                Assert.Equal("caf\u00E9", editable.GetChars(6, -1));
            });
        }

        [Fact]
        public void An_editable_selection_is_in_characters_and_moves_with_text_inserted_before_it()
        {
            Run(() =>
            {
                var entry = new Gtk.Entry { Text = "na\u00EFve caf\u00E9" };
                var editable = (Gtk.IEditable) entry;

                Assert.False(editable.GetSelectionBounds(out _, out _));

                editable.SelectRegion(6, 10);
                Assert.True(editable.GetSelectionBounds(out int start, out int end));
                Assert.Equal(6, start);
                Assert.Equal(10, end);
                Assert.Equal("caf\u00E9", editable.GetChars(start, end));

                // Two characters in front of the selection push it along by two,
                // not by the four bytes they occupy.
                int position = 0;
                editable.InsertText("\u00E9\u00E8", ref position);

                Assert.True(editable.GetSelectionBounds(out start, out end));
                Assert.Equal(8, start);
                Assert.Equal(12, end);
                Assert.Equal("caf\u00E9", editable.GetChars(start, end));
            });
        }

        // --------------------------------------------- Adjustment / SpinButton

        [Fact]
        public void An_adjustments_real_maximum_is_its_upper_less_the_page_size()
        {
            // The page size is the part of the range that is already on screen,
            // so a scrollbar can only ever reach upper - page_size. Code that
            // treats Upper as the maximum scrolls to a position the adjustment
            // will not take, and is told nothing.
            Run(() =>
            {
                var adjustment = new Gtk.Adjustment(50, 0, 100, 1, 10, 20);

                adjustment.Value = 95;
                Assert.Equal(80.0, adjustment.Value);       // 100 - 20, not 95

                adjustment.Value = -5;
                Assert.Equal(0.0, adjustment.Value);

                // With no page size the whole range is reachable.
                var whole = new Gtk.Adjustment(0, 0, 100, 1, 10, 0);
                whole.Value = 95;
                Assert.Equal(95.0, whole.Value);
                whole.Value = 1000;
                Assert.Equal(100.0, whole.Value);
            });
        }

        [Fact]
        public void Shrinking_the_range_leaves_the_value_outside_it_until_something_writes_it()
        {
            // Worth pinning because the obvious expectation is wrong and the
            // symptom is invisible: gtk_adjustment_set_upper does not re-clamp
            // the value. An adjustment whose model shrank therefore reports a
            // value its own range no longer contains, and only the NEXT write
            // -- even a write of the same number -- brings it back.
            Run(() =>
            {
                var adjustment = new Gtk.Adjustment(50, 0, 100, 1, 10, 20);
                adjustment.Value = 95;
                Assert.Equal(80.0, adjustment.Value);

                adjustment.Upper = 60;

                Assert.Equal(80.0, adjustment.Value);       // 80 > 60 - 20
                Assert.True(adjustment.Value > adjustment.Upper - adjustment.PageSize);

                adjustment.Value = adjustment.Value;        // no-op, and yet
                Assert.Equal(40.0, adjustment.Value);

                // Configure sets everything at once and does clamp.
                adjustment.Configure(1000, 0, 60, 1, 10, 20);
                Assert.Equal(40.0, adjustment.Value);
            });
        }

        [Fact]
        public void An_adjustment_separates_a_change_of_range_from_a_change_of_value()
        {
            // Two signals, and the reason there are two: a scrollbar redraws its
            // slider position on value-changed and its slider SIZE on changed.
            Run(() =>
            {
                var adjustment = new Gtk.Adjustment(0, 0, 100, 1, 10, 0);
                int changed = 0, valueChanged = 0;
                adjustment.Changed += (o, a) => changed++;
                adjustment.ValueChanged += (o, a) => valueChanged++;

                adjustment.Value = 42;
                Assert.Equal(1, valueChanged);
                Assert.Equal(0, changed);

                adjustment.Value = 42;                   // same number, no signal
                Assert.Equal(1, valueChanged);

                adjustment.Upper = 200;
                Assert.Equal(1, changed);
                Assert.Equal(1, valueChanged);

                // MinimumIncrement is the step increment, or the page increment
                // when there is no step -- the amount a key press moves by.
                Assert.Equal(1.0, adjustment.MinimumIncrement);
            });
        }

        [Fact]
        public void Spinning_a_spin_button_reads_its_argument_for_a_step_and_ignores_it_for_a_page()
        {
            // Written expecting "increment steps of the step increment", which
            // is how gtk_spin_button_spin reads, and every assertion here says
            // otherwise. The two directions do not even agree with each other:
            //
            //   STEP_FORWARD  moves by the ARGUMENT; the adjustment's step
            //                 increment plays no part at all, which makes it
            //                 identical to USER_DEFINED
            //   PAGE_FORWARD  moves by the adjustment's PAGE INCREMENT and
            //                 ignores the argument entirely
            //
            // So the argument means a distance in one case and nothing in the
            // other, and a caller who sets a step increment of 3 and asks for
            // one step gets 1. Pinned rather than relaxed, because the plausible
            // reading produces a value that is wrong by a factor rather than an
            // error.
            Run(() =>
            {
                var spin = new Gtk.SpinButton(0, 100, 1) { Digits = 0 };
                spin.Adjustment.StepIncrement = 3;
                spin.Adjustment.PageIncrement = 25;

                spin.Value = 10;
                spin.Spin(Gtk.SpinType.StepForward, 1);
                Assert.Equal(11.0, spin.Value);            // 10 + 1, not 10 + 3

                spin.Value = 10;
                spin.Spin(Gtk.SpinType.StepForward, 2);
                Assert.Equal(12.0, spin.Value);            // 10 + 2, not 10 + 6

                spin.Value = 10;
                spin.Spin(Gtk.SpinType.StepBackward, 4);
                Assert.Equal(6.0, spin.Value);

                spin.Value = 10;
                spin.Spin(Gtk.SpinType.PageForward, 1);
                Assert.Equal(35.0, spin.Value);            // 10 + 25

                spin.Value = 10;
                spin.Spin(Gtk.SpinType.PageForward, 2);
                Assert.Equal(35.0, spin.Value);            // still 10 + 25

                // UserDefined is documented as "move by the argument", and is
                // what StepForward already does.
                spin.Value = 10;
                spin.Spin(Gtk.SpinType.UserDefined, 7);
                Assert.Equal(17.0, spin.Value);

                spin.Spin(Gtk.SpinType.End, 0);
                Assert.Equal(100.0, spin.Value);

                spin.Spin(Gtk.SpinType.Home, 0);
                Assert.Equal(0.0, spin.Value);
            });
        }

        [Fact]
        public void A_wrapping_spin_button_crosses_from_the_top_back_to_the_bottom()
        {
            Run(() =>
            {
                var spin = new Gtk.SpinButton(0, 11, 1) { Digits = 0, Value = 11 };

                spin.Spin(Gtk.SpinType.StepForward, 1);
                Assert.Equal(11.0, spin.Value);            // clamped, not wrapped

                spin.Wrap = true;
                spin.Spin(Gtk.SpinType.StepForward, 1);
                Assert.Equal(0.0, spin.Value);

                spin.Spin(Gtk.SpinType.StepBackward, 1);
                Assert.Equal(11.0, spin.Value);
            });
        }

        [Fact]
        public void Snapping_to_ticks_rounds_a_typed_value_to_the_nearest_step()
        {
            // SnapToTicks acts when the TEXT is parsed, not when Value is
            // assigned, so the round trip only happens across Update(). That is
            // the trap: a program that sets Value and reads it straight back
            // sees no snapping at all and concludes the property does nothing.
            Run(() =>
            {
                var spin = new Gtk.SpinButton(0, 100, 5) { Digits = 0, SnapToTicks = true };

                spin.Value = 12;
                Assert.Equal(12.0, spin.Value);            // not yet

                spin.Update();
                Assert.Equal(10.0, spin.Value);            // |12-10| < |15-12|
                Assert.Equal("10", spin.Text);

                spin.Value = 13;
                spin.Update();
                Assert.Equal(15.0, spin.Value);            // |13-15| < |13-10|
            });
        }

        // ----------------------------------------- Scale / LevelBar / Progress

        [Fact]
        public void A_labelled_scale_mark_takes_a_line_of_room_and_clearing_the_marks_gives_it_back()
        {
            // gtk_scale_add_mark has no getter of any kind, so the only oracle
            // for "did the mark arrive" is that the scale now asks for room to
            // draw it, and ClearMarks has to return the widget to exactly where
            // it started.
            //
            // What is asserted is deliberately narrower than "a mark makes the
            // scale taller", which is FALSE and cost a full-suite failure to
            // learn: an unlabelled mark measured 40 against a bare 34 on its
            // own and 28 against 34 inside the suite, because AdwaitaTests calls
            // adw_init, which replaces the process's stylesheet ("Adwaita-empty"
            // plus libadwaita's own CSS) and changes every metric in it. A mark
            // moves the value into the marks area rather than simply adding to
            // it, so whether that is taller is the theme's business.
            //
            // A LABEL is a line of text, which needs a line of room under any
            // stylesheet, so those two comparisons hold either way.
            Run(() =>
            {
                var scale = new Gtk.Scale(Gtk.Orientation.Horizontal, 0, 100, 1);

                scale.Measure(Gtk.Orientation.Vertical, -1, out int bare, out _, out _, out _);

                scale.AddMark(25, Gtk.PositionType.Bottom, null);
                scale.Measure(Gtk.Orientation.Vertical, -1, out int ticked, out _, out _, out _);

                scale.AddMark(75, Gtk.PositionType.Bottom, "three quarters");
                scale.Measure(Gtk.Orientation.Vertical, -1, out int labelled, out _, out _, out _);

                Assert.True(labelled > ticked,
                            $"a label took no more room than a bare tick: {ticked} -> {labelled}");
                Assert.True(labelled > bare,
                            $"a label took no more room than no marks at all: {bare} -> {labelled}");

                scale.ClearMarks();
                scale.Measure(Gtk.Orientation.Vertical, -1, out int cleared, out _, out _, out _);
                Assert.Equal(bare, cleared);
            });
        }

        [Fact]
        public void A_level_bar_arrives_with_the_three_offsets_a_theme_colours()
        {
            // GTK_LEVEL_BAR_OFFSET_LOW/HIGH/FULL are defined by GTK, not by the
            // theme, and their values are part of its API: 0.25, 0.75, 1.0.
            Run(() =>
            {
                var bar = new Gtk.LevelBar();

                Assert.True(bar.GetOffsetValue("low", out double low));
                Assert.Equal(0.25, low);
                Assert.True(bar.GetOffsetValue("high", out double high));
                Assert.Equal(0.75, high);
                Assert.True(bar.GetOffsetValue("full", out double full));
                Assert.Equal(1.0, full);

                // A name that is not there is answered with false, and the out
                // parameter is left at zero -- which is a legitimate offset, so
                // the bool is the only thing that separates the two cases.
                Assert.False(bar.GetOffsetValue("nonesuch", out double missing));
                Assert.Equal(0.0, missing);
            });
        }

        [Fact]
        public void A_level_bar_offset_can_be_added_under_a_name_and_taken_away_again()
        {
            Run(() =>
            {
                var bar = new Gtk.LevelBar();
                var offsets = new List<string>();
                bar.OffsetChanged += (o, args) => offsets.Add(args.Name);

                bar.AddOffsetValue("danger", 0.9);
                Assert.True(bar.GetOffsetValue("danger", out double danger));
                Assert.Equal(0.9, danger);

                bar.AddOffsetValue("danger", 0.8);          // replaces, not adds
                Assert.True(bar.GetOffsetValue("danger", out danger));
                Assert.Equal(0.8, danger);

                // ::offset-changed names the offset that was defined, and it is
                // emitted by DEFINING one -- not, as the name suggests, when
                // the bar's value crosses it. Removing an offset says nothing
                // at all, so a listener maintaining its own copy of the set
                // sees the additions and misses the deletions.
                Assert.Equal(new[] { "danger", "danger" }, offsets);

                bar.RemoveOffsetValue("danger");
                Assert.False(bar.GetOffsetValue("danger", out _));
                Assert.Equal(2, offsets.Count);

                bar.Value = 0.95;
                Assert.Equal(2, offsets.Count);
            });
        }

        [Fact]
        public void Pulsing_a_progress_bar_says_nothing_at_all_through_its_fraction()
        {
            // Pulse mode and fraction mode are separate states of one widget,
            // and the pulse position is not readable from anywhere. A caller
            // polling Fraction to find out whether the bar is animating gets
            // 0 forever -- and no notify either, so a binding cannot be used to
            // watch it. Assigning a fraction is what leaves pulse mode.
            Run(() =>
            {
                var bar = new Gtk.ProgressBar();
                int notifications = 0;
                bar.AddNotification("fraction", (o, a) => notifications++);

                Assert.Equal(0.1, bar.PulseStep);
                bar.PulseStep = 0.25;

                for (int i = 0; i < 6; i++)
                    bar.Pulse();

                Assert.Equal(0.0, bar.Fraction);
                Assert.Equal(0, notifications);

                bar.Fraction = 0.5;
                Assert.Equal(0.5, bar.Fraction);
                Assert.Equal(1, notifications);
            });
        }

        // -------------------------------------------------------------- Calendar

        [Fact]
        public void A_calendars_month_counts_from_zero_and_the_date_it_hands_back_counts_from_one()
        {
            // The trap, and it is silent: GtkCalendar:month is 0-based because
            // that is what struct tm uses, while the GDateTime from
            // gtk_calendar_get_date is 1-based because that is what GLib uses.
            // Round-tripping a date through a calendar without the conversion
            // moves it a month and produces a perfectly plausible answer.
            //
            // The oracle is the calendar: 2024 is a leap year, so 29 February
            // exists, and it fell on a Thursday.
            Run(() =>
            {
                var calendar = new Gtk.Calendar { Year = 2024, Month = 1, Day = 29 };

                Assert.Equal(1, calendar.Month);

                var date = calendar.Date;
                Assert.Equal(2024, date.Year);
                Assert.Equal(2, date.Month);
                Assert.Equal(29, date.DayOfMonth);
                Assert.Equal(4, date.DayOfWeek);            // ISO: Thursday
            });
        }

        [Fact]
        public void Selecting_a_day_on_a_calendar_moves_all_three_fields_and_announces_it()
        {
            // 1 January 2021 was a Friday, which is ISO day 5.
            Run(() =>
            {
                var calendar = new Gtk.Calendar { Year = 1999, Month = 5, Day = 6 };
                int selections = 0;
                calendar.DaySelected += (o, a) => selections++;

                using (var utc = new GLib.TimeZone("UTC"))
                using (var newYear = new GLib.DateTime(utc, 2021, 1, 1, 0, 0, 0))
                    calendar.SelectDay(newYear);

                Assert.Equal(2021, calendar.Year);
                Assert.Equal(0, calendar.Month);            // January, 0-based
                Assert.Equal(1, calendar.Day);
                Assert.True(selections > 0);
                Assert.Equal(5, calendar.Date.DayOfWeek);   // Friday
            });
        }

        [Fact]
        public void Marked_days_on_a_calendar_are_remembered_by_number_and_cleared_together()
        {
            Run(() =>
            {
                var calendar = new Gtk.Calendar { Year = 2021, Month = 0, Day = 1 };

                calendar.MarkDay(1);
                calendar.MarkDay(31);

                Assert.True(calendar.GetDayIsMarked(1));
                Assert.True(calendar.GetDayIsMarked(31));
                Assert.False(calendar.GetDayIsMarked(2));

                calendar.UnmarkDay(1);
                Assert.False(calendar.GetDayIsMarked(1));
                Assert.True(calendar.GetDayIsMarked(31));

                // Marks belong to the day number rather than to the month, so
                // they survive a move to a month that is one day shorter.
                calendar.Month = 3;                         // April, 30 days
                Assert.True(calendar.GetDayIsMarked(31));

                calendar.ClearMarks();
                Assert.False(calendar.GetDayIsMarked(31));
            });
        }

        // -------------------------------------------------------------- Notebook

        [Fact]
        public void Reordering_a_notebook_page_moves_it_and_the_selection_follows_the_widget()
        {
            // A page number is a position, not an identity. Moving the current
            // page therefore changes Notebook.Page without changing which child
            // is on screen -- and code that remembered the index instead of the
            // widget now points at a different tab.
            Run(() =>
            {
                var notebook = new Gtk.Notebook();
                var first = new Gtk.Label("first");
                var second = new Gtk.Label("second");
                var third = new Gtk.Label("third");

                notebook.AppendPage(first, new Gtk.Label("1"));
                notebook.AppendPage(second, new Gtk.Label("2"));
                notebook.AppendPage(third, new Gtk.Label("3"));
                notebook.Page = 2;

                var moved = new List<(string child, uint to)>();
                notebook.PageReordered += (o, args) =>
                    moved.Add((((Gtk.Label) args.Child).Text, args.PageNum));

                notebook.ReorderChild(third, 0);

                Assert.Equal("third", ((Gtk.Label) notebook.GetNthPage(0)).Text);
                Assert.Equal("first", ((Gtk.Label) notebook.GetNthPage(1)).Text);
                Assert.Equal("second", ((Gtk.Label) notebook.GetNthPage(2)).Text);

                Assert.Equal(0, notebook.PageNum(third));
                Assert.Equal(0, notebook.Page);             // still showing "third"
                Assert.Equal(new[] { ("third", 0u) }, moved);

                // The tab label travelled with the page.
                Assert.Equal("3", notebook.GetTabLabelText(third));
            });
        }

        [Fact]
        public void Removing_the_current_notebook_page_leaves_the_notebook_on_one_that_still_exists()
        {
            Run(() =>
            {
                var notebook = new Gtk.Notebook();
                var first = new Gtk.Label("first");
                var second = new Gtk.Label("second");
                var third = new Gtk.Label("third");
                notebook.AppendPage(first, new Gtk.Label("1"));
                notebook.AppendPage(second, new Gtk.Label("2"));
                notebook.AppendPage(third, new Gtk.Label("3"));

                Assert.Equal(3, notebook.NPages);

                notebook.Page = 1;
                notebook.RemovePage(1);

                Assert.Equal(2, notebook.NPages);
                Assert.Equal(-1, notebook.PageNum(second));      // gone
                Assert.InRange(notebook.Page, 0, 1);
                Assert.NotNull(notebook.GetNthPage(notebook.Page));

                // Insert puts a page where it is asked to, not at the end.
                notebook.InsertPage(second, new Gtk.Label("2"), 1);
                Assert.Equal(1, notebook.PageNum(second));
                Assert.Equal("third", ((Gtk.Label) notebook.GetNthPage(2)).Text);
            });
        }

        // ----------------------------------------------------------------- Stack

        [Fact]
        public void A_stacks_pages_are_a_list_model_and_selecting_one_shows_it()
        {
            // GtkStack.Pages is how Gtk 4 replaced "walk the container": it is a
            // GtkSelectionModel over GtkStackPage, and it is what a
            // GtkStackSwitcher is driven from. Both halves have to work -- the
            // list model half to enumerate, the selection half to switch.
            //
            // It did not: GtkSelectionModel's gir declares
            // <prerequisite name="Gio.ListModel"/>, which GirToGapi drops, so
            // Gtk.ISelectionModel derived from nothing. That was invisible while
            // the object behind it was a bound type -- Gtk.SingleSelection and
            // Adw.ViewStackPages implement GLib.IListModel themselves -- but
            // GtkStackPages is private to GTK and appears in no gir, so this
            // came back as a bare adapter that threw InvalidCastException on
            // (GLib.IListModel) and had no NItems to offer.
            Run(() =>
            {
                var stack = new Gtk.Stack();
                var inbox = new Gtk.Label("inbox");
                var sent = new Gtk.Label("sent");
                stack.AddNamed(inbox, "inbox");
                stack.AddTitled(sent, "sent", "Sent");

                var pages = stack.Pages;

                Assert.Equal(2u, pages.NItems);
                Assert.Equal(Gtk.StackPage.GType, pages.ItemType);

                var first = Assert.IsType<Gtk.StackPage>(pages.GetObject(0));
                Assert.Equal("inbox", first.Name);
                Assert.Equal(inbox.Handle, first.Child.Handle);

                var second = Assert.IsType<Gtk.StackPage>(pages.GetObject(1));
                Assert.Equal("Sent", second.Title);

                // The selection and the visible child are one state seen twice.
                stack.VisibleChildName = "sent";
                Assert.False(pages.IsSelected(0));
                Assert.True(pages.IsSelected(1));

                pages.SelectItem(0, true);
                Assert.Equal("inbox", stack.VisibleChildName);
                Assert.Equal(inbox.Handle, stack.VisibleChild.Handle);

                stack.Remove(sent);
                Assert.Equal(1u, pages.NItems);
                Assert.Null(stack.GetChildByName("sent"));
            });
        }

        [Fact]
        public void A_stack_names_its_transition_and_reports_how_long_it_should_last()
        {
            Run(() =>
            {
                var stack = new Gtk.Stack
                {
                    TransitionType = Gtk.StackTransitionType.SlideLeftRight,
                    TransitionDuration = 350,
                };
                stack.AddNamed(new Gtk.Label("a"), "a");
                stack.AddNamed(new Gtk.Label("b"), "b");

                Assert.Equal(Gtk.StackTransitionType.SlideLeftRight, stack.TransitionType);
                Assert.Equal(350u, stack.TransitionDuration);

                // Nothing is animating while the stack is not on screen: the
                // transition runs off the frame clock, which an unmapped widget
                // does not have. So TransitionRunning is a fact about being
                // drawn, not about having been asked to switch.
                stack.VisibleChildName = "b";
                Assert.False(stack.TransitionRunning);
                Assert.Equal("b", stack.VisibleChildName);

                // A name that is not there leaves the stack where it was.
                stack.VisibleChildName = "nonesuch";
                Assert.Equal("b", stack.VisibleChildName);
            });
        }

        // ------------------------------------------- Expander / Popover / scroll

        [Fact]
        public void An_expanded_expander_asks_for_room_for_its_child()
        {
            // The only externally visible consequence of Expanded, short of
            // rendering: a collapsed expander measures its label alone, an
            // expanded one measures label plus child. A three-line child makes
            // that a large difference rather than a rounding one.
            Run(() =>
            {
                var expander = new Gtk.Expander("Details")
                {
                    Child = new Gtk.Label("one\ntwo\nthree"),
                };

                Assert.False(expander.Expanded);
                expander.Measure(Gtk.Orientation.Vertical, -1, out int collapsed, out _, out _, out _);

                expander.Expanded = true;
                expander.Measure(Gtk.Orientation.Vertical, -1, out int expanded, out _, out _, out _);

                Assert.True(expanded > collapsed,
                            $"expanding did not ask for more height: {collapsed} -> {expanded}");

                expander.Expanded = false;
                expander.Measure(Gtk.Orientation.Vertical, -1, out int again, out _, out _, out _);
                Assert.Equal(collapsed, again);
            });
        }

        [SkippableFact]
        public void A_popover_pops_up_and_down_and_reports_its_closing()
        {
            // A popover is a separate surface in Gtk 4 rather than a child of
            // its parent, so it needs a mapped parent to be shown at all -- and
            // the parent is set with gtk_widget_set_parent, not by adding it to
            // a container, which is the part that catches people porting.
            Skip.If(Run(() => Gdk.Display.Default == null), "no display");

            Run(() =>
            {
                var window = new Gtk.Window();
                var button = new Gtk.Button();
                window.Child = button;

                var popover = new Gtk.Popover { Child = new Gtk.Label("in the popover") };
                popover.Parent = button;
                Assert.Equal(button.Handle, popover.Parent.Handle);

                Settle(window);

                int closings = 0;
                popover.Closed += (o, a) => closings++;

                popover.Popup();
                Assert.True(Pump(() => popover.Visible));

                popover.Popdown();
                Assert.True(Pump(() => !popover.Visible));
                Assert.Equal(1, closings);

                popover.Unparent();
                window.Destroy();
            });
        }

        [SkippableFact]
        public void A_scrolled_windows_adjustment_describes_the_child_it_could_not_fit()
        {
            // The adjustment is the contract between the scrolled window and
            // whatever draws a scrollbar: upper is the child's size, page size
            // is the part of it on screen, and the greatest reachable value is
            // the difference. All three are arithmetic once the window has been
            // allocated -- which is why this one has to be presented.
            Skip.If(Run(() => Gdk.Display.Default == null), "no display");

            Run(() =>
            {
                var box = new Gtk.Box(Gtk.Orientation.Vertical, 0);
                for (int i = 0; i < 40; i++)
                    box.Append(new Gtk.Label("row " + i));

                var scroller = new Gtk.ScrolledWindow { Child = box };
                var window = new Gtk.Window { DefaultWidth = 120, DefaultHeight = 90, Child = scroller };
                Settle(window);

                var vertical = scroller.Vadjustment;

                Assert.True(vertical.Upper > vertical.PageSize,
                            $"forty labels fitted in {vertical.PageSize}px?");
                Assert.Equal(0.0, vertical.Lower);
                Assert.Equal(0.0, vertical.Value);

                int scrolls = 0;
                vertical.ValueChanged += (o, a) => scrolls++;

                vertical.Value = 1e9;
                Assert.Equal(vertical.Upper - vertical.PageSize, vertical.Value, 6);
                Assert.Equal(1, scrolls);

                // A scrolled window that needs a scrollbar under the automatic
                // policy has one.
                Assert.Equal(Gtk.PolicyType.Automatic, scroller.VscrollbarPolicy);
                Assert.True(scroller.VScrollbar.Visible);

                window.Destroy();
            });
        }

        // ------------------------------------------- DropDown / search widgets

        [Fact]
        public void A_drop_down_always_has_something_selected_and_a_new_model_resets_it()
        {
            // GtkDropDown wraps whatever model it is given in a
            // GtkSingleSelection with autoselect on, so GTK_INVALID_LIST_POSITION
            // -- the value that means "nothing" everywhere else in the list
            // stack -- is refused here without a word. There is no empty state.
            Run(() =>
            {
                var model = new Gtk.StringList(new[] { "alpha", "beta", "gamma" });
                var dropDown = new Gtk.DropDown(model, null);

                Assert.Equal(0u, dropDown.Selected);

                dropDown.Selected = 2;
                Assert.Equal(2u, dropDown.Selected);
                var selected = GLib.Object.GetObject(dropDown.SelectedItem) as Gtk.StringObject;
                Assert.Equal("gamma", selected.String);

                dropDown.Selected = uint.MaxValue;          // GTK_INVALID_LIST_POSITION
                Assert.Equal(2u, dropDown.Selected);

                dropDown.Model = new Gtk.StringList(new[] { "only" });
                Assert.Equal(0u, dropDown.Selected);
                Assert.Equal("only",
                             (GLib.Object.GetObject(dropDown.SelectedItem) as Gtk.StringObject).String);
            });
        }

        [Fact]
        public void A_search_entry_holds_search_changed_back_but_reports_an_emptying_at_once()
        {
            // The whole point of GtkSearchEntry over GtkEntry: ::changed fires
            // on every keystroke and ::search-changed only once the typing has
            // stopped, so an expensive search runs once. But clearing the entry
            // is NOT delayed -- an empty search is cheap and the results have to
            // disappear immediately -- and that asymmetry is invisible in a test
            // that only types.
            Run(() =>
            {
                var entry = new Gtk.SearchEntry { SearchDelay = 30 };
                int changes = 0, searches = 0;
                ((Gtk.IEditable) entry).Changed += (o, a) => changes++;
                entry.SearchChanged += (o, a) => searches++;

                entry.Text = "gtk";
                Assert.Equal(1, changes);
                Assert.Equal(0, searches);                  // still waiting

                Assert.True(Pump(() => searches == 1), "search-changed never arrived");

                // Emptying it: two ::changed (the delete and the insert of
                // nothing) and a search-changed with no wait at all.
                searches = 0;
                entry.Text = "";
                Assert.Equal(1, searches);

                int stops = 0;
                entry.StopSearch += (o, a) => stops++;
                GLib.Signal.Emit(entry, "stop-search");
                Assert.Equal(1, stops);
            });
        }

        [Fact]
        public void A_search_bar_shows_one_state_under_two_property_names()
        {
            // SearchMode and SearchModeEnabled are the same GObject property
            // reached two ways -- the hand-written alias and the generated
            // one -- so a binding mistake would let them disagree.
            Run(() =>
            {
                var bar = new Gtk.SearchBar();
                var entry = new Gtk.SearchEntry();
                bar.Child = entry;
                bar.ConnectEntry(entry);

                int notifications = 0;
                bar.AddNotification("search-mode-enabled", (o, a) => notifications++);

                Assert.False(bar.SearchMode);
                Assert.False(bar.SearchModeEnabled);

                bar.SearchModeEnabled = true;
                Assert.True(bar.SearchMode);
                Assert.Equal(1, notifications);

                bar.SearchMode = false;
                Assert.False(bar.SearchModeEnabled);
                Assert.Equal(2, notifications);

                // The bar wraps its child rather than holding it directly: the
                // entry's parent is the revealer's box, not the search bar. So
                // Child is the property to read it back from, and walking up
                // from the entry does not reach the bar in one step.
                Assert.Equal(entry.Handle, bar.Child.Handle);
                Assert.NotEqual(bar.Handle, entry.Parent.Handle);
            });
        }

        // ------------------------------------------------------------ Clipboard

        [SkippableFact]
        public void Text_put_on_the_clipboard_reads_back_through_the_asynchronous_api()
        {
            // The Gtk 4 clipboard is asynchronous in both directions and has no
            // synchronous form at all, so nothing here can be tested without
            // running the main loop -- which is why none of it ever had been.
            Skip.If(Run(() => Gdk.Display.Default == null), "no display");

            Run(() =>
            {
                var clipboard = Gdk.Display.Default.Clipboard;

                clipboard.Text = "a string this test chose";

                // Local means the content provider on this side is the one that
                // will answer, which is what makes the round trip meaningful
                // rather than a report about whatever else is on the desktop.
                Assert.True(clipboard.IsLocal);

                string read = null;
                Exception failure = null;
                bool finished = false;

                clipboard.ReadTextAsync(null, (source, result, data) =>
                {
                    try { read = clipboard.ReadTextFinish(result); }
                    catch (Exception e) { failure = e; }
                    finished = true;
                });

                Assert.True(Pump(() => finished), "the read never called back");
                Assert.Null(failure);
                Assert.Equal("a string this test chose", read);
            });
        }

        [SkippableFact]
        public void A_value_on_the_clipboard_is_offered_as_every_mime_type_gdk_can_serialise_it_to()
        {
            // Content negotiation: a provider built around a GValue of one type
            // advertises that GType AND every mime type Gdk knows how to turn it
            // into. That is what lets a Gtk application copy a string and a
            // non-Gtk one paste text/plain -- and it is why reading the same
            // clipboard as a stream and as a value must agree.
            Skip.If(Run(() => Gdk.Display.Default == null), "no display");

            Run(() =>
            {
                var clipboard = Gdk.Display.Default.Clipboard;

                using (var value = new GLib.Value("negotiated"))
                    Assert.True(clipboard.SetContent(new Gdk.ContentProvider(value)));

                var formats = clipboard.Formats;
                Assert.True(formats.ContainGtype(GLib.GType.String));
                Assert.True(formats.ContainMimeType("text/plain;charset=utf-8"),
                            "a string was not offered as utf-8 text: " + formats);
                Assert.False(formats.ContainGtype(GLib.GType.Int));

                // As a value.
                GLib.Value asValue = default;
                bool valueDone = false;
                clipboard.ReadValueAsync(GLib.GType.String, 0, null, (source, result, data) =>
                {
                    asValue = clipboard.ReadValueFinish(result);
                    valueDone = true;
                });
                Assert.True(Pump(() => valueDone), "the value read never called back");
                Assert.Equal("negotiated", (string) asValue.Val);

                // As a stream, through one of the mime types it advertised.
                string mime = null, asText = null;
                bool streamDone = false;
                clipboard.ReadAsync(new[] { "text/plain;charset=utf-8" }, 0, null,
                                    (source, result, data) =>
                {
                    var stream = clipboard.ReadFinish(result, out mime);
                    var buffer = new byte[64];
                    var read = stream.Read(buffer, (ulong) buffer.Length, null);
                    asText = System.Text.Encoding.UTF8.GetString(buffer, 0, (int) read);
                    streamDone = true;
                });
                Assert.True(Pump(() => streamDone), "the stream read never called back");
                Assert.Equal("text/plain;charset=utf-8", mime);
                Assert.Equal("negotiated", asText);
            });
        }

        [SkippableFact]
        public void Reading_the_clipboard_as_a_type_it_does_not_hold_reports_why()
        {
            // gdk_clipboard_read_value_finish returns NULL and fills the GError
            // when no format matches. Codegen converted the return value BEFORE
            // testing the error, so Marshal.PtrToStructure was handed NULL and
            // the caller got a NullReferenceException naming nothing, with the
            // GError leaked -- for what is an ordinary answer rather than a
            // fault. Every *_finish in the tree had the same ordering.
            Skip.If(Run(() => Gdk.Display.Default == null), "no display");

            Run(() =>
            {
                var clipboard = Gdk.Display.Default.Clipboard;

                using (var value = new GLib.Value("not a number"))
                    clipboard.SetContent(new Gdk.ContentProvider(value));

                Exception failure = null;
                bool finished = false;

                clipboard.ReadValueAsync(GLib.GType.Int, 0, null, (source, result, data) =>
                {
                    try { clipboard.ReadValueFinish(result); }
                    catch (Exception e) { failure = e; }
                    finished = true;
                });

                Assert.True(Pump(() => finished), "the read never called back");
                var error = Assert.IsType<GLib.GException>(failure);
                Assert.False(string.IsNullOrEmpty(error.Message));
            });
        }

        [SkippableFact]
        public void Setting_the_clipboards_content_announces_a_change()
        {
            Skip.If(Run(() => Gdk.Display.Default == null), "no display");

            Run(() =>
            {
                var clipboard = Gdk.Display.Default.Clipboard;
                int changes = 0;
                clipboard.Changed += (o, a) => changes++;

                using var value = new GLib.Value("first");
                var provider = new Gdk.ContentProvider(value);

                Assert.True(clipboard.SetContent(provider));
                Assert.Equal(1, changes);

                // The clipboard hands back the very provider it was given, so
                // the value can be asked for again without a round trip.
                Assert.Equal(provider.Handle, clipboard.Content.Handle);
                Assert.True(clipboard.Content.GetValue(GLib.GType.String, out var held));
                Assert.Equal("first", (string) held.Val);
                held.Dispose();

                clipboard.Text = "second";
                Assert.Equal(2, changes);
            });
        }

        // --------------------------------------------- DragSource / DropTarget

        [Fact]
        public void A_drag_sources_prepare_hands_out_the_provider_it_holds_and_nothing_when_it_holds_none()
        {
            // ::prepare is asked, at the moment a drag begins, what is being
            // dragged; returning NULL is how a source refuses. The default
            // handler answers with the :content property, so a source with no
            // content refuses by construction -- which is the behaviour a
            // program relies on when it sets the content from a handler
            // instead.
            Run(() =>
            {
                var source = new Gtk.DragSource { Actions = Gdk.DragAction.Copy };

                Assert.Null(source.Content);
                Assert.Null(GLib.Signal.Emit(source, "prepare", 1.0, 2.0));

                using var value = new GLib.Value("dragged");
                var provider = new Gdk.ContentProvider(value);
                source.Content = provider;

                var prepared = Assert.IsType<Gdk.ContentProvider>(
                    GLib.Signal.Emit(source, "prepare", 1.0, 2.0));
                Assert.Equal(provider.Handle, prepared.Handle);

                using var formats = prepared.RefFormats();
                Assert.True(formats.ContainGtype(GLib.GType.String));

                // No drag is in progress, so there is no GdkDrag to report.
                Assert.Null(source.Drag);
            });
        }

        [Fact]
        public void A_drop_target_accepts_more_than_one_type_only_through_an_array()
        {
            // gtk_drop_target_set_gtypes takes "const GType *, gsize" and
            // gtk_drop_target_get_gtypes returns the same pair. Codegen has a
            // rule for a NULL-terminated array and none for pointer-plus-count,
            // so both came out over a single GLib.GType: the setter passed the
            // GType's numeric VALUE as the address of the array (G_TYPE_STRING
            // is 64, so GTK dereferenced address 64) and the getter wrapped the
            // array's address in a GType. The constructor takes one type, so
            // this is the only way to accept two, and it could not be used.
            Run(() =>
            {
                var target = new Gtk.DropTarget(GLib.GType.String, Gdk.DragAction.Copy);

                Assert.Equal(new[] { GLib.GType.String }, target.GetGtypes());

                target.SetGtypes(new[] { GLib.GType.String, Gdk.Texture.GType });

                Assert.Equal(new[] { GLib.GType.String, Gdk.Texture.GType }, target.GetGtypes());

                // And the formats -- which is what GTK matches a drag against --
                // now name both.
                var formats = target.Formats;
                Assert.True(formats.ContainGtype(GLib.GType.String));
                Assert.True(formats.ContainGtype(Gdk.Texture.GType));
                Assert.False(formats.ContainGtype(GLib.GType.Int));

                target.SetGtypes(new GLib.GType[0]);
                Assert.Empty(target.GetGtypes());
            });
        }

        [Fact]
        public void A_drop_target_with_no_drag_in_progress_has_no_value_rather_than_no_answer()
        {
            // gtk_drop_target_get_value returns NULL whenever there is no drop,
            // which is nearly always. Marshal.PtrToStructure raises
            // NullReferenceException on NULL, so simply reading the property --
            // the first thing anyone does while writing a drop handler -- threw
            // from inside the binding with nothing in the message.
            Run(() =>
            {
                var target = new Gtk.DropTarget(GLib.GType.String, Gdk.DragAction.Copy);

                Assert.Null(target.Value.Val);
                Assert.Null(target.CurrentDrop);
                Assert.False(target.Preload);

                target.Preload = true;
                Assert.True(target.Preload);
            });
        }

        [Fact]
        public void A_drop_targets_answers_come_back_out_of_the_emission_that_asked_for_them()
        {
            // ::accept decides whether a drag may land, ::motion decides which
            // action it would perform, and both are RUN_LAST signals whose
            // return value GTK reads. Overriding the vfuncs is what a managed
            // subclass does; emitting the signal is what GTK does. This drives
            // one against the other, which is the only way to reach the drop
            // machinery without a pointer device.
            //
            // ::accept is emitted with a NULL GdkDrop here on purpose: it is a
            // nullable object parameter, and GLib.Value (object) took the GType
            // off the argument's managed type, so a null argument threw
            // NullReferenceException out of Signal.Emit. The type now comes from
            // the signal, which g_signal_query already knows.
            Run(() =>
            {
                var target = new RecordingDropTarget(GLib.GType.String, Gdk.DragAction.Copy);

                Assert.Equal(true, GLib.Signal.Emit(target, "accept", (Gdk.Drop) null));
                Assert.Equal(1, target.Accepts);
                Assert.True(target.AcceptSawNullDrop);

                target.AcceptAnswer = false;
                Assert.Equal(false, GLib.Signal.Emit(target, "accept", (Gdk.Drop) null));
                Assert.Equal(2, target.Accepts);

                Assert.Equal(Gdk.DragAction.Move, GLib.Signal.Emit(target, "motion", 3.5, 4.5));
                Assert.Equal(1, target.Motions);
                Assert.Equal(3.5, target.MotionX);
                Assert.Equal(4.5, target.MotionY);

                GLib.Signal.Emit(target, "leave");
                Assert.Equal(1, target.Leaves);

                // The wrong number of arguments is a mistake worth naming,
                // rather than a GValue array GObject reads past the end of.
                Assert.Throws<ArgumentException>(() => GLib.Signal.Emit(target, "motion", 1.0));
            });
        }

        [Fact]
        public void A_drop_target_hands_its_handler_the_value_that_was_dropped()
        {
            // ::drop carries a GValue, which is a G_TYPE_VALUE boxed inside the
            // signal's own GValue -- two levels. GLib.Value (object) described
            // the argument by its managed type, which is not a GType at all, so
            // the emission produced a value of no usable type and the handler
            // threw "Unknown type" from inside the marshaller, where nothing
            // could catch it. Signal.Emit now boxes it properly, which is what
            // makes the one signal a drop target exists for reachable.
            Run(() =>
            {
                var target = new RecordingDropTarget(GLib.GType.String, Gdk.DragAction.Copy);

                using var payload = new GLib.Value("dropped here");
                var handled = GLib.Signal.Emit(target, "drop", payload, 7.0, 9.0);

                Assert.Equal(true, handled);
                Assert.Equal(1, target.Drops);
                Assert.Equal("dropped here", target.Dropped);
                Assert.Equal(7.0, target.DropX);
                Assert.Equal(9.0, target.DropY);
            });
        }

        [Fact]
        public void A_content_provider_built_from_two_providers_offers_what_both_offer()
        {
            // gdk_content_provider_new_union is how a drag source offers the
            // same thing several ways -- a file as a URI and as text. It takes
            // an array of providers and CONSUMES a reference to each, which is
            // the shape that has bitten this binding repeatedly.
            Run(() =>
            {
                using var text = new GLib.Value("as text");
                var asText = new Gdk.ContentProvider(text);
                var asBytes = new Gdk.ContentProvider("application/x-gtksharp-test",
                                                      new GLib.Bytes(new byte[] { 1, 2, 3 }));

                var both = new Gdk.ContentProvider(new[] { asText, asBytes });

                using var formats = both.RefFormats();
                Assert.True(formats.ContainGtype(GLib.GType.String));
                Assert.True(formats.ContainMimeType("application/x-gtksharp-test"));

                // Each half still answers for itself through the union.
                Assert.True(both.GetValue(GLib.GType.String, out var got));
                Assert.Equal("as text", (string) got.Val);
                got.Dispose();
            });
        }
    }
}
