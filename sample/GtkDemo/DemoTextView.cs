/* Text Widget/Multiple Views
 *
 * The Gtk.TextView widget displays a Gtk.TextBuffer. One Gtk.TextBuffer
 * can be displayed by multiple Gtk.TextViews. This demo has two views
 * displaying a single buffer, and shows off the widget's text
 * formatting features.
 */

using System;
using System.IO;

using Gdk;
using Gtk;

namespace GtkDemo
{
	[Demo ("Multiple Views", "DemoTextView.cs", "Text Widget")]
	public class DemoTextView : Gtk.Window
	{
		TextView view1;
		TextView view2;

		public DemoTextView () : base ("TextView")
		{
			SetDefaultSize (450,450);
			BorderWidth = 0;

			VPaned vpaned = new VPaned ();
			vpaned.BorderWidth = 5;
			Add (vpaned);

			// For convenience, we just use the autocreated buffer from
			// the first text view; you could also create the buffer
			// by itself, then later create a view widget.
			view1 = new TextView ();
			TextBuffer buffer = view1.Buffer;
			view2 = new TextView (buffer);

			ScrolledWindow sw = new ScrolledWindow ();
			sw.SetPolicy (PolicyType.Automatic, PolicyType.Automatic);
			vpaned.Add1 (sw);
			sw.Add (view1);

			sw = new ScrolledWindow ();
			sw.SetPolicy (PolicyType.Automatic, PolicyType.Automatic);
			vpaned.Add2 (sw);
			sw.Add (view2);

			CreateTags (buffer);
			InsertText (buffer);

			AttachWidgets (view1);
			AttachWidgets (view2);

			ShowAll ();
		}

		private TextChildAnchor buttonAnchor;
		private TextChildAnchor menuAnchor;
		private TextChildAnchor scaleAnchor;
		private TextChildAnchor animationAnchor;
		private TextChildAnchor entryAnchor;

		private void AttachWidgets (TextView textView)
		{
			// This is really different from the C version, but the
			// C versions seems a little pointless.

			Button button = new Button ("Click Me");
			button.Clicked +=  new EventHandler(EasterEggCB);
			textView.AddChildAtAnchor (button, buttonAnchor);
			button.ShowAll ();

			ComboBox combo = ComboBox.NewText ();
			combo.AppendText ("Option 1");
			combo.AppendText ("Option 2");
			combo.AppendText ("Option 3");

 			textView.AddChildAtAnchor (combo, menuAnchor);

			HScale scale = new HScale (null);
			scale.SetRange (0,100);
			scale.SetSizeRequest (70, -1);
			textView.AddChildAtAnchor (scale, scaleAnchor);
			scale.ShowAll ();

			Gtk.Image image = Gtk.Image.LoadFromResource ("floppybuddy.gif");
			textView.AddChildAtAnchor (image, animationAnchor);
			image.ShowAll ();

			Entry entry = new Entry ();
			textView.AddChildAtAnchor (entry, entryAnchor);
			entry.ShowAll ();
		}

		const int gray50_width = 2;
		const int gray50_height = 2;
		const string gray50_bits = "\x02\x01";

