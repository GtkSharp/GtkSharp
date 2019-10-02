// ActionGroup.cs - Syntactic C# sugar for easily defining Actions.
//
// Authors:  Jeroen Zwartepoorte  <jeroen@xs4all.nl>
//
// Copyright (c) 2004 Jeroen Zwartepoorte
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

namespace Gtk {

	using System;
	using System.Runtime.InteropServices;

	public partial class ActionGroup {

		public Action this[string name] {
			get {
				return GetAction (name);
			}
		}

		public void Add (ActionEntry[] entries)
		{
			foreach (ActionEntry entry in entries) {
				Action action = new Action (entry.name, entry.label, entry.tooltip, entry.stock_id);
				if (entry.activated != null)
					action.Activated += entry.activated;
				if (entry.accelerator == null)
					Add (action);
				else
					Add (action, entry.accelerator);
			}
		}

		public void Add (ToggleActionEntry[] entries)
		{
			foreach (ToggleActionEntry entry in entries) {
				ToggleAction action = new ToggleAction (entry.name, entry.label, entry.tooltip, entry.stock_id);
				action.Active = entry.active;
				if (entry.activated != null)
					action.Activated += entry.activated;
				if (entry.accelerator == null)
					Add (action);
				else
					Add (action, entry.accelerator);
			}
		}

		public void Add (RadioActionEntry[] entries, int value, ChangedHandler changed)
		{
			RadioAction[] group = null;
			RadioAction[] actions = new RadioAction[entries.Length];
			for (int i = 0; i < entries.Length; i++) {
				actions[i] = new RadioAction (entries[i].name, entries[i].label,
				                              entries[i].tooltip, entries[i].stock_id, entries[i].value);
				actions[i].Group = group;
				group = actions[i].Group;
				actions[i].Active = value == entries[i].value;
				if (entries[i].accelerator == null)
					Add (actions[i]);
				else
					Add (actions[i], entries[i].accelerator);
			}

			// Add the ChangedHandler when we're done adding all the actions.
			// Otherwise, setting the Active property will trigger a premature event.
			if (changed != null)
				actions[0].Changed += changed;
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate IntPtr d_gtk_action_group_list_actions(IntPtr raw);
		static d_gtk_action_group_list_actions gtk_action_group_list_actions = FuncLoader.LoadFunction<d_gtk_action_group_list_actions>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_action_group_list_actions"));

		public Gtk.Action[] ListActions() {
			IntPtr raw_ret = gtk_action_group_list_actions (Handle);
			GLib.List list = new GLib.List (raw_ret);
 			Gtk.Action[] result = new Gtk.Action [list.Count];
			for (int i = 0; i < list.Count; i++)
				result [i] = list [i] as Gtk.Action;
			return result;
		}
	}
}

