// updater.cs: Mono documentation updater
//
// Authors:
//      Duncan Mak (duncan@ximian.com)
//	Mike Kestner <mkestner@ximian.com>
//
// Copyright (c) 2003-2004 Ximian, Inc.
//

namespace GtkSharp.Docs {

using System;
using System.Globalization;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

class Match
{
	string signature;
	bool found;
	MemberInfo member;
	XmlNode node;

	public Match (string signature, bool found)
	{
		this.signature = signature;
		this.found = found;
	}

	public MemberInfo Member {
		get { return member; }
		set { member = value; }
	}

	public XmlNode Node {
		get { return node; }
		set { node = value; }
	}

	public string Signature {
		get { return signature; }
		set { signature = value; }
	}

	public bool Found {
		get { return found; }
		set { found = value; }
	}
}

public enum MemberType {
	Method,
	Property,
	Constructor,
	Event,
	Field,
}

class Updater {

	const string EmptyString = "To be added";

	DirectoryInfo root_dir;
	Assembly assembly;
	string ns;

	static void Main (string [] args)
	{
		if (args.Length < 1 || args.Length > 3) {
			Console.WriteLine ("Usage: updater <assembly> [-o <output dir>]");
			return;
		}

		Updater updater;
		if (args.Length == 3 && args [1] == "-o" )
			updater = new Updater (args [0], args [2]);
		else
			updater = new Updater (args [0]);

		Console.WriteLine ("Updating assembly {0}:", args [0]);
		updater.Update ();
	}

	public Updater (string assembly_path) : this (assembly_path, Directory.GetCurrentDirectory ()) {}

	public Updater (string assembly_path, string root_path)
	{
		assembly = Assembly.LoadFrom (assembly_path);

		if (assembly == null)
			throw new Exception ("unable to load assembly from " + assembly_path);

		root_dir = new DirectoryInfo (root_path);
		if (!root_dir.Exists)
			root_dir.Create ();
	}
	
	void Update ()
	{
		int count = 0;
		foreach (Type t in assembly.GetTypes ())
			if (Update (t))
				count++;

		Console.WriteLine ("Wrote {0} files.\n", count);
	}

	DirectoryInfo GetDirectoryInfo (Type type)
	{
		string path = root_dir.FullName + Path.DirectorySeparatorChar + type.Namespace;
		if (Directory.Exists (path))
			return new DirectoryInfo (path);
		else
			return root_dir.CreateSubdirectory (type.Namespace);
	}

	bool Update (Type type)
	{
		DirectoryInfo directory = GetDirectoryInfo (type);
		string filename = directory.FullName + Path.DirectorySeparatorChar + type.Name + ".xml";

		XmlDocument doc;

		ns = type.Namespace;
		if (File.Exists (filename)) {
			doc = new XmlDocument ();
			Stream stream = File.OpenRead (filename);
			doc.Load (stream);
			stream.Close ();
			if (!Compare (type, doc))
				return false;
		} else 
			doc = Generate (type);

		if (doc == null)
			return false;

		XmlTextWriter writer = new XmlTextWriter (filename, Encoding.ASCII);
		writer.Formatting = Formatting.Indented;
		doc.WriteContentTo (writer);
		writer.Flush ();
		Console.WriteLine (type.FullName);
		return true;
	}

	bool Compare (Type t, XmlDocument doc)
	{
		bool changed = false;

		if (!t.IsAbstract && typeof (System.Delegate).IsAssignableFrom (t))
			return CompareDelegate (t, doc);

		TypeReflector reflector = new TypeReflector (t);
		changed |= Compare (doc, MemberType.Field, reflector.Fields, GetNodesOfType (doc, "Field"));
		changed |= Compare (doc, MemberType.Property, reflector.Properties, GetNodesOfType (doc, "Property"));
		changed |= Compare (doc, MemberType.Event, reflector.Events, GetNodesOfType (doc, "Event"));
		changed |= Compare (doc, MemberType.Method, reflector.Methods, GetNodesOfType (doc, "Method"));
		changed |= Compare (doc, MemberType.Constructor, reflector.Constructors, GetNodesOfType (doc, "Constructor"));

		return changed;
	}

	bool CompareDelegate (Type t, XmlDocument doc)
	{
		bool is_delegate;
		string node_signature = GetNodeSignature (doc);
		string del_signature = AddTypeSignature (t, out is_delegate);
		if (node_signature == del_signature)
			return false;
		else {
			SetTypeSignature (doc, del_signature);
			return true;
		}
	}

