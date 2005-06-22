/* Text Widget/Hypertext
 *
 * Usually, tags modify the appearance of text in the view, e.g. making it
 * bold or colored or underlined. But tags are not restricted to appearance.
 * They can also affect the behavior of mouse and key presses, as this demo
 * shows.
 */

using System;
using System.Collections;
using Gtk;

namespace GtkDemo
{
	[Demo ("Hyper Text", "DemoHyperText.cs", "Text Widget")]
	public class DemoHyperText : Gtk.Window
	{
		bool hoveringOverLink = false;
		Gdk.Cursor handCursor, regularCursor;

		public DemoHyperText () : base ("HyperText")
		{
			handCursor = new Gdk.Cursor (Gdk.CursorType.Hand2);
			regularCursor = new Gdk.Cursor (Gdk.CursorType.Xterm);

			SetDefaultSize (450, 450);

			TextView view = new TextView ();
			view.WrapMode = WrapMode.Word;
			view.KeyPressEvent += new KeyPressEventHandler (KeyPress);
			view.WidgetEventAfter += new WidgetEventAfterHandler (EventAfter);
			view.MotionNotifyEvent += new MotionNotifyEventHandler (MotionNotify);
			view.VisibilityNotifyEvent += new VisibilityNotifyEventHandler (VisibilityNotify);

			ScrolledWindow sw = new ScrolledWindow ();
			sw.SetPolicy (Gtk.PolicyType.Automatic, Gtk.PolicyType.Automatic);
			Add (sw);
			sw.Add (view);

			ShowPage (view.Buffer, 1);
			ShowAll ();
		}

		Hashtable tag_pages = new Hashtable ();

		// Inserts a piece of text into the buffer, giving it the usual
		// appearance of a hyperlink in a web browser: blue and underlined.
		// Additionally, attaches some data on the tag, to make it recognizable
		// as a link.
		void InsertLink (TextBuffer buffer, ref TextIter iter, string text, int page)
		{
			TextTag tag = new TextTag (null);
			tag.Foreground = "blue";
			tag.Underline = Pango.Underline.Single;
			tag_pages [tag] = page;
			buffer.TagTable.Add (tag);
			buffer.InsertWithTags (ref iter, text, tag);
		}

		// Fills the buffer with text and interspersed links. In any real
		// hypertext app, this method would parse a file to identify the links.
		void ShowPage (TextBuffer buffer, int page)
		{
			buffer.Text = "";
			TextIter iter = buffer.StartIter;

			if (page == 1) {
				buffer.Insert (ref iter, "Some text to show that simple ");
				InsertLink (buffer, ref iter, "hypertext", 3);
				buffer.Insert (ref iter, " can easily be realized with ");
				InsertLink (buffer, ref iter, "tags", 2);
				buffer.Insert (ref iter, ".");
			} else if (page == 2) {
				buffer.Insert (ref iter,
					       "A tag is an attribute that can be applied to some range of text. " +
					       "For example, a tag might be called \"bold\" and make the text inside " +
					       "the tag bold. However, the tag concept is more general than that; " +
					       "tags don't have to affect appearance. They can instead affect the " +
					       "behavior of mouse and key presses, \"lock\" a range of text so the " +
					       "user can't edit it, or countless other things.\n");
      				InsertLink (buffer, ref iter, "Go back", 1);
			} else if (page == 3) {
				TextTag tag = buffer.TagTable.Lookup ("bold");
				if (tag == null) {
					tag = new TextTag ("bold");
					tag.Weight = Pango.Weight.Bold;
					buffer.TagTable.Add (tag);
				}
				buffer.InsertWithTags (ref iter, "hypertext:\n", tag);
				buffer.Insert (ref iter,
					       "machine-readable text that is not sequential but is organized " +
					       "so that related items of information are connected.\n");
				InsertLink (buffer, ref iter, "Go back", 1);
			}
		}

		// Looks at all tags covering the position of iter in the text view,
		// and if one of them is a link, follow it by showing the page identified
		// by the data attached to it.
		void FollowIfLink (TextView view, TextIter iter)
		{
			foreach (TextTag tag in iter.Tags) {
				object page = tag_pages [tag];
				if (page is int)
					ShowPage (view.Buffer, (int)page);
			}
		}

		// Looks at all tags covering the position (x, y) in the text view,
		// and if one of them is a link, change the cursor to the "hands" cursor
		// typically used by web browsers.
		void SetCursorIfAppropriate (TextView view, int x, int y)
		{
			bool hovering = false;
			TextIter iter = view.GetIterAtLocation (x, y);

			foreach (TextTag tag in iter.Tags) {
				if (tag_pages [tag] is int) {
					hovering = true;
					break;
				}
			}

			if (hovering != hoveringOverLink) {
				Gdk.Window window = view.GetWindow (Gtk.TextWindowType.Text);

				hoveringOverLink = hovering;
				if (hoveringOverLink)
					window.Cursor = handCursor;
				else
					window.Cursor = regularCursor;
			}
		}

		// Links can be activated by pressing Enter.
		void KeyPress (object sender, KeyPressEventArgs args)
		{
			TextView view = sender as TextView;

			switch ((Gdk.Key) args.Event.KeyValue) {
			case Gdk.Key.Return:
			case Gdk.Key.KP_Enter:
				TextIter iter = view.Buffer.GetIterAtMark (view.Buffer.InsertMark);
				FollowIfLink (view, iter);
				break;
			default:
				break;
			}
		}

		// Links can also be activated by clicking.
		void EventAfter (object sender, WidgetEventAfterArgs args)
		{
			if (args.Event.Type != Gdk.EventType.ButtonRelease)
				return;

			Gdk.EventButton evt = (Gdk.EventButton)args.Event;

			if (evt.Button != 1)
				return;

			TextView view = sender as TextView;
			TextIter start, end, iter;
			int x, y;

			// we shouldn't follow a link if the user has selected something
			view.Buffer.GetSelectionBounds (out start, out end);
			if (start.Offset != end.Offset)
				return;

			view.WindowToBufferCoords (TextWindowType.Widget, (int) evt.X, (int) evt.Y, out x, out y);
			iter = view.GetIterAtLocation (x, y);

			FollowIfLink (view, iter);
		}

		// Update the cursor image if the pointer moved.
		void MotionNotify (object sender, MotionNotifyEventArgs args)
		{
			TextView view = sender as TextView;
			int x, y;
			Gdk.ModifierType state;

			view.WindowToBufferCoords (TextWindowType.Widget, (int) args.Event.X, (int) args.Event.Y, out x, out y);
			SetCursorIfAppropriate (view, x, y);

			view.GdkWindow.GetPointer (out x, out y, out state);
		}

		// Also update the cursor image if the window becomes visible
		// (e.g. when a window covering it got iconified).
		void VisibilityNotify (object sender, VisibilityNotifyEventArgs a)
		{
			TextView view = sender as TextView;
			int wx, wy, bx, by;

			view.GetPointer (out wx, out wy);
			view.WindowToBufferCoords (TextWindowType.Widget, wx, wy, out bx, out by);
			SetCursorIfAppropriate (view, bx, by);
		}

		protected override bool OnDeleteEvent (Gdk.Event evt)
		{
			Destroy ();
			return true;
		}
	}
}
