using System;
using Gtk;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// Signals that share a name with another signal in the same assembly, and
    /// the args class each of them is supposed to get.
    /// </summary>
    /// <remarks>
    /// GapiCodegen names a signal's args class after the signal, so two signals
    /// of one name in one assembly ask for the same class and only one shape can
    /// have it. Every other user of that name is then handed members reading the
    /// wrong Args[] slot: the wrong type, or past the end of the array. Nothing
    /// about it fails to compile, and the handler still runs -- it just reads
    /// the wrong thing.
    ///
    /// GioSharp fixed the first instance (GFileMonitor::changed against
    /// GSettings::changed). This file pins the rest, found by an audit that
    /// groups every signal in an assembly by name and reports any name carrying
    /// two or more distinct parameter shapes. A signal with no parameters emits
    /// System.EventHandler and cannot collide.
    ///
    /// The signals are emitted directly rather than provoked, because most of
    /// them need a pointer device, a display or a session bus that a test does
    /// not have -- and the argument marshalling, which is the part that was
    /// wrong, is exercised either way.
    /// </remarks>
    public class SignalArgsCollisionTests : GtkTestBase
    {
        public SignalArgsCollisionTests(GtkFixture fixture) : base(fixture) { }

        // ----------------------------------------------- GestureClick vs LongPress

        [Fact]
        public void A_click_gesture_reports_the_press_count_and_the_coordinates()
        {
            // GtkGestureClick::pressed is (n_press, x, y) and
            // GtkGestureLongPress::pressed is (x, y). The long-press shape won,
            // so PressedArgs.X read Args[0] -- the click's n_press -- and no
            // member held the coordinates at all.
            Run(() =>
            {
                var gesture = new GestureClick();

                var presses = 0;
                double x = 0, y = 0;
                gesture.Pressed += (o, args) =>
                {
                    presses = args.NPress;
                    x = args.X;
                    y = args.Y;
                };

                GLib.Signal.Emit(gesture, "pressed", 2, 12.5, 34.5);

                Assert.Equal(2, presses);
                Assert.Equal(12.5, x);
                Assert.Equal(34.5, y);
            });
        }

        [Fact]
        public void A_long_press_gesture_keeps_its_own_two_argument_shape()
        {
            Run(() =>
            {
                var gesture = new GestureLongPress();

                double x = 0, y = 0;
                gesture.LongPressed += (o, args) => { x = args.X; y = args.Y; };

                GLib.Signal.Emit(gesture, "pressed", 4.0, 8.0);

                Assert.Equal(4.0, x);
                Assert.Equal(8.0, y);
            });
        }

        // ------------------------------------------------ Range vs SpinButton

        [Fact]
        public void A_range_change_value_handler_can_read_the_proposed_value()
        {
            // GtkRange::change-value is (scroll, value); GtkSpinButton's is
            // (scroll) alone, and its shape won, so the value a Range handler
            // exists to inspect was not on the args object at all.
            Run(() =>
            {
                var scale = new Scale(Orientation.Horizontal, 0, 100, 1);

                ScrollType scroll = ScrollType.None;
                double value = double.NaN;
                scale.ChangeValue += (o, args) =>
                {
                    scroll = args.Scroll;
                    value = args.Value;
                    args.RetVal = false;
                };

                GLib.Signal.Emit(scale, "change-value", ScrollType.StepForward, 42.0);

                Assert.Equal(ScrollType.StepForward, scroll);
                Assert.Equal(42.0, value);
            });
        }

        [Fact]
        public void A_spin_button_keeps_its_own_one_argument_shape()
        {
            Run(() =>
            {
                var spin = new SpinButton(0, 100, 1);

                ScrollType scroll = ScrollType.None;
                spin.ChangeValueByScroll += (o, args) => scroll = args.Scroll;

                GLib.Signal.Emit(spin, "change-value", ScrollType.PageBackward);

                Assert.Equal(ScrollType.PageBackward, scroll);
            });
        }

        // ------------------------------------------- DragSource vs GestureDrag

        [Fact]
        public void A_drag_source_hands_its_handler_the_drag_rather_than_offsets()
        {
            // GtkDragSource::drag-begin carries the GdkDrag; GtkGestureDrag's
            // carries (start_x, start_y). The gesture's shape won, so the one
            // object a DragSource handler exists to receive was unreachable.
            Run(() =>
            {
                var source = new DragSource();

                var raised = 0;
                Gdk.Drag drag = null;
                source.DragBegin += (o, args) => { raised++; drag = args.Drag; };

                // A real GdkDrag needs a device and a surface; null still proves
                // the member is typed as the drag and reads Args[0].
                GLib.Signal.Emit(source, "drag-begin", (Gdk.Drag) null);

                Assert.Equal(1, raised);
                Assert.Null(drag);
            });
        }

        [Fact]
        public void A_drag_gesture_keeps_its_offsets_under_its_own_name()
        {
            Run(() =>
            {
                var gesture = new GestureDrag();

                double startX = 0, startY = 0, offsetX = 0, offsetY = 0;
                gesture.DragStarted += (o, args) => { startX = args.StartX; startY = args.StartY; };
                gesture.DragEnded += (o, args) => { offsetX = args.OffsetX; offsetY = args.OffsetY; };

                GLib.Signal.Emit(gesture, "drag-begin", 3.0, 5.0);
                GLib.Signal.Emit(gesture, "drag-end", 7.0, 11.0);

                Assert.Equal(3.0, startX);
                Assert.Equal(5.0, startY);
                Assert.Equal(7.0, offsetX);
                Assert.Equal(11.0, offsetY);
            });
        }

        // ------------------------------------------- Assistant vs DragSource

        private int _prepares;
        private double _prepareX, _prepareY;

        // Named, and [ConnectBefore], because it has to be: see the test below.
        [GLib.ConnectBefore]
        private void OnPrepare(object o, PrepareArgs args)
        {
            _prepares++;
            _prepareX = args.X;
            _prepareY = args.Y;
        }

        [Fact]
        public void A_drag_source_prepare_handler_reads_the_coordinates()
        {
            // GtkDragSource::prepare is (x, y) and GtkAssistant::prepare is
            // (page). The drag source's shape won here, so it was the assistant
            // that read a GtkWidget* as a double.
            Run(() =>
            {
                var source = new DragSource();
                source.Prepare += OnPrepare;

                GLib.Signal.Emit(source, "prepare", 1.5, 2.5);

                Assert.Equal(1, _prepares);
                Assert.Equal(1.5, _prepareX);
                Assert.Equal(2.5, _prepareY);
            });
        }

        [Fact]
        public void A_lambda_on_an_accumulator_stopping_signal_never_runs_at_all()
        {
            // The ConnectBefore trap in its severest form. += connects after the
            // class closure, and GtkDragSource::prepare and GtkDropTarget::accept
            // use an accumulator that stops the emission as soon as the class
            // handler has answered -- so an "after" handler is not merely late,
            // it is never invoked. A lambda cannot carry [GLib.ConnectBefore],
            // because the attribute is read off the delegate's MethodInfo, so
            // these two signals cannot be handled with one at all.
            //
            // GtkRange::change-value is the contrast: its accumulator stops only
            // on a handler returning true, so an "after" lambda does run.
            Run(() =>
            {
                var source = new DragSource();
                var lambdaPrepares = 0;
                source.Prepare += (o, args) => lambdaPrepares++;
                GLib.Signal.Emit(source, "prepare", 1.0, 2.0);
                Assert.Equal(0, lambdaPrepares);

                var target = new DropTarget(GLib.GType.String, Gdk.DragAction.Copy);
                var lambdaAccepts = 0;
                target.Accept += (o, args) => lambdaAccepts++;
                GLib.Signal.Emit(target, "accept", (Gdk.Drop) null);
                Assert.Equal(0, lambdaAccepts);

                var scale = new Scale(Orientation.Horizontal, 0, 100, 1);
                var lambdaChanges = 0;
                scale.ChangeValue += (o, args) => { lambdaChanges++; args.RetVal = false; };
                GLib.Signal.Emit(scale, "change-value", ScrollType.StepForward, 42.0);
                Assert.Equal(1, lambdaChanges);
            });
        }

        [Fact]
        public void An_assistant_prepare_handler_reads_the_page_widget()
        {
            Run(() =>
            {
                var assistant = new Assistant();
                var page = new Label("page");

                Widget seen = null;
                assistant.PreparePage += (o, args) => seen = args.Page;

                GLib.Signal.Emit(assistant, "prepare", page);

                Assert.Same(page, seen);
            });
        }

        // -------------------------------------- TextBuffer vs TextTagTable

        [Fact]
        public void Removing_a_tag_from_a_range_reports_the_tag_and_the_range()
        {
            // GtkTextBuffer::remove-tag was renamed TagRemoved, which is the
            // name GtkTextTagTable::tag-removed already carries. The table's
            // one-member shape won, so the buffer's handler could not reach the
            // Start and End that say which range lost the tag.
            Run(() =>
            {
                var buffer = new TextBuffer(new TextTagTable()) { Text = "hello world" };
                var tag = new TextTag("marked");
                buffer.TagTable.Add(tag);
                buffer.ApplyTag(tag, buffer.GetIterAtOffset(0), buffer.GetIterAtOffset(5));

                TextTag seen = null;
                int start = -1, end = -1;
                buffer.TagUnapplied += (o, args) =>
                {
                    seen = args.Tag;
                    start = args.Start.Offset;
                    end = args.End.Offset;
                };

                buffer.RemoveTag(tag, buffer.GetIterAtOffset(0), buffer.GetIterAtOffset(5));

                Assert.Same(tag, seen);
                Assert.Equal(0, start);
                Assert.Equal(5, end);
            });
        }

        [Fact]
        public void A_tag_table_reports_the_tag_that_left_it()
        {
            Run(() =>
            {
                var table = new TextTagTable();
                var tag = new TextTag("marked");
                table.Add(tag);

                TextTag seen = null;
                table.TagRemoved += (o, args) => seen = args.Tag;

                table.Remove(tag);

                Assert.Same(tag, seen);
            });
        }

        // ------------------------------------------------- Filter vs Sorter

        [Fact]
        public void A_filter_and_a_sorter_each_report_their_own_change_enum()
        {
            // Three signals called "changed" carry three different payloads:
            // GtkFilter's GtkFilterChange, GtkSorter's GtkSorterChange, and
            // GtkCellRendererCombo's (path, iter). The sorter's won, so a
            // filter's change reason arrived typed as a sorter's.
            Run(() =>
            {
                var filter = new CustomFilter(_ => true);
                var sorter = new StringSorter(
                    new PropertyExpression(StringObject.GType, null, "string"));

                FilterChange filterChange = (FilterChange) (-1);
                SorterChange sorterChange = (SorterChange) (-1);

                filter.FilterChanged += (o, args) => filterChange = args.Change;
                sorter.SorterChanged += (o, args) => sorterChange = args.Change;

                // The method is EmitChanged; metadata renames it so it does not
                // collide with the signal it raises.
                filter.EmitChanged(FilterChange.MoreStrict);
                sorter.EmitChanged(SorterChange.Inverted);

                Assert.Equal(FilterChange.MoreStrict, filterChange);
                Assert.Equal(SorterChange.Inverted, sorterChange);
            });
        }

        // ------------------------------------------- Dialog vs NativeDialog

        [Fact]
        public void A_dialog_response_arrives_as_a_ResponseType()
        {
            // GtkDialog declares response_id as a GtkResponseType and
            // GtkNativeDialog as a bare gint. The gint won, so the member was
            // untyped for both. The metadata widens the native dialog's
            // declaration instead of renaming either signal.
            Run(() =>
            {
                var dialog = new Dialog();

                ResponseType seen = (ResponseType) int.MinValue;
                dialog.Response += (o, args) => seen = args.ResponseId;

                GLib.Signal.Emit(dialog, "response", (int) ResponseType.Accept);

                Assert.Equal(ResponseType.Accept, seen);
                dialog.Destroy();
            });
        }

        // ------------------------------------------------------- move-cursor

        [Fact]
        public void The_four_move_cursor_shapes_each_keep_their_own_arguments()
        {
            // Four widget families declare move-cursor with four different
            // parameter lists. GtkTreeView's won, so the three-parameter ones
            // read a fourth argument that was not there.
            Run(() =>
            {
                var listBox = new ListBox();
                MovementStep listStep = 0;
                var listCount = 0;
                var listExtend = false;
                var listModify = false;
                listBox.MoveCursor += (o, args) =>
                {
                    listStep = args.Step;
                    listCount = args.Count;
                    listExtend = args.Extend;
                    listModify = args.Modify;
                    args.RetVal = false;
                };
                GLib.Signal.Emit(listBox, "move-cursor",
                                 MovementStep.DisplayLines, 3, true, false);
                Assert.Equal(MovementStep.DisplayLines, listStep);
                Assert.Equal(3, listCount);
                Assert.True(listExtend);
                Assert.False(listModify);

                var view = new TextView();
                var textCount = 0;
                var textExtend = false;
                view.MoveTextCursor += (o, args) =>
                {
                    textCount = args.Count;
                    textExtend = args.ExtendSelection;
                };
                GLib.Signal.Emit(view, "move-cursor", MovementStep.Words, 2, true);
                Assert.Equal(2, textCount);
                Assert.True(textExtend);

                var tree = new TreeView();
                var treeDirection = 0;
                tree.MoveTreeCursor += (o, args) =>
                {
                    treeDirection = args.Direction;
                    args.RetVal = false;
                };
                GLib.Signal.Emit(tree, "move-cursor",
                                 MovementStep.DisplayLines, 1, false, false);
                Assert.Equal(1, treeDirection);
            });
        }
    }
}