	bool Compare (XmlDocument document, MemberType member_type, MemberInfo [] members, XmlNodeList nodes)
	{
		int members_count = ((members == null) ? 0 :members.Length);
		int nodes_count = ((nodes == null) ? 0 : nodes.Count);

		// If we have no existing members, we deprecate all nodes
		if (nodes_count > 0 && members_count == 0 ) {
			DeprecateNodes (document, nodes);
			return true;
		}

		ArrayList types_list;

		switch (member_type) {
		case MemberType.Field:
			types_list = MakeFieldList (members);
			break;
		case MemberType.Property:
			types_list = MakePropertyList (members);
			break;
		case MemberType.Method:
			types_list = MakeMethodList (members);
			break;
		case MemberType.Constructor:
			types_list = MakeConstructorList (members);
			break;
		case MemberType.Event:
			types_list = MakeEventList (members);
			break;
		default:
			throw new ArgumentException ();
		}

		if (types_list.Count == 0 && nodes == null)
			return false;

		// There are no existing nodes, we generate all members
		if (types_list.Count > 0 && nodes == null) {
			GenerateMembers (members, document);
			return true;
		}

		Match [] nodes_list = MakeNodesList (nodes);

		// Look for stuff that we can match between the type and the document
		foreach (object obj in types_list) {
			foreach (Match node in nodes_list) {
				Match type = (Match) obj;
				if (node.Signature != type.Signature) {
					continue;
				} else {
					node.Found = true;
					type.Found = true;
				}
			}
		}

		bool changed = false;

		// Go thru each item found in the type,
		// If it's not found, then generate it in the document
		foreach (object o in types_list) {
			Match match = (Match) o;
			if (match.Found) {
				continue;
			} else {
				XmlElement element = GetMembersElement (document);
				switch (member_type) {
				case MemberType.Constructor:
					AddConstructor (element, (ConstructorInfo) match.Member);
					break;
				case MemberType.Event:
					AddEvent (element, (EventInfo) match.Member);
					break;
				case MemberType.Method:
					AddMethod (element, (MethodInfo) match.Member);
					break;
				case MemberType.Property:
					AddProperty (element, (PropertyInfo) match.Member);
					break;
				case MemberType.Field:
					AddField (element, (FieldInfo) match.Member);
					break;
				default:
					throw new Exception ();
				}

				changed = true;
			}
		}

		// Go thru each item in the node list,
		// If there's no corresponding MemberInfo in the type,
		// then mark it as deprecated and print a warning.
		foreach (Match match in nodes_list) {
			if (match.Found)
				continue;
			else {
				DeprecateNode (document, match.Node);
				changed = true;
			}
		}

		return changed;
	}

	void GenerateMembers (MemberInfo [] members, XmlDocument document)
	{
		XmlElement element = GetMembersElement (document);
		foreach (MemberInfo member in members) {
			if (member is FieldInfo) {
				Console.WriteLine ("Adding field: " + member);
				AddField (element, (FieldInfo) member);
				continue;

			} else if (member is PropertyInfo) {
				Console.WriteLine ("Adding prop: " + member);
				AddProperty (element, (PropertyInfo) member);
				continue;

			} else if (member is ConstructorInfo) {
				Console.WriteLine ("Adding ctor: " + member);
				AddConstructor (element, (ConstructorInfo) member);
				continue;

			} else if (member is MethodInfo) {
				Console.WriteLine ("Adding method: " + member);
				AddMethod (element, (MethodInfo) member);
				continue;

			} else if (member is EventInfo) {
				Console.WriteLine ("Adding event: " + member);
				AddEvent (element, (EventInfo) member);
				continue;

			} else {
				Console.WriteLine ("{0} is of type {1}", member.Name, member.GetType ());
				throw new Exception ("Can't generate member.");
			}
		}
	}

	void DeprecateNodes (XmlDocument document, XmlNodeList nodes)
	{
		foreach (XmlNode node in nodes)
			DeprecateNode (document, node);
	}

	void DeprecateNode (XmlDocument document, XmlNode node)
	{
		Console.Write ("Warning! {0} not found in assembly.", GetNodeName (node));

/*
		if (IsStubbed (node)) {
			XmlNode members = document.SelectSingleNode ("/Type/Members");
			node.RemoveAll ();
			members.RemoveChild (node);
			Console.WriteLine (" Removed, since it was stubbed.");
		} else {
*/
			Console.WriteLine (" Marking it as deprecated.");
			((XmlElement) node).SetAttribute ("Deprecated", "true");

		//}
	}

	ArrayList MakeMethodList (MemberInfo [] members)
	{
		ArrayList list = new ArrayList ();
		if (members.Length == 0)
			return list;

		MethodInfo [] methods = (MethodInfo []) members;

		foreach (MethodInfo method in methods) {

			// Filter out methods that are also properties
			if (method.IsSpecialName)
				continue;

			// Filter out methods from events
			if (method.Name.StartsWith ("add_") || method.Name.StartsWith ("remove_"))
				continue;

			string signature = AddMethodSignature (method);

			if (signature == null)
				continue;

			Match m = new Match (signature, false);
			m.Member = method;
			list.Add (m);
		}

		return list;
	}

	string GetNodeSignature (XmlDocument document)
	{
		XmlElement type_signature = (XmlElement) document.SelectSingleNode ("/Type/TypeSignature");
		return type_signature.GetAttribute ("Value");
	}

