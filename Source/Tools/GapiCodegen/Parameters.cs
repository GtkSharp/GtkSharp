// GtkSharp.Generation.Parameters.cs - The Parameters Generation Class.
//
// Author: Mike Kestner <mkestner@speakeasy.net>
//
// Copyright (c) 2001-2003 Mike Kestner
// Copyright (c) 2004 Novell, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the GNU General Public
// License as published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
//
// You should have received a copy of the GNU General Public
// License along with this program; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place - Suite 330,
// Boston, MA 02111-1307, USA.


namespace GtkSharp.Generation {

	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Xml;

	public class Parameters : IEnumerable<Parameter> {
		
		IList<Parameter> param_list = new List<Parameter> ();
		XmlElement elem;
		bool first_is_instance;

		public Parameters (XmlElement elem) : this (elem, false) { }

		public Parameters (XmlElement elem, bool first_is_instance)
		{
			if (elem == null)
				valid = true;
			this.elem = elem;
			this.first_is_instance = first_is_instance;
			if (first_is_instance)
				is_static = false;
		}

		public int Count {
			get { return param_list.Count; }
		}

		// gapi assumes GError** parameters to be error parameters in version 1 and 2
		private bool throws = false;
		public bool Throws {
			get {
				if (Parser.GetVersion (elem.OwnerDocument.DocumentElement) <= 2)
					return true;
				if (!throws && elem.HasAttribute ("throws"))
					throws = elem.GetAttributeAsBoolean ("throws");
				return throws;
			}
		}

		public int VisibleCount {
			get {
				int visible = 0;
				foreach (Parameter p in this) {
					if (!IsHidden (p))
						visible++;
				}
				return visible;
			}
		}

		public Parameter this [int idx] {
			get {
				return param_list [idx];
			}
		}

		public bool IsHidden (Parameter p)
		{
			int idx = param_list.IndexOf (p);

			if (idx > 0 && p.IsLength && p.PassAs == String.Empty && this [idx - 1].IsString)
				return true;

			if (p.IsCount)
				return true;

			if (p.IsHidden)
				return true;

			if (p.CType == "GError**" && Throws)
				return true;

			if (HasCB || HideData) {

				if (Parser.GetVersion (elem.OwnerDocument.DocumentElement) >= 3) {
					foreach (Parameter param in param_list) {
						if (param.Closure == idx)
							return true;
						if (param.DestroyNotify == idx)
							return true;
					}
				} else {
					if (p.IsUserData && (idx == Count - 1))
						return true;
					if (p.IsUserData && (idx == Count - 2) && this [Count - 1] is ErrorParameter)
						return true;
					if (p.IsUserData && idx > 0 && this [idx - 1].Generatable is CallbackGen)
						return true;
					if (p.IsDestroyNotify && (idx == Count - 1) && this [idx - 1].IsUserData)
						return true;
				}
			}

			return false;
		}

		bool has_cb;
		public bool HasCB {
			get { return has_cb; }
			set { has_cb = value; }
		}

		public bool HasOutParam {
			get {
				foreach (Parameter p in this)
					if (p.PassAs == "out")
						return true;
				return false;
			}
		}

		bool hide_data;
		public bool HideData {
			get { return hide_data; }
			set { hide_data = value; }
		}

		bool is_static;
		public bool Static {
			get { return is_static; }
			set { is_static = value; }
		}

		bool has_optional;
		internal bool HasOptional {
			get { return has_optional;}
		}

		public Parameter GetCountParameter (string param_name)
		{
			foreach (Parameter p in this)
				if (p.Name == param_name) {
					p.IsCount = true;
					return p;
				}
			return null;
		}

		void Clear ()
		{
			elem = null;
			param_list.Clear ();
			param_list = null;
		}

		public IEnumerator<Parameter> GetEnumerator ()
		{
			return param_list.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		bool valid = false;

		public bool Validate (LogWriter log)
		{
			if (valid)
				return true;

			if (elem == null)
				return false;

			for (int i = first_is_instance ? 1 : 0; i < elem.ChildNodes.Count; i++) {
				XmlElement parm = elem.ChildNodes [i] as XmlElement;
				if (parm == null || parm.Name != "parameter")
					continue;
				Parameter p = new Parameter (parm);
				
				if (p.IsEllipsis) {
					log.Warn ("Ellipsis parameter: hide and bind manually if no alternative exists. ");
					Clear ();
					return false;
				}

				if ((p.CSType == "") || (p.Name == "") || 
				    (p.MarshalType == "") || (SymbolTable.Table.CallByName(p.CType, p.Name) == "")) {
					log.Warn ("Unknown type {1} on parameter {0}", p.Name, p.CType);
					Clear ();
					return false;
				}

				if (p.IsOptional && p.PassAs == String.Empty && p.IsUserData == false)
					has_optional = true;

				IGeneratable gen = p.Generatable;

				if (p.IsArray) {
					p = new ArrayParameter (parm);
					if (i < elem.ChildNodes.Count - 1) {
						XmlElement next = elem.ChildNodes [i + 1] as XmlElement;
						if (next != null || next.Name == "parameter") {
							Parameter c = new Parameter (next);
							if (c.IsCount) {
								p = new ArrayCountPair (parm, next, false);
								i++;
							}
						}
					}
				} else if (p.IsCount) {
					p.IsCount = false;
					if (i < elem.ChildNodes.Count - 1) {
						XmlElement next = elem.ChildNodes [i + 1] as XmlElement;
						if (next != null || next.Name == "parameter") {
							Parameter a = new Parameter (next);
							if (a.IsArray) {
								p = new ArrayCountPair (next, parm, true);
								i++;
							}
						}
					}
				} else if (p.CType == "GError**" && Throws)
					p = new ErrorParameter (parm);
				else if (gen is StructBase || gen is ByRefGen) {
					p = new StructParameter (parm);
				} else if (gen is CallbackGen) {
					has_cb = true;
				}
				param_list.Add (p);
			}

			if (Parser.GetVersion (elem.OwnerDocument.DocumentElement) < 3 &&
			    has_cb && Count > 2 && this [Count - 3].Generatable is CallbackGen && this [Count - 2].IsUserData && this [Count - 1].IsDestroyNotify)
				this [Count - 3].Scope = "notified";

			valid = true;
			return true;
		}

		public bool IsAccessor {
			get {
				return VisibleCount == 1 && AccessorParam.PassAs == "out";
			}
		}

		public Parameter AccessorParam {
			get {
				foreach (Parameter p in this) {
					if (!IsHidden (p))
						return p;
				}
				return null;
			}
		}

		public string AccessorReturnType {
			get {
				Parameter p = AccessorParam;
				if (p != null)
					return p.CSType;
				else
					return null;
			}
		}

		public string AccessorName {
			get {
				Parameter p = AccessorParam;
				if (p != null)
					return p.Name;
				else
					return null;
			}
		}

		public string ImportSignature {
			get {
				if (Count == 0)
					return String.Empty;

				string[] result = new string [Count];
				for (int i = 0; i < Count; i++)
					result [i] = this [i].NativeSignature;

				return String.Join (", ", result);
			}
		}
	}
}
