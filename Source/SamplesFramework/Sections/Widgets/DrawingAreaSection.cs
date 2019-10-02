// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

// Ported from https://developer.gnome.org/gtkmm-tutorial/3.24/gtkmm-tutorial.html

using Cairo;
using Gtk;
using System;

namespace Samples
{
    [Section(ContentType = typeof(DrawingArea), Category = Category.Widgets)]
    class DrawingAreaSection : ListSection
    {
        public DrawingAreaSection()
        {
            AddItem(DrawCircles());
            AddItem(DrawText());
        }

        public (string, Widget) DrawCircles()
        {
            DrawingArea drawingArea = new DrawingArea();
            drawingArea.SetSizeRequest(50,50);
            drawingArea.Drawn += DrawingAreaCirclesOnDrawn;

            return ("Draw Circles :", drawingArea);
        }

        public (string, Widget) DrawText()
        {
            DrawingArea drawingArea = new DrawingArea();
            drawingArea.SetSizeRequest(200,100);
            drawingArea.Drawn += DrawingAreaTextOnDrawn;

            return ("Draw Pango Text :", drawingArea);
        }
        
        private void DrawingAreaCirclesOnDrawn(object o, DrawnArgs args)
        {
            if (o is DrawingArea da)
            {
                Context cr = args.Cr;
                
                int width = da.Allocation.Width;
                int height = da.Allocation.Height;
                
                cr.LineWidth = 5;
                cr.SetSourceRGB(0.8, 0.8, 0.8);
                
                cr.Rectangle(0, 0, width, height);
                cr.Fill();
                
                cr.SetSourceRGB(0.9, 0, 0);
                cr.Translate(width/2d, height/2d);
                cr.Arc(0, 0, (width < height ? width : height) / 2 - 10, 0, 2*Math.PI);
                cr.StrokePreserve();
                
                cr.SetSourceRGB(0, 0.9, 0);
                cr.Fill();
                
                cr.GetTarget().Dispose();
                cr.Dispose();
            } 
        }
                
        private void DrawingAreaTextOnDrawn(object o, DrawnArgs args)
        {
            if (o is DrawingArea da)
            {
                Context cr = args.Cr;

                int width = da.Allocation.Width;
                int height = da.Allocation.Height;
                
                int rectangle_width = width;
                int rectangle_height = height / 2;
                
                // Draw a black rectangle
                cr.SetSourceRGB(0, 0, 0);
                cr.Rectangle(0, 0, rectangle_width, rectangle_height);
                cr.Fill();
                
                // and some white text
                cr.SetSourceRGB(1.0, 1.0, 1.0);
                draw_text(cr, rectangle_width, rectangle_height, "Hi there!");
                
                // flip the image vertically
                // see http://www.cairographics.org/documentation/cairomm/reference/classCairo_1_1Matrix.html
                // the -1 corresponds to the yy part (the flipping part)
                // the height part is a translation (we could have just called cr->translate(0, height) instead)
                // it's height and not height / 2, since we want this to be on the second part of our drawing
                // (otherwise, it would draw over the previous part)
                Cairo.Matrix matrix = new Matrix(1.0, 0.0, 0.0, -1.0, 0.0, height);
                
                // apply the matrix
                cr.Transform(matrix);
                
                // white rectangle
                cr.SetSourceRGB(1.0, 1.0, 1.0);
                cr.Rectangle(0, 0, rectangle_width, rectangle_height);
                cr.Fill();

                // black text
                cr.SetSourceRGB(0, 0, 0);
                draw_text(cr, rectangle_width, rectangle_height, "Hi there!");
                
                cr.GetTarget().Dispose();
                cr.Dispose();
            } 
        }
        
        private void draw_text(Context cr, int rectangle_width, int rectangle_height, string text)
        {
            // http://developer.gnome.org/pangomm/unstable/classPango_1_1FontDescription.html
            Pango.FontDescription font = new Pango.FontDescription();

            font.Family = "Monospace";
            font.Weight = Pango.Weight.Bold;

            // http://developer.gnome.org/pangomm/unstable/classPango_1_1Layout.html
            Pango.Layout layout = CreatePangoLayout(text);

            layout.FontDescription = font;

            int text_width;
            int text_height;

            //get the text dimensions (it updates the variables -- by reference)
            layout.GetPixelSize(out text_width, out text_height);

            // Position the text in the middle
            cr.MoveTo((rectangle_width - text_width) / 2d, (rectangle_height - text_height) / 2d);

            Pango.CairoHelper.ShowLayout(cr, layout);
        }
    }
}
