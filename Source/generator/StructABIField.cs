namespace GtkSharp.Generation {

	using System;
	using System.Collections;
	using System.IO;
	using System.Xml;

	public class StructABIField : StructField {
		protected new ClassBase container_type;

		public StructABIField (XmlElement elem, ClassBase container_type) : base (elem, container_type) {
			this.container_type = container_type;
			this.getOffsetName = null;
		}

		public override void Generate (GenerationInfo gen_info, string indent) {
			this.getOffsetName = "Get" + CName + "Offset";
			base.Generate(gen_info, indent);
		}

		// All field are visible and private
		// as the goal is to respect the ABI
		protected override string Access {
			get {
				return "private";
			}
		}

		public override bool Hidden {
			get {
				return false;
			}
		}

		public override bool Validate (LogWriter log)
		{
			string cstype = SymbolTable.Table.GetCSType(CType, true);

			if (cstype == null || cstype == "") {
				log.Warn (" field \"" + CName + "\" has no cstype, can't generate ABI field.");

				return false;
			}

			if (!base.Validate (log))
				return false;

			if (IsBitfield) {
				log.Warn ("bitfields are not supported");
				return false;
			}

			return true;
		}
	}
}

