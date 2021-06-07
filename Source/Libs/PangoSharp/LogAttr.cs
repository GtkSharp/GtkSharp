namespace Pango {

	using System;

	public partial struct LogAttr {

		public LogAttr(uint bitfield) => _bitfield0 = bitfield;
		public override string ToString() => Convert.ToString(_bitfield0 & 0x1FFF, 2).PadLeft(13, '0');

		public uint Bitfield => _bitfield0;
		public bool IsLineBreak => (_bitfield0 & (1 << 0)) != 0;
		public bool IsMandatoryBreak => (_bitfield0 & (1 << 1)) != 0;
		public bool IsCharBreak => (_bitfield0 & (1 << 2)) != 0;
		public bool IsWhite => (_bitfield0 & (1 << 3)) != 0;
		public bool IsCursorPosition => (_bitfield0 & (1 << 4)) != 0;
		public bool IsWordStart => (_bitfield0 & (1 << 5)) != 0;
		public bool IsWordEnd => (_bitfield0 & (1 << 6)) != 0;
		public bool IsSentenceBoundary => (_bitfield0 & (1 << 7)) != 0;
		public bool IsSentenceStart => (_bitfield0 & (1 << 8)) != 0;
		public bool IsSentenceEnd => (_bitfield0 & (1 << 9)) != 0;
		public bool BackspaceDeletesCharacter => (_bitfield0 & (1 << 10)) != 0;
		public bool IsExpandableSpace => (_bitfield0 & (1 << 11)) != 0;
		public bool IsWordBoundary => (_bitfield0 & (1 << 12)) != 0;
	}
}
