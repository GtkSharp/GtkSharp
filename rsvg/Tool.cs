//
// rsvg/Tool.cs - Rsvg Tool class
//
// Author: Charles Iliya Krempeaux <charles@reptile.ca>
//
// Copyright (C) 2003 Reptile Consulting & Services Ltd.
// Copyright (C) 2003 Charles Iliya Krempeaux.
//



// O B J E C T S ////////////////////////////////////////////////////////////////////////////////////////////////////////////

    namespace Rsvg {

        class Tool
        {


            // D L L   I M P O R T S ////////////////////////////////////////////////////////////////////////////////////////

                [System.Runtime.InteropServices.DllImport("rsvg-2")]
                static extern System.IntPtr rsvg_pixbuf_from_file( string file_name
                                                                 , out System.IntPtr error
                                                                 );

                [System.Runtime.InteropServices.DllImport("rsvg-2")]
                static extern
                System.IntPtr
                rsvg_pixbuf_from_file_at_zoom( string file_name
                                             , double x_zoom
                                             , double y_zoom
                                             , out System.IntPtr error
                                             );

                [System.Runtime.InteropServices.DllImport("rsvg-2")]
                static extern
                System.IntPtr
                rsvg_pixbuf_from_file_at_size( string file_name
                                             , int  width
                                             , int  height
                                             , out System.IntPtr error
                                             );

                [System.Runtime.InteropServices.DllImport("rsvg-2")]
                static extern
                System.IntPtr
                rsvg_pixbuf_from_file_at_max_size( string file_name
                                                 , int max_width
                                                 , int max_height
                                                 , out System.IntPtr error
                                                 );

                [System.Runtime.InteropServices.DllImport("rsvg-2")]
                static extern
                System.IntPtr
                rsvg_pixbuf_from_file_at_zoom_with_max( string file_name
                                                      , double x_zoom
                                                      , double y_zoom
                                                      , int max_width
                                                      , int max_height
                                                      , out System.IntPtr error
                                                      );

            //////////////////////////////////////////////////////////////////////////////////////// D L L   I M P O R T S //



            // P R O C E D U R E S //////////////////////////////////////////////////////////////////////////////////////////

                public static Gdk.Pixbuf PixbufFromFile(string file_name)
                {
                    System.IntPtr error = System.IntPtr.Zero;

                    System.IntPtr raw_pixbuf = rsvg_pixbuf_from_file(file_name, out error);

                    if (System.IntPtr.Zero != error) {
                        throw new GLib.GException ( error );
                    } else {
                        return new Gdk.Pixbuf( raw_pixbuf );
                    }
                }

                public static Gdk.Pixbuf PixbufFromFileAtZoom(string file_name, double x_zoom, double y_zoom)
                {
                    System.IntPtr error = System.IntPtr.Zero;

                    System.IntPtr raw_pixbuf = rsvg_pixbuf_from_file_at_zoom(file_name, x_zoom, y_zoom, out error);

                    if (System.IntPtr.Zero != error) {
                        throw new GLib.GException( error );;
                    } else {
                        return new Gdk.Pixbuf( raw_pixbuf );
                    }
                }

                public static Gdk.Pixbuf PixbufFromFileAtSize(string file_name, int width, int height)
                {
                    System.IntPtr error = System.IntPtr.Zero;

                    System.IntPtr raw_pixbuf = rsvg_pixbuf_from_file_at_size(file_name, width, height, out error);

                    if (System.IntPtr.Zero != error) {
                        throw new GLib.GException( error );;
                    } else {
                        return new Gdk.Pixbuf( raw_pixbuf );
                    }
                }

                public static Gdk.Pixbuf PixbufFromFileAtMaxSize(string file_name, int max_width, int max_height)
                {
                    System.IntPtr error = System.IntPtr.Zero;

                    System.IntPtr raw_pixbuf = rsvg_pixbuf_from_file_at_max_size(file_name, max_width, max_height, out error);

                    if (System.IntPtr.Zero != error) {
                        throw new GLib.GException( error );;
                    } else {
                        return new Gdk.Pixbuf( raw_pixbuf );
                    }
                }

                public static Gdk.Pixbuf PixbufFromFileAtZoomWithMaxSize(string file_name, double x_zoom, double y_zoom, int max_width, int max_height)
                {
                    System.IntPtr error = System.IntPtr.Zero;

                    System.IntPtr raw_pixbuf = rsvg_pixbuf_from_file_at_zoom_with_max(file_name, x_zoom, y_zoom, max_width, max_height, out error);

                    if (System.IntPtr.Zero != error) {
                        throw new GLib.GException( error );;
                    } else {
                        return new Gdk.Pixbuf( raw_pixbuf );
                    }
                }

            ////////////////////////////////////////////////////////////////////////////////////////// P R O C E D U R E S //


        } // class Tool

    } // namespace Rsvg

//////////////////////////////////////////////////////////////////////////////////////////////////////////// O B J E C T S //