	void SetTypeSignature (XmlDocument document, string signature)
	{
		XmlElement type_signature = (XmlElement) document.SelectSingleNode ("/Type/TypeSignature");
		type_signature.SetAttribute ("Value", signature);
	}

	ArrayList MakePropertyList (MemberInfo [] members)
	{
		ArrayList list = new ArrayList ();
		if (members.Length == 0)
			return list;

		PropertyInfo [] properties = (PropertyInfo []) members;

		foreach (PropertyInfo property in properties) {

			string signature = AddPropertySignature (property);

			if (signature == null)
				continue;

			Match m = new Match (signature, false);
			m.Member = property;
			list.Add (m);
		}
		return list;
	}

	ArrayList MakeConstructorList (MemberInfo [] members)
	{
		ArrayList list = new ArrayList ();
		if (members.Length == 0)
			return list;

		ConstructorInfo [] constructors = (ConstructorInfo []) members;

		foreach (ConstructorInfo constructor in constructors) {
			string signature = AddConstructorSignature (constructor);

			// .cctors are not suppose to be visible
			if (signature == null || constructor.Name == ".cctor")
				continue;

			Match m = new Match (signature, false);
			m.Member = constructor;
			list.Add (m);
		}
		return list;
	}

	ArrayList MakeFieldList (MemberInfo [] members)
	{
		ArrayList list = new ArrayList ();
		if (members.Length == 0)
			return list;

		FieldInfo [] fields = (FieldInfo []) members;

		foreach (FieldInfo field in fields) {
			string signature = AddFieldSignature (field);

			if (signature == null)
				continue;

			Match m = new Match (signature, false);
			m.Member = field;
			list.Add (m);
		}
		return list;
	}

	ArrayList MakeEventList (MemberInfo [] members)
	{
		ArrayList list = new ArrayList ();
		if (members.Length == 0)
			return list;

		EventInfo [] events = (EventInfo []) members;

		foreach (EventInfo ev in events) {
			string signature = AddEventSignature (ev);

			if (signature == null)
				continue;

			Match m = new Match (signature, false);
			m.Member = ev;
			list.Add (m);
		}
		return list;
	}

	Match [] MakeNodesList (XmlNodeList nodes)
	{
		Match [] list = new Match [nodes.Count];

		if (nodes.Count == 0)
			return list;

		for (int i = 0; i < list.Length; i ++) {
			Match m = new Match (GetNodeSignature (nodes [i]), false);
			m.Node = nodes [i];
			list [i] = m;
		}

		return list;
	}

	XmlNodeList GetNodesOfType (XmlDocument document, string type)
	{
		string expression = String.Format ("/Type/Members/Member[MemberType=\"{0}\"]", type);
		XmlNodeList nodes = document.SelectNodes (expression);

		if (nodes.Count == 0)
			return null;

		return nodes;
	}

	string GetNodeSignature (XmlNode node)
	{
		XmlElement signature = node.SelectSingleNode ("./MemberSignature") as XmlElement;
		return signature.GetAttribute ("Value");
	}

	string GetNodeName (XmlNode node)
	{
		XmlElement signature = (XmlElement) node;
		return signature.GetAttribute ("MemberName");
	}

	XmlElement GetMembersElement (XmlDocument document)
	{
		XmlNode node =  document.SelectSingleNode ("/Type/Members");

		return (XmlElement) node;
	}

