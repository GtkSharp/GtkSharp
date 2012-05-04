
AC_INIT(README)
AC_CANONICAL_SYSTEM
AM_CONFIG_HEADER(config.h)
AM_INIT_AUTOMAKE(gtk-sharp, 2.99.0)
AM_MAINTAINER_MODE

API_VERSION=3.0.0.0
AC_SUBST(API_VERSION)
POLICY_VERSIONS=""
AC_SUBST(POLICY_VERSIONS)

PACKAGE_VERSION=gtk-sharp-3.0
AC_SUBST(PACKAGE_VERSION)

WIN64DEFINES=

case "$host" in
	x86_64-*-mingw*|x86_64-*-cygwin*)
		WIN64DEFINES="-define:WIN64LONGS"
		platform_win32=yes
		AC_MSG_RESULT([yes, 64-bit])
		AC_DEFINE(PLATFORM_WIN32,1,[Platform is Win32])
		if test "x$cross_compiling" = "xno"; then
			CC="gcc -mno-cygwin -g"
			HOST_CC="gcc"
		fi
		;;
	*-*-mingw*|*-*-cygwin*)
		platform_win32=yes
		AC_MSG_RESULT([yes, 32-bit])
		AC_DEFINE(PLATFORM_WIN32,1,[Platform is Win32])
		if test "x$cross_compiling" = "xno"; then
			CC="gcc -mno-cygwin -g"
			HOST_CC="gcc"
		fi
		;;
	*)
		platform_win32=no
		AC_MSG_RESULT([no])
		;;
esac

AM_CONDITIONAL(PLATFORM_WIN32, test x$platform_win32 = xyes)

AC_CHECK_TOOL(CC, gcc, gcc)
AC_PROG_CC
AM_PROG_CC_STDC
AC_PROG_INSTALL

dnl may require a specific autoconf version
dnl AC_PROG_CC_FOR_BUILD
dnl CC_FOR_BUILD not automatically detected
CC_FOR_BUILD=$CC
BUILD_EXEEXT=
if test "x$cross_compiling" = "xyes"; then
	CC_FOR_BUILD=cc
	BUILD_EXEEXT=""
fi
AC_SUBST(CC_FOR_BUILD)
AC_SUBST(HOST_CC)
AC_SUBST(BUILD_EXEEXT)

# Set STDC_HEADERS
AC_HEADER_STDC
AC_LIBTOOL_WIN32_DLL
AM_PROG_LIBTOOL

# not 64 bit clean in cross-compile
AC_CHECK_SIZEOF(void *, 4)

CFLAGS="${CFLAGS} -g -Wall -Wunused -Wmissing-prototypes -Wmissing-declarations -Wstrict-prototypes  -Wmissing-prototypes -Wnested-externs  -Wshadow -Wpointer-arith -Wno-cast-qual -Wcast-align -Wwrite-strings"

if test "x$enable_maintainer_mode" = "xyes"; then
DEBUG_FLAGS='-debug'
RUNTIME_DEBUG_FLAGS=' --debug'
else
DEBUG_FLAGS=
RUNTIME_DEBUG_FLAGS=
AC_ARG_ENABLE(debug, [  --enable-debug          Build debugger (.mdb) files for dlls],
	DEBUG_FLAGS='-debug'
)
fi

AC_PATH_PROG(PKG_CONFIG, pkg-config, no)
if test "x$PKG_CONFIG" = "xno"; then
	AC_MSG_ERROR([You need to install pkg-config])
fi

dnl for use on the build system
BUILD_GTK_CFLAGS=`$PKG_CONFIG --cflags gtk+-2.0`
BUILD_GTK_LIBS=`$PKG_CONFIG --libs gtk+-2.0`
AC_SUBST(BUILD_GTK_CFLAGS)
AC_SUBST(BUILD_GTK_LIBS)

AC_CHECK_SIZEOF(off_t)
OFF_T_FLAGS="-define:OFF_T_$ac_cv_sizeof_off_t"
AC_SUBST(OFF_T_FLAGS)

MONO_REQUIRED_VERSION=2.8
PKG_CHECK_MODULES(MONO_DEPENDENCY, mono >= $MONO_REQUIRED_VERSION, has_mono=true, has_mono=false)

AC_PATH_PROG(GACUTIL, gacutil, no)
if test "x$GACUTIL" = "xno" ; then
	AC_MSG_ERROR([No gacutil tool found. You need to install either the mono or .Net SDK.])
fi

AC_PATH_PROG(AL, al, no)
if test "x$AL" = "xno" ; then
	AC_MSG_ERROR([No al tool found. You need to install either the mono or .Net SDK.])
fi

