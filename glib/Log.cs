// Log.cs - Wrapper for message logging functions
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	
//
// (c) 2002 Gonzalo Paniagua
//

namespace GLib {

	using System;
	using System.Runtime.InteropServices;

	public delegate void LogFunc (string log_domain,
				      LogLevelFlags log_level,
				      string message,
				      IntPtr user_data);

	public delegate void PrintFunc (string message);

	[Flags]
	public enum LogLevelFlags : uint
	{
		/* log flags */
		FlagRecursion          = 1 << 0,
		FlagFatal              = 1 << 1,

		/* GLib log levels */
		Error                  = 1 << 2,       /* always fatal */
		Critical               = 1 << 3,
		Warning                = 1 << 4,
		Message                = 1 << 5,
		Info                   = 1 << 6,
		Debug                  = 1 << 7,

		/* Convenience values */
		AllButFatal            = 253,
		AllButRecursion        = 254,
		All                    = 255,

		FlagMask               = 3,
		LevelMask              = (uint) ~3
	}

	public class Log {

		[DllImport("glib-2.0")]
		static extern void g_logv (string log_domain, LogLevelFlags flags, string message);
		
		public void WriteLog (string logDomain, LogLevelFlags flags, string format, params object [] args)
		{
			g_logv (logDomain, flags, String.Format (format, args));
		}

		[DllImport("glib-2.0")]
		static extern uint g_log_set_handler (string log_domain,
						      LogLevelFlags flags,
						      LogFunc log_func,
						      IntPtr user_data);
		
		public static uint SetLogHandler (string logDomain,
						  LogLevelFlags flags,
						  LogFunc logFunc,
						  IntPtr userData)
		{
			return g_log_set_handler (logDomain, flags, logFunc, userData);
		}

		[DllImport("glib-2.0")]
		static extern uint g_log_remove_handler (string log_domain, uint handler_id);

		public static void RemoveLogHandler (string logDomain, uint handlerID)
		{
			g_log_remove_handler (logDomain, handlerID);
		}


		[DllImport("glib-2.0")]
		static extern PrintFunc g_set_print_handler (PrintFunc handler);

		public static PrintFunc SetPrintHandler (PrintFunc handler)
		{
			return g_set_print_handler (handler);
		}
		
		[DllImport("glib-2.0")]
		static extern PrintFunc g_set_printerr_handler (PrintFunc handler);

		public static PrintFunc SetPrintErrorHandler (PrintFunc handler)
		{
			return g_set_printerr_handler (handler);
		}
		
		[DllImport("glib-2.0")]
		static extern void g_log_default_handler (string log_domain,
							  LogLevelFlags log_level,
							  string message,
							  IntPtr unused_data);

		public static void DefaultHandler (string logDomain,
						   LogLevelFlags logLevel,
						   string message,
						   IntPtr unusedData)
		{
			g_log_default_handler (logDomain, logLevel, message, unusedData);
		}

		[DllImport("glib-2.0")]
		extern static LogLevelFlags g_log_set_always_fatal (LogLevelFlags fatal_mask);
		
		public static LogLevelFlags SetAlwaysFatal (LogLevelFlags fatalMask)
		{
			return g_log_set_always_fatal (fatalMask);
		}

		[DllImport("glib-2.0")]
		extern static LogLevelFlags g_log_set_fatal_mask (string log_domain, LogLevelFlags fatal_mask);
		
		public static LogLevelFlags SetAlwaysFatal (string logDomain, LogLevelFlags fatalMask)
		{
			return g_log_set_fatal_mask (logDomain, fatalMask);
		}

		/*
		 * Some common logging methods.
		 *
		 * Sample usage:
		 *
		 *	// Print the messages for the NULL domain
		 *	LogFunc logFunc = new GLib.LogFunc (Glib.Log.PrintLogFunction);
		 *	Log.SetLogHandler (null, GLib.LogLevelFlags.All, logFunc, IntPtr.Zero);
		 *
		 *	// Print messages and stack trace for Gtk critical messages
		 *	logFunc = new GLib.LogFunc (Glib.Log.PrintTraceLogFunction);
		 *	Log.SetLogHandler ("Gtk", Glib.LogLevelFlags.Critical, logFunc, IntPtr.Zero);
		 *
		 */

		public static void PrintLogFunction (string domain, LogLevelFlags level, string message, IntPtr data)
		{
			Console.WriteLine ("Domain: '{0}' Level: {1}", domain, level);
			Console.WriteLine ("Message: {0}", message);
		}

		public static void PrintTraceLogFunction (string domain, LogLevelFlags level, string message, IntPtr data)
		{
			PrintLogFunction (domain, level, message, data);
			Console.WriteLine ("Trace follows:\n{0}", new System.Diagnostics.StackTrace ());
		}
	}
}