	XmlDocument Generate (Type type)
	{
		bool isDelagate;
		string signature = AddTypeSignature (type, out isDelagate);

		if (signature == null)
			return null;

		XmlDocument document = new XmlDocument ();

		//
		// This is the top level <type> node
		//
		XmlElement type_node = document.CreateElement ("Type");
		document.AppendChild (type_node);
		type_node.SetAttribute ("Name", type.Name);
		type_node.SetAttribute ("FullName", type.FullName);

		XmlElement type_signature = document.CreateElement ("TypeSignature");
		type_signature.SetAttribute ("Language", "C#");
		type_signature.SetAttribute ("Value", signature);
		type_signature.SetAttribute ("Maintainer", "auto");

		type_node.AppendChild (type_signature);

		//
		// This is for the AssemblyInfo nodes
		//
		XmlElement assembly_info = document.CreateElement ("AssemblyInfo");
		type_node.AppendChild (assembly_info);

		AssemblyName asm_name = type.Assembly.GetName ();

		byte[] public_key = asm_name.GetPublicKey ();
		string key;

		if (public_key == null)
			key = "null";
		else
			key = GetKeyString (public_key);

		CultureInfo ci = asm_name.CultureInfo;
		string culture;

		if ((ci == null) || (ci.LCID == CultureInfo.InvariantCulture.LCID))
			culture = "neutral";
		else
			culture = ci.ToString ();

		assembly_info.AppendChild (AddElement (document, "AssemblyName", asm_name.Name));
		assembly_info.AppendChild (AddElement (document, "AssemblyPublicKey", key));
		assembly_info.AppendChild (AddElement (document, "AssemblyVersion", asm_name.Version.ToString ()));
		assembly_info.AppendChild (AddElement (document, "AssemblyCulture", culture));

		//
		// Assembly-level Attribute nodes
		//
		XmlElement assembly_attributes = document.CreateElement ("Attributes");
		assembly_info.AppendChild (assembly_attributes);

		object [] attrs = type.Assembly.GetCustomAttributes (false);
		AddAttributes (document, assembly_attributes, attrs);

		//
		// Thread-safety node
		//
		XmlElement thread_safety_statement = document.CreateElement ("ThreadSafetyStatement");
		XmlElement link_element = document.CreateElement ("link");
		link_element.SetAttribute ("location", "node:gtk-sharp/programming/threads");
		link_element.AppendChild (document.CreateTextNode ("Gtk# Thread Programming"));

		thread_safety_statement.AppendChild (
			document.CreateTextNode ("Gtk# is thread aware, but not thread safe; See the "));
		thread_safety_statement.AppendChild (link_element);
		thread_safety_statement.AppendChild (
			document.CreateTextNode (" for details."));
		
		type_node.AppendChild (thread_safety_statement);

		//
		// Class-level <Docs> node
		//
		type_node.AppendChild (AddDocsNode (document));

		//
		// <Base>
		//
		XmlElement base_node = document.CreateElement ("Base");
		type_node.AppendChild (base_node);

		if (type.IsEnum)
			base_node.AppendChild (AddElement (document, "BaseTypeName", "System.Enum"));

		else if (type.IsValueType)
			base_node.AppendChild (AddElement (document, "BaseTypeName", "System.ValueType"));

		else if (isDelagate)
			base_node.AppendChild (AddElement (document, "BaseTypeName", "System.Delegate"));

		else if (type.IsClass && type != typeof (object))
			base_node.AppendChild (AddElement (document, "BaseTypeName", type.BaseType.FullName));

		//
		// <Interfaces>
		//
		XmlElement interfaces = document.CreateElement ("Interfaces");
		type_node.AppendChild (interfaces);
		Type [] ifaces = type.GetInterfaces ();

		if (ifaces != null) {
			foreach (Type iface in ifaces ) {
				XmlElement interface_node = document.CreateElement ("Interface");
				interfaces.AppendChild (interface_node);
				XmlElement interface_name = AddElement (
					document, "InterfaceName", iface.FullName);
				interface_node.AppendChild (interface_name);
			}
		}

		//
		// <Attributes>
		//
		XmlElement class_attributes = document.CreateElement ("Attributes");
		object [] class_attrs = type.GetCustomAttributes (false);
		AddAttributes (document, class_attributes, class_attrs);

		type_node.AppendChild (class_attributes);

		//
		// <Members>
		//
		XmlElement members;

		//
		// delegates have an empty <Members> element.
		//
		if (isDelagate)
			members = document.CreateElement ("Members");
		else
			members = AddMembersNode (document, type);

		type_node.AppendChild (members);

		//
		// delegates have a top-level parameters and return value section
		//
		if (isDelagate) {
			System.Reflection.MethodInfo method = type.GetMethod ("Invoke");
			Type return_type = method.ReturnType;
			ParameterInfo [] parameters = method.GetParameters ();
					
			type_node.AppendChild (AddReturnValue (document, return_type));
			type_node.AppendChild (AddParameters (document, parameters));
		}

		return document;
	}

	string GetKeyString (byte [] key)
	{
		if (key.Length == 0)
			return String.Empty;

		string s = BitConverter.ToString (key);
		s = s.Replace ('-', ' ');

		return '[' + s + ']';
	}

	void AddAttributes (XmlDocument document, XmlElement root_element, object [] attributes)
	{
		if (attributes == null)
			return;

		foreach (object attr in attributes) {
			//
			// Filter out the AssemblyFunkyAttributes
			//
			if (((Attribute) attr).GetType ().FullName.StartsWith ("System.Reflection.Assembly"))
				continue;

			else {
				XmlElement attribute = document.CreateElement ("Attribute");
				root_element.AppendChild (attribute);
				XmlElement attribute_name = AddElement (document,
						"AttributeName", ((Attribute) attr).GetType ().FullName);
				attribute.AppendChild (attribute_name);
			}
		}
	}

	XmlElement AddElement (XmlDocument document, string name, string text)
	{
		XmlElement element = document.CreateElement (name);

		if (text != null) {
			XmlText text_node = document.CreateTextNode (text);
			element.AppendChild (text_node);
		}

		return element;
	}

	XmlElement AddDocsNode (XmlDocument document)
	{
		XmlElement docs = document.CreateElement ("Docs");
		docs.AppendChild (AddElement (document, "summary", EmptyString));
		docs.AppendChild (AddElement (document, "remarks", EmptyString));

		return docs;
	}

