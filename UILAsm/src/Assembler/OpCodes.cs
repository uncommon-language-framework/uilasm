namespace UILAsm.Assembler;

enum OpCodes : byte
{
	Add,
	Sub,
	Mul,
	Div,
	Mod,
	DivMod,
	And,
	Or,
	Not,
	Xor,
	CstNV, // Cast Numerical Value
	LdStr, // to fix GC problem with ldstr, maybe allocate separately (on own) & delete at the end of JITContext
	LdNC, // load numerical constant
	LdFld, // load field
	LdLoc, // load local
	LdAPL, // load argument-passed local
	LdElem, // load element
	LdITO, // may remove, probably useless [load internal type object]
	LdETO, // may remove, may not be feasible (also same GC problem?) [load external type object]
	LdLst, // load last - pushes the most recently popped item back onto the evaluation stack (this is only guaranteed to work after a StXXX instruction and cannot be used successively)
	VTSCp, // ValueType Stack Copy [may remove - find uses?]
	Call,
	Box,
	UnBox,
	VCall, // virtual call
	StFld, // store field
	StLoc, // store local
	StAPL, // store argument-passed local
	StElem, // store element
	AllocS, // allocate (but don't initialize) new object [short lifetime - until next valtype allocation by Call/VCall or AllocS]
	AllocL, // allocate (but don't initialize) new object [long lifetime]
	NewArr, // create array
	Ret,
	
	/* Jumping Instructions */
	Jmp, // jump
	JNE, // jump if not eq
	JEq, // jump if eq
	JLT, // jump if less than
	JGT, // jump if greater than
	JLU, // jump if less than (unsigned)
	JGU, // jump if greater than (unsigned)
	JLE, // jump if less than or eq
	JGE, // jump if greater than or eq
	JLEU, // jump ig less than or eq (unsigned)
	JGEU, // jump if greater than or eq (unsigned)
	JNZ, // jump if truthy/nonzero (e.g != 0)
	JEqZ, // jump if false (e.g. == 0)

	/* Below are UIL signals, used to section code */
	BeginType,
	EndTypeMeta,
	EndType,
	BeginMethod,
	BeginCtor,
	BeginDtor,
	EndMethod,
	BeginSection,
	EndAssembly,
	NewArg,

	FieldDecl,
	LocalDecl,
}