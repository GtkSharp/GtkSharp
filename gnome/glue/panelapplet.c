#include <panel-applet.h>

typedef struct _ContextMenuItem
{
	const gchar *verb;
	gpointer callback;
} ContextMenuItem;

void panelapplet_setup_menu (PanelApplet *applet, const gchar *xml, ContextMenuItem *menuitems, gint cnt);

void
panelapplet_setup_menu (PanelApplet *applet, const gchar *xml, ContextMenuItem *menuitems, gint cnt)
{
	int i;
	BonoboUIVerb *menu_verbs = NULL;

	menu_verbs = g_new0 (BonoboUIVerb, cnt + 1);

	for (i = 0; i < cnt; i++) {
		menu_verbs[i].cname = menuitems[i].verb;
		menu_verbs[i].cb = menuitems[i].callback;
		menu_verbs[i].user_data = NULL;
	}
	menu_verbs[cnt].cname = NULL;
	menu_verbs[cnt].cb = NULL;
	menu_verbs[cnt].user_data = NULL;
	panel_applet_setup_menu (applet, xml, menu_verbs, NULL);
	g_free (menu_verbs);
}