	XmlElement AddDocsNode (XmlDocument document, Type return_value, ParameterInfo [] pi)
	{
		XmlElement docs = document.CreateElement ("Docs");
		docs.AppendChild (AddElement (document, "summary", EmptyString));

		if (pi != null)
			foreach (ParameterInfo param in pi)
				docs.AppendChild (AddDocsParamNode (document, param));

		XmlElement returns = AddDocsReturnsNode (document, return_value);

		if (returns != null)
			docs.AppendChild (returns);

		docs.AppendChild (AddElement (document, "remarks", EmptyString));

		return docs;
	}

	XmlElement AddDocsParamNode (XmlDocument document, ParameterInfo parameter)
	{
		Type param_type = parameter.ParameterType;
		Type element_type = param_type.GetElementType();
		XmlElement see_node = document.CreateElement ("see");
		see_node.SetAttribute ("cref", "T:" + (element_type == null ? param_type.ToString() : element_type.ToString()));

		XmlElement param = document.CreateElement ("param");
		param.SetAttribute ("name", parameter.Name);
		XmlText text_node =  document.CreateTextNode ("a ");
		param.AppendChild (text_node);
		param.AppendChild (see_node);

		return param;
	}

	XmlElement AddDocsReturnsNode (XmlDocument document, Type return_value)
	{
		string return_type = ConvertCTSName (return_value.FullName);

		//
		// Return now if it returns void here.
		//
		if (return_type == "void")
			return null;

		XmlElement see_node = document.CreateElement ("see");
		see_node.SetAttribute ("cref", "T:" + return_value.FullName);

		XmlElement param = document.CreateElement ("returns");
		XmlText text_node =  document.CreateTextNode ("a ");
		param.AppendChild (text_node);
		param.AppendChild (see_node);

		return param;
	}

	XmlElement AddMembersNode (XmlDocument document, Type t)
	{
		XmlElement members = document.CreateElement ("Members");
		BindingFlags static_flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly |BindingFlags.Static;
		BindingFlags  instance_flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly |BindingFlags.Instance;

		foreach (FieldInfo fi in t.GetFields (static_flag))
			AddField (members, fi);

		foreach (FieldInfo fi in t.GetFields (instance_flag))
			AddField (members, fi);

		foreach (MethodInfo mi in t.GetMethods (static_flag))
			AddMethod (members, mi);

		foreach (MethodInfo mi in t.GetMethods (instance_flag))
			AddMethod (members, mi);

		foreach (ConstructorInfo ci in t.GetConstructors (static_flag))
			AddConstructor (members, ci);

		foreach (ConstructorInfo ci in t.GetConstructors (instance_flag))
			AddConstructor (members, ci);

		foreach (PropertyInfo pi in t.GetProperties (static_flag))
			AddProperty (members, pi);

		foreach (PropertyInfo pi in t.GetProperties (instance_flag))
			AddProperty (members, pi);

		foreach (EventInfo ei in t.GetEvents (static_flag))
			AddEvent (members, ei);

		foreach (EventInfo ei in t.GetEvents (instance_flag))
			AddEvent (members, ei);

		return members;
	}

	void AddField (XmlElement members, FieldInfo field)
	{
		XmlDocument document = members.OwnerDocument;
		string signature = AddFieldSignature (field);

		if (signature == null)
			return;

		XmlElement member = document.CreateElement ("Member");
		member.SetAttribute ("MemberName", field.Name);
		XmlElement field_signature = document.CreateElement ("MemberSignature");
		field_signature.SetAttribute ("Language", "C#");
		field_signature.SetAttribute ("Value", signature);

		members.AppendChild (member);
		member.AppendChild (field_signature);
		member.AppendChild (AddElement (document, "MemberType", "Field"));
		member.AppendChild (AddReturnValue (document, field.FieldType));
		member.AppendChild (AddElement (document, "Parameters", String.Empty));
		member.AppendChild (AddDocsNode (document));
	}

	void AddMethod (XmlElement members, MethodInfo method)
	{
		XmlDocument document = members.OwnerDocument;
		string signature = AddMethodSignature (method);

		if (signature == null)
			return;

		//
		// Filter out methods that are also properties
		//
		if (method.IsSpecialName)
			return;

		//
		// Filter out methods from events
		// This is a lame hack, but there's no way around it.
		//
		if (method.Name.StartsWith ("add_") || method.Name.StartsWith ("remove_"))
			return;

		XmlElement member = document.CreateElement ("Member");
		member.SetAttribute ("MemberName", method.Name);
		XmlElement method_signature = document.CreateElement ("MemberSignature");
		method_signature.SetAttribute ("Language", "C#");
		method_signature.SetAttribute ("Value", signature);

		members.AppendChild (member);
		member.AppendChild (method_signature);
		member.AppendChild (AddElement (document, "MemberType", "Method"));

		Type return_type = method.ReturnType;
		ParameterInfo [] parameters = method.GetParameters ();

		member.AppendChild (AddReturnValue (document, return_type));
		member.AppendChild (AddParameters (document, parameters));
		member.AppendChild (AddDocsNode (document, return_type, parameters));
	}

