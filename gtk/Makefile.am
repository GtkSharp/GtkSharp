SUBDIRS = . glue

pkg = gtk
pkgconfigdir=$(libdir)/pkgconfig
pkgconfig_DATA=gtk-sharp-2.0.pc

SYMBOLS = gtk-symbols.xml
INCLUDE_API = ../pango/pango-api.xml ../atk/atk-api.xml ../gdk/gdk-api.xml
METADATA = Gtk.metadata
references = ../glib/glib-sharp.dll ../pango/pango-sharp.dll ../atk/atk-sharp.dll ../gdk/gdk-sharp.dll

sources = 				\
	ActionEntry.cs			\
	Application.cs			\
	BindingAttribute.cs		\
	ChildPropertyAttribute.cs	\
	ITreeNode.cs			\
	NodeCellDataFunc.cs		\
	NodeSelection.cs		\
	NodeStore.cs			\
	NodeView.cs			\
	RadioActionEntry.cs		\
	ThreadNotify.cs			\
	ToggleActionEntry.cs		\
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
	Button.custom			\
	Calendar.custom			\
	CellRenderer.custom		\
	CellLayout.custom		\
	CellView.custom			\
	CheckMenuItem.custom		\
	Clipboard.custom		\
	ColorSelection.custom		\
	ColorSelectionDialog.custom	\
	Combo.custom			\
	ComboBox.custom			\
	Container.custom		\
	Dialog.custom			\
	Entry.custom			\
	EntryCompletion.custom		\
	FileChooserDialog.custom	\
	FileChooserWidget.custom	\
	FileSelection.custom		\
	FileSystemModel.custom		\
	Frame.custom			\
	HBox.custom			\
	HScale.custom			\
	IconFactory.custom		\
	IconSet.custom			\
	IconTheme.custom		\
	Image.custom			\
	ImageMenuItem.custom		\
	Init.custom			\
	InputDialog.custom		\
	ItemFactory.custom		\
	Label.custom			\
	Layout.custom			\
	ListStore.custom		\
	MessageDialog.custom		\
	Menu.custom			\
	MenuItem.custom			\
	Notebook.custom			\
	Object.custom			\
	Plug.custom			\
	RadioButton.custom		\
	RadioMenuItem.custom		\
	ScrolledWindow.custom		\
	SelectionData.custom		\
	Settings.custom			\
	SpinButton.custom		\
	Stock.custom			\
	StockItem.custom		\
	Style.custom			\
	Table.custom			\
	TargetEntry.custom		\
	TargetList.custom		\
	TextBuffer.custom		\
	TextChildAnchor.custom		\
	TextIter.custom			\
	TextTag.custom			\
	TextView.custom			\
	TreeIter.custom			\
	TreeModel.custom		\
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

add_dist = makefile.win32 gtk-sharp-2.0.pc.in

include ../Makefile.include

