SUBDIRS = glue

API = gtk-api.xml
RAW_API = gtk-api.raw
ADDITIONAL_API = gtk-symbols.xml
INCLUDE_API = ../pango/pango-api.xml ../atk/atk-api.xml ../gdk/gdk-api.xml $(top_srcdir)/gdk/gdk-symbols.xml
METADATA = Gtk.metadata
ASSEMBLY_NAME = gtk-sharp
references = /r:../glib/glib-sharp.dll /r:../pango/pango-sharp.dll /r:../atk/atk-sharp.dll /r:../gdk/gdk-sharp.dll

ASSEMBLY = $(ASSEMBLY_NAME).dll
assemblydir = $(libdir)
gapidir = $(datadir)/gapi
assembly_DATA = $(ASSEMBLY)
gapi_DATA = $(API) $(ADDITIONAL_API)
CLEANFILES = $(ASSEMBLY) generated-stamp generated/*.cs $(API)

sources = 				\
	Application.cs			\
	ITreeNode.cs			\
	NodeStore.cs			\
	ThreadNotify.cs			\
	TreeNodeAttribute.cs		\
	TreeNode.cs			\
	TreeNodeValueAttribute.cs

build_sources = $(addprefix $(srcdir)/, $(sources))

customs = 				\
	Accel.custom			\
	AccelKey.custom			\
	Adjustment.custom		\
	Button.custom			\
	Calendar.custom			\
	Clipboard.custom		\
	ColorSelection.custom		\
	ColorSelectionDialog.custom	\
	Combo.custom			\
	Container.custom		\
	Dialog.custom			\
	Entry.custom			\
	FileSelection.custom		\
	Frame.custom			\
	IconFactory.custom		\
	IconSet.custom			\
	Label.custom			\
	Layout.custom			\
	ListStore.custom		\
	MessageDialog.custom		\
	Notebook.custom			\
	Object.custom			\
	Paned.custom			\
	RadioButton.custom		\
	RadioMenuItem.custom		\
	ScrolledWindow.custom		\
	SelectionData.custom		\
	Stock.custom			\
	Style.custom			\
	Table.custom			\
	TargetEntry.custom		\
	TextBuffer.custom		\
	TextChildAnchor.custom		\
	TextIter.custom			\
	TextTag.custom			\
	TextView.custom			\
	TreeIter.custom			\
	TreeModel.custom		\
	TreeModelSort.custom		\
	TreePath.custom			\
	TreeSelection.custom		\
	TreeStore.custom		\
	TreeViewColumn.custom		\
	TreeView.custom			\
	Viewport.custom			\
	Widget.custom			\
	Window.custom

build_customs = $(addprefix $(srcdir)/, $(customs))

EXTRA_DIST =			\
	makefile.win32		\
	$(RAW_API)		\
	$(ADDITIONAL_API)	\
	$(METADATA)		\
	$(customs)		\
	$(sources)

$(API): $(srcdir)/$(RAW_API) $(srcdir)/$(METADATA)
	cp $(srcdir)/$(RAW_API) $(API)
	chmod u+w $(API)
	$(RUNTIME) ../parser/gapi-fixup.exe --api=$(API) --metadata=$(srcdir)/$(METADATA)

generated-stamp: $(API) $(srcdir)/$(ADDITIONAL_API) ../generator/gapi_codegen.exe $(build_customs)
	$(RUNTIME) ../generator/gapi_codegen.exe --generate $(API) $(srcdir)/$(ADDITIONAL_API) \
	--include $(INCLUDE_API) --outdir=generated --customdir=$(srcdir) --assembly-name=$(ASSEMBLY_NAME) \
	&& touch generated-stamp

$(ASSEMBLY): $(build_sources) generated-stamp
	$(CSC) --unsafe /out:$(ASSEMBLY) /target:library $(references) $(build_sources) generated/*.cs

