// Log.cs - Wrapper for message logging functions
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	
//
// Copyright (c) 2002 Gonzalo Paniagua
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

//

namespace GLib {

	using System;
	using System.Collections;
	using System.Runtime.InteropServices;

	public delegate void LogFunc (string log_domain,
				      LogLevelFlags log_level,
				      string message);

	public delegate void PrintFunc (string message);

	[Flags]
	public enum LogLevelFlags : int
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
		LevelMask              = unchecked ((int) 0xFFFFFFFC)
	}

	public class Log {

		static Hashtable handlers;

		static void EnsureHash ()
		{
			if (handlers == null)
				handlers = new Hashtable ();
		}

		[DllImport("libglib-2.0-0.dll")]
		static extern void g_logv (string log_domain, LogLevelFlags flags, string message);
		
		public void WriteLog (string logDomain, LogLevelFlags flags, string format, params object [] args)
		{
			g_logv (logDomain, flags, String.Format (format, args));
		}

		[DllImport("libglib-2.0-0.dll")]
		static extern uint g_log_set_handler (string log_domain,
						      LogLevelFlags flags,
						      LogFunc log_func,
						      IntPtr user_data);
		
		public static uint SetLogHandler (string logDomain,
						  LogLevelFlags flags,
						  LogFunc logFunc)
						  
		{
			uint result = g_log_set_handler (logDomain, flags, logFunc, IntPtr.Zero);
			EnsureHash ();
			handlers [result] = logFunc;

			return result;
		}

		[DllImport("libglib-2.0-0.dll")]
		static extern uint g_log_remove_handler (string log_domain, uint handler_id);

		public static void RemoveLogHandler (string logDomain, uint handlerID)
		{
			if (handlers != null && handlers.ContainsKey (handlerID))
				handlers.Remove (handlerID);
			
			g_log_remove_handler (logDomain, handlerID);
		}


		[DllImport("libglib-2.0-0.dll")]
		static extern PrintFunc g_set_print_handler (PrintFunc handler);

		public static PrintFunc SetPrintHandler (PrintFunc handler)
		{
			EnsureHash ();
			handlers ["PrintHandler"] = handler;

			return g_set_print_handler (handler);
		}
		
		[DllImport("libglib-2.0-0.dll")]
		static extern PrintFunc g_set_printerr_handler (PrintFunc handler);

		public static PrintFunc SetPrintErrorHandler (PrintFunc handler)
		{
			EnsureHash ();
			handlers ["PrintErrorHandler"] = handler;

			return g_set_printerr_handler (handler);
		}
		
		[DllImport("libglib-2.0-0.dll")]
		static extern void g_log_default_handler (string log_domain,
							  LogLevelFlags log_level,
							  string message,
							  IntPtr unused_data);

		public static void DefaultHandler (string logDomain,
						   LogLevelFlags logLevel,
						   string message)
						   
		{
			g_log_default_handler (logDomain, logLevel, message, IntPtr.Zero);
		}

		[DllImport("libglib-2.0-0.dll")]
		extern static LogLevelFlags g_log_set_always_fatal (LogLevelFlags fatal_mask);
		
		public static LogLevelFlags SetAlwaysFatal (LogLevelFlags fatalMask)
		{
			return g_log_set_always_fatal (fatalMask);
		}

		[DllImport("libglib-2.0-0.dll")]
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
		 *	LogFunc logFunc = new LogFunc (Log.PrintLogFunction);
		 *	Log.SetLogHandler (null, LogLevelFlags.All, logFunc);
		 *
		 *	// Print messages and stack trace for Gtk critical messages
		 *	logFunc = new LogFunc (Log.PrintTraceLogFunction);
		 *	Log.SetLogHandler ("Gtk", LogLevelFlags.Critical, logFunc);
		 *
		 */

		public static void PrintLogFunction (string domain, LogLevelFlags level, string message)
		{
			Console.WriteLine ("Domain: '{0}' Level: {1}", domain, level);
			Console.WriteLine ("Message: {0}", message);
		}

		public static void PrintTraceLogFunction (string domain, LogLevelFlags level, string message)
		{
			PrintLogFunction (domain, level, message);
			Console.WriteLine ("Trace follows:\n{0}", new System.Diagnostics.StackTrace ());
		}
	}
}

