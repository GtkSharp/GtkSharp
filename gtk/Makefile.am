SUBDIRS = . glue gui-thread-check

pkg = gtk
pkgconfigdir=$(libdir)/pkgconfig
pkgconfig_DATA=gtk-sharp-3.0.pc

SYMBOLS = gtk-symbols.xml
INCLUDE_API = $(top_srcdir)/glib/glib-api.xml $(top_srcdir)/cairo/cairo-api.xml $(top_builddir)/gio/gio-api.xml $(top_builddir)/pango/pango-api.xml $(top_builddir)/atk/atk-api.xml $(top_builddir)/gdk/gdk-api.xml
METADATA = Gtk.metadata
references = $(top_builddir)/glib/glib-sharp.dll $(top_builddir)/cairo/cairo-sharp.dll $(top_builddir)/gio/gio-sharp.dll $(top_builddir)/pango/pango-sharp.dll $(top_builddir)/atk/atk-sharp.dll $(top_builddir)/gdk/gdk-sharp.dll
glue_includes = gtk/gtk.h

sources = 				\
	Accel.cs			\
	AccelKey.cs			\
	Action.cs			\
	ActionEntry.cs			\
	ActionGroup.cs		\
	Adjustment.cs			\
	Application.cs			\
	Bin.cs				\
	BindingAttribute.cs		\
	Builder.cs			\
	Button.cs			\
	Calendar.cs			\
	CellAreaBox.cs			\
	CellLayoutAdapter.cs		\
	CellRenderer.cs		\
	CellView.cs			\
	CheckMenuItem.cs		\
	ChildPropertyAttribute.cs	\
	Clipboard.cs			\
	ColorSelection.cs		\
	ComboBox.cs			\
	ComboBoxText.cs			\
	Container.cs			\
	Dialog.cs			\
	Drag.cs			\
	Entry.cs			\
	EntryCompletion.cs		\
	FileChooserDialog.cs		\
	Frame.cs			\
	Global.cs			\
	HBox.cs			\
	HScale.cs			\
	ICellLayout.cs			\
	IconFactory.cs		\
	IconSet.cs			\
	IconTheme.cs			\
	IconView.cs			\
	Image.cs			\
	ImageMenuItem.cs		\
	Init.cs			\
	ITreeModel.cs			\
	ITreeNode.cs			\
	Key.cs				\
	Label.cs			\
	ListStore.cs			\
	Menu.cs			\
	MenuItem.cs			\
	MessageDialog.cs		\
	NodeCellDataFunc.cs		\
	NodeSelection.cs		\
	NodeStore.cs			\
	NodeView.cs			\
	Notebook.cs			\
	PaperSize.cs			\
	Plug.cs			\
	Printer.cs			\
	RadioAction.cs		\
	RadioActionEntry.cs		\
	RadioButton.cs		\
	RadioMenuItem.cs		\
	RadioToolButton.cs		\
	RecentManager.cs		\
	RowsReorderedHandler.cs		\
	ScrolledWindow.cs		\
	SelectionData.cs		\
	Settings.cs			\
	SpinButton.cs			\
	StatusIcon.cs			\
	Stock.cs			\
	StockItem.cs			\
	StockManager.cs			\
	Style.cs			\
	StyleContext.cs			\
	Target.cs			\
	TargetEntry.cs		\
	TargetList.cs			\
	TextAttributes.cs		\
	TextBuffer.cs			\
	TextChildAnchor.cs		\
	TextIter.cs			\
	TextMark.cs			\
	TextTag.cs			\
	TextView.cs			\
	ThreadNotify.cs			\
	ToggleActionEntry.cs		\
	TreeEnumerator.cs		\
	TreeIter.cs			\
	TreeMenu.cs			\
	TreeModelAdapter.cs		\
	TreeModelFilter.cs		\
	TreeModelSort.cs		\
	TreeNode.cs			\
	TreeNodeAttribute.cs		\
	TreeNodeValueAttribute.cs	\
	TreePath.cs			\
	TreeSelection.cs		\
	TreeStore.cs			\
	TreeView.cs			\
	TreeViewColumn.cs		\
	UIManager.cs			\
	VBox.cs			\
	Viewport.cs			\
	VScale.cs			\
	Widget.cs			\
	Window.cs

add_dist = gtk-sharp-3.0.pc.in

include $(top_srcdir)/Makefile.include

