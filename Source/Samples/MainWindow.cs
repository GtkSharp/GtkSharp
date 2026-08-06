// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using Gtk;
using GtkSource;
using System;
using System.Collections.Generic;
using System.IO;

namespace Samples
{
    // Public so the test project can construct it, as the real program does.
    public class MainWindow : Window
    {
        private HeaderBar _headerBar;
        private TreeView _treeView;
        private Box _boxContent;
        private TreeStore _store;
        private Dictionary<string, (Type type, Widget widget)> _items;
        private SourceView _textViewCode;
        private Notebook _notebook;

        public MainWindow() : base()
        {
            // Setup GUI. Gtk 4 has no WindowType -- a GtkWindow is always a
            // toplevel -- and no WindowPosition, because positioning windows is
            // the compositor's business, not the application's.
            Title = "GtkSharp Sample Application";
            SetDefaultSize(800, 600);

            _headerBar = new HeaderBar();
            _headerBar.ShowTitleButtons = true;

            // A button shows an icon by setting IconName; Gtk 3 needed a child
            // Image widget and AlwaysShowImage to defeat the theme's setting.
            var btnClickMe = new Button();
            btnClickMe.IconName = "document-new-symbolic";
            _headerBar.PackStart(btnClickMe);

            Titlebar = _headerBar;

            var hpanned = new Paned(Orientation.Horizontal);
            hpanned.Position = 200;

            _treeView = new TreeView();
            _treeView.HeadersVisible = false;
            // Pack1/Pack2 became StartChild/EndChild, with the resize and
            // shrink flags now properties of the paned itself.
            hpanned.StartChild = _treeView;
            hpanned.ResizeStartChild = false;
            hpanned.ShrinkStartChild = true;

            _notebook = new Notebook();

            var scroll1 = new ScrolledWindow();
            var vpanned = new Paned(Orientation.Vertical);
            vpanned.Position = 300;
            _boxContent = new Box(Orientation.Vertical, 0);
            SetAllMargins(_boxContent, 8);
            vpanned.StartChild = _boxContent;
            vpanned.ResizeStartChild = true;
            vpanned.ShrinkStartChild = true;
            // ApplicationOutput is a singleton, so a second MainWindow would be
            // handed a widget that still belongs to the first one. Gtk 4 refuses
            // to re-parent in place -- gtk_paned_set_end_child asserts the child
            // has no parent -- so it has to be detached first.
            var output = ApplicationOutput.Widget;
            if (output.Parent != null)
                output.Unparent();
            vpanned.EndChild = output;
            vpanned.ResizeEndChild = false;
            vpanned.ShrinkEndChild = true;
            scroll1.Child = vpanned;
            _notebook.AppendPage(scroll1, new Label { Text = "Data" });

            var scroll2 = new ScrolledWindow();

            _textViewCode = new SourceView();
            _textViewCode.ShowLineNumbers = true;
            _textViewCode.Buffer.Language = new LanguageManager().GetLanguage("c-sharp");

            SetAllMargins(_textViewCode, 3);
            scroll2.Child = _textViewCode;
            _notebook.AppendPage(scroll2, new Label { Text = "Code" });

            hpanned.EndChild = _notebook;
            hpanned.ResizeEndChild = true;
            hpanned.ShrinkEndChild = true;

            Child = hpanned;

            // Fill up data
            FillUpTreeView();

            // Connect events
            _treeView.Selection.Changed += Selection_Changed;

            // Gtk 4 removed GtkWidget::destroy. CloseRequest is the signal a
            // window gets when the user asks to close it; returning false lets
            // the default handler proceed with the close.
            CloseRequest += (sender, e) => {
                Application.Quit();
                e.RetVal = false;
            };
        }

        // Gtk 4 has no single Margin property; the four edges are separate.
        static void SetAllMargins(Widget widget, int margin)
        {
            widget.MarginTop = margin;
            widget.MarginBottom = margin;
            widget.MarginStart = margin;
            widget.MarginEnd = margin;
        }

        private void Selection_Changed(object sender, System.EventArgs e)
        {
            if (_treeView.Selection.GetSelected(out TreeIter iter))
            {
                var s = _store.GetValue(iter, 0).ToString();

                // Gtk 4 has no Children array; a widget's children are a
                // sibling list reached from FirstChild.
                while (_boxContent.FirstChild != null)
                    _boxContent.Remove(_boxContent.FirstChild);
                _notebook.CurrentPage = 0;
                _notebook.ShowTabs = false;

                if (_items.TryGetValue(s, out var item))
                {
                    _notebook.ShowTabs = true;

                    if (item.widget == null)
                        _items[s] = item = (item.type, Activator.CreateInstance(item.type) as Widget);

                    using (var stream = typeof(ListSection).Assembly.GetManifestResourceStream("GtkSharp.Samples." + item.type.Name + ".cs"))
                        using (var reader = new StreamReader(stream))
                            _textViewCode.Buffer.Text = reader.ReadToEnd();

                    item.widget.Vexpand = true;
                    _boxContent.Append(item.widget);
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
                        _store.AppendValues(dict[a.Category], a.ContentType.Name);
                        _items[a.ContentType.Name] = (type, null);
                    }
                }
            }

            _treeView.ExpandAll();
        }
    }
}
