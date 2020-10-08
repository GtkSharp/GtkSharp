// Container.cs - customizations to Gtk.Container
//
// Authors: Mike Kestner  <mkestner@ximian.com>
//
// Copyright (c) 2004 Novell, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General 
// Public License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.

namespace Gtk
{

    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    public partial class Container : IEnumerable
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate IntPtr d_gtk_container_class_find_child_property(IntPtr cclass, string property_name);
        static d_gtk_container_class_find_child_property gtk_container_class_find_child_property = FuncLoader.LoadFunction<d_gtk_container_class_find_child_property>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_container_class_find_child_property"));
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void d_gtk_container_child_get_property(IntPtr container, IntPtr child, string property_name, ref GLib.Value value);
        static d_gtk_container_child_get_property gtk_container_child_get_property = FuncLoader.LoadFunction<d_gtk_container_child_get_property>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_container_child_get_property"));

        struct GTypeInstance
        {
            public IntPtr GTypeClass;
        }

        struct GParamSpec
        {
            public GTypeInstance g_type_instance;

            public IntPtr name;
            public int flags;
            public GLib.GType value_type;
            public GLib.GType owner_type;

            IntPtr _nick;
            IntPtr _blurb;
            IntPtr qdata;
            uint ref_count;
            uint param_id;
        };

        public GLib.Value ChildGetProperty(Gtk.Widget child, string property_name)
        {
            var value = new GLib.Value();
            var cclass = ((GTypeInstance)Marshal.PtrToStructure(Handle, typeof(GTypeInstance))).GTypeClass;
            var propPtr = gtk_container_class_find_child_property(cclass, property_name);
            var prop = (GParamSpec)Marshal.PtrToStructure(propPtr, typeof(GParamSpec));

            value.Init(prop.value_type);
            gtk_container_child_get_property(Handle, child.Handle, property_name, ref value);

            return value;
        }

        public IEnumerator GetEnumerator()
        {
            return Children.GetEnumerator();
        }

        class ChildAccumulator
        {
            public ArrayList Children = new ArrayList();

            public void Add(Gtk.Widget widget)
            {
                Children.Add(widget);
            }
        }

        public IEnumerable AllChildren
        {
            get
            {
                ChildAccumulator acc = new ChildAccumulator();
                Forall(new Gtk.Callback(acc.Add));
                return acc.Children;
            }
        }
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool d_gtk_container_get_focus_chain(IntPtr raw, out IntPtr list_ptr);
		static d_gtk_container_get_focus_chain gtk_container_get_focus_chain = FuncLoader.LoadFunction<d_gtk_container_get_focus_chain>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_container_get_focus_chain"));
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void d_gtk_container_set_focus_chain(IntPtr raw, IntPtr list_ptr);
		static d_gtk_container_set_focus_chain gtk_container_set_focus_chain = FuncLoader.LoadFunction<d_gtk_container_set_focus_chain>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_container_set_focus_chain"));

        public Widget[] FocusChain
        {
            get
            {
                IntPtr list_ptr;
                bool success = gtk_container_get_focus_chain(Handle, out list_ptr);
                if (!success)
                    return new Widget[0];

                GLib.List list = new GLib.List(list_ptr);
                Widget[] result = new Widget[list.Count];
                for (int i = 0; i < list.Count; i++)
                    result[i] = list[i] as Widget;
                return result;
            }
            set
            {
                GLib.List list = new GLib.List(IntPtr.Zero);
                foreach (Widget val in value)
                    list.Append(val.Handle);
                gtk_container_set_focus_chain(Handle, list.Handle);
            }

        }

        // Compatibility code for old ChildType() virtual method
        static IntPtr ObsoleteChildType_cb(IntPtr raw)
        {
            try
            {
                Container obj = GLib.Object.GetObject(raw, false) as Container;
                GLib.GType gtype = obj.ChildType();
                return gtype.Val;
            }
            catch (Exception e)
            {
                GLib.ExceptionManager.RaiseUnhandledException(e, false);
            }

            return GLib.GType.Invalid.Val;
        }

        static ChildTypeNativeDelegate ObsoleteChildTypeVMCallback;

        static void OverrideObsoleteChildType(GLib.GType gtype)
        {
            if (ObsoleteChildTypeVMCallback == null)
                ObsoleteChildTypeVMCallback = new ChildTypeNativeDelegate(ObsoleteChildType_cb);
            OverrideChildType(gtype, ObsoleteChildTypeVMCallback); // -> autogenerated method
        }

        [Obsolete("Replaced by OnChildType for implementations and SupportedChildType for callers.")]
        [GLib.DefaultSignalHandler(Type = typeof(Gtk.Container), ConnectionMethod = "OverrideObsoleteChildType")]
        public virtual GLib.GType ChildType()
        {
            return InternalChildType(); // -> autogenerated method
        }

        public class ContainerChild
        {
            protected Container parent;
            protected Widget child;

            public ContainerChild(Container parent, Widget child)
            {
                this.parent = parent;
                this.child = child;
            }

            public Container Parent
            {
                get
                {
                    return parent;
                }
            }

            public Widget Child
            {
                get
                {
                    return child;
                }
            }
        }

        public virtual ContainerChild this[Widget w]
        {
            get
            {
                return new ContainerChild(this, w);
            }
        }
    }
}

