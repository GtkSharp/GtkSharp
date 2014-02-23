ASSEMBLY_NAME = cairo-sharp
ASSEMBLY_NAME_VERSION = $(ASSEMBLY_NAME),Version=$(CAIRO_API_VERSION)
ASSEMBLY = $(ASSEMBLY_NAME).dll
SNK = $(srcdir)/mono.snk

TARGET=$(ASSEMBLY)

noinst_DATA = $(TARGET)

CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb

sources = \
	Antialias.cs \
	Cairo.cs \
	CairoDebug.cs \
	Color.cs \
	Content.cs \
	Context.cs \
	Device.cs \
	DirectFBSurface.cs \
	Distance.cs \
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
	Point.cs \
	PointD.cs \
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

build_sources = $(addprefix $(srcdir)/, $(sources)) $(builddir)/AssemblyInfo.cs

$(ASSEMBLY): $(build_sources) $(SNK)
	@rm -f $(ASSEMBLY).mdb
	$(CSC) $(CSFLAGS) -keyfile:$(SNK) -out:$(ASSEMBLY) -target:library $(references) $(build_sources)

install-data-local:
	@if test -n '$(TARGET)'; then							\
	  echo "$(GACUTIL) -i $(ASSEMBLY) -f $(GACUTIL_FLAGS)";				\
	  "$(GACUTIL)" -i $(ASSEMBLY) -f $(GACUTIL_FLAGS) || exit 1;			\
	fi

uninstall-local:
	@if test -n '$(TARGET)'; then							\
	  echo "$(GACUTIL) -u $(ASSEMBLY_NAME_VERSION) $(GACUTIL_FLAGS)";			\
	  "$(GACUTIL)" -u $(ASSEMBLY_NAME_VERSION) $(GACUTIL_FLAGS) || exit 1;			\
	fi

EXTRA_DIST = $(sources) cairo-api.xml mono.snk