	void AddConstructor (XmlElement members, ConstructorInfo constructor)
	{
		XmlDocument document = members.OwnerDocument;
		string signature = AddConstructorSignature (constructor);
		string constructor_name = constructor.Name;

		// .cctors are not suppose to be visible
		if (signature == null || constructor.Name == ".cctor")
			return;

		XmlElement member = document.CreateElement ("Member");
		member.SetAttribute ("MemberName", constructor_name);
		members.AppendChild (member);
		XmlElement constructor_signature = document.CreateElement ("MemberSignature");
		constructor_signature.SetAttribute ("Language", "C#");
		constructor_signature.SetAttribute ("Value", signature);
		member.AppendChild (constructor_signature);
		member.AppendChild (AddElement (document, "MemberType", "Constructor"));

		Type return_type = constructor.DeclaringType;
		ParameterInfo [] parameters = constructor.GetParameters ();

		// constructors have an empty ReturnValue node.
		member.AppendChild (document.CreateElement ("ReturnValue"));
		member.AppendChild (AddParameters (document, parameters));
		member.AppendChild (AddDocsNode (document, return_type, parameters));
	}

	void AddProperty (XmlElement members, PropertyInfo property)
	{
		XmlDocument document = members.OwnerDocument;
		string signature = AddPropertySignature (property);

		if (signature == null)
			return;

		XmlElement member = document.CreateElement ("Member");
		member.SetAttribute ("MemberName", property.Name);
		members.AppendChild (member);
		XmlElement property_signature = document.CreateElement ("MemberSignature");
		property_signature.SetAttribute ("Language", "C#");
		property_signature.SetAttribute ("Value", signature);
		member.AppendChild (property_signature);
		member.AppendChild (AddElement (document, "MemberType", "Property"));

		Type return_type = property.PropertyType;
		member.AppendChild (AddReturnValue (document, return_type));

		if (property.CanRead && property.GetGetMethod () != null) {
			ParameterInfo [] parameters = property.GetGetMethod ().GetParameters ();
			member.AppendChild (AddParameters (document, parameters));
			member.AppendChild (AddDocsNode (document, return_type, parameters));
		} else
			member.AppendChild (AddDocsNode (document, return_type, null));
	}

	void AddEvent (XmlElement members, EventInfo ev)
	{
		XmlDocument document = members.OwnerDocument;
		string signature = AddEventSignature (ev);

		if (signature == null)
			return;

		XmlElement member = document.CreateElement ("Member");
		member.SetAttribute ("MemberName", ev.Name);
		members.AppendChild (member);
		XmlElement event_signature = document.CreateElement ("MemberSignature");
		event_signature.SetAttribute ("Language", "C#");
		event_signature.SetAttribute ("Value", signature);
		member.AppendChild (event_signature);
		member.AppendChild (AddElement (document, "MemberType", "Event"));
		member.AppendChild (AddReturnValue (document, ev.EventHandlerType));
		member.AppendChild (AddElement (document, "Parameters", null));
		member.AppendChild (AddDocsNode (document));
	}

	XmlElement AddReturnValue (XmlDocument document, Type retval)
	{
		XmlElement return_value = document.CreateElement ("ReturnValue");
		XmlElement return_type = document.CreateElement ("ReturnType");
		XmlText value = document.CreateTextNode (retval.FullName);

		return_type.AppendChild (value);
		return_value.AppendChild (return_type);

		return return_value;
	}

	XmlElement AddParameters (XmlDocument document, ParameterInfo [] pi)
	{
		XmlElement parameters = document.CreateElement ("Parameters");

		foreach (ParameterInfo p in pi) {
			XmlElement parameter = document.CreateElement ("Parameter");
			parameter.SetAttribute ("Name", p.Name);
			parameter.SetAttribute ("Type", p.ParameterType.FullName);

			if (p.ParameterType.IsByRef) {
				if (!p.IsOut)
					parameter.SetAttribute ("RefType", "ref");
				else
					parameter.SetAttribute ("RefType", "out");
			}
			parameters.AppendChild (parameter);
		}

		return parameters;
	}

	string GetTypeKind (Type t)
	{
		if (t.IsEnum || t == typeof (System.Enum))
			return "enum";
		if (t.IsClass)
			return "class";
		if (t.IsInterface)
			return "interface";
		if (t.IsValueType)
			return "struct";
		else
			throw new ArgumentException ();
	}

	string GetTypeVisibility (TypeAttributes ta)
	{
		switch (ta & TypeAttributes.VisibilityMask){
		case TypeAttributes.Public:
		case TypeAttributes.NestedPublic:
			return "public";

		case TypeAttributes.NestedFamily:
		case TypeAttributes.NestedFamANDAssem:
		case TypeAttributes.NestedFamORAssem:
			return "protected";

		default:
			return null;
		}
	}

