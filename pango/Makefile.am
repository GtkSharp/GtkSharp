SUBDIRS = . glue

if ENABLE_MONO_CAIRO
local_mono_cairo=$(top_builddir)/cairo/Mono.Cairo.dll
else
local_mono_cairo=
endif

pkg = pango
METADATA = Pango.metadata
SYMBOLS = pango-symbols.xml
references = ../glib/glib-sharp.dll $(local_mono_cairo)
glue_includes = pango/pango.h

sources =			\
	Attribute.cs		\
	AttrBackground.cs	\
	AttrColor.cs		\
	AttrFallback.cs		\
	AttrFamily.cs		\
	AttrFloat.cs		\
	AttrFontDesc.cs		\
	AttrForeground.cs	\
	AttrGravity.cs		\
	AttrGravityHint.cs	\
	AttrInt.cs		\
	AttrLanguage.cs		\
	AttrLetterSpacing.cs	\
	AttrRise.cs		\
	AttrScale.cs		\
	AttrShape.cs		\
	AttrSize.cs		\
	AttrStretch.cs		\
	AttrStrikethroughColor.cs \
	AttrStrikethrough.cs	\
	AttrStyle.cs		\
	AttrUnderlineColor.cs	\
	AttrUnderline.cs	\
	AttrVariant.cs		\
	AttrWeight.cs		\
	Scale.cs		\
	ScriptIter.cs

customs =			\
	Analysis.custom		\
	AttrIterator.custom	\
	AttrList.custom		\
	Context.custom		\
	Coverage.custom		\
	FontFamily.custom	\
	FontMap.custom		\
	Global.custom		\
	GlyphItem.custom	\
	GlyphString.custom	\
	Item.custom		\
	Layout.custom		\
	LayoutLine.custom	\
	LayoutRun.custom	\
	Matrix.custom		\
	TabArray.custom		\
	Units.custom

add_dist =

include ../Makefile.include

