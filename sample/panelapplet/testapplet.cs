using System;
using PanelApplet;
using Gtk;

namespace AppletTest
{
	public class PanelAppletClass : PanelApplet
	{

		protected PanelAppletClass (IntPtr raw) : base (raw) {}

		static void Main (string[] argv)
		{
			Gnome.Program p = new Gnome.Program ("CSharpTestApplet", "0.1", Gnome.Modules.UI, argv);
			AppletFactory.Register (typeof (PanelAppletClass));
		}

		Label testLabel;
		public override void Creation ()
		{
			testLabel = new Label ("MonoTest");
			this.Add (testLabel);
			this.ShowAll ();
			string xml = "<popup name=\"button3\"><menuitem name=\"Properties\" verb=\"LabelChange\" _label=\"_Change Label\" pixtype=\"stock\" pixname=\"gtk-properties\"/></popup>";
			this.SetupMenu (xml, new BonoboUIVerb [] { new BonoboUIVerb ("LabelChange", new ContextMenuItemCallback (LabelChangeCB)) });
		}

		int i = 0;
		void LabelChangeCB ()
		{
			testLabel.Text = String.Format ("Changed {0} time(s).", ++i);
		}

		public override string IID {
			get { return "OAFIID:CSharpTestApplet"; }
		}

		public override string FactoryIID {
			get { return "OAFIID:CSharpTestApplet_Factory"; }
		}
	}
}
