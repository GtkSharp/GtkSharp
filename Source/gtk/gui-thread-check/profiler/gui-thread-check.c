/* -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */

/*
 * gui-thread-check.c
 *
 * Copyright (C) 2008 Novell, Inc.
 *
 */

/*
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of version 2 of the GNU General Public
 * License as published by the Free Software Foundation.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307
 * USA.
 */

#include <string.h>
#include <glib.h>
#include <mono/metadata/profiler.h>
#include <stdio.h>


extern pthread_t pthread_self (void);

static gboolean
stack_walk_fn (MonoMethod *method, gint32 native_offset, gint32 il_offset, gboolean managed, gpointer data)
{
	if (managed) {
		MonoClass* klass = mono_method_get_class (method);
		const char* method_name = mono_method_get_name (method);
		const char* klass_name = mono_class_get_name (klass);
		printf ("   %s.%s\n", klass_name, method_name);
	}
    return FALSE;
}

static void
simple_method_enter (MonoProfiler *prof, MonoMethod *method)
{
	static int guithread;
	static gboolean guithread_set = FALSE;
	MonoClass* klass;
	const char* name_space;
	
	klass = mono_method_get_class (method);
	name_space = mono_class_get_namespace (klass);
	
	int current_thread_id = (int) pthread_self();
	
	if (strstr (name_space, "Gtk") == name_space || strstr (name_space, "Gdk") == name_space) {
		const char* method_name = mono_method_get_name (method);
		const char* klass_name = mono_class_get_name (klass);
		
		if (!guithread_set && strcmp (klass_name, "Application")==0 && strcmp (method_name, "Init")==0) {
			guithread_set = TRUE;
			guithread = current_thread_id;
			printf ("*** GUI THREAD INITIALIZED: %u\n", guithread);
			fflush (NULL);
			return;
		}
		if (!guithread_set) {
			return;
		}
		if (current_thread_id != guithread &&
			!(strcmp (klass_name, "Object")==0 && strcmp (method_name, "Dispose")==0) &&
			!(strcmp (klass_name, "Widget")==0 && strcmp (method_name, "Dispose")==0) &&
			!(strcmp (klass_name, "Application")==0 && strcmp (method_name, "Invoke")==0) &&
			!(strcmp (method_name, "Finalize")==0) &&
			!(strcmp (method_name, "get_NativeDestroyHandler")==0) &&
			!(strcmp (method_name, "remove_InternalDestroyed")==0) &&
			!(strcmp (method_name, "remove_Destroyed")==0)
		) {
			printf ("*** GTK CALL NOT IN GUI THREAD: %s.%s\n", klass_name, method_name);
	        mono_stack_walk_no_il (stack_walk_fn, NULL);
			fflush (NULL);
		}
	}
}

void
mono_profiler_startup (const char *desc)
{
	g_print ("*** Running with gui-thread-check ***\n");
	
	mono_profiler_install (NULL, NULL);
	mono_profiler_install_enter_leave (simple_method_enter, NULL);
	mono_profiler_set_events (MONO_PROFILE_ENTER_LEAVE);
}

