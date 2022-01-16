using System;
using System.Runtime.InteropServices;

namespace Cairo
{
	[StructLayout(LayoutKind.Explicit)]
	public struct PathData
	{
		[FieldOffset(0)]
		public PathDataHeader Header;
		[FieldOffset(0)]
		public PathDataPoint Point;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct PathDataHeader
	{
		public PathDataType Type;
		public int Length;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct PathDataPoint
	{
		public double X;
		public double Y;
	}
}
