// Statistics.cs : Generation statistics class implementation
//
// Author: Mike Kestner  <mkestner@speakeasy.net>
//
// <c> 2002 Mike Kestner

namespace GtkSharp.Generation {
	
	using System;
	using System.Collections;
	
	public class Statistics {
		
		static int cbs = 0;
		static int enums = 0;
		static int objects = 0;
		static int structs = 0;
		static int boxed = 0;
		static int interfaces = 0;
		static int methods = 0;
		static int ctors = 0;
		static int props = 0;
		static int sigs = 0;
		static int throttled = 0;
		static int ignored = 0;
		
		public static int CBCount {
			get {
				return cbs;
			}
			set {
				cbs = value;
			}
		}

		public static int EnumCount {
			get {
				return enums;
			}
			set {
				enums = value;
			}
		}

		public static int ObjectCount {
			get {
				return objects;
			}
			set {
				objects = value;
			}
		}

		public static int StructCount {
			get {
				return structs;
			}
			set {
				structs = value;
			}
		}

		public static int BoxedCount {
			get {
				return boxed;
			}
			set {
				boxed = value;
			}
		}

		public static int CtorCount {
			get {
				return ctors;
			}
			set {
				ctors = value;
			}
		}
		
		public static int MethodCount {
			get {
				return methods;
			}
			set {
				methods = value;
			}
		}

		public static int PropCount {
			get {
				return props;
			}
			set {
				props = value;
			}
		}

		public static int SignalCount {
			get {
				return sigs;
			}
			set {
				sigs = value;
			}
		}

		public static int IFaceCount {
			get {
				return interfaces;
			}
			set {
				interfaces = value;
			}
		}

		public static int ThrottledCount {
			get {
				return throttled;
			}
			set {
				throttled = value;
			}
		}
		
		public static int IgnoreCount {
			get {
				return ignored;
			}
			set {
				ignored = value;
			}
		}
		
		public static void Report()
		{
			Console.WriteLine("Generation Summary:");
			Console.WriteLine("\tEnums: " + enums);
			Console.WriteLine("\tStructs: " + structs);
			Console.WriteLine("\tBoxed: " + boxed);
			Console.WriteLine("\tInterfaces: " + interfaces);
			Console.WriteLine("\tCallbacks: " + cbs);
			Console.WriteLine("\tObjects: " + objects);
			Console.WriteLine("\tProperties: " + props);
			Console.WriteLine("\tSignals: " + sigs);
			Console.WriteLine("\tMethods: " + methods);
			Console.WriteLine("\tConstructors: " + ctors);
			Console.WriteLine("\tThrottled: " + throttled);
			Console.WriteLine("\tIgnored: " + ignored);
			Console.WriteLine("Total Nodes: " + (enums+structs+boxed+interfaces+cbs+objects+props+sigs+methods+ctors+throttled+ignored));
		}
	}
}
