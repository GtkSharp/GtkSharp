/*
 * Pipe for ThreadNotify
 *
 * (C) 2004 Gonzalo Paniagua Javier (gonzalo@ximian.com)
 */

#include <glib.h>
#ifdef G_OS_WIN32
#include <windows.h>
#else
#include <stdio.h>
#include <unistd.h>
#endif

gint pipe_create (gint *fds);

gint
pipe_create (gint *fds)
{
#ifdef G_OS_WIN32
	return !CreatePipe ((PHANDLE) fds, (PHANDLE) &fds [1], NULL, 1024);
#else
	return pipe (fds);
#endif
}

gint pipe_read (gint fd, gchar *buffer, gint maxcount);

gint
pipe_read (gint fd, gchar *buffer, gint maxcount)
{
#ifdef G_OS_WIN32
	glong dummy;
	return !ReadFile ((HANDLE) fd, buffer, maxcount, &dummy, NULL);
#else
	return (read (fd, buffer, maxcount) < 0);
#endif
}

gint pipe_write (gint fd, gchar *buffer, gint maxcount);

gint
pipe_write (gint fd, gchar *buffer, gint maxcount)
{
#ifdef G_OS_WIN32
	glong dummy;
	return !WriteFile ((HANDLE) fd, buffer, maxcount, &dummy, NULL);
#else
	return (write (fd, buffer, maxcount) < 0);
#endif
}

void pipe_close (gint *fds);

void
pipe_close (gint *fds)
{
#ifdef G_OS_WIN32
	CloseHandle ((HANDLE) fds [0]);
	CloseHandle ((HANDLE) fds [1]);
#else
	close (fds [0]);
	close (fds [1]);
#endif
}

