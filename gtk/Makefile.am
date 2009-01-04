SUBDIRS = . glue

if ENABLE_MONO_CAIRO
local_mono_cairo=$(top_builddir)/cairo/Mono.Cairo.dll
else
local_mono_cairo=
endif

pkg = gtk
pkgconfigdir=$(libdir)/pkgconfig
pkgconfig_DATA=gtk-sharp-2.0.pc

SYMBOLS = gtk-symbols.xml
INCLUDE_API = $(srcdir)/../glib/glib-api.xml $(top_builddir)/gio/gio-api.xml $(top_builddir)/pango/pango-api.xml $(top_builddir)/atk/atk-api.xml $(top_builddir)/gdk/gdk-api.xml
METADATA = Gtk.metadata
references = $(top_builddir)/glib/glib-sharp.dll $(top_builddir)/gio/gio-sharp.dll $(top_builddir)/pango/pango-sharp.dll $(top_builddir)/atk/atk-sharp.dll $(top_builddir)/gdk/gdk-sharp.dll $(local_mono_cairo)
glue_includes = gtk/gtk.h

sources = 				\
	ActionEntry.cs			\
	Application.cs			\
	BindingAttribute.cs		\
	ChildPropertyAttribute.cs	\
	ITreeNode.cs			\
	Key.cs				\
	NodeCellDataFunc.cs		\
	NodeSelection.cs		\
	NodeStore.cs			\
	NodeView.cs			\
	RadioActionEntry.cs		\
	RowsReorderedHandler.cs		\
	StockManager.cs			\
	ThreadNotify.cs			\
	ToggleActionEntry.cs		\
	Timeout.cs			\
	TreeEnumerator.cs		\
	TreeNodeAttribute.cs		\
	TreeNode.cs			\
	TreeNodeValueAttribute.cs

customs = 				\
	AboutDialog.custom		\
	Accel.custom			\
	AccelKey.custom			\
	Action.custom			\
	ActionGroup.custom		\
	Adjustment.custom		\
	Bin.custom			\
	Builder.custom			\
	Button.custom			\
	Calendar.custom			\
	CellRenderer.custom		\
	CellRendererAccel.custom	\
	CellRendererCombo.custom	\
	CellRendererPixbuf.custom	\
	CellRendererProgress.custom	\
	CellRendererSpin.custom		\
	CellRendererText.custom		\
	CellRendererToggle.custom	\
	CellLayout.custom		\
	CellLayoutAdapter.custom	\
	CellView.custom			\
	CheckMenuItem.custom		\
	Clipboard.custom		\
	ColorSelection.custom		\
	ColorSelectionDialog.custom	\
	Combo.custom			\
	ComboBox.custom			\
	ComboBoxEntry.custom		\
	Container.custom		\
	Dialog.custom			\
	Drag.custom			\
	Entry.custom			\
	EntryCompletion.custom		\
	FileChooser.custom		\
	FileChooserButton.custom	\
	FileChooserDialog.custom	\
	FileChooserWidget.custom	\
	FileSelection.custom		\
	Frame.custom			\
	HBox.custom			\
	HScale.custom			\
	IconFactory.custom		\
	IconSet.custom			\
	IconTheme.custom		\
	IconView.custom			\
	Image.custom			\
	ImageMenuItem.custom		\
	Init.custom			\
	Input.custom			\
	ItemFactory.custom		\
	Label.custom			\
	ListStore.custom		\
	MessageDialog.custom		\
	Menu.custom			\
	MenuItem.custom			\
	Notebook.custom			\
	Object.custom			\
	Plug.custom			\
	PrintContext.custom		\
	Printer.custom			\
	RadioButton.custom		\
	RadioMenuItem.custom		\
	RadioToolButton.custom		\
	ScrolledWindow.custom		\
	SelectionData.custom		\
	Settings.custom			\
	SpinButton.custom		\
	StatusIcon.custom		\
	Stock.custom			\
	StockItem.custom		\
	Style.custom			\
	Table.custom			\
	TargetEntry.custom		\
	TargetList.custom		\
	TargetPair.custom		\
	TextAttributes.custom		\
	TextAppearance.custom		\
	TextBuffer.custom		\
	TextChildAnchor.custom		\
	TextIter.custom			\
	TextMark.custom			\
	TextTag.custom			\
	TextView.custom			\
	Toolbar.custom			\
	TooltipsData.custom		\
	TreeIter.custom			\
	TreeModel.custom		\
	TreeModelAdapter.custom		\
	TreeModelFilter.custom		\
	TreeModelSort.custom		\
	TreePath.custom			\
	TreeSelection.custom		\
	TreeSortable.custom		\
	TreeSortableAdapter.custom	\
	TreeStore.custom		\
	TreeViewColumn.custom		\
	TreeView.custom			\
	UIManager.custom		\
	VBox.custom			\
	VScale.custom			\
	Viewport.custom			\
	Widget.custom			\
	Window.custom

add_dist = gtk-sharp-2.0.pc.in

include ../Makefile.include

