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
