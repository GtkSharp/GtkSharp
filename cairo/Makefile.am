ASSEMBLY_NAME = Mono.Cairo
ASSEMBLY = $(ASSEMBLY_NAME).dll

if ENABLE_MONO_CAIRO
TARGET=$(ASSEMBLY)
else
TARGET=
endif

noinst_DATA = $(TARGET)

CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb gtk-sharp.snk AssemblyInfo.cs

sources = \
	Antialias.cs \
	Cairo.cs \
	Content.cs \
	Context.cs \
	DirectFBSurface.cs \
	Extend.cs \
	FillRule.cs \
	Filter.cs \
	FontExtents.cs \
	FontFace.cs \
	FontOptions.cs \
	FontSlant.cs \
	FontType.cs \
	FontWeight.cs \
	Format.cs \
	GlitzSurface.cs \
	Glyph.cs \
	Gradient.cs \
	HintMetrics.cs \
	HintStyle.cs \
	ImageSurface.cs \
	LinearGradient.cs \
	LineCap.cs \
	LineJoin.cs \
	Matrix.cs \
	NativeMethods.cs \
	Operator.cs \
	Path.cs \
	Pattern.cs \
	PatternType.cs \
	PdfSurface.cs \
	PSSurface.cs \
	RadialGradient.cs \
	Rectangle.cs \
	ScaledFont.cs \
	SolidPattern.cs \
	Status.cs \
	SubpixelOrder.cs \
	Surface.cs \
	SurfacePattern.cs \
	SurfaceType.cs \
	SvgSurface.cs \
	SvgVersion.cs \
	TextExtents.cs \
	Win32Surface.cs \
	XcbSurface.cs \
	XlibSurface.cs \
	#

build_sources = $(addprefix $(srcdir)/, $(sources)) AssemblyInfo.cs

EXTRA_DIST = $(sources)

gtk-sharp.snk: $(top_srcdir)/gtk-sharp.snk
	cp $(top_srcdir)/gtk-sharp.snk .

AssemblyInfo.cs: $(top_builddir)/AssemblyInfo.cs
	@echo "sed -e "s/@API_VERSION@/1.0.5000.0/" -e "s/\[assembly:GLib.*//" $(top_srcdir)/AssemblyInfo.cs.in > $@"
	@sed -e "s/@API_VERSION@/1.0.5000.0/" -e "s/\[assembly:GLib.IgnoreClassInitializers\]//" $(top_builddir)/AssemblyInfo.cs > $@

$(ASSEMBLY): $(build_sources) gtk-sharp.snk AssemblyInfo.cs
	@rm -f $(ASSEMBLY).mdb
	$(CSC) $(CSFLAGS) -nowarn:0169,0612,0618 -out:$(ASSEMBLY) -target:library $(references) $(build_sources)
	$(GAPI_CDECL_INSERT)

install-data-local:
	@if test -n '$(TARGET)'; then							\
	  echo "$(GACUTIL) /i $(ASSEMBLY) /f $(GACUTIL_FLAGS)";				\
	  $(GACUTIL) /i $(ASSEMBLY) /f $(GACUTIL_FLAGS) || exit 1;			\
	fi

uninstall-local:
	@if test -n '$(TARGET)'; then							\
	  echo "$(GACUTIL) /u $(ASSEMBLY_NAME) $(GACUTIL_FLAGS)";			\
	  $(GACUTIL) /u $(ASSEMBLY_NAME) $(GACUTIL_FLAGS) || exit 1;			\
	fi

