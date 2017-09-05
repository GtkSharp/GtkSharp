using System;
using System.Collections.Generic;
using GLib;

namespace sample
{
	public class VariantDemo
	{
		public VariantDemo ()
		{
			var strv = new string[] {"String 1", "String 2"};
			var variant = new Variant (strv);
			Console.WriteLine (variant.Print (true));

			variant = Variant.NewTuple (new Variant[] {variant, new Variant ("String 3")});
			Console.WriteLine (variant.Print (true));

			variant = Variant.NewTuple (null);
			Console.WriteLine (variant.Print (true));

			variant = Variant.NewArray (new Variant[] {new Variant ("String 4"), new Variant ("String 5")});
			Console.WriteLine (variant.Print (true));

			variant = Variant.NewArray (VariantType.String, null);
			Console.WriteLine (variant.Print (true));

			var dict = new Dictionary<string, Variant> ();
			dict.Add ("strv", new Variant (strv));
			dict.Add ("unit", Variant.NewTuple (null));
			dict.Add ("str", new Variant ("String 6"));
			variant = new Variant (dict);
			Console.WriteLine (variant.Print (true));

			var asv = variant.ToAsv ();
			Console.WriteLine ("strv: " + asv["strv"].Print(true));
			Console.WriteLine ("unit: " + asv["unit"].Print(true));

			Console.WriteLine ("type: " + variant.Type.ToString ());

			Variant tmp;
			asv.TryGetValue ("str", out tmp);
			var str = (string) tmp;
			Console.WriteLine ("out str " + str);
		}

		public static void Main (string[] args)
		{
			new VariantDemo ();
		}
	}
}
