ASSEMBLY_NAME = Mono.Cairo
ASSEMBLY = $(ASSEMBLY_NAME).dll

if ENABLE_MONO_CAIRO
TARGET=$(ASSEMBLY)
else
TARGET=
endif

noinst_DATA = $(TARGET)

CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb mono.snk

sources = \
	Antialias.cs \
	AssemblyInfo.cs \
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

build_sources = $(addprefix $(srcdir)/, $(sources))

mono.snk: $(top_srcdir)/mono.snk
	cp $(top_srcdir)/mono.snk .

$(ASSEMBLY): $(build_sources) mono.snk
	@rm -f $(ASSEMBLY).mdb
	$(CSC) $(CSFLAGS) -nowarn:0169,0612,0618 -out:$(ASSEMBLY) -target:library $(references) $(build_sources)

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

EXTRA_DIST = $(sources)

