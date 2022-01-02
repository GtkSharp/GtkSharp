using System.Collections.Generic;
using Gtk;

namespace Samples
{
	[Section(ContentType = typeof(ListStore), Category = Category.Widgets)]
	class ListStoreSection : Box
	{
		private readonly TreeView _tree;
		private ListStore _model;
        private readonly List<Bug> _data = new List<Bug>
        {
              new Bug ( false, 60482, "Normal",      "scrollable notebooks and hidden tabs" ),
              new Bug ( false, 60620, "Critical",    "gdk_window_clear_area (gdkwindow-win32.c) is not thread-safe" ),
              new Bug ( false, 50214, "Major",       "Xft support does not clean up correctly" ),
              new Bug ( true,  52877, "Major",       "GtkFileSelection needs a refresh method. " ),
              new Bug ( false, 56070, "Normal",      "Can't click button after setting in sensitive" ),
              new Bug ( true,  56355, "Normal",      "GtkLabel - Not all changes propagate correctly" ),
              new Bug ( false, 50055, "Normal",      "Rework width/height computations for TreeView" ),
              new Bug ( false, 58278, "Normal",      "gtk_dialog_set_response_sensitive () doesn't work" ),
              new Bug ( false, 55767, "Normal",      "Getters for all setters" ),
              new Bug ( false, 56925, "Normal",      "Gtkcalender size" ),
              new Bug ( false, 56221, "Normal",      "Selectable label needs right-click copy menu" ),
              new Bug ( true,  50939, "Normal",      "Add shift clicking to GtkTextView" ),
              new Bug ( false, 6112,  "Enhancement", "netscape-like collapsable toolbars" ),
              new Bug ( false, 1,     "Normal",      "First bug :=)" ),
        };

        public ListStoreSection() : base(Orientation.Vertical, 3)
        {
            CreateModel();
            _tree = new TreeView(_model);
            AddColumn();

            var treeScroll = new ScrolledWindow
            {
                Expand = true
            };
            treeScroll.Add(_tree);

            PackStart(treeScroll, true, true, 0);

            GLib.Timeout.Add(100, SpinerTimeout);
        }

		private enum Column
        {
            Fixed,
            Number,
            Severity,
            Description,
            Pulse,
            Icon,
            Active,
            Sensitive,
            Num
        };

        private readonly struct Bug
		{
            public bool Fixed { get; }
            public int Number { get; }
            public string Severity { get; }
            public string Description { get; }

            public Bug(bool isFixed, int number, string severity, string description)
			{
                Fixed = isFixed;
                Number = number;
                Severity = severity;
                Description = description;
			}
        }

        private void CreateModel()
        {
            /* create list store */
            _model = new ListStore(typeof(bool), typeof(int), typeof(string), typeof(string), typeof(int), typeof(string), typeof(bool), typeof(bool));

            /* add data to the list store */
            for (int i = 0; i < _data.Count; i++)
            {
                string iconName;
                bool sensitive;

                if (i == 1 || i == 3)
                    iconName = "battery-caution-charging-symbolic";
                else
                    iconName = null;
                if (i == 3)
                    sensitive = false;
                else
                    sensitive = true;
                TreeIter iter = _model.Append();
                _model.SetValue(iter, (int)Column.Fixed, _data[i].Fixed);
                _model.SetValue(iter, (int)Column.Number, _data[i].Number);
                _model.SetValue(iter, (int)Column.Severity, _data[i].Severity);
                _model.SetValue(iter, (int)Column.Description, _data[i].Description);
                _model.SetValue(iter, (int)Column.Pulse, 5);
                _model.SetValue(iter, (int)Column.Icon, iconName);
                _model.SetValue(iter, (int)Column.Active, false);
                _model.SetValue(iter, (int)Column.Sensitive, sensitive);
            }
        }

        private void AddColumn()
        {
            CellRenderer renderer;
            TreeViewColumn column;

            /* column for fixed toggles */
            var rendererToggle = new CellRendererToggle();
            rendererToggle.Toggled += RendererToggle_Toggled;
            column = new TreeViewColumn("Fixed?", rendererToggle, "active", Column.Fixed, null)
            {
                /* set this column to a fixed sizing (of 50 pixels) */
                FixedWidth = 50
            };
            _tree.AppendColumn(column);

            /* column for bug numbers */
            renderer = new CellRendererText();
            column = new TreeViewColumn("Bug number", renderer, "text", Column.Number, null);
            column.SortColumnId = (int)Column.Number;
            _tree.AppendColumn(column);

            /* column for severities */
            renderer = new CellRendererText();
            column = new TreeViewColumn("Severity", renderer, "text", Column.Severity, null)
            {
                SortColumnId = (int)Column.Severity
            };
            _tree.AppendColumn(column);

            /* column for description */
            renderer = new CellRendererText();
            column = new TreeViewColumn("Description", renderer, "text", Column.Description, null)
            {
                SortColumnId = (int)Column.Description
            };
            _tree.AppendColumn(column);

            /* column for spinner */
            renderer = new CellRendererSpinner();
            column = new TreeViewColumn("Spinning", renderer, "pulse", Column.Pulse, "active", Column.Active, null)
            {
                SortColumnId = (int)Column.Pulse
            };
            _tree.AppendColumn(column);

            /* column for symbolic icon */
            renderer = new CellRendererPixbuf();
            column = new TreeViewColumn("Symbolic icon", renderer, "icon-name", Column.Icon, "sensitive", Column.Sensitive, null);
            column.SortColumnId = (int)Column.Icon;
            _tree.AppendColumn(column);
        }

        private void RendererToggle_Toggled(object o, ToggledArgs args)
        {
            TreePath path = new TreePath(args.Path);

            /* get toggled iter */
            TreeIter iter;
            _model.GetIter(out iter, path);
            bool isFixed = (bool)_model.GetValue(iter, (int)Column.Fixed);

            /* do something with the value */
            isFixed ^= true;

            /* set new value */
            _model.SetValue(iter, (int)Column.Fixed, isFixed);
        }

        private bool SpinerTimeout()
        {
            if (_model == null)
            {
                return false;
            }

			_model.GetIterFirst(out TreeIter iter);
            int pulse = (int)_model.GetValue(iter, (int)Column.Pulse);
            if (pulse == int.MaxValue)
                pulse = 0;
            else
                pulse++;

            _model.SetValues(iter, (int)Column.Pulse, pulse);
            _model.SetValue(iter, (int)Column.Active, true);

            return true;
        }
	}
}
