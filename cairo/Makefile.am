ASSEMBLY_NAME = cairo-sharp
ASSEMBLY = $(ASSEMBLY_NAME).dll
SNK = $(srcdir)/mono.snk

TARGET=$(ASSEMBLY)

noinst_DATA = $(TARGET)

CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb

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
	Region.cs \
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

$(ASSEMBLY): $(build_sources) $(SNK)
	@rm -f $(ASSEMBLY).mdb
	$(CSC) $(CSFLAGS) -keyfile:$(SNK) -nowarn:0169,0612,0618 -out:$(ASSEMBLY) -target:library $(references) $(build_sources)

install-data-local:
	@if test -n '$(TARGET)'; then							\
	  echo "$(GACUTIL) -i $(ASSEMBLY) -f $(GACUTIL_FLAGS)";				\
	  $(GACUTIL) -i $(ASSEMBLY) -f $(GACUTIL_FLAGS) || exit 1;			\
	fi

uninstall-local:
	@if test -n '$(TARGET)'; then							\
	  echo "$(GACUTIL) -u $(ASSEMBLY_NAME) $(GACUTIL_FLAGS)";			\
	  $(GACUTIL) -u $(ASSEMBLY_NAME) $(GACUTIL_FLAGS) || exit 1;			\
	fi

EXTRA_DIST = $(sources) mono.snk

