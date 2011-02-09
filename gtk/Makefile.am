SUBDIRS = . glue

pkg = gtk
pkgconfigdir=$(libdir)/pkgconfig
pkgconfig_DATA=gtk-sharp-3.0.pc

SYMBOLS = gtk-symbols.xml
INCLUDE_API = $(top_srcdir)/glib/glib-api.xml $(top_srcdir)/cairo/cairo-api.xml $(top_builddir)/gio/gio-api.xml $(top_builddir)/pango/pango-api.xml $(top_builddir)/atk/atk-api.xml $(top_builddir)/gdk/gdk-api.xml
METADATA = Gtk.metadata
references = $(top_builddir)/glib/glib-sharp.dll $(top_builddir)/cairo/cairo-sharp.dll $(top_builddir)/gio/gio-sharp.dll $(top_builddir)/pango/pango-sharp.dll $(top_builddir)/atk/atk-sharp.dll $(top_builddir)/gdk/gdk-sharp.dll
glue_includes = gtk/gtk.h

sources = 				\
	ActionEntry.cs			\
	Application.cs			\
	BindingAttribute.cs		\
	CellAreaBox.cs			\
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
	TreeEnumerator.cs		\
	TreeMenu.cs			\
	TreeNodeAttribute.cs		\
	TreeNode.cs			\
	TreeNodeValueAttribute.cs

customs = 				\
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
	CellLayout.custom		\
	CellLayoutAdapter.custom	\
	CellView.custom			\
	CheckMenuItem.custom		\
	Clipboard.custom		\
	ColorSelection.custom		\
	ColorSelectionDialog.custom	\
	ComboBox.custom			\
	ComboBoxEntry.custom		\
	Container.custom		\
	Dialog.custom			\
	Drag.custom			\
	Entry.custom			\
	EntryCompletion.custom		\
	FileChooserDialog.custom	\
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
	PaperSize.custom		\
	Plug.custom			\
	PrintContext.custom		\
	Printer.custom			\
	RadioButton.custom		\
	RadioMenuItem.custom		\
	RadioToolButton.custom		\
	RecentManager.custom		\
	ScrolledWindow.custom		\
	SelectionData.custom		\
	Settings.custom			\
	SpinButton.custom		\
	StatusIcon.custom		\
	Stock.custom			\
	StockItem.custom		\
	Style.custom			\
	Table.custom			\
	Target.custom			\
	TargetEntry.custom		\
	TargetList.custom		\
	TargetPair.custom		\
	TextAttributes.custom		\
	TextBuffer.custom		\
	TextChildAnchor.custom		\
	TextIter.custom			\
	TextMark.custom			\
	TextTag.custom			\
	TextView.custom			\
	TooltipsData.custom		\
	TreeIter.custom			\
	TreeModel.custom		\
	TreeModelAdapter.custom		\
	TreeModelFilter.custom		\
	TreeModelSort.custom		\
	TreePath.custom			\
	TreeSelection.custom		\
	TreeStore.custom		\
	TreeViewColumn.custom		\
	TreeView.custom			\
	UIManager.custom		\
	VBox.custom			\
	VScale.custom			\
	Viewport.custom			\
	Widget.custom			\
	Window.custom

add_dist = gtk-sharp-3.0.pc.in

include $(top_srcdir)/Makefile.include

