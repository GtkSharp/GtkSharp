// Copyright (c) 2011 Novell Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


using System;

namespace GtkSharp.Generation {

	public class LogWriter {
		
		string type;
		string member;
		int level;

		public LogWriter () {
			var l = Environment.GetEnvironmentVariable("CODEGEN_DEBUG");

			level = 1;
			if (l != null) {
				level = Int32.Parse(l);
			}
		}

		public LogWriter (string type): this()
		{
			this.type = type;
		}

		public string Member {
			get { return member; }
			set { member = value; }
		}

		public string Type {
			get { return type; }
			set { type = value; }
		}

		public void Warn (string format, params object[] args)
		{
			Warn (String.Format (format, args));
		}

		public void Warn (string warning)
		{
			if (level > 0)
				Console.WriteLine ("WARN: {0}{1} - {2}", Type, String.IsNullOrEmpty (Member) ? String.Empty : "." + Member, warning);
		}

		public void Info (string info)
		{
			if (level > 1)
				Console.WriteLine ("INFO: {0}{1} - {2}", Type, String.IsNullOrEmpty (Member) ? String.Empty : "." + Member, info);
		}
	}
}