if test "x$has_mono" = "xtrue"; then
GACUTIL_FLAGS='/package $(PACKAGE_VERSION) /gacdir $(DESTDIR)$(prefix)/lib'
GENERATED_SOURCES=generated/*.cs
AC_PATH_PROG(RUNTIME, mono, no)

# If mono is found, it's in the path.  Require it to be in the path at runtime as well
if test "x$RUNTIME" != "no" ; then
	RUNTIME="mono$RUNTIME_DEBUG_FLAGS"
fi

AC_PATH_PROG(CSC, gmcs, no)
if test `uname -s` = "Darwin"; then
	LIB_PREFIX=
	LIB_SUFFIX=.dylib
else
	LIB_PREFIX=.so
	LIB_SUFFIX=
fi
SDCHECK="`$GACUTIL /l |grep ^System.Drawing, | head -n1 |cut -f1 -d','`"
if test "x$SDCHECK" = "xSystem.Drawing"; then
	enable_dotnet=yes
else
	enable_dotnet=no
fi
else
AC_PATH_PROG(CSC, csc.exe, no)
GACUTIL_FLAGS=
GENERATED_SOURCES=generated\\\\*.cs
enable_dotnet=yes

if test x$CSC = "xno"; then
	AC_MSG_ERROR([You need to install either mono (>=$MONO_REQUIRED_VERSION) or .Net])
else
RUNTIME=
LIB_PREFIX=
LIB_SUFFIX=.dylib
fi
fi

CS="C#"
if test "x$CSC" = "xno" ; then
	AC_MSG_ERROR([No $CS compiler found])
fi

AC_SUBST(RUNTIME)
AC_SUBST(CSC)
AC_SUBST(GACUTIL)
AC_SUBST(GACUTIL_FLAGS)
AC_SUBST(LIB_PREFIX)
AC_SUBST(LIB_SUFFIX)
AC_SUBST(GENERATED_SOURCES)

GTK_REQUIRED_VERSION=3.0.0
GLIB_REQUIRED_VERSION=2.28.0

PKG_CHECK_MODULES(GLIB, gobject-2.0 >= $GLIB_REQUIRED_VERSION)
AC_SUBST(GLIB_CFLAGS)
AC_SUBST(GLIB_LIBS)

PKG_CHECK_MODULES(GIO, gio-2.0 >= $GLIB_REQUIRED_VERSION)
AC_SUBST(GIO_CFLAGS)
AC_SUBST(GIO_LIBS)

CSFLAGS="$DEBUG_FLAGS $WIN64DEFINES"
PKG_CHECK_MODULES(GLIB_2_31,
	glib-2.0 >= 2.31,
	HAVE_GLIB_2_31_OR_HIGHER=yes, HAVE_GLIB_2_31_OR_HIGHER=no)
if test "x$HAVE_GLIB_2_31_OR_HIGHER" = "xyes" ; then
	CSFLAGS="$CSFLAGS -define:DISABLE_GTHREAD_CHECK"
fi
AC_SUBST(CSFLAGS)

PKG_CHECK_MODULES(PANGO, pango)
AC_SUBST(PANGO_CFLAGS)
AC_SUBST(PANGO_LIBS)

PKG_CHECK_MODULES(ATK, atk)
AC_SUBST(ATK_CFLAGS)
AC_SUBST(ATK_LIBS)

PKG_CHECK_MODULES(GTK, gtk+-3.0 >= $GTK_REQUIRED_VERSION)
AC_SUBST(GTK_CFLAGS)
AC_SUBST(GTK_LIBS)

GDK_BACKEND=`pkg-config --variable=target gtk+-3.0`
AC_SUBST(GDK_BACKEND)

AC_PATH_PROG(MDOC, mdoc, no)
if test "x$MDOC" = "xno"; then
  enable_monodoc=no
  doc_sources_dir=
else
  enable_monodoc=yes
  doc_sources_dir="`pkg-config --variable=sourcesdir monodoc`"
fi
AC_SUBST(MDOC)

AM_CONDITIONAL(ENABLE_DOTNET, test "x$enable_dotnet" = "xyes")
AM_CONDITIONAL(ENABLE_MONODOC, test "x$enable_monodoc" = "xyes")

AC_SUBST(CFLAGS)

AC_OUTPUT([
AssemblyInfo.cs
Makefile
policy.config
sources/Makefile
parser/Makefile
parser/gapi-3.0.pc
parser/gapi3-fixup
parser/gapi3-parser
generator/Makefile
generator/gapi3-codegen
glib/Makefile
glib/glib-sharp-3.0.pc
glib/glib-sharp.dll.config
gio/Makefile
gio/gio-sharp-3.0.pc
gio/gio-sharp.dll.config
gio/glue/Makefile
cairo/Makefile
pango/Makefile
pango/pango-sharp.dll.config
pango/glue/Makefile
atk/Makefile
atk/atk-sharp.dll.config
atk/glue/Makefile
gdk/Makefile
gdk/gdk-sharp.dll.config
gtk/Makefile
gtk/gtk-sharp-3.0.pc
gtk/gtk-sharp.dll.config
gtk/glue/Makefile
gtkdotnet/Makefile
gtkdotnet/gtk-dotnet.dll.config
gtkdotnet/gtk-dotnet-3.0.pc
doc/Makefile
sample/GtkDemo/Makefile
sample/Makefile
sample/pixmaps/Makefile
sample/test/Makefile
sample/valtest/Makefile
sample/valtest/valtest.exe.config
sample/opaquetest/Makefile
sample/opaquetest/opaquetest.exe.config
sample/gio/Makefile
sample/gtk-gio/Makefile
])

if test x$platform_win32 = xyes; then
   # Get rid of 'cyg' prefixes in library names
   sed -e "s/\/cyg\//\/\//" libtool > libtool.new; mv libtool.new libtool; chmod 755 libtool
fi

if test "x$enable_mono_cairo" = "xyes"; then
  cairo_comment="building local copy"
else
  cairo_comment="using system assembly"
fi

echo "---"
echo "Configuration summary"
echo ""
echo "   * Installation prefix = $prefix"
echo "   * $CS compiler: $CSC $CSFLAGS"
echo ""
echo "   Optional assemblies included in the build:"
echo ""
echo "      * gtk-dotnet.dll: $enable_dotnet"
echo ""
echo "      NOTE: if any of the above say 'no' you may install the"
echo "            corresponding development packages for them, rerun"
echo "            autogen.sh to include them in the build."
echo ""
echo "   * Documentation build enabled: $enable_monodoc "
if test "x$enable_monodoc" = "xyes" -a "x$doc_sources_dir" != "x$prefix/lib/monodoc/sources"; then
  echo "      WARNING: The install prefix is different than the monodoc prefix."
  echo "               Monodoc will not be able to load the documentation."
fi
echo "---"

