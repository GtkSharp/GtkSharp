// GTK.Label.cs - GTK Label class implementation
//
// Author: Bob Smith <bob@thestuff.net>
//
// (c) 2001 Bob Smith

namespace GTK {

	using System;
	using System.Runtime.InteropServices;

	public class Label : Widget {

		/// <summary>
		///	Label Object Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a Label Wrapper.
		/// </remarks>

		public Label (IntPtr o)
		{
			Object = o;
		}

		/// <summary>
		///	Label Constructor
		/// </summary>
		/// 
		/// <remarks>
		///	Constructs a new Label with the specified content.
		/// </remarks>

		[DllImport("gtk-1.3")]
		static extern IntPtr gtk_label_new (String str);

		public Label (String str)
		{
			Object = gtk_label_new (str);
		}

		/// <summary>
		///	Text Property
		/// </summary>
		/// 
		/// <remarks>
		///	The raw text of the label.
		/// </remarks>

		[DllImport("gtk-1.3")]
		static extern void gtk_label_set_text (IntPtr hnd, const String str);
		[DllImport("gtk-1.3")]
		static extern String gtk_label_get_text (IntPtr hnd);

		public String Text {
			get
			{
				return gtk_label_get_text (Object);
			}
			set
			{
				gtk_label_set_text (Object, value);
			}
		}

		/// <summary>
		///	Markup Property
		/// </summary>
		/// 
		/// <remarks>
		///	Text to parse.
		/// </remarks>

		[DllImport("gtk-1.3")]
		static extern void gtk_label_set_markup (IntPtr hnd, const String str);

		public String Markup {
			set
			{
				gtk_label_set_markup (Object, value);
			}
		}

		/// <summary>
		///	Label Property
		/// </summary>
		/// 
		/// <remarks>
		///	Parsed content.
		/// </remarks>

		[DllImport("gtk-1.3")]
		static extern void gtk_label_set_label (IntPtr hnd, const String str);
		[DllImport("gtk-1.3")]
		static extern String gtk_label_get_label (IntPtr hnd);

		public String Label {
			get
			{
				return gtk_label_get_label (Object);
			}
			set
			{
				gtk_label_set_label (Object, value);
			}
		}

		/// <summary>
		///	Selectable Property
		/// </summary>
		/// 
		/// <remarks>
		///	Is the user able to select text from the label.
		/// </remarks>

		[DllImport("gtk-1.3")]
		static extern void gtk_label_set_selectable (IntPtr hnd, bool setting);
		[DllImport("gtk-1.3")]
		static extern bool gtk_label_get_selectable (IntPtr hnd);

		public bool Selectable {
			get
			{
				return gtk_label_get_selectable (Object);
			}
			set
			{
				gtk_label_set_selectable (Object, value, value);
			}
		}

		/// <summary>
		///	UseUnderline Property
		/// </summary>
		/// 
		/// <remarks>
		///	Indicates that the next character after an underline should be the accelerator key.
		/// </remarks>

		[DllImport("gtk-1.3")]
		static extern void gtk_label_set_use_underline (IntPtr hnd, bool setting);
		[DllImport("gtk-1.3")]
		static extern bool gtk_label_get_use_underline (IntPtr hnd);

		public bool UseUnderline {
			get
			{
				return gtk_label_get_use_underline (Object);
			}
			set
			{
				gtk_label_set_use_underline (Object, value, value);
			}
		}

		/// <summary>
		///	UseMarkup Property
		/// </summary>
		/// 
		/// <remarks>
		///	Indicates that the text contains markup.
		/// </remarks>

		[DllImport("gtk-1.3")]
		static extern void gtk_label_set_use_markup (IntPtr hnd, bool setting);
		[DllImport("gtk-1.3")]
		static extern bool gtk_label_get_use_markup (IntPtr hnd);

		public bool UseMarkup {
			get
			{
				return gtk_label_get_use_markup (Object);
			}
			set
			{
				gtk_label_set_use_markup (Object, value, value);
			}
		}

		/// <summary>
		///	LineWrap Property
		/// </summary>
		/// 
		/// <remarks>
		///	Indicates that the text is automatically wrapped if to long.
		/// </remarks>

		[DllImport("gtk-1.3")]
		static extern void gtk_label_set_line_wrap (IntPtr hnd, bool setting);
		[DllImport("gtk-1.3")]
		static extern bool gtk_label_get_line_wrap (IntPtr hnd);

		public bool LineWrap {
			get
			{
				return gtk_label_get_line_wrap (Object);
			}
			set
			{
				gtk_label_set_line_wrap (Object, value, value);
			}
		}


/*
TODO:

void gtk_label_set_attributes(GtkLabel *label, PangoAttrList *attrs);
void gtk_label_set_markup_with_mnemonic(GtkLabel *label, const gchar *str);
void gtk_label_set_pattern(GtkLabel *label, const gchar *pattern);
void gtk_label_set_justify(GtkLabel *label, GtkJustification jtype);
guint gtk_label_parse_uline(GtkLabel *label, const gchar *string);
void        gtk_label_get_layout_offsets (GtkLabel *label,
                                             gint *x,
                                             gint *y);
guint gtk_label_get_mnemonic_keyval (GtkLabel *label);
GtkWidget*  gtk_label_new_with_mnemonic (const char *str);
void        gtk_label_select_region (GtkLabel *label,
                                             gint start_offset,
                                             gint end_offset);
void        gtk_label_set_mnemonic_widget (GtkLabel *label,
                                             GtkWidget *widget);
void        gtk_label_set_text_with_mnemonic
(GtkLabel *label,
                                             const gchar *str);
PangoAttrList* gtk_label_get_attributes (GtkLabel *label);
GtkJustification gtk_label_get_justify (GtkLabel *label);
PangoLayout* gtk_label_get_layout (GtkLabel *label);

GtkWidget*  gtk_label_get_mnemonic_widget (GtkLabel *label);
gboolean gtk_label_get_selection_bounds (GtkLabel *label,
                                             gint *start,
                                             gint *end);

*/


	}
}