		private void CreateTags (TextBuffer buffer)
		{
			// Create a bunch of tags. Note that it's also possible to
			// create tags with gtk_text_tag_new() then add them to the
			// tag table for the buffer, gtk_text_buffer_create_tag() is
			// just a convenience function. Also note that you don't have
			// to give tags a name; pass NULL for the name to create an
			// anonymous tag.
			//
			// In any real app, another useful optimization would be to create
			// a GtkTextTagTable in advance, and reuse the same tag table for
			// all the buffers with the same tag set, instead of creating
			// new copies of the same tags for every buffer.
			//
			// Tags are assigned default priorities in order of addition to the
			// tag table.	 That is, tags created later that affect the same text
			// property affected by an earlier tag will override the earlier
			// tag.  You can modify tag priorities with
			// gtk_text_tag_set_priority().

			TextTag tag  = new TextTag ("heading");
			tag.Weight = Pango.Weight.Bold;
			tag.Size = (int) Pango.Scale.PangoScale * 15;
			buffer.TagTable.Add (tag);

			tag  = new TextTag ("italic");
			tag.Style = Pango.Style.Italic;
			buffer.TagTable.Add (tag);

			tag  = new TextTag ("bold");
			tag.Weight = Pango.Weight.Bold;
			buffer.TagTable.Add (tag);

			tag  = new TextTag ("big");
			tag.Size = (int) Pango.Scale.PangoScale * 20;
			buffer.TagTable.Add (tag);

			tag  = new TextTag ("xx-small");
			tag.Scale = Pango.Scale.XXSmall;
			buffer.TagTable.Add (tag);

			tag  = new TextTag ("x-large");
			tag.Scale = Pango.Scale.XLarge;
			buffer.TagTable.Add (tag);

			tag  = new TextTag ("monospace");
			tag.Family = "monospace";
			buffer.TagTable.Add (tag);

			tag  = new TextTag ("blue_foreground");
			tag.Foreground = "blue";
			buffer.TagTable.Add (tag);

			tag  = new TextTag ("red_background");
			tag.Background = "red";
			buffer.TagTable.Add (tag);

			// The C gtk-demo passes NULL for the drawable param, which isn't
			// multi-head safe, so it seems bad to allow it in the C# API.
			// But the Window isn't realized at this point, so we can't get
			// an actual Drawable from it. So we kludge for now.
			Pixmap stipple = Pixmap.CreateBitmapFromData (Gdk.Screen.Default.RootWindow, gray50_bits, gray50_width, gray50_height);

			tag  = new TextTag ("background_stipple");
			tag.BackgroundStipple = stipple;
			buffer.TagTable.Add (tag);

			tag  = new TextTag ("foreground_stipple");
			tag.ForegroundStipple = stipple;
			buffer.TagTable.Add (tag);

			tag  = new TextTag ("big_gap_before_line");
			tag.PixelsAboveLines = 30;
			buffer.TagTable.Add (tag);

			tag  = new TextTag ("big_gap_after_line");
			tag.PixelsBelowLines = 30;
			buffer.TagTable.Add (tag);

			tag  = new TextTag ("double_spaced_line");
			tag.PixelsInsideWrap = 10;
			buffer.TagTable.Add (tag);

			tag  = new TextTag ("not_editable");
			tag.Editable = false;
			buffer.TagTable.Add (tag);

			tag  = new TextTag ("word_wrap");
			tag.WrapMode = WrapMode.Word;
			buffer.TagTable.Add (tag);

			tag  = new TextTag ("char_wrap");
			tag.WrapMode = WrapMode.Char;
			buffer.TagTable.Add (tag);

			tag  = new TextTag ("no_wrap");
			tag.WrapMode = WrapMode.None;
			buffer.TagTable.Add (tag);

			tag  = new TextTag ("center");
			tag.Justification = Justification.Center;
			buffer.TagTable.Add (tag);

			tag  = new TextTag ("right_justify");
			tag.Justification = Justification.Right;
			buffer.TagTable.Add (tag);

			tag  = new TextTag ("wide_margins");
			tag.LeftMargin = 50;
			tag.RightMargin = 50;
			buffer.TagTable.Add (tag);

			tag  = new TextTag ("strikethrough");
			tag.Strikethrough = true;
			buffer.TagTable.Add (tag);

			tag  = new TextTag ("underline");
			tag.Underline = Pango.Underline.Single;
			buffer.TagTable.Add (tag);

			tag  = new TextTag ("double_underline");
			tag.Underline = Pango.Underline.Double;
			buffer.TagTable.Add (tag);

			tag  = new TextTag ("superscript");
			tag.Rise = (int) Pango.Scale.PangoScale * 10;
			tag.Size = (int) Pango.Scale.PangoScale * 8;
			buffer.TagTable.Add (tag);

			tag  = new TextTag ("subscript");
			tag.Rise = (int) Pango.Scale.PangoScale * -10;
			tag.Size = (int) Pango.Scale.PangoScale * 8;
			buffer.TagTable.Add (tag);

			tag  = new TextTag ("rtl_quote");
			tag.WrapMode = WrapMode.Word;
			tag.Direction = TextDirection.Rtl;
			tag.Indent = 30;
			tag.LeftMargin = 20;
			tag.RightMargin = 20;
			buffer.TagTable.Add (tag);
		}

