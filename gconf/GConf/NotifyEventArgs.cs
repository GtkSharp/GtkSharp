// NotifyEventArgs.cs -
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


namespace GConf
{
	public class NotifyEventArgs : System.EventArgs
	{
		string key;
		object val;
		
		public NotifyEventArgs (string key, object val)
		{
			this.key = key;
			this.val = val;
		}

		public string Key
		{
			get { return key; }
		}

		public object Value
		{
			get { return val; }
		}
	}
}

