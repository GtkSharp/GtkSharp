// PropertyEditor.cs -
//
// Author: Rachel Hestilow  <hestilow@nullenvoid.com>
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

namespace GConf.PropertyEditors
{
	using Gtk;

	public abstract class PropertyEditor
	{
		protected abstract void ValueChanged (object sender, NotifyEventArgs args);
		protected abstract void ConnectHandlers ();

		string key;
		Client client;
		ChangeSet cs;
		Widget control;
		
		public string Key
		{
			get { return key; }
		}

		public Widget Control
		{
			get { return control; }
		}

		protected object Get ()
		{
			ClientBase c = (cs != null) ? (ClientBase) cs : (ClientBase) client;
			try {
				return c.Get (key);
			} catch (NoSuchKeyException e) {
			}

			if (cs != null)
			{
				try {
					return client.Get (key);
				} catch (NoSuchKeyException e) {
				}
			}	

			return null;
		}
	
		protected virtual void Set (object val)
		{
			ClientBase c = (cs != null) ? (ClientBase) cs : (ClientBase) client;
			c.Set (key, val);	
		}

		public virtual void Setup ()
		{
			if (client == null)
				client = new Client ();
			
			ValueChanged (client, new NotifyEventArgs (key, Get ()));
			ConnectHandlers ();
			client.AddNotify (key, new NotifyEventHandler (ValueChanged));
		}

		public Client Client
		{
			get { return client; }
			set { client = value; }
		}

		public ChangeSet ChangeSet
		{
			get { return cs; }
			set { cs = value; }
		}

		public PropertyEditor (string key, Widget control)
		{
			this.key = key;
			this.control = control;
		}
	}
}
