/* fileselection.c : Glue for accessing fields in the GtkFileSelection widget.
 *
 * Author: Duncan Mak  (duncan@ximian.com)
 *
 * (C) Ximian, INc.
 */

#include <gtk/gtkfilesel.h>

/* Forward declarations */
GtkWidget *gtksharp_file_selection_get_dir_list (GtkFileSelection *file);

GtkWidget *gtksharp_file_selection_get_file_list (GtkFileSelection *file);

GtkWidget *gtksharp_file_selection_get_selection_entry (GtkFileSelection *file);

GtkWidget *gtksharp_file_selection_get_selection_text (GtkFileSelection *file);

GtkWidget *gtksharp_file_selection_get_main_vbox (GtkFileSelection *file);

GtkWidget *gtksharp_file_selection_get_ok_button (GtkFileSelection *file);

GtkWidget *gtksharp_file_selection_get_cancel_button (GtkFileSelection *file);

GtkWidget *gtksharp_file_selection_get_help_button (GtkFileSelection *file);

GtkWidget *gtksharp_file_selection_get_history_pulldown (GtkFileSelection *file);

GtkWidget *gtksharp_file_selection_get_history_menu (GtkFileSelection *file);

GtkWidget *gtksharp_file_selection_get_fileop_dialog (GtkFileSelection *file);

GtkWidget *gtksharp_file_selection_get_fileop_entry (GtkFileSelection *file);

gchar     *gtksharp_file_selection_get_fileop_file (GtkFileSelection *file);

GtkWidget *gtksharp_file_selection_get_fileop_c_dir (GtkFileSelection *file);

GtkWidget *gtksharp_file_selection_get_fileop_del_file (GtkFileSelection *file);

GtkWidget *gtksharp_file_selection_get_fileop_ren_file (GtkFileSelection *file);

GtkWidget *gtksharp_file_selection_get_button_area (GtkFileSelection *file);

GtkWidget *gtksharp_file_selection_get_action_area (GtkFileSelection *file);
/* */

GtkWidget*
gtksharp_file_selection_get_dir_list (GtkFileSelection *file)
{
	return file->dir_list;
}

GtkWidget*
gtksharp_file_selection_get_file_list (GtkFileSelection *file)
{
	return file->file_list;
}

GtkWidget*
gtksharp_file_selection_get_selection_entry (GtkFileSelection *file)
{
	return file->selection_entry;
}

GtkWidget*
gtksharp_file_selection_get_selection_text (GtkFileSelection *file)
{
	return file->selection_text;
}

GtkWidget*
gtksharp_file_selection_get_main_vbox (GtkFileSelection *file)
{
	return file->main_vbox;
}

GtkWidget*
gtksharp_file_selection_get_ok_button (GtkFileSelection *file)
{
	return file->ok_button;
}

GtkWidget*
gtksharp_file_selection_get_cancel_button (GtkFileSelection *file)
{
	return file->cancel_button;
}

GtkWidget*
gtksharp_file_selection_get_help_button (GtkFileSelection *file)
{
	return file->help_button;
}

GtkWidget*
gtksharp_file_selection_get_history_pulldown (GtkFileSelection *file)
{
	return file->history_pulldown;
}

GtkWidget*
gtksharp_file_selection_get_history_menu (GtkFileSelection *file)
{
	return file->history_menu;
}

GtkWidget*
gtksharp_file_selection_get_fileop_dialog (GtkFileSelection *file)
{
	return file->fileop_dialog;
}

GtkWidget*
gtksharp_file_selection_get_fileop_entry (GtkFileSelection *file)
{
	return file->fileop_entry;
}

gchar*
gtksharp_file_selection_get_fileop_file (GtkFileSelection *file)
{
	return file->fileop_file;
}

GtkWidget*
gtksharp_file_selection_get_fileop_c_dir (GtkFileSelection *file)
{
	return file->fileop_c_dir;
}

GtkWidget*
gtksharp_file_selection_get_fileop_del_file (GtkFileSelection *file)
{
	return file->fileop_del_file;
}

GtkWidget*
gtksharp_file_selection_get_fileop_ren_file (GtkFileSelection *file)
{
	return file->fileop_ren_file;
}

GtkWidget*
gtksharp_file_selection_get_button_area (GtkFileSelection *file)
{
	return file->button_area;
}

GtkWidget*
gtksharp_file_selection_get_action_area (GtkFileSelection *file)
{
	return file->action_area;
}
