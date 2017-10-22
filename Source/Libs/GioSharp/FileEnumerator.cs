// FileEnumerator.cs - customizations to GLib.FileEnumerator
//
// Authors: Stephane Delcroix  <stephane@delcroix.org>
//
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

namespace GLib {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	
	public partial class FileEnumerator : IEnumerable<FileInfo> {
		public IEnumerator<FileInfo> GetEnumerator ()
		{
			return new Enumerator (this);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		public FileInfo NextFile ()
		{
			return NextFile ((Cancellable) null);
		}

		class Enumerator : IEnumerator<FileInfo>
		{
			FileEnumerator file_enumerator;

			public Enumerator (FileEnumerator file_enumerator)
			{
				this.file_enumerator = file_enumerator;
			}

			void IDisposable.Dispose() { }

			FileInfo current;
			public FileInfo Current {
				get {
					return current;
				}
			}

			object IEnumerator.Current
			{
				get { return Current; }
			}

			public bool MoveNext ()
			{
				current = file_enumerator.NextFile ();
				if (current == null)
					return false;
				return true;
			}
		
			public void Reset ()
			{
				throw new NotImplementedException ();
			}
		}
	}
}