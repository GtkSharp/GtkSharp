namespace GConf
{
	using System;
	
	public class NoSuchKeyException : Exception
	{
		public NoSuchKeyException ()
			: base ("The requested GConf key was not found")
		{
		}

		public NoSuchKeyException (string key)
			: base ("Key '" + key + "' not found in GConf")
		{
		}
	}
}