		private void InsertText (TextBuffer buffer)
		{
			Pixbuf pixbuf = Gdk.Pixbuf.LoadFromResource ("gtk-logo-rgb.gif");
			pixbuf = pixbuf.ScaleSimple (32, 32, InterpType.Bilinear);

			// get start of buffer; each insertion will revalidate the
			// iterator to point to just after the inserted text.

			TextIter insertIter = buffer.StartIter;
			buffer.Insert (ref insertIter,
				       "The text widget can display text with all kinds of nifty attributes. It also supports multiple views of the same buffer; this demo is showing the same buffer in two places.\n\n");

			buffer.InsertWithTagsByName (ref insertIter, "Font styles. ", "heading");

			buffer.Insert (ref insertIter, "For example, you can have ");
			buffer.InsertWithTagsByName (ref insertIter, "italic", "italic");
		        buffer.Insert (ref insertIter, ", ");
			buffer.InsertWithTagsByName (ref insertIter, "bold", "bold");
		        buffer.Insert (ref insertIter, ", or ");
			buffer.InsertWithTagsByName (ref insertIter, "monospace (typewriter)", "monospace");
		        buffer.Insert (ref insertIter, ", or  ");
			buffer.InsertWithTagsByName (ref insertIter, "big", "big");
			buffer.Insert (ref insertIter, " text. ");
			buffer.Insert (ref insertIter,
				       "It's best not to hardcode specific text sizes; you can use relative sizes as with CSS, such as ");
			buffer.InsertWithTagsByName (ref insertIter, "xx-small", "xx-small");
		        buffer.Insert (ref insertIter, " or");
			buffer.InsertWithTagsByName (ref insertIter, "x-large", "x-large");
			buffer.Insert (ref insertIter,
				       " to ensure that your program properly adapts if the user changes the default font size.\n\n");

			buffer.InsertWithTagsByName (ref insertIter, "Colors. ", "heading");

			buffer.Insert (ref insertIter, "Colors such as ");
			buffer.InsertWithTagsByName (ref insertIter, "a blue foreground", "blue_foreground");
		        buffer.Insert (ref insertIter, " or ");
			buffer.InsertWithTagsByName (ref insertIter, "a red background", "red_background");
		        buffer.Insert (ref insertIter, " or even ");
			buffer.InsertWithTagsByName (ref insertIter, "a stippled red background",
						     "red_background",
						     "background_stipple");

		        buffer.Insert (ref insertIter, " or ");
                        buffer.InsertWithTagsByName (ref insertIter,
						     "a stippled blue foreground on solid red background",
						     "blue_foreground",
						     "red_background",
						     "foreground_stipple");
		        buffer.Insert (ref insertIter, " (select that to read it) can be used.\n\n");

			buffer.InsertWithTagsByName (ref insertIter, "Underline, strikethrough, and rise. ", "heading");

			buffer.InsertWithTagsByName (ref insertIter, "Strikethrough", "strikethrough");
			buffer.Insert (ref insertIter, ", ");
			buffer.InsertWithTagsByName (ref insertIter, "underline", "underline");
			buffer.Insert (ref insertIter, ", ");
			buffer.InsertWithTagsByName (ref insertIter, "double underline", "double_underline");
			buffer.Insert (ref insertIter, ", ");
			buffer.InsertWithTagsByName (ref insertIter, "superscript", "superscript");
		        buffer.Insert (ref insertIter, ", and ");
			buffer.InsertWithTagsByName (ref insertIter, "subscript", "subscript");
			buffer.Insert (ref insertIter, " are all supported.\n\n");

			buffer.InsertWithTagsByName (ref insertIter, "Images. ", "heading");

			buffer.Insert (ref insertIter, "The buffer can have images in it: ");

			buffer.InsertPixbuf (ref insertIter, pixbuf);
			buffer.InsertPixbuf (ref insertIter, pixbuf);
			buffer.InsertPixbuf (ref insertIter, pixbuf);
			buffer.Insert (ref insertIter, " for example.\n\n");

			buffer.InsertWithTagsByName (ref insertIter, "Spacing. ", "heading");

			buffer.Insert (ref insertIter, "You can adjust the amount of space before each line.\n");
			buffer.InsertWithTagsByName (ref insertIter, "This line has a whole lot of space before it.\n",
						     "big_gap_before_line", "wide_margins");
			buffer.InsertWithTagsByName (ref insertIter, "You can also adjust the amount of space after each line; this line has a whole lot of space after it.\n",
						     "big_gap_after_line", "wide_margins");

			buffer.InsertWithTagsByName (ref insertIter, "You can also adjust the amount of space between wrapped lines; this line has extra space between each wrapped line in the same paragraph. To show off wrapping, some filler text: the quick brown fox jumped over the lazy dog. Blah blah blah blah blah blah blah blah blah.\n",
						     "double_spaced_line", "wide_margins");

			buffer.Insert (ref insertIter, "Also note that those lines have extra-wide margins.\n\n");

			buffer.InsertWithTagsByName (ref insertIter, "Editability. ", "heading");

			buffer.InsertWithTagsByName (ref insertIter, "This line is 'locked down' and can't be edited by the user - just try it! You can't delete this line.\n\n",
						     "not_editable");

			buffer.InsertWithTagsByName (ref insertIter, "Wrapping. ", "heading");

			buffer.Insert (ref insertIter, "This line (and most of the others in this buffer) is word-wrapped, using the proper Unicode algorithm. Word wrap should work in all scripts and languages that GTK+ supports. Let's make this a long paragraph to demonstrate: blah blah blah blah blah blah blah blah blah blah blah blah blah blah blah blah blah blah blah\n\n");

			buffer.InsertWithTagsByName (ref insertIter,  "This line has character-based wrapping, and can wrap between any two character glyphs. Let's make this a long paragraph to demonstrate: blah blah blah blah blah blah blah blah blah blah blah blah blah blah blah blah blah blah blah\n\n",
						     "char_wrap");

			buffer.InsertWithTagsByName (ref insertIter, "This line has all wrapping turned off, so it makes the horizontal scrollbar appear.\n\n\n",
						     "no_wrap");

			buffer.InsertWithTagsByName (ref insertIter, "Justification. ", "heading");


			buffer.InsertWithTagsByName (ref insertIter, "\nThis line has center justification.\n", "center");

			buffer.InsertWithTagsByName (ref insertIter, "This line has right justification.\n", "right_justify");

			buffer.InsertWithTagsByName (ref insertIter, "\nThis line has big wide margins. Text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text.\n",
						     "wide_margins");

			buffer.InsertWithTagsByName (ref insertIter, "Internationalization. ", "heading");

			buffer.Insert (ref insertIter, "You can put all sorts of Unicode text in the buffer.\n\nGerman (Deutsch S\u00fcd) Gr\u00fc\u00df Gott\nGreek (\u0395\u03bb\u03bb\u03b7\u03bd\u03b9\u03ba\u03ac) \u0393\u03b5\u03b9\u03ac \u03c3\u03b1\u03c2\nHebrew	\u05e9\u05dc\u05d5\u05dd\nJapanese (\u65e5\u672c\u8a9e)\n\nThe widget properly handles bidirectional text, word wrapping, DOS/UNIX/Unicode paragraph separators, grapheme boundaries, and so on using the Pango internationalization framework.\n");

			buffer.Insert (ref insertIter, "Here's a word-wrapped quote in a right-to-left language:\n");
			buffer.InsertWithTagsByName (ref insertIter,  "\u0648\u0642\u062f \u0628\u062f\u0623 \u062b\u0644\u0627\u062b \u0645\u0646 \u0623\u0643\u062b\u0631 \u0627\u0644\u0645\u0624\u0633\u0633\u0627\u062a \u062a\u0642\u062f\u0645\u0627 \u0641\u064a \u0634\u0628\u0643\u0629 \u0627\u0643\u0633\u064a\u0648\u0646 \u0628\u0631\u0627\u0645\u062c\u0647\u0627 \u0643\u0645\u0646\u0638\u0645\u0627\u062a \u0644\u0627 \u062a\u0633\u0639\u0649 \u0644\u0644\u0631\u0628\u062d\u060c \u062b\u0645 \u062a\u062d\u0648\u0644\u062a \u0641\u064a \u0627\u0644\u0633\u0646\u0648\u0627\u062a \u0627\u0644\u062e\u0645\u0633 \u0627\u0644\u0645\u0627\u0636\u064a\u0629 \u0625\u0644\u0649 \u0645\u0624\u0633\u0633\u0627\u062a \u0645\u0627\u0644\u064a\u0629 \u0645\u0646\u0638\u0645\u0629\u060c \u0648\u0628\u0627\u062a\u062a \u062c\u0632\u0621\u0627 \u0645\u0646 \u0627\u0644\u0646\u0638\u0627\u0645 \u0627\u0644\u0645\u0627\u0644\u064a \u0641\u064a \u0628\u0644\u062f\u0627\u0646\u0647\u0627\u060c \u0648\u0644\u0643\u0646\u0647\u0627 \u062a\u062a\u062e\u0635\u0635 \u0641\u064a \u062e\u062f\u0645\u0629 \u0642\u0637\u0627\u0639 \u0627\u0644\u0645\u0634\u0631\u0648\u0639\u0627\u062a \u0627\u0644\u0635\u063a\u064a\u0631\u0629\u002e \u0648\u0623\u062d\u062f \u0623\u0643\u062b\u0631 \u0647\u0630\u0647 \u0627\u0644\u0645\u0624\u0633\u0633\u0627\u062a \u0646\u062c\u0627\u062d\u0627 \u0647\u0648 \u00bb\u0628\u0627\u0646\u0643\u0648\u0633\u0648\u0644\u00ab \u0641\u064a \u0628\u0648\u0644\u064a\u0641\u064a\u0627.\n\n", "rtl_quote");

			buffer.Insert (ref insertIter, "You can put widgets in the buffer: Here's a button: ");
			buttonAnchor = buffer.CreateChildAnchor (ref insertIter);
			buffer.Insert (ref insertIter, " and a menu: ");
		        menuAnchor = buffer.CreateChildAnchor (ref insertIter);
			buffer.Insert (ref insertIter, " and a scale: ");
			scaleAnchor = buffer.CreateChildAnchor (ref insertIter);
			buffer.Insert (ref insertIter, " and an animation: ");
			animationAnchor	= buffer.CreateChildAnchor (ref insertIter);
			buffer.Insert (ref insertIter, " finally a text entry: ");
			entryAnchor = buffer.CreateChildAnchor (ref insertIter);
			buffer.Insert (ref insertIter, ".\n");

 			buffer.Insert (ref insertIter, "\n\nThis demo doesn't demonstrate all the GtkTextBuffer features; it leaves out, for example: invisible/hidden text (doesn't work in GTK 2, but planned), tab stops, application-drawn areas on the sides of the widget for displaying breakpoints and such...");

			buffer.ApplyTag ("word_wrap", buffer.StartIter, buffer.EndIter);
		}

