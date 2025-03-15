using UILAsm.Assembler;

namespace UILAsm;

public class UILAsmMain
{
	static int Main(string[] args)
	{
		// temp

		string il = File.ReadAllText(args[0]);

		var assembled = ParseGen.Assemble(il);

		File.WriteAllBytes($"{args[0]}.ulas", [..BitConverter.GetBytes(assembled.il.Count), ..assembled.il, ..assembled.stringRef]);

		return 0;
	}
}