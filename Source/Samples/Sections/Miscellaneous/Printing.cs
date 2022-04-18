using Gtk;
using System;
using System.IO;

namespace Samples.Sections.Miscellaneous
{
    [Section(ContentType = typeof(Printing), Category = Category.Miscellaneous)]
    class Printing : Box
    {
        private const double HEADER_HEIGHT = 10*72/25.4;
        private const double HEADER_GAP = 3*72/25.4;
        private const string GTK_PRINT_SETTINGS_OUTPUT_BASENAME = "output-basename";
        private PrintData data;

        private class PrintData
        {
            public string Resourcename { get; set; }
            public double FontSize { get; set; }

            public int LinesPerPage { get; set; }
            public string[] Lines { get; set; }
            public int NumLines { get; set; }
            public int NumPages { get; set; }
        }

        public Printing() : base(Orientation.Vertical, 3)
        {
            Button button = new Button("Printing");
            button.Clicked += Button_Clicked;
            this.Add(button);
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            DoPrinting();
        }

        private void DoPrinting()
        {
            data = new PrintData
            {
                Resourcename = @"\Source\Samples\Sections\Miscellaneous\Printing.cs",
                FontSize = 12.0
            };

            PrintOperation operation = new PrintOperation
            {
                UseFullPage = false,
                Unit = Unit.Points,
                EmbedPageSetup = true
            };
            operation.BeginPrint += Operation_BeginPrint;
            operation.DrawPage += Operation_DrawPage;

            PrintSettings settings = new PrintSettings();
            settings.Set(GTK_PRINT_SETTINGS_OUTPUT_BASENAME, "gtk-demo");
            
            operation.PrintSettings = settings;

            Gtk.Window parentWindow = (Window)this.Parent.Parent.Parent.Parent.Parent.Parent.Parent;
            PrintOperationResult result = operation.Run(PrintOperationAction.PrintDialog, parentWindow);

            operation.Dispose();
            settings.Dispose();

            if (result == PrintOperationResult.Error)
            {
                MessageDialog dialog = new MessageDialog(parentWindow,
                                                        DialogFlags.DestroyWithParent,
                                                        MessageType.Error,
                                                        ButtonsType.Close,
                                                        string.Format("PrintOperationResult = {0}", result.ToString()));
                dialog.Response += Dialog_Response;

                dialog.Show();
            }
        }

        private void Dialog_Response(object o, ResponseArgs args)
        {
            ((Widget)o).Destroy();
        }

        private void Operation_BeginPrint(object printOperation, BeginPrintArgs args)
        {
            double height = args.Context.Height - HEADER_HEIGHT - HEADER_GAP;

            data.Lines = File.ReadAllLines(Directory.GetCurrentDirectory() + @"\..\.." + data.Resourcename);
            data.LinesPerPage = (int)Math.Floor(height / data.FontSize);
            data.NumLines = data.Lines.Length;
            data.NumPages = (data.NumLines - 1) / data.LinesPerPage + 1;
            
            ((PrintOperation)printOperation).NPages = data.NumPages;
        }

        private void Operation_DrawPage(object printOperation, DrawPageArgs args)
        {
            PrintContext context = args.Context;
            Cairo.Context cr = context.CairoContext;
            
            double width = context.Width;
            cr.Rectangle(0, 0, width, HEADER_HEIGHT);

            cr.SetSourceRGB(0.8, 0.8, 0.8);
            cr.FillPreserve();

            cr.SetSourceRGB(0, 0, 0);
            cr.LineWidth = 1;
            cr.Stroke();

            Pango.FontDescription desc = Pango.FontDescription.FromString("sans 14");
            Pango.Layout layout = context.CreatePangoLayout();
            layout.FontDescription = desc;

            layout.SetText(data.Resourcename);
            layout.GetPixelSize(out int textWidth, out int textHeight);

            if (textWidth > width)
            {
                layout.Width = (int)width;
                layout.Ellipsize = Pango.EllipsizeMode.Start;
                layout.GetPixelSize(out textWidth, out textHeight);
            }

            cr.MoveTo((width - textWidth) / 2, (HEADER_HEIGHT - textHeight) / 2);
            Pango.CairoHelper.ShowLayout(cr, layout);

            string page_str = string.Format("{0}/{1}", args.PageNr + 1, data.NumPages);
            layout.SetText(page_str);

            layout.Width = -1;
            layout.GetPixelSize(out textWidth, out textHeight);
            cr.MoveTo(width - textWidth - 4, (HEADER_HEIGHT - textHeight) / 2);
            Pango.CairoHelper.ShowLayout(cr, layout);

            layout = context.CreatePangoLayout();

            desc = Pango.FontDescription.FromString("monospace");
            desc.Size = (int)(data.FontSize * Pango.Scale.PangoScale);
            layout.FontDescription = desc;

            cr.MoveTo(0, HEADER_HEIGHT + HEADER_GAP);
            int line = args.PageNr * data.LinesPerPage;
            for (int i = 0; i < data.LinesPerPage && line < data.NumLines; i++)
            {
                layout.SetText(data.Lines[line]);
                Pango.CairoHelper.ShowLayout(cr, layout);
                cr.RelMoveTo(0, data.FontSize);
                line++;
            }
            layout.Dispose();
        }
    }
}
