#include <libgnome/gnome-program.h>
#include <libgnome/gnome-init.h>
#include <string.h>
#include <config.h>

typedef struct {
	char *name;
	GValue *value;
} PropertyArg;

/* Forward declarations */
GnomeProgram*
gtksharp_gnome_program_init (const char *app_id, const char *app_version,
			     const GnomeModuleInfo *module_info,
			     int argc, char **argv,
			     int nargs, PropertyArg* args);
/* */

static gchar*
get_default (GObjectClass *klass, const gchar *property)
{
	GParamSpec *spec = g_object_class_find_property (klass, property);
	GParamSpecString *strspec;
	gchar *ret;

	g_return_val_if_fail (spec != NULL, NULL);
	g_return_val_if_fail (strspec != NULL, NULL);
	
	strspec = G_PARAM_SPEC_STRING (spec);
	ret = g_strdup (strspec->default_value);
	//g_param_spec_unref (spec);
	
	return ret;
}

/* FIXME: HACK */
GnomeProgram*
gtksharp_gnome_program_init (const char *app_id, const char *app_version,
			     const GnomeModuleInfo *module_info,
			     int argc, char **argv,
			     int nargs, PropertyArg* args)
{
	GnomeProgram *ret;
	int i;
	gboolean *unhandled = g_new0 (gboolean, nargs);

	/* ok, these are the known construct-time arguments which means we
	 * _have_ to pass them into init. */

	GObjectClass *klass = g_type_class_ref (GNOME_TYPE_PROGRAM);
	gchar *human_readable_name = NULL; 
	gchar *gnome_path = NULL;
	gchar *gnome_prefix = NULL;
	gchar *gnome_libdir = NULL;
	gchar *gnome_datadir = NULL;
	gchar *gnome_sysconfdir = NULL;
	gboolean create_directories = TRUE;
	gchar *gnome_espeaker = NULL;

	for (i = 0; i < nargs; i++)
	{
		if (!strcmp (args[i].name, GNOME_PARAM_HUMAN_READABLE_NAME))
			human_readable_name = g_strdup (g_value_get_string (args[i].value));
		else if (!strcmp (args[i].name, GNOME_PARAM_GNOME_PATH))
			gnome_path = g_strdup (g_value_get_string (args[i].value));
		else if (!strcmp (args[i].name, GNOME_PARAM_GNOME_PREFIX))
			gnome_prefix = g_strdup (g_value_get_string (args[i].value));
		else if (!strcmp (args[i].name, GNOME_PARAM_GNOME_LIBDIR))
			gnome_libdir = g_strdup (g_value_get_string (args[i].value));
		else if (!strcmp (args[i].name, GNOME_PARAM_GNOME_DATADIR))
			gnome_datadir = g_strdup (g_value_get_string (args[i].value));
		else if (!strcmp (args[i].name, GNOME_PARAM_GNOME_SYSCONFDIR))
			gnome_sysconfdir = g_strdup (g_value_get_string (args[i].value));
		else if (!strcmp (args[i].name, GNOME_PARAM_CREATE_DIRECTORIES))
			create_directories = g_value_get_boolean (args[i].value);
		else
			unhandled[i] = TRUE;
	}

	if (!human_readable_name)
		human_readable_name = g_strdup (app_id);
	if (!gnome_path)
		gnome_path = get_default (klass, GNOME_PARAM_GNOME_PATH);
	if (!gnome_prefix)
		gnome_prefix = get_default (klass, GNOME_PARAM_GNOME_PREFIX);
	if (!gnome_libdir)
		gnome_libdir = get_default (klass, GNOME_PARAM_GNOME_LIBDIR);
	if (!gnome_datadir)
		gnome_datadir = get_default (klass, GNOME_PARAM_GNOME_DATADIR);
	if (!gnome_sysconfdir)
		gnome_sysconfdir = get_default (klass, GNOME_PARAM_GNOME_SYSCONFDIR);
	
	ret = gnome_program_init (app_id, app_version, module_info,
				  argc, argv,
				  GNOME_PARAM_HUMAN_READABLE_NAME,
				  human_readable_name,
				  GNOME_PARAM_GNOME_PREFIX,
				  gnome_prefix,
				  GNOME_PARAM_GNOME_LIBDIR,
				  gnome_libdir,
				  GNOME_PARAM_GNOME_DATADIR,
				  gnome_datadir,
				  GNOME_PARAM_GNOME_SYSCONFDIR,
				  gnome_sysconfdir,
				  GNOME_PARAM_CREATE_DIRECTORIES,
				  create_directories,
				  (gnome_path) ? (GNOME_PARAM_GNOME_PATH) : ((gnome_espeaker) ? GNOME_PARAM_ESPEAKER : NULL),
				  (gnome_path) ? (gnome_path) : ((gnome_espeaker) ? gnome_espeaker : NULL),
				  (gnome_espeaker && gnome_path) ? GNOME_PARAM_ESPEAKER : NULL,
				  (gnome_espeaker && gnome_path) ? gnome_espeaker : NULL,
				  NULL);

	for (i = 0; i < nargs; i++)
	{
		if (unhandled[i])
			g_object_set_property (G_OBJECT (ret),
					       args[i].name, args[i].value); 
	}

	if (human_readable_name)
		g_free (human_readable_name);
	if (gnome_path)
		g_free (gnome_path);
	if (gnome_prefix)
		g_free (gnome_prefix);
	if (gnome_libdir)
		g_free (gnome_libdir);
	if (gnome_datadir)
		g_free (gnome_datadir);
	if (gnome_sysconfdir)
		g_free (gnome_sysconfdir);
	if (gnome_espeaker)
		g_free (gnome_espeaker);
	
	g_free (unhandled);

	return ret;
}

