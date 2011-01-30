SUBDIRS = . glue

pkg = pango
METADATA = Pango.metadata
INCLUDE_API = $(top_srcdir)/cairo/cairo-api.xml
SYMBOLS = pango-symbols.xml
references = $(top_builddir)/glib/glib-sharp.dll $(top_builddir)/cairo/cairo-sharp.dll
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

include $(top_srcdir)/Makefile.include

