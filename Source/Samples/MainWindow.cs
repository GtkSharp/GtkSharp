using Gtk;
using System;
using System.Collections.Generic;

namespace Samples
{
    class MainWindow : Window
    {
        private HeaderBar _headerBar;
        private TreeView _treeView;
        private Box _boxContent;
        private TreeStore _store;
        private Dictionary<string, (Type type, Widget widget)> _items;

        public MainWindow() : base(WindowType.Toplevel)
        {
            // Setup GUI
            WindowPosition = WindowPosition.Center;
            DefaultSize = new Gdk.Size(800, 600);

            _headerBar = new HeaderBar();
            _headerBar.ShowCloseButton = true;
            _headerBar.Title = "GtkSharp Sample Application";

            var btnClickMe = new Button();
            btnClickMe.AlwaysShowImage = true;
            btnClickMe.Image = Image.NewFromIconName("document-new-symbolic", IconSize.Button);
            _headerBar.PackStart(btnClickMe);

            Titlebar = _headerBar;

            var hpanned = new HPaned();
            hpanned.Position = 200;

            _treeView = new TreeView();
            _treeView.HeadersVisible = false;
            hpanned.Pack1(_treeView, false, true);

            var vpanned = new VPaned();
            vpanned.Position = 400;

            _boxContent = new Box(Orientation.Vertical, 0);
            _boxContent.Margin = 8;
            vpanned.Pack1(_boxContent, true, true);

            vpanned.Pack2(ApplicationOutput.Widget, false, true);

            hpanned.Pack2(vpanned, true, true);

            Child = hpanned;

            // Fill up data
            FillUpTreeView();

            // Connect events
            _treeView.Selection.Changed += Selection_Changed;
            Destroyed += (sender, e) => Application.Quit();
        }

        private void Selection_Changed(object sender, EventArgs e)
        {
            if (_treeView.Selection.GetSelected(out TreeIter iter))
            {
                var s = _store.GetValue(iter, 0).ToString();

                while (_boxContent.Children.Length > 0)
                    _boxContent.Remove(_boxContent.Children[0]);

                if (_items.TryGetValue(s, out var item))
                {
                    if (item.widget == null)
                        _items[s] = item = (item.type, Activator.CreateInstance(item.type) as Widget);
                    
                    _boxContent.PackStart(item.widget, true, true, 0);
                    _boxContent.ShowAll();
                }

            }
        }

        private void FillUpTreeView()
        {
            // Init cells
            var cellName = new CellRendererText();

            // Init columns
            var columeSections = new TreeViewColumn();
            columeSections.Title = "Sections";
            columeSections.PackStart(cellName, true);

            columeSections.AddAttribute(cellName, "text", 0);

            _treeView.AppendColumn(columeSections);

            // Init treeview
            _store = new TreeStore(typeof(string));
            _treeView.Model = _store;

            // Setup category base
            var dict = new Dictionary<Category, TreeIter>();
            foreach (var category in Enum.GetValues(typeof(Category)))
                dict[(Category)category] = _store.AppendValues(category.ToString());

            // Fill up categories
            _items = new Dictionary<string, (Type type, Widget widget)>();
            var maintype = typeof(SectionAttribute);

            foreach (var type in maintype.Assembly.GetTypes())
            {
                foreach (var attribute in type.GetCustomAttributes(true))
                {
                    if (attribute is SectionAttribute a)
                    {
                        _store.AppendValues(dict[a.Category], a.Name);
                        _items[a.Name] = (type, null);
                    }
                }
            }

            _treeView.ExpandAll();
        }
    }
}