	string AddTypeSignature (Type type, out bool isDelagate)
	{
		// Assume it is not a delegate
		isDelagate = false;

		if (type == null)
			return null;

		string signature;
		bool colon = true;

		string name = type.Name;
		StringBuilder extends = new StringBuilder ();
		string modifiers = String.Empty;

		TypeAttributes ta = type.Attributes;
		string kind = GetTypeKind (type);
		string visibility = GetTypeVisibility (ta);

		if (visibility == null)
			return null;

		//
		// Modifiers
		//
		if (type.IsAbstract)
			modifiers = " abstract";
		if (type.IsSealed)
			modifiers =  " sealed";

		//
		// handle delegates
		//
		if (kind == "class" && !type.IsAbstract &&
		    typeof (System.Delegate).IsAssignableFrom (type)) {
			isDelagate = true;
			return AddDelegateSignature (visibility, modifiers, name, type);
		}

		//
		// get BaseType
		//
		if (type != typeof (object) && kind == "class" && type.BaseType != typeof (object)) {
			extends.Append (" : " + type.BaseType.FullName);
			colon = false;
		}

		//
		// Implements interfaces...
		//
		Type [] interfaces = type.GetInterfaces ();

		if (interfaces.Length != 0) {
			if (colon)
				extends.Append (" : ");
			else
				extends.Append (", ");

			for (int i = 0; i < interfaces.Length; i ++){
				extends.Append (interfaces [i].Name);
				if (i + 1 != interfaces.Length) extends.Append (", ");
			}
		}

		//
		// Put it together
		//
		switch (kind){
		case "class":
			signature = String.Format ("{0}{1} {2} {3}{4}",
					visibility, modifiers, kind, name, extends.ToString ());
			break;

		case "enum":
			signature = String.Format ("{0} {1} {2}",
					visibility, kind, name);
			break;

		case "interface":
		case "struct":
		default:			
			signature = String.Format ("{0}{1} {2} {3}",
					visibility, modifiers, kind, name);
			break;
		}

		return signature;
	}

	string AddDelegateSignature (string visibility, string modifiers, string name, Type type)
	{
		string signature;

		MethodInfo invoke = type.GetMethod ("Invoke");
		string arguments = GetMethodParameters (invoke);
		string return_value = ConvertCTSName (invoke.ReturnType.FullName);

		signature = String.Format ("{0}{1} delegate {2} {3} {4};",
				visibility, modifiers, return_value, name, arguments);

		return signature;
	}

	string GetFieldVisibility (FieldInfo field)
	{
		if (field.IsPublic)
			return "public";

		if (field.IsFamily)
			return "protected";

		else
			return null;
	}

	string GetFieldModifiers (FieldInfo field)
	{
		if (field.IsStatic)
			return " static";

		if (field.IsInitOnly)
			return " readonly";

		else
			return null;
	}

	string GetMethodVisibility (MethodBase method)
	{
		if (method.IsPublic)
			return "public";

		if (method.IsFamily)
			return "protected";
		else
			return null;
	}

	string GetMethodModifiers (MethodBase method)
	{
		if (method.IsStatic)
			return " static";

		if (method.IsVirtual) {
			if ((method.Attributes & MethodAttributes.NewSlot) != 0)
				return " virtual";
			else
				return " override";

		} else
			return null;
	}

	string GetMethodParameters (MethodBase method)
	{
		StringBuilder sb;
		ParameterInfo [] pi = method.GetParameters ();

		if (pi.Length == 0)
			return "()";

		else {
			sb = new StringBuilder ();
			sb.Append ('(');

			int i = 0;
			string modifier;
			foreach (ParameterInfo parameter in pi) {
				bool isPointer = false;
				if (parameter.ParameterType.IsByRef) {
					sb.Append (GetParameterModifier (parameter));
					isPointer = true;
				}
				string param = ConvertCTSName (parameter.ParameterType.FullName, isPointer);

				sb.Append (param);
				sb.Append (" " + parameter.Name);
				if (i + 1 < pi.Length) sb.Append (", ");
				i++;
			}
			sb.Append (')');
		}

		return sb.ToString ();
	}

	string GetParameterModifier (ParameterInfo parameter)
	{
		int a = (int) parameter.Attributes;
		if (parameter.IsOut)
			return "out ";

		return "ref ";
	}

	string GetPropertyVisibility (PropertyInfo property)
	{
		MethodBase mb = property.GetSetMethod (true);

		if (mb == null)
			mb = property.GetGetMethod (true);

		return GetMethodVisibility (mb);
	}

	string GetPropertyModifiers (PropertyInfo property)
	{
		MethodBase mb = property.GetSetMethod (true);

		if (mb == null)
			mb = property.GetGetMethod (true);

		return GetMethodModifiers (mb);
	}

	string GetEventModifiers (EventInfo ev)
	{
		return GetMethodModifiers (ev.GetAddMethod ());
	}

	string GetEventVisibility (EventInfo ev)
	{
		MethodInfo add = ev.GetAddMethod ();
		if (add == null)
			return null;
		return GetMethodVisibility (add);
	}

