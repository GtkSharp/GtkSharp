//  VfsStreamAsyncResult.cs - IAsyncResult implementation for VfsStream.
//
//  Authors:  Jeroen Zwartepoorte  <jeroen@xs4all.nl>
//
//  Copyright (c) 2004 Jeroen Zwartepoorte
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
using System.Threading;

namespace Gnome.Vfs {
	public class VfsStreamAsyncResult : IAsyncResult {
		private object state;
		private bool completed = false;
		private bool done = false;
		private Exception exception = null;
		private int nbytes = -1;

		public VfsStreamAsyncResult (object state)
		{
			this.state = state;
		}

		public object AsyncState {
			get {
				return state;
			}
		}
		
		public WaitHandle AsyncWaitHandle {
			get {
				throw new NotSupportedException (
					"Do NOT use the AsyncWaitHandle to " + 
					"wait until a Begin[Read,Write] call " +
					"has finished since it will also block " +
					"the gnome-vfs callback which unlocks " +
					"the WaitHandle, causing a deadlock. " +
					"Instead, use \"while (!asyncResult.IsCompleted) " +
					"GLib.MainContext.Iteration ();\"");
			}
		}
		
		public bool CompletedSynchronously {
			get {
				return true;
			}
		}
		
		public bool Done {
			get {
				return done;
			}
			set {
				done = value;
			}
		}
		
		public Exception Exception {
			get {
				return exception;
			}
		}
		
		public bool IsCompleted {
			get {
				return completed;
			}
		}
		
		public int NBytes {
			get {
				return nbytes;
			}
		}
		
		public void SetComplete (Exception e)
		{
			exception = e;
			completed = true;
		}
		
		public void SetComplete (Exception e, int nbytes)
		{
			this.nbytes = nbytes;
			SetComplete (e);
		}
	}
}
