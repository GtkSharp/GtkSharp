// HandlerNotFoundException.cs 
//
// Author: Ricardo Fernández Pascual <ric@users.sourceforge.net>
//
// (c) 2002 Ricardo Fernández Pascual

namespace Glade {

	using System;
	using System.Reflection;
	using System.Runtime.Serialization;

	/// <summary>
	///	Exception thrown when signal autoconnection fails.
	/// </summary>
	[Serializable]
	public class HandlerNotFoundException : SystemException 
	{
		string handler_name;
		string signal_name;
		EventInfo evnt;
		Type delegate_type;

		public HandlerNotFoundException (string handler_name, string signal_name, 
						 EventInfo evnt, Type delegate_type)
			: this (handler_name, signal_name, evnt, delegate_type, null)
		{
		}				 

		public HandlerNotFoundException (string handler_name, string signal_name, 
						 EventInfo evnt, Type delegate_type, Exception inner)
			: base ("No handler " + handler_name + " found for signal " + signal_name,
				inner)
		{
			this.handler_name = handler_name;
			this.signal_name = signal_name;
			this.evnt = evnt;
			this.delegate_type = delegate_type;
		}				 

		protected HandlerNotFoundException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			handler_name = info.GetString ("HandlerName");
			signal_name = info.GetString ("SignalName");
			evnt = info.GetValue ("Event", typeof (EventInfo)) as EventInfo;
			delegate_type = info.GetValue ("DelegateType", typeof (Type)) as Type;
		}

		public string HandlerName
		{
			get { 
				return handler_name;
			}
		}

		public string SignalName
		{
			get {
				return signal_name;
			}
		}

		public EventInfo Event
		{
			get {
				return evnt;
			}
		}

		public Type DelegateType 
		{
			get {
				return delegate_type;
			}
		}

		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("HandlerName", handler_name);
			info.AddValue ("SignalName", signal_name);
			info.AddValue ("Event", evnt);
			info.AddValue ("DelegateType", delegate_type);
		}
	}

}

