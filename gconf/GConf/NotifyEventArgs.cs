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

