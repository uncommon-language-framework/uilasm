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
		LdStr, // load string
		LdNC, // load numerical constant
		LdFld, // load field
		LdLoc, // load local
		LdAPL, // load argument-passed local
		LdElem, // load element
		LdITO, // may remove, probably useless [load internal type object]
		LdETO, // may remove, [load external type object]
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
		CtObjSS, // construct new object on stack (short lifetime, only guaranteed to last until next call/vcall/ctobjs* allocation or end of section/scope, whichever comes first)
		CtObjSL, // construct new object on stack (long lifetime, guaranteed to last until end of section/scope)
		CtObjH, // construct new object on heap (heap lifetime, guaranteed to last until all references are lost)
		NewArr, // allocate new array
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
		Dependency,

		FieldDecl,
		LocalDecl,
}