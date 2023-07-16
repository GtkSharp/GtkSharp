// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using System;
using Gtk;

namespace Samples
{
    [Section(ContentType = typeof(TreeView), Category = Category.Widgets)]
    class TreeViewSection : Box
    {
        const int ColumnIndex = 0;
        const int ColumnName = 1;
        const int ColumnIcon = 2;

        TreeView tree;
        TreeStore store;
        readonly Entry entry;
        readonly Gdk.Pixbuf icon = new Gdk.Pixbuf(typeof(ImageSection).Assembly, "Testpic", 32, 32);

        public TreeViewSection() : base(Orientation.Vertical, 3)
        {
            CreateTreeView();

            var treeScroll = new ScrolledWindow();
            treeScroll.Expand = true;
            treeScroll.Add(tree);

            var boxEdit = new Box(Orientation.Horizontal, 3);

            var btn1 = new Button() { Label = "Add" };
            btn1.Clicked += OnAddClicked;

            var btn2 = new Button() { Label = "Edit" };
            btn2.Clicked += OnEditClicked;

            var btn3 = new Button() { Label = "Remove" };
            btn3.Clicked += OnRemoveClicked;

            entry = new Entry();

            boxEdit.PackStart(entry, true, true, 0);
            boxEdit.PackStart(btn1, false, true, 0);
            boxEdit.PackStart(btn2, false, true, 0);
            boxEdit.PackStart(btn3, false, true, 0);

            PackStart(boxEdit, false, true, 0);
            PackStart(treeScroll, true, true, 0);
        }

        void CreateTreeView()
        {
            store = new TreeStore(typeof(int), typeof(string), typeof(Gdk.Pixbuf));
            store.RowInserted += OnStoreRowInserted;
            store.RowDeleted += OnStoreRowDeleted;
            store.RowChanged += OnStoreRowChanged;
            store.RowsReordered += OnStoreRowsReordered;
            store.RowHasChildToggled += OnStoreRowHasChildToggled;

            tree = new TreeView();

            var col = tree.AppendColumn("Index", new CellRendererText(), "text", ColumnIndex);
            col.Resizable = true;
            col.SortColumnId = 0;
            col = tree.AppendColumn("Name", new CellRendererText(), "text", ColumnName);
            col.Resizable = true;
            col.Expand = true;
            col.SortColumnId = 1;

            col = tree.AppendColumn("Icon", new CellRendererPixbuf(), "pixbuf", ColumnIcon);
            col.Resizable = true;
            col.Expand = true;
            col.Alignment = .5f;

            FillTreeView();

            tree.Model = store;
            tree.Selection.Changed += OnTreeSelectionChanged;
        }

        void FillTreeView()
        {
            int idx = 0;

            TreeIter it = store.InsertWithValues(-1, idx++, "Adam", null);
            store.InsertWithValues(it, -1, idx++, "Adam child 1", null);
            store.InsertWithValues(it, -1, idx++, "Adam child 2", icon);
            store.InsertWithValues(it, -1, idx++, "Adam child 3", null);

            store.InsertWithValues(-1, idx++, "Eve", null);
            store.InsertWithValues(-1, idx++, "Zack", null);
            store.InsertWithValues(-1, idx++, "John", icon);

            it = store.InsertWithValues(-1, idx++, "Amy", null);
            store.InsertWithValues(it, -1, idx++, "Amy child 1", null);
            store.InsertWithValues(it, -1, idx++, "Amy child 2", null);

            store.InsertWithValues(-1, idx++, "William", null);
            store.InsertWithValues(-1, idx++, "Evelyn", icon);
            store.InsertWithValues(-1, idx++, "Wyatt", null);
        }

        private void OnTreeSelectionChanged(object sender, EventArgs e)
        {
            if (!tree.Selection.GetSelected(out TreeIter it))
                return;

            TreePath path = store.GetPath(it);

            var name = (string)store.GetValue(it, ColumnName);
            entry.Text = name;

            ApplicationOutput.WriteLine(sender, $"SelectionChanged, path {path}, name {name}");
        }

        private void OnStoreRowInserted(object sender, RowInsertedArgs args)
        {
            var name = (string)store.GetValue(args.Iter, ColumnName);
            ApplicationOutput.WriteLine(sender, $"RowInserted, path {args.Path}, name {name}");
        }

        private void OnStoreRowDeleted(object sender, RowDeletedArgs args)
        {
            ApplicationOutput.WriteLine(sender, $"RowDeleted, path {args.Path}");
        }

        private void OnStoreRowChanged(object sender, RowChangedArgs args)
        {
            var name = (string)store.GetValue(args.Iter, ColumnName);
            ApplicationOutput.WriteLine(sender, $"RowChanged, path {args.Path}, name {name}");
        }

        private void OnStoreRowsReordered(object sender, RowsReorderedArgs args)
        {
            ApplicationOutput.WriteLine(sender, $"RowsReordered, path {args.Path}");
        }

        private void OnStoreRowHasChildToggled(object sender, RowHasChildToggledArgs args)
        {
            var name = (string)store.GetValue(args.Iter, ColumnName);
            ApplicationOutput.WriteLine(sender, $"RowHasChildToggled, path {args.Path}, name {name}");
        }

        private void OnAddClicked(object sender, EventArgs e)
        {
            if (!tree.Selection.GetSelected(out TreeIter it))
                return;

            string txt = entry.Text.Trim();
            if (string.IsNullOrEmpty(txt))
                return;

            int idx = Environment.TickCount % 100;
            store.InsertWithValues(it, -1, idx, txt, null);
        }

        private void OnEditClicked(object sender, EventArgs e)
        {
            if (!tree.Selection.GetSelected(out TreeIter it))
                return;

            string txt = entry.Text.Trim();
            if (string.IsNullOrEmpty(txt))
                return;

            store.SetValue(it, ColumnName, txt);
        }

        private void OnRemoveClicked(object sender, EventArgs e)
        {
            if (!tree.Selection.GetSelected(out TreeIter it))
                return;

            store.Remove(ref it);
        }
    }
}