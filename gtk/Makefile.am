SUBDIRS = . glue

API = gtk-api.xml
RAW_API = gtk-api.raw
ADDITIONAL_API = gtk-symbols.xml
INCLUDE_API = ../pango/pango-api.xml ../atk/atk-api.xml ../gdk/gdk-api.xml $(top_srcdir)/gdk/gdk-symbols.xml
METADATA = Gtk.metadata
ASSEMBLY_NAME = gtk-sharp
references = /r:../glib/glib-sharp.dll /r:../pango/pango-sharp.dll /r:../atk/atk-sharp.dll /r:../gdk/gdk-sharp.dll

TARGET = $(ASSEMBLY)
ASSEMBLY = $(ASSEMBLY_NAME).dll
gapidir = $(datadir)/gapi
noinst_DATA = $(ASSEMBLY) $(ASSEMBLY).config
gapi_DATA = $(API) $(ADDITIONAL_API)
CLEANFILES = $(ASSEMBLY) generated-stamp generated/*.cs $(API) glue/generated.c gtk-sharp.pub
DISTCLEANFILES = $(ASSEMBLY).config AssemblyInfo.cs

sources = 				\
	Application.cs			\
	ITreeNode.cs			\
	NodeStore.cs			\
	ThreadNotify.cs			\
	TreeNodeAttribute.cs		\
	TreeNode.cs			\
	TreeNodeValueAttribute.cs

build_sources = $(addprefix $(srcdir)/, $(sources)) AssemblyInfo.cs

customs = 				\
	Accel.custom			\
	AccelKey.custom			\
	Adjustment.custom		\
	Button.custom			\
	Calendar.custom			\
	CheckMenuItem.custom		\
	Clipboard.custom		\
	ColorSelection.custom		\
	ColorSelectionDialog.custom	\
	Combo.custom			\
	Container.custom		\
	Dialog.custom			\
	Entry.custom			\
	FileSelection.custom		\
	Frame.custom			\
	HBox.custom			\
	HScale.custom			\
	IconFactory.custom		\
	IconSet.custom			\
	ImageMenuItem.custom		\
	ItemFactory.custom		\
	Label.custom			\
	Layout.custom			\
	ListStore.custom		\
	MessageDialog.custom		\
	MenuItem.custom			\
	Notebook.custom			\
	Object.custom			\
	Paned.custom			\
	Plug.custom			\
	RadioButton.custom		\
	RadioMenuItem.custom		\
	ScrolledWindow.custom		\
	SelectionData.custom		\
	SpinButton.custom		\
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
	VBox.custom			\
	VScale.custom			\
	Viewport.custom			\
	Widget.custom			\
	Window.custom

build_customs = $(addprefix $(srcdir)/, $(customs))

EXTRA_DIST =			\
	makefile.win32		\
	$(ASSEMBLY).config.in	\
	AssemblyInfo.cs.in	\
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
	$(RUNTIME) ../generator/gapi_codegen.exe --generate $(API) $(srcdir)/$(ADDITIONAL_API) 			\
	--include $(INCLUDE_API) --outdir=generated --customdir=$(srcdir) --assembly-name=$(ASSEMBLY_NAME) 	\
	--gluelib-name=gtksharpglue --glue-filename=glue/generated.c						\
	&& touch generated-stamp

gtk-sharp.pub: $(top_srcdir)/gtk-sharp.pub
	cp $(top_srcdir)/gtk-sharp.pub .

$(ASSEMBLY): $(build_sources) generated-stamp gtk-sharp.pub
	$(CSC) --unsafe /out:$(ASSEMBLY) /target:library $(references) $(build_sources) generated/*.cs

install-data-local:
	@if test -n '$(TARGET)'; then						\
	  echo "$(GACUTIL) /i $(ASSEMBLY) /f /package $(PACKAGE) /root $(DESTDIR)$(libdir)";	\
	  $(GACUTIL) /i $(ASSEMBLY) /f /package $(PACKAGE) /root $(DESTDIR)$(libdir) || exit 1;	\
	fi

uninstall-local:
	@if test -n '$(TARGET)'; then						\
	  echo "$(GACUTIL) /u $(ASSEMBLY_NAME) /package $(PACKAGE) /root $(DESTDIR)$(libdir)";	\
	  $(GACUTIL) /u $(ASSEMBLY_NAME) /package $(PACKAGE) /root $(DESTDIR)$(libdir) || exit 1;	\
	fi

