pkg = 'gtk'
assembly_name = pkg + '-sharp'
symbols = join_paths(meson.current_source_dir(), pkg + '-symbols.xml')

raw_api_fname = join_paths(meson.current_source_dir(), pkg + '-api.raw')
metadata = files('Gtk.metadata')
glueincludes = 'gtk/gtk.h'
gluefile = join_paths(meson.current_build_dir(), 'generate.c')

configure_file(input: assembly_name + '.dll.config.in',
        output: assembly_name + '.dll.config',
        configuration : remap_dl_data)

if install
    configure_file(input: assembly_name + '-3.0.pc.in',
            output: assembly_name + '-3.0.pc',
            configuration : version_data,
            install_dir: pkg_install_dir)
endif

subdir('generated')

sources = [
    'Accel.cs',
    'AccelKey.cs',
    'Action.cs',
    'ActionEntry.cs',
    'ActionGroup.cs',
    'Adjustment.cs',
    'Application.cs',
    'ArrayExtensions.cs',
    'Bin.cs',
    'BindingAttribute.cs',
    'Builder.cs',
    'Button.cs',
    'Calendar.cs',
    'CellAreaBox.cs',
    'CellLayoutAdapter.cs',
    'CellRenderer.cs',
    'CellView.cs',
    'CheckMenuItem.cs',
    'ChildPropertyAttribute.cs',
    'Clipboard.cs',
    'ColorSelection.cs',
    'ComboBox.cs',
    'ComboBoxText.cs',
    'Container.cs',
    'Dialog.cs',
    'Drag.cs',
    'Entry.cs',
    'EntryCompletion.cs',
    'FileChooserDialog.cs',
    'FileChooserNative.cs',
    'Frame.cs',
    'Global.cs',
    'HBox.cs',
    'HScale.cs',
    'ICellLayout.cs',
    'IconFactory.cs',
    'IconSet.cs',
    'IconTheme.cs',
    'IconView.cs',
    'Image.cs',
    'ImageMenuItem.cs',
    'Init.cs',
    'ITreeModel.cs',
    'ITreeNode.cs',
    'Key.cs',
    'Label.cs',
    'ListStore.cs',
    'Menu.cs',
    'MenuItem.cs',
    'MessageDialog.cs',
    'NativeDialog.cs',
    'NodeCellDataFunc.cs',
    'NodeSelection.cs',
    'NodeStore.cs',
    'NodeView.cs',
    'Notebook.cs',
    'PaperSize.cs',
    'Plug.cs',
    'Printer.cs',
    'RadioAction.cs',
    'RadioActionEntry.cs',
    'RadioButton.cs',
    'RadioMenuItem.cs',
    'RadioToolButton.cs',
    'RecentManager.cs',
    'RowsReorderedHandler.cs',
    'ScrolledWindow.cs',
    'SelectionData.cs',
    'Settings.cs',
    'SpinButton.cs',
    'StatusIcon.cs',
    'Stock.cs',
    'StockItem.cs',
    'StockManager.cs',
    'Style.cs',
    'StyleContext.cs',
    'StyleProviderPriority.cs',
    'Target.cs',
    'TargetEntry.cs',
    'TargetList.cs',
    'TextAttributes.cs',
    'TextBuffer.cs',
    'TextChildAnchor.cs',
    'TextIter.cs',
    'TextMark.cs',
    'TextTag.cs',
    'TextView.cs',
    'ThreadNotify.cs',
    'ToggleActionEntry.cs',
    'TreeEnumerator.cs',
    'TreeIter.cs',
    'TreeMenu.cs',
    'TreeModelAdapter.cs',
    'TreeModelFilter.cs',
    'TreeModelSort.cs',
    'TreeNode.cs',
    'TreeNodeAttribute.cs',
    'TreeNodeValueAttribute.cs',
    'TreePath.cs',
    'TreeSelection.cs',
    'TreeStore.cs',
    'TreeView.cs',
    'TreeViewColumn.cs',
    'UIManager.cs',
    'VBox.cs',
    'Viewport.cs',
    'VScale.cs',
    'Widget.cs',
    'Window.cs',
]

deps = [glib_sharp, pango_sharp, cairo_sharp, gio_sharp, atk_sharp, gdk_sharp]
gtk_sharp = library(assembly_name, source_gen, sources, assemblyinfo,
        cs_args: ['-unsafe', '-nowarn:0618,0612,0169'],
        link_with: deps,
        install: install,
        install_dir: lib_install_dir
)

nuget_infos += [['GtkSharp', gtk_sharp, ['GlibSharp', 'GioSharp',
    'AtkSharp', 'GdkSharp']]]
install_infos += [assembly_name, gtk_sharp.full_path()]
gtk_sharp_dep = declare_dependency(link_with: deps + [gtk_sharp])

if add_languages('c', required: false) and csc.get_id() == 'mono'
    c_abi_exe = executable(assembly_name + '_c_abi', c_abi,
            c_args: ['-Wno-deprecated', '-Wno-deprecated-declarations'],
            dependencies: [glib_dep, gio_dep, atk_dep, gdk_dep, gtk_dep])

    cs_abi_exe = executable(assembly_name + '_cs_abi', cs_abi,
            cs_args: ['-nowarn:169', '-nowarn:108', '-nowarn:114', '-nowarn:0618', '-unsafe'],
            dependencies: [gtk_sharp_dep])

    env = environment()
    env.prepend('MONO_PATH', mono_path)
    test(assembly_name + 'abi', diff, args: [c_abi_exe.full_path(), cs_abi_exe.full_path()],
            env: env)
else
    message('Not running tests ' + csc.get_id())
endif
