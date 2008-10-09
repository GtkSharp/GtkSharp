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
	Mono.Cairo/Antialias.cs \
	Mono.Cairo/Cairo.cs \
	Mono.Cairo/Content.cs \
	Mono.Cairo/Context.cs \
	Mono.Cairo/DirectFBSurface.cs \
	Mono.Cairo/Extend.cs \
	Mono.Cairo/FillRule.cs \
	Mono.Cairo/Filter.cs \
	Mono.Cairo/FontExtents.cs \
	Mono.Cairo/FontFace.cs \
	Mono.Cairo/FontOptions.cs \
	Mono.Cairo/FontSlant.cs \
	Mono.Cairo/FontType.cs \
	Mono.Cairo/FontWeight.cs \
	Mono.Cairo/Format.cs \
	Mono.Cairo/GlitzSurface.cs \
	Mono.Cairo/Glyph.cs \
	Mono.Cairo/Gradient.cs \
	Mono.Cairo/HintMetrics.cs \
	Mono.Cairo/HintStyle.cs \
	Mono.Cairo/ImageSurface.cs \
	Mono.Cairo/LinearGradient.cs \
	Mono.Cairo/LineCap.cs \
	Mono.Cairo/LineJoin.cs \
	Mono.Cairo/Matrix.cs \
	Mono.Cairo/NativeMethods.cs \
	Mono.Cairo/Operator.cs \
	Mono.Cairo/Path.cs \
	Mono.Cairo/Pattern.cs \
	Mono.Cairo/PatternType.cs \
	Mono.Cairo/PdfSurface.cs \
	Mono.Cairo/PSSurface.cs \
	Mono.Cairo/RadialGradient.cs \
	Mono.Cairo/Rectangle.cs \
	Mono.Cairo/ScaledFont.cs \
	Mono.Cairo/SolidPattern.cs \
	Mono.Cairo/Status.cs \
	Mono.Cairo/SubpixelOrder.cs \
	Mono.Cairo/Surface.cs \
	Mono.Cairo/SurfacePattern.cs \
	Mono.Cairo/SurfaceType.cs \
	Mono.Cairo/SvgSurface.cs \
	Mono.Cairo/SvgVersion.cs \
	Mono.Cairo/TextExtents.cs \
	Mono.Cairo/Win32Surface.cs \
	Mono.Cairo/XcbSurface.cs \
	Mono.Cairo/XlibSurface.cs \
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

