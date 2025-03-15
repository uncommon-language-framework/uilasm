namespace UILAsm.Assembler;

enum TypeType : byte // also used with IL
{
	Class,
	Struct,
	RefStruct,
	Record,
	Interface,
	ArrayType,
	None = 255
};

enum Modifiers : ushort // also used with IL
{
	Private = 0,
	Public = 1 << 0,
	Protected = 1 << 1,
	Internal = 1 << 2,
	Static = 1 << 3,
	Readonly = 1 << 4,
	Virtual = 1 << 5,
	Abstract = 1 << 6,
	Partial = 1 << 7,
	Extern = 1 << 8,
	Sealed = 1 << 9,
	New = 1 << 10,
};