	string GetEventType (EventInfo ev)
	{
		ParameterInfo [] pi = ev.GetAddMethod ().GetParameters ();

		if (pi.Length != 1)
			throw new ArgumentException ("There is more than one argument to the add_ method of this event.");

		return ConvertCTSName (pi [0].ParameterType.FullName);
	}

	string AddFieldSignature (FieldInfo field)
	{
		string signature;
		string visibility = GetFieldVisibility (field);

		if (visibility == null)
			return null;

		string type = ConvertCTSName (field.FieldType.FullName);
		string name = field.Name;
		string modifiers = GetFieldModifiers (field);

		signature = String.Format ("{0}{1} {2} {3};",
				visibility, modifiers, type, name);

		if (field.DeclaringType.IsEnum)
			signature = name;
		
		return signature;
	}

	string AddMethodSignature (MethodInfo method)
	{
		string signature;
		string visibility = GetMethodVisibility (method);

		if (visibility == null)
			return null;

		string modifiers = GetMethodModifiers (method);
		string return_type = ConvertCTSName (method.ReturnType.FullName);
		string method_name = method.Name;
		string parameters = GetMethodParameters (method);

		signature = String.Format ("{0}{1} {2} {3} {4};",
				visibility, modifiers, return_type, method_name, parameters);

		return signature;
	}

	string AddConstructorSignature (ConstructorInfo constructor)
	{
		string signature;
		string visibility = GetMethodVisibility (constructor);

		if (visibility == null)
			return null;

		string modifiers = GetMethodModifiers (constructor);
		string name = constructor.DeclaringType.Name;
		string parameters = GetMethodParameters (constructor);

		signature = String.Format ("{0}{1} {2} {3};",
				visibility, modifiers, name, parameters);

		return signature;
	}

	string AddPropertySignature (PropertyInfo property)
	{
		string signature;
		string visibility = GetPropertyVisibility (property);

		if (visibility == null)
			return null;

		string modifiers = GetPropertyModifiers (property);
		string name = property.Name;

		string type_name = property.PropertyType.FullName;

		if (property.PropertyType.IsArray) {
			int i = type_name.IndexOf ('[');
			if (type_name [i - 1] != ' ')
				type_name = type_name.Insert (i, " "); // always put a space before the []
		}

		string return_type = ConvertCTSName (type_name);
		string arguments = null;

		if (property.CanRead && property.CanWrite)
			arguments = "{ set; get; }";

		else if (property.CanRead)
			arguments = "{ get; }";

		else if (property.CanWrite)
			arguments = "{ set; }";

		signature = String.Format ("{0}{1} {2} {3} {4};",
				visibility, modifiers, return_type, name, arguments);

		return signature;
	}

	string AddEventSignature (EventInfo ev)
	{
		string signature;
		string visibility = GetEventVisibility (ev);

		if (visibility == null)
			return null;

		string modifiers = GetEventModifiers (ev);
		string name = ev.Name;
		string type = GetEventType (ev);

		signature = String.Format ("{0}{1} event {2} {3};",
				visibility, modifiers, type, name);

		return signature;
	}

	string ConvertCTSName (string type, bool shorten)
	{
		if (shorten)
			type =  type.Substring (0, type.Length - 1);

		string retval =  ConvertCTSName (type);

		return retval;
	}

	//
	// Utility function: converts a fully .NET qualified type name into a C#-looking one
	//
	string ConvertCTSName (string type)
	{
		string retval = String.Empty;
		bool isArray = false;
		bool isPointer = false;

		if (!type.StartsWith ("System."))
			return type;

		if (type.EndsWith ("[]")) {
			isArray = true;
			type = type.Substring (0, type.Length - 2);
			type = type.TrimEnd ();
		}

		if (type.EndsWith ("&")) {
			isPointer = true;
			type = type.Substring (0, type.Length - 1);
			type = type.TrimEnd ();
		}

		switch (type) {
		case "System.Byte": retval = "byte"; break;
		case "System.SByte": retval = "sbyte"; break;
		case "System.Int16": retval = "short"; break;
		case "System.Int32": retval = "int"; break;
		case "System.Int64": retval = "long"; break;

		case "System.UInt16": retval = "ushort"; break;
		case "System.UInt32": retval = "uint"; break;
		case "System.UInt64": retval = "ulong"; break;

		case "System.Single":  retval = "float"; break;
		case "System.Double":  retval = "double"; break;
		case "System.Decimal": retval = "decimal"; break;
		case "System.Boolean": retval = "bool"; break;
		case "System.Char":    retval = "char"; break;
		case "System.Void":    retval = "void"; break;
		case "System.String":  retval = "string"; break;
		case "System.Object":  retval = "object"; break;

		default:
			if (type.StartsWith (ns))
				retval = type.Substring (ns.Length + 1);
			else if (type.StartsWith ("System") && 
					(type.IndexOf ('.') == type.LastIndexOf ('.')))
				retval = type.Substring (7);
			else
				retval = type;
			break;
		}

		if (isArray)
			retval = retval + " []";

		if (isPointer)
			retval = retval + "&";

		return retval;
	}
}
}
