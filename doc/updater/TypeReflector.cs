// TypeReflector.cs - Type reflection class
//
// Author: Mike Kestner  <mkestner@ximian.com>
//
// Copyright (c) 2004, Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person 
// obtaining a copy of this software and associated documentation 
// files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, 
// publish, distribute, sublicense, and/or sell copies of the Software, 
// and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be 
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS 
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN 
// ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN 
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
// SOFTWARE.


namespace GtkSharp.Docs {

	using System;
	using System.Collections;
	using System.Reflection;

	public class TypeReflector {

		const BindingFlags static_flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly |BindingFlags.Static;
		const BindingFlags instance_flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly |BindingFlags.Instance;

		Type t;

		public TypeReflector (Type t)
		{
			this.t = t;
		}

		public FieldInfo [] Fields {
			get {
				ArrayList members = new ArrayList ();
				foreach (object o in t.GetFields (static_flag))
					members.Add (o);
				foreach (object o in t.GetFields (instance_flag))
					members.Add (o);

				return (FieldInfo []) members.ToArray (typeof (FieldInfo));
			}
		}

		public MethodInfo [] Methods {
			get {
				ArrayList members = new ArrayList ();
				foreach (object o in t.GetMethods (static_flag))
					members.Add (o);
				foreach (object o in t.GetMethods (instance_flag))
					members.Add (o);

				return (MethodInfo []) members.ToArray (typeof (MethodInfo));
			}
		}

		public PropertyInfo [] Properties {
			get {
				ArrayList members = new ArrayList ();
				foreach (object o in t.GetProperties (static_flag))
					members.Add (o);
				foreach (object o in t.GetProperties (instance_flag))
					members.Add (o);

				return (PropertyInfo []) members.ToArray (typeof (PropertyInfo));
			}
		}

		public EventInfo [] Events {
			get {
				ArrayList members = new ArrayList ();
				foreach (object o in t.GetEvents (static_flag))
					members.Add (o);
				foreach (object o in t.GetEvents (instance_flag))
					members.Add (o);

				return (EventInfo []) members.ToArray (typeof (EventInfo));
			}
		}

		public ConstructorInfo [] Constructors {
			get {
				ArrayList members = new ArrayList ();
				foreach (object o in t.GetConstructors (instance_flag))
					members.Add (o);

				return (ConstructorInfo []) members.ToArray (typeof (ConstructorInfo));
			}
		}
	}
}
