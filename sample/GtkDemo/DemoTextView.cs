//
// DemoTextView.cs, port of textview.c form gtk-demo
//
// Author: Daniel Kornhauser <dkor@alum.mit.edu>
//
// (C) 2003 Ximian, Inc.

/* Text Widget
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
using GtkSharp;

namespace GtkDemo
{
	public class DemoTextView
	{
		private Gtk.Window window;
		TextView view1;
		TextView view2;
		public DemoTextView ()
		{	
			window = new Gtk.Window ("TextView Demo");
			window.DeleteEvent += new DeleteEventHandler (WindowDelete);
			window.SetDefaultSize (450,450);
			window.BorderWidth = 0;

			VPaned vpaned = new VPaned ();
			vpaned.BorderWidth = 5;
			window.Add (vpaned);

			/* For convenience, we just use the autocreated buffer from
			 * the first text view; you could also create the buffer
			 * by itself with new Gtk.TextBuffer (), then later create
			 * a view widget.
			 */
			view1 = new TextView ();
			TextBuffer buffer = view1.Buffer;
			view2 = new TextView(buffer);

			ScrolledWindow scrolledWindow = new  ScrolledWindow ();
			scrolledWindow.SetPolicy (PolicyType.Automatic, Gtk.PolicyType.Automatic);
			vpaned.Add1 (scrolledWindow);
			scrolledWindow.Add (view1);

			scrolledWindow = new  Gtk.ScrolledWindow ();
			scrolledWindow.SetPolicy (Gtk.PolicyType.Automatic, Gtk.PolicyType.Automatic);
			vpaned.Add2 (scrolledWindow);
			scrolledWindow.Add (view2);

			CreateTags (buffer);
			InsertText(buffer);

			AttachWidgets(view1);
			AttachWidgets(view2);

			vpaned.ShowAll();

			window.ShowAll ();

		}
		private TextChildAnchor buttonAnchor;
		private TextChildAnchor menuAnchor;
		private TextChildAnchor scaleAnchor;
		private TextChildAnchor animationAnchor;
		private TextChildAnchor entryAnchor;
		private void AttachWidgets (TextView textView)
		{
			Button button = new Button ("Click Me");
			button.Clicked +=  new EventHandler(EasterEggCB);
			textView.AddChildAtAnchor (button, buttonAnchor);
			button.ShowAll ();

			OptionMenu option = new OptionMenu ();
			Menu menu = new Menu ();
			MenuItem menuItem = new MenuItem ("Option 1");
 			menu.Append(menuItem);
 			menuItem = new MenuItem ("Option 2");
 			menu.Append(menuItem);
 			menuItem = new MenuItem ("Option 3");
 			menu.Append(menuItem);
			option.Menu = menu;
 			textView.AddChildAtAnchor (option, menuAnchor);
			menu.ShowAll ();

                        HScale scale = new HScale (null);
			scale.SetRange (0,100);
                        scale.SetSizeRequest (70, -1);
			textView.AddChildAtAnchor (scale, scaleAnchor);
			scale.ShowAll ();

			Gtk.Image image = new Gtk.Image ("images/floppybuddy.gif");
			textView.AddChildAtAnchor (image, animationAnchor);
			image.ShowAll ();

			Entry entry = new Entry ();
			textView.AddChildAtAnchor (entry, entryAnchor);
			image.ShowAll ();

		}

		private void CreateTags (TextBuffer buffer)
		{
		/* Create a bunch of tags. Note that it's also possible to
		 * create tags with gtk_text_tag_new() then add them to the
		 * tag table for the buffer, gtk_text_buffer_create_tag() is
		 * just a convenience function. Also note that you don't have
		 * to give tags a name; pass NULL for the name to create an
		 * anonymous tag.
		 *
		 * In any real app, another useful optimization would be to create
		 * a GtkTextTagTable in advance, and reuse the same tag table for
		 * all the buffers with the same tag set, instead of creating
		 * new copies of the same tags for every buffer.
		 *
		 * Tags are assigned default priorities in order of addition to the
		 * tag table.	 That is, tags created later that affect the same text
		 * property affected by an earlier tag will override the earlier
		 * tag.  You can modify tag priorities with
		 * gtk_text_tag_set_priority().
		 */

			TextTag tag  = new TextTag("heading");
			tag.FontDesc.Weight = Pango.Weight.Bold;
			tag.Size = (int) Pango.Scale.PangoScale * 15;
			buffer.TagTable.Add(tag);

			tag  = new TextTag("italic");
			tag.Style = Pango.Style.Italic;
			buffer.TagTable.Add(tag);

			tag  = new TextTag("bold");
			tag.FontDesc.Weight = Pango.Weight.Bold;
			buffer.TagTable.Add(tag);

			tag  = new TextTag("big");
			tag.Size = (int) Pango.Scale.PangoScale * 20;
			buffer.TagTable.Add(tag);

			tag  = new TextTag("xx-small");
			tag.Scale = Pango.Scale.XX_Small; 
			buffer.TagTable.Add(tag);

			tag  = new TextTag("x-large");
			tag.Scale = Pango.Scale.X_Large; 
			buffer.TagTable.Add(tag);

			tag  = new TextTag("monospace");
			tag.Family = "monospace";
			buffer.TagTable.Add(tag);

			tag  = new TextTag("blue_foreground");
			tag.Foreground = "blue";
			buffer.TagTable.Add(tag);

			tag  = new TextTag("red_background");
			tag.Background = "red";
			buffer.TagTable.Add(tag);

			int gray50_width = 2;
			int gray50_height = 2;
			string gray50_bits = new  string ((char) 0x02, (char) 0x01);
			// Pixmap stipple = Pixmap.CreateFromData (null, (string) gray50_bits, gray50_width, gray50_height, 16, Color.Zero, Color.Zero);
			
			tag  = new TextTag("background_stipple");
			// tag.BackgroundStipple = stipple;
			// Cannot convert type 'Gdk.Bitmap' to 'Gdk.Pixmap'
			buffer.TagTable.Add(tag);

			tag  = new TextTag("foreground_stipple");
			// Cannot convert type 'Gdk.Bitmap' to 'Gdk.Pixmap'
			// tag.ForegroundStipple = stipple;			
			buffer.TagTable.Add(tag);

			tag  = new TextTag("big_gap_before_line");
			tag.PixelsAboveLines = 30;
			buffer.TagTable.Add(tag);

			tag  = new TextTag("big_gap_after_line");
			tag.PixelsBelowLines = 30;
			buffer.TagTable.Add(tag);

			tag  = new TextTag("double_spaced_line");
			tag.PixelsInsideWrap = 10;
			buffer.TagTable.Add(tag);

			tag  = new TextTag("not_editable");
			tag.Editable = false;
			buffer.TagTable.Add(tag);

			tag  = new TextTag("word_wrap");
			tag.WrapMode = WrapMode.Word;
			buffer.TagTable.Add(tag);

			tag  = new TextTag("char_wrap");
			tag.WrapMode = WrapMode.Char;
			buffer.TagTable.Add(tag);

			tag  = new TextTag("no_wrap");
			tag.WrapMode = WrapMode.None;
			buffer.TagTable.Add(tag);

			tag  = new TextTag("center");
			tag.Justification = Justification.Center;
			buffer.TagTable.Add(tag);

			tag  = new TextTag("right_justify");
			tag.Justification = Justification.Right;
			buffer.TagTable.Add(tag);

			tag  = new TextTag("wide_margins");
			tag.LeftMargin = 50;
			tag.RightMargin = 50;
			buffer.TagTable.Add(tag);

			tag  = new TextTag("strikethrough");
			tag.Strikethrough = true;
			buffer.TagTable.Add(tag);

			tag  = new TextTag("underline");
			tag.Underline = Pango.Underline.Single;
			buffer.TagTable.Add(tag);

			tag  = new TextTag("double_underline");
			tag.Underline = Pango.Underline.Double;
			buffer.TagTable.Add(tag);

			tag  = new TextTag("superscript");
			tag.Rise = (int) Pango.Scale.PangoScale * 10;
			tag.Size = (int) Pango.Scale.PangoScale * 8;
			buffer.TagTable.Add(tag);

			tag  = new TextTag("subscript");
			tag.Rise = (int) Pango.Scale.PangoScale * -10;
			tag.Size = (int) Pango.Scale.PangoScale * 8;
			buffer.TagTable.Add(tag);

			tag  = new TextTag("rtl_quote");
			tag.WrapMode = WrapMode.Word;
			tag.Direction = TextDirection.Rtl;
			tag.Indent = 30;
			tag.LeftMargin = 20; 
			tag.RightMargin = 20;
			buffer.TagTable.Add(tag);
		}  

		private void InsertText (TextBuffer buffer)
		{
			
		/* demo_find_file() looks in the the current directory first,
		 * so you can run gtk-demo without installing GTK, then looks
		 * in the location where the file is installed.
		 */
			
			// Error handling here, check for file existence, etc.
			Pixbuf pixbuf = null;
			
			if (File.Exists ("images/gtk-logo-rgb.gif"))
			{
				pixbuf = new Pixbuf ("images/gtk-logo-rgb.gif");
				pixbuf.ScaleSimple (32, 32, InterpType.Bilinear);
			}
		/* get start of buffer; each insertion will revalidate the
		 * iterator to point to just after the inserted text.
		 */

			TextIter insertIter;

			insertIter = buffer.GetIterAtOffset (0);
			buffer.Insert(insertIter,
			"The text widget can display text with all kinds of nifty attributes.It also supports multiple views of the same buffer; this demo is showing the same buffer in two places.\n\n");

			InsertWithTagsByName (buffer, "Font styles. ", new string[] {"heading"});
			Insert (buffer, "For example, you can have ");
			InsertWithTagsByName (buffer, "italic", new string[] {"italic"});
		        Insert (buffer, ", ");
			InsertWithTagsByName (buffer, "bold", new string[] {"bold"});
		        Insert (buffer, ", or ");
			InsertWithTagsByName (buffer, "monospace (typewriter)", new string[] {"monospace"});
		        Insert (buffer, ", or  ");
			InsertWithTagsByName (buffer, "big", new string[] {"big"});
			Insert (buffer, " text");
			Insert (buffer,
			"It's best not to hardcode specific text sizes; you can use relative sizes as with CSS, such as ");
			InsertWithTagsByName (buffer, "xx-small", new string[] {"xx-small"});
		        Insert (buffer, ", or");
			InsertWithTagsByName (buffer, "x-large", new string[] {"x-large"});
			Insert (buffer,
			" to ensure that your program properly adapts if the user changes the default font size.\n\n");
			InsertWithTagsByName (buffer, "Colors such as", new string[] {"heading"});
			InsertWithTagsByName (buffer, "a blue foreground", new string[] {"blue_foreground"});
		        Insert (buffer, ", or  ");
			InsertWithTagsByName (buffer, "a red background", new string[] {"red_background"});
		        Insert (buffer, " or  even ");
			// Change InsertWithTagsByName to work with 2 and 3 args
			// InsertWithTagsByName ("a stippled red background",
			//		    "red_background",
			//		    "background_stipple");
		        //Insert (buffer, ", or  ");
                        //InsertWithTagsByName ("a stippled red background",
			//		"a stippled blue foreground on solid red background", -1,
			//		"blue_foreground",
			//		"red_background",
			//		"foreground_stipple")
		        Insert (buffer, " (select that to read it) can be used.\n\n");
			InsertWithTagsByName (buffer, "Underline, strikethrough, and rise. ", new string[] {"heading"});
			InsertWithTagsByName (buffer, "Strikethrough", new string[] {"strikethrough"});
			Insert (buffer, ", ");
			InsertWithTagsByName (buffer, "underline", new string[] {"underline"});
			Insert (buffer, ", ");
			InsertWithTagsByName (buffer, "double_underline", new string[] {"double_underline"});
			Insert (buffer, ", ");
			InsertWithTagsByName (buffer, "superscript", new string[] {"superscript"});
		        Insert (buffer, ", and  ");
			InsertWithTagsByName (buffer, "subscript", new string[] {"subscript"});
			Insert (buffer," are all supported.\n\n");
			InsertWithTagsByName (buffer, "Images. ", new string[] {"heading"});
			Insert (buffer,"The buffer can have images in it: ");
			
			buffer.GetIterAtMark (out insertIter, buffer.InsertMark);
			buffer.InsertPixbuf (insertIter, pixbuf);
			buffer.GetIterAtMark (out insertIter, buffer.InsertMark);
			buffer.InsertPixbuf (insertIter, pixbuf);
			buffer.GetIterAtMark (out insertIter, buffer.InsertMark);
			buffer.InsertPixbuf (insertIter, pixbuf);

			Insert (buffer, " for example.\n\n");
			InsertWithTagsByName (buffer, "Spacing. ", new string[] {"heading"});
			InsertWithTagsByName (buffer, "You can adjust the amount of space before each line.\n", new string[] {"big_gap_before_line", "wide_margins"});
			InsertWithTagsByName (buffer, "You can also adjust the amount of space after each line; this line has a whole lot of space after it.\n", new string[] {"big_gap_after_line", "wide_margins"});
			InsertWithTagsByName (buffer, "You can also adjust the amount of space between wrapped lines; this line has extra space between each wrapped line in the same paragraph. To show off wrapping, some filler text: the quick brown fox jumped over the lazy dog. Blah blah blah blah blah blah blah blah blah.\n", new string[] {"double_spaced_line", "wide_margins"});
			Insert (buffer, "Also note that those lines have extra-wide margins.\n\n");
			InsertWithTagsByName (buffer, "Editability. ", new string[] {"heading"});
			InsertWithTagsByName (buffer, "This line is 'locked down' and can't be edited by the user - just try it! You can't delete this line.\n\n", new string[] {"not_editable"});
			InsertWithTagsByName (buffer, "Wrapping. ", new string[] {"heading"});
			Insert (buffer,"This line (and most of the others in this buffer) is word-wrapped, using the proper Unicode algorithm. Word wrap should work in all scripts and languages that GTK+ supports. Let's make this a long paragraph to demonstrate: blah blah blah blah blah blah blah blah blah blah blah blah blah blah blah blah blah blah blah\n\n");
			InsertWithTagsByName (buffer,  "This line has character-based wrapping, and can wrap between any two character glyphs. Let's make this a long paragraph to demonstrate: blah blah blah blah blah blah blah blah blah blah blah blah blah blah blah blah blah blah blah\n\n", new string[] {"char_wrap"});
			InsertWithTagsByName (buffer, "This line has all wrapping turned off, so it makes the horizontal scrollbar appear.\n\n\n", new string[] {"no_wrap"});
			InsertWithTagsByName (buffer, "Justification. ", new string[] {"heading"});
			InsertWithTagsByName (buffer, "\nThis line has center justification.\n", new string[] {"center"});
			InsertWithTagsByName (buffer, "This line has right justification.\n", new string[] {"right_justify"});
			InsertWithTagsByName (buffer, "\nThis line has big wide margins. Text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text text.\n", new string[] {"wide_margins"});
			InsertWithTagsByName (buffer, "Internationalization. ", new string[] {"heading"});

			//Insert (buffer, "You can put all sorts of Unicode text in the buffer.\n\nGerman (Deutsch S\303\274d) Gr\303\274\303\237 Gott\nGreek (\316\225\316\273\316\273\316\267\316\275\316\271\316\272\316\254) \316\223\316\265\316\271\316\254 \317\203\316\261\317\202\nHebrew	\327\251\327\234\327\225\327\235\nJapanese (\346\227\245\346\234\254\350\252\236)\n\nThe widget properly handles bidirectional text, word wrapping, DOS/UNIX/Unicode paragraph separators, grapheme boundaries, and so on using the Pango internationalization framework.\n");
			Insert (buffer, "Here's a word-wrapped quote in a right-to-left language:\n");

			//InsertWithTagsByName (buffer,  "\331\210\331\202\330\257 \330\250\330\257\330\243 \330\253\331\204\330\247\330\253 \331\205\331\206 \330\243\331\203\330\253\330\261 \330\247\331\204\331\205\330\244\330\263\330\263\330\247\330\252 \330\252\331\202\330\257\331\205\330\247 \331\201\331\212 \330\264\330\250\331\203\330\251 \330\247\331\203\330\263\331\212\331\210\331\206 \330\250\330\261\330\247\331\205\330\254\331\207\330\247 \331\203\331\205\331\206\330\270\331\205\330\247\330\252 \331\204\330\247 \330\252\330\263\330\271\331\211 \331\204\331\204\330\261\330\250\330\255\330\214 \330\253\331\205 \330\252\330\255\331\210\331\204\330\252 \331\201\331\212 \330\247\331\204\330\263\331\206\331\210\330\247\330\252 \330\247\331\204\330\256\331\205\330\263 \330\247\331\204\331\205\330\247\330\266\331\212\330\251 \330\245\331\204\331\211 \331\205\330\244\330\263\330\263\330\247\330\252 \331\205\330\247\331\204\331\212\330\251 \331\205\331\206\330\270\331\205\330\251\330\214 \331\210\330\250\330\247\330\252\330\252 \330\254\330\262\330\241\330\247 \331\205\331\206 \330\247\331\204\331\206\330\270\330\247\331\205 \330\247\331\204\331\205\330\247\331\204\331\212 \331\201\331\212 \330\250\331\204\330\257\330\247\331\206\331\207\330\247\330\214 \331\210\331\204\331\203\331\206\331\207\330\247 \330\252\330\252\330\256\330\265\330\265 \331\201\331\212 \330\256\330\257\331\205\330\251 \331\202\330\267\330\247\330\271 \330\247\331\204\331\205\330\264\330\261\331\210\330\271\330\247\330\252 \330\247\331\204\330\265\330\272\331\212\330\261\330\251. \331\210\330\243\330\255\330\257 \330\243\331\203\330\253\330\261 \331\207\330\260\331\207 \330\247\331\204\331\205\330\244\330\263\330\263\330\247\330\252 \331\206\330\254\330\247\330\255\330\247 \331\207\331\210 \302\273\330\250\330\247\331\206\331\203\331\210\330\263\331\210\331\204\302\253 \331\201\331\212 \330\250\331\210\331\204\331\212\331\201\331\212\330\247.\n\n", new string[] {"rtl_quote"});
			//InsertWithTagsByName (buffer,  "\x2", new string[] {"rtl_quote"});
			Insert (buffer, "You can put widgets in the buffer: Here's a button: ");
			buffer.GetIterAtMark(out insertIter, buffer.InsertMark);
			buttonAnchor    = buffer.CreateChildAnchor (insertIter);

			Insert (buffer, "and a menu");
			buffer.GetIterAtMark(out insertIter, buffer.InsertMark);
		        menuAnchor      = buffer.CreateChildAnchor (insertIter);

			Insert (buffer, "and a scale");
			buffer.GetIterAtMark(out insertIter, buffer.InsertMark);
			scaleAnchor	= buffer.CreateChildAnchor (insertIter);

			Insert (buffer, "and an animation");
			buffer.GetIterAtMark(out insertIter, buffer.InsertMark);
			animationAnchor	= buffer.CreateChildAnchor (insertIter);

			Insert (buffer, " finally a text entry: ");
			buffer.GetIterAtMark(out insertIter, buffer.InsertMark);
			entryAnchor	= buffer.CreateChildAnchor (insertIter);

			Insert (buffer, "\n");

 			Insert (buffer, "\n\nThis demo doesn't demonstrate all the GtkTextBuffer features; it leaves out, for example: invisible/hidden text (doesn't work in GTK 2, but planned), tab stops, application-drawn areas on the sides of the widget for displaying breakpoints and such...");
			//Insert (buffer,);
			//InsertWithTagsByName (buffer, , new string[] {});

			buffer.ApplyTag("word_wrap", buffer.StartIter, buffer.EndIter);		
			
		}

		private void InsertWithTagsByName (TextBuffer buffer , string insertText, string[] fontName)
		{
			TextIter insertIter, beginIter, endIter;
			int begin, end;


			begin = buffer.CharCount;
			buffer.GetIterAtMark(out insertIter, buffer.InsertMark);
			buffer.Insert (insertIter, insertText);
			end = buffer.CharCount;
			foreach (string fontItem in fontName){
				buffer.GetIterAtOffset (out endIter, end);
				buffer.GetIterAtOffset (out beginIter, begin);
				buffer.ApplyTag (fontItem, beginIter, endIter);}		
		}

		private void Insert (TextBuffer buffer , string insertText)
		{
			TextIter insertIter;

			buffer.GetIterAtMark(out insertIter, buffer.InsertMark);
			buffer.Insert (insertIter, insertText);
		}

  		private void WindowDelete (object o, DeleteEventArgs args)
		{
			window.Hide ();
			window.Destroy ();
		}

		private void RecursiveAttach (int depth, TextView view, TextChildAnchor anchor) 
		{
			  if (depth > 4)
				  return;

			  TextView childView = new TextView (view.Buffer);
			  /* Event box is to add a black border around each child view */
			  EventBox eventBox = new EventBox ();
			  Gdk.Color blackColor = new Gdk.Color (0x0, 0x0, 0x0);
			  eventBox.ModifyBg(StateType.Normal,blackColor);
			  
			  Alignment align = new Alignment (0.5f, 0.5f, 1.0f, 1.0f);
			  align.BorderWidth = 1;

			  eventBox.Add (align);
			  align.Add (childView);

			  view.AddChildAtAnchor (eventBox, anchor);

			  RecursiveAttach(depth+1, childView, anchor);

		}
		
		private void EasterEggCB (object o, EventArgs args)
		{
			TextIter insertIter;

			TextBuffer bufferCB = new TextBuffer (null);
			Insert(bufferCB, "This buffer is shared by a set of nested text views.\n Nested view:\n");
			bufferCB.GetIterAtMark(out insertIter, bufferCB.InsertMark);
			TextChildAnchor anchor = bufferCB.CreateChildAnchor (insertIter);
			Insert(bufferCB, "\nDon't do this in real applications, please.\n");
			TextView viewCB = new TextView (bufferCB);
			
			RecursiveAttach(0, viewCB, anchor);

			Gtk.Window window = new Gtk.Window (null);
			ScrolledWindow scrolledWindow = new ScrolledWindow(null, null);
			scrolledWindow.SetPolicy (PolicyType.Automatic, PolicyType.Automatic);

			window.Add (scrolledWindow);
			scrolledWindow.Add (viewCB);
		
			window.SetDefaultSize (300, 400);
			window.ShowAll ();

		}
	}
}
