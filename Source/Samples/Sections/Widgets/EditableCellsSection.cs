using System;
using System.Collections.Generic;
using Gtk;

namespace Samples.Sections.Widgets
{
	[Section(ContentType = typeof(EditableCellsSection), Category = Category.Widgets)]
	class EditableCellsSection : Box
	{
		private readonly TreeView _treeView;
		private readonly ListStore _itemsModel;
		private readonly Dictionary<CellRenderer, int> _cellColumnsRender;
		private List<Item> _articles;

		public EditableCellsSection() : base(Orientation.Vertical, 3)
		{
			_cellColumnsRender = new Dictionary<CellRenderer, int>();
			ListStore numbers_model;

			ScrolledWindow sw = new ScrolledWindow
			{
				ShadowType = ShadowType.EtchedIn
			};
			sw.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);

			this.PackStart(sw, true, true, 0);

			/* create models */
			_itemsModel = CreateItemsModel();
			numbers_model = CreateNumbersModel();

			/* create tree view */
			_treeView = new TreeView(_itemsModel);
			_treeView.Selection.Mode = SelectionMode.Single;

			AddColumns(numbers_model);

			sw.Add(_treeView);

			/* some buttons */
			Box hbox = new Box(Orientation.Horizontal, 4)
			{
				Homogeneous = true
			};
			this.PackStart(hbox, false, false, 0);

			Button button = new Button("Add item");
			button.Clicked += AddItem;
			hbox.PackStart(button, true, true, 0);

			button = new Button("Remove item");
			button.Clicked += RemoveItem;
			hbox.PackStart(button, true, true, 0);
		}

		private class Item
		{
			public int Number;
			public string Product;
			public int Yummy;
		}

		private enum ColumnItem
		{
			Number,
			Product,
			Yummy,
			Num
		};

		private enum ColumnNumber
		{
			Text,
			Num
		};

		private ListStore CreateItemsModel()
		{
			ListStore model;
			TreeIter iter;

			/* create array */
			_articles = new List<Item>();

			AddItems();

			/* create list store */
			model = new ListStore(typeof(int), typeof(string), typeof(int), typeof(bool));

			/* add items */
			for (int i = 0; i < _articles.Count; i++)
			{
				iter = model.Append();
				model.SetValue(iter, (int)ColumnItem.Number, _articles[i].Number);
				model.SetValue(iter, (int)ColumnItem.Product, _articles[i].Product);
				model.SetValue(iter, (int)ColumnItem.Yummy, _articles[i].Yummy);
			}

			return model;
		}

		private static ListStore CreateNumbersModel()
		{
			ListStore model;
			TreeIter iter;

			/* create list store */
			model = new ListStore(typeof(string), typeof(int));

			/* add numbers */
			for (int i = 0; i < 10; i++)
			{
				iter = model.Append();
				model.SetValue(iter, (int)ColumnNumber.Text, i.ToString());
			}

			return model;
		}

		private void AddItems()
		{
			Item foo = new Item
			{
				Number = 3,
				Product = "bottles of coke",
				Yummy = 20
			};
			_articles.Add(foo);

			foo = new Item
			{
				Number = 5,
				Product = "packages of noodles",
				Yummy = 50
			};
			_articles.Add(foo);

			foo = new Item
			{
				Number = 2,
				Product = "packages of chocolate chip cookies",
				Yummy = 90
			};
			_articles.Add(foo);

			foo = new Item
			{
				Number = 1,
				Product = "can vanilla ice cream",
				Yummy = 60
			};
			_articles.Add(foo);

			foo = new Item
			{
				Number = 6,
				Product = "eggs",
				Yummy = 10
			};
			_articles.Add(foo);
		}

		private void AddColumns(ITreeModel numbersModel)
		{
			/* number column */
			CellRendererCombo rendererCombo = new CellRendererCombo
			{
				Model = numbersModel,
				TextColumn = (int)ColumnNumber.Text,
				HasEntry = false,
				Editable = true
			};
			rendererCombo.Edited += CellEdited;
			rendererCombo.EditingStarted += EditingStarted;
			_cellColumnsRender.Add(rendererCombo, (int)ColumnItem.Number);

			_treeView.InsertColumn(-1, "Number", rendererCombo, "text", (int)ColumnItem.Number);

			/* product column */
			CellRendererText rendererText = new CellRendererText
			{
				Editable = true
			};
			rendererText.Edited += CellEdited;
			_cellColumnsRender.Add(rendererText, (int)ColumnItem.Product);

			_treeView.InsertColumn(-1, "Product", rendererText, "text", (int)ColumnItem.Product);

			/* yummy column */
			CellRendererProgress rendererProgress = new CellRendererProgress();
			_cellColumnsRender.Add(rendererProgress, (int)ColumnItem.Yummy);

			_treeView.InsertColumn(-1, "Yummy", rendererProgress, "value", (int)ColumnItem.Yummy);
		}

		private void AddItem(object sender, EventArgs e)
		{
			TreeIter iter;

			if (_articles == null)
			{
				return;
			}

			Item foo = new Item
			{
				Number = 0,
				Product = "Description here",
				Yummy = 50
			};
			_articles.Add(foo);

			/* Insert a new row below the current one */
			_treeView.GetCursor(out TreePath path, out _);
			if (path != null)
			{
				_ = _itemsModel.GetIter(out TreeIter current, path);
				iter = _itemsModel.InsertAfter(current);
			}
			else
			{
				iter = _itemsModel.Insert(-1);
			}

			/* Set the data for the new row */
			_itemsModel.SetValue(iter, (int)ColumnItem.Number, foo.Number);
			_itemsModel.SetValue(iter, (int)ColumnItem.Product, foo.Product);
			_itemsModel.SetValue(iter, (int)ColumnItem.Yummy, foo.Yummy);

			/* Move focus to the new row */
			path = _itemsModel.GetPath(iter);
			TreeViewColumn column = _treeView.GetColumn(0);
			_treeView.SetCursor(path, column, false);
		}

		private void RemoveItem(object sender, EventArgs e)
		{
			TreeSelection selection = _treeView.Selection;

			if (selection.GetSelected(out TreeIter iter))
			{
				TreePath path = _itemsModel.GetPath(iter);
				int i = path.Indices[0];
				_itemsModel.Remove(ref iter);
				_articles.RemoveAt(i);
			}
		}

		private void CellEdited(object data, EditedArgs args)
		{
			TreePath path = new TreePath(args.Path);
			int column = _cellColumnsRender[(CellRenderer)data];
			_itemsModel.GetIter(out TreeIter iter, path);

			switch (column)
			{
				case (int)ColumnItem.Number:
					{
						int i = path.Indices[0];
						_articles[i].Number = int.Parse(args.NewText);

						_itemsModel.SetValue(iter, column, _articles[i].Number);
					}
					break;

				case (int)ColumnItem.Product:
					{
						string oldText = (string)_itemsModel.GetValue(iter, column);
						int i = path.Indices[0];
						_articles[i].Product = args.NewText;

						_itemsModel.SetValue(iter, column, _articles[i].Product);
					}
					break;
			}
		}

		private void EditingStarted(object o, EditingStartedArgs args)
		{
			((ComboBox)args.Editable).RowSeparatorFunc += SeparatorRow;
		}

		private bool SeparatorRow(ITreeModel model, TreeIter iter)
		{
			TreePath path = model.GetPath(iter);
			int idx = path.Indices[0];

			return idx == 5;
		}
	}
}