  		protected override bool OnDeleteEvent (Gdk.Event evt)
		{
			Destroy ();
			return true;
		}

		private void RecursiveAttach (int depth, TextView view, TextChildAnchor anchor)
		{
			if (depth > 4)
				return;

			TextView childView = new TextView (view.Buffer);

			// Event box is to add a black border around each child view
			EventBox eventBox = new EventBox ();
			Gdk.Color color = new Gdk.Color ();
			Gdk.Color.Parse ("black", ref color);
			eventBox.ModifyBg (StateType.Normal, color);

			Alignment align = new Alignment (0.5f, 0.5f, 1.0f, 1.0f);
			align.BorderWidth = 1;

			eventBox.Add (align);
			align.Add (childView);

			view.AddChildAtAnchor (eventBox, anchor);

			RecursiveAttach (depth+1, childView, anchor);
		}

		private void EasterEggCB (object o, EventArgs args)
		{
			TextIter insertIter;

			TextBuffer buffer = new TextBuffer (null);
			insertIter = buffer.StartIter;
			buffer.Insert (ref insertIter, "This buffer is shared by a set of nested text views.\n Nested view:\n");
			TextChildAnchor anchor = buffer.CreateChildAnchor (ref insertIter);
			buffer.Insert (ref insertIter, "\nDon't do this in real applications, please.\n");
			TextView view = new TextView (buffer);

			RecursiveAttach (0, view, anchor);

			Gtk.Window window = new Gtk.Window (Gtk.WindowType.Toplevel);
			ScrolledWindow sw = new ScrolledWindow ();
			sw.SetPolicy (PolicyType.Automatic, PolicyType.Automatic);

			window.Add (sw);
			sw.Add (view);

			window.SetDefaultSize (300, 400);
			window.ShowAll ();
		}
	}
}
