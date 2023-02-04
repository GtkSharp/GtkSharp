// HandlerNotFoundException.cs - exception for Gtk.Builder
//
// Authors: Stephane Delcroix  <stephane@delcroix.org>
// The biggest part of this code is adapted from glade#, by
//	Ricardo Fernández Pascual <ric@users.sourceforge.net>
//	Rachel Hestilow <hestilow@ximian.com>
//
// Copyright (c) 2002 Ricardo Fernández Pascual
// Copyright (c) 2003 Rachel Hestilow
// Copyright (c) 2008 Novell, Inc.
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

using System;

namespace Gtk
{
		[System.Serializable]
		public class HandlerNotFoundException : SystemException
		{
			string handler_name;
			string signal_name;
			System.Reflection.EventInfo evnt;
			Type delegate_type;
		
			public HandlerNotFoundException (string handler_name, string signal_name,
							 System.Reflection.EventInfo evnt, Type delegate_type)
				: this (handler_name, signal_name, evnt, delegate_type, null)
			{
			}
		
			public HandlerNotFoundException (string handler_name, string signal_name,
							 System.Reflection.EventInfo evnt, Type delegate_type, Exception inner)
				: base ("No handler " + handler_name + " found for signal " + signal_name,
					inner)
			{
				this.handler_name = handler_name;
				this.signal_name = signal_name;
				this.evnt = evnt;
				this.delegate_type = delegate_type;
			}
		
			public HandlerNotFoundException (string message, string handler_name, string signal_name,
							 System.Reflection.EventInfo evnt, Type delegate_type)
				: base ((message != null) ? message : "No handler " + handler_name + " found for signal " + signal_name,
					null)
			{
				this.handler_name = handler_name;
				this.signal_name = signal_name;
				this.evnt = evnt;
				this.delegate_type = delegate_type;
			}
		
			protected HandlerNotFoundException (System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base (info, context)
			{
				handler_name = info.GetString ("HandlerName");
				signal_name = info.GetString ("SignalName");
				evnt = info.GetValue ("Event", typeof (System.Reflection.EventInfo)) as System.Reflection.EventInfo;
				delegate_type = info.GetValue ("DelegateType", typeof (Type)) as Type;
			}
		
			public string HandlerName
			{
				get {
					return handler_name;
				}
			}
		
			public string SignalName
			{
				get {
					return signal_name;
				}
			}
		
			public System.Reflection.EventInfo Event
			{
				get {
					return evnt;
				}
			}
		
			public Type DelegateType
			{
				get {
					return delegate_type;
				}
			}
		
			public override void GetObjectData (System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			{
				base.GetObjectData (info, context);
				info.AddValue ("HandlerName", handler_name);
				info.AddValue ("SignalName", signal_name);
				info.AddValue ("Event", evnt);
				info.AddValue ("DelegateType", delegate_type);
			}
		}
}
