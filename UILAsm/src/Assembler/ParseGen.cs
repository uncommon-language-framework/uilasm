using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace UILAsm.Assembler;

public static class ParseGen // TODO: refactor into instantiable class (then we don't have to pass around so many arguments)
{
	public static (List<byte> il, IEnumerable<byte> stringRef) Assemble(string uilstr)
	{
		List<byte> il = [];
		IEnumerable<byte> stringRef = [];

		string[] lines = uilstr.Split('\n');

		for (int i = 0; i < lines.Length; i++)
		{
			var line = lines[i];

			var parts = line.Split((char[]?) null, StringSplitOptions.RemoveEmptyEntries);

			Modifiers attrs = Modifiers.Private;

			TypeType declType = TypeType.None;

			string typeName = string.Empty;
			
			foreach (var part in parts)
			{
				if ((typeName != string.Empty) && (declType != TypeType.None)) break;

				switch (part)
				{
					case "public":
						attrs |= Modifiers.Public;
						break;
					case "class":
						declType = TypeType.Class;
						break;

					// TODO: add more

					case var name:
						typeName = name;
						break;
				}
			}

			il.Add((byte) OpCodes.BeginType);
			il.Add((byte) declType);

			AppendShort(il, (ushort) attrs);

			AppendLong(il, 8); // size: todo

			RefString(il, ref stringRef, typeName);

			RefString(il, ref stringRef, "[System]Object"); // base string ref

			il.Add((byte) OpCodes.EndTypeMeta);

			il.AddRange(CompileType(ref i, lines, ref stringRef));

			il.Add((byte) OpCodes.EndType);
		}

		il.Add((byte) OpCodes.EndAssembly);

		return (il, stringRef);
	}

	static List<byte> CompileType(ref int i, string[] lines, ref IEnumerable<byte> stringRef)
	{
		List<byte> il = [];

		for (++i; i < lines.Length; i++)
		{
			var line = lines[i];

			var parts = line.Split((char[]?) null, StringSplitOptions.RemoveEmptyEntries);

			Modifiers attrs = Modifiers.Private;

			string retTypeName = string.Empty;
			string methodName = string.Empty;

			List<string> argTypes = [];
			
			foreach (var part in parts)
			{
				if (methodName != string.Empty) break;

				switch (part)
				{
					case "public":
						attrs |= Modifiers.Public;
						break;
					case "static":
						attrs |= Modifiers.Static;
						break;
					// TODO: add more

					case var other:
						if (retTypeName == string.Empty)
						{
							retTypeName = other;
							break;
						}

						methodName = other[..other.IndexOf('(')];

						foreach (var argType in other[(other.IndexOf('(')+1)..other.IndexOf(')')].Split(',', StringSplitOptions.RemoveEmptyEntries))
						{
							argTypes.Add(argType);
						}

						break;
				}
			}

			il.Add((byte) OpCodes.BeginMethod);
			il.Add(0); // overload number: todo: add

			RefString(il, ref stringRef, methodName);

			AppendShort(il, (ushort) attrs);

			RefString(il, ref stringRef, retTypeName);

			var methodBody = CompileMethodBody(ref i, lines, ref stringRef);

			AppendLong(il, methodBody.Count+1); // method size (+1 because methodBody doesn't include the first BeginSection OpCode)

			// method args

			foreach (var argType in argTypes)
			{
				il.Add((byte) OpCodes.NewArg);
				RefString(il, ref stringRef, argType);
			}

			// end method args


			il.Add((byte) OpCodes.BeginSection);
			il.AddRange(methodBody);
			il.Add((byte) OpCodes.EndMethod);
		}	

		return il;
	}

	static List<byte> CompileMethodBody(ref int i, string[] lines, ref IEnumerable<byte> stringRef)
	{
		List<byte> il = new(100);

		for (++i; i < lines.Length; i++)
		{
			var line = lines[i];
			
			string[] parts = line.Split((char[]?) null, StringSplitOptions.RemoveEmptyEntries);
			
			if (parts.Length == 0) continue;

			string opstr = parts[0];

			switch (opstr)
			{
				case "add":
					if (EnsureIndexExists(parts, 1, "Expected numerical type identifier after 'add'"))
					{
						il.Add((byte) OpCodes.Add);
						il.Add((byte) Enum.Parse<NumericalTypeIdentifiers>(parts[1]));

						continue;
					}
					break;
				case "sub":
					if (EnsureIndexExists(parts, 1, "Expected numerical type identifier after 'sub'"))
					{
						il.Add((byte) OpCodes.Sub);
						il.Add((byte) Enum.Parse<NumericalTypeIdentifiers>(parts[1]));

						continue;
					}
					break;
				case "mul":
					if (EnsureIndexExists(parts, 1, "Expected numerical type identifier after 'mul'"))
					{
						il.Add((byte) OpCodes.Mul);
						il.Add((byte) Enum.Parse<NumericalTypeIdentifiers>(parts[1]));

						continue;
					}
					break;
				case "div":
					if (EnsureIndexExists(parts, 1, "Expected numerical type identifier after 'div'"))
					{
						il.Add((byte) OpCodes.Div);
						il.Add((byte) Enum.Parse<NumericalTypeIdentifiers>(parts[1]));

						continue;
					}
					break;
				case "mod":
					if (EnsureIndexExists(parts, 1, "Expected numerical type identifier after 'mod'"))
					{
						il.Add((byte) OpCodes.Mod);
						il.Add((byte) Enum.Parse<NumericalTypeIdentifiers>(parts[1]));

						continue;
					}
					break;
				case "divmod":
					if (EnsureIndexExists(parts, 1, "Expected numerical type identifier after 'divmod'"))
					{
						il.Add((byte) OpCodes.DivMod);
						il.Add((byte) Enum.Parse<NumericalTypeIdentifiers>(parts[1]));

						continue;
					}
					break;

				case "cstnv":
					if (EnsureIndexExists(parts, 1, "Expected two numerical type identifiers after 'cstnv'"))
					{
						if (EnsureIndexExists(parts, 1, "Expected two numerical type identifiers after 'cstnv'"))
						{
							il.Add((byte) OpCodes.CstNV);
							il.Add((byte) Enum.Parse<NumericalTypeIdentifiers>(parts[1]));
							il.Add((byte) Enum.Parse<NumericalTypeIdentifiers>(parts[2]));

							continue;
						}
					}
					break;

				case "ldstr":
					if (EnsureIndexExists(parts, 1, "expected C-style string literal following 'ldstr'"))
					{
						string str = parts[1];

						if (!((str[0] == '"') && (str[^1] == '"')))
						{
							Console.WriteLine("error: invalid string literal following 'ldstr'");
						}
						else
						{
							try
							{
								il.Add((byte) OpCodes.LdStr);
								RefString(il, ref stringRef, Regex.Unescape(str));
							}
							catch (ArgumentException exc)
							{
								Console.WriteLine($"error: {exc.Message}");
							}
						}
					}
					break;
				case "ldnc":
					if (EnsureIndexExists(parts, 1, "expected numerical type identifier after 'ldnc'"))
					{
						if (!EnsureIndexExists(parts, 2, "expected numerical constant after type identifer")) break;

						NumericalTypeIdentifiers ident = Enum.Parse<NumericalTypeIdentifiers>(parts[1]);

						il.Add((byte) OpCodes.LdNC);
						il.Add((byte) ident);

						switch (ident)
						{
							case NumericalTypeIdentifiers.i8:
								il.Add(byte.Parse(parts[2]));
								break;
							case NumericalTypeIdentifiers.u8:
								il.Add((byte) sbyte.Parse(parts[2]));
								break;
							case NumericalTypeIdentifiers.i16:
								AppendShort(il, short.Parse(parts[2]));
								break;
							case NumericalTypeIdentifiers.u16:
								AppendShort(il, ushort.Parse(parts[2]));
								break;
							case NumericalTypeIdentifiers.i32:
								AppendLong(il, int.Parse(parts[2]));
								break;
							case NumericalTypeIdentifiers.u32:
								AppendLong(il, uint.Parse(parts[2]));
								break;
							case NumericalTypeIdentifiers.i64:
								AppendLongLong(il, long.Parse(parts[2]));
								break;
							case NumericalTypeIdentifiers.u64:
								AppendLongLong(il, ulong.Parse(parts[2]));
								break;
							case NumericalTypeIdentifiers.f32:
								AppendFloat(il, float.Parse(parts[2]));
								break;
							case NumericalTypeIdentifiers.f64:
								AppendDouble(il, double.Parse(parts[2]));
								break;
						}
					}
					break;
				case "ldfld":
					if (!EnsureIndexExists(parts, 1, "expected storage binding after 'ldfld'")) break;
					
					if (parts[1] != "static" || parts[1] != "instance")
					{
						Console.WriteLine($"Unknown storage binding {parts[1]}");
						break;
					}

					if (!EnsureIndexExists(parts, 2, "expected field name after storage binding")) break;

					string fldQual = parts[2];

					var lastDot = fldQual.LastIndexOf('.');

					(string typeName, string fldName) = (fldQual[..lastDot], fldQual[(lastDot + 1)..]);

					RefString(il, ref stringRef, typeName);
					RefString(il, ref stringRef, fldName);

					break;
				case "ldapl":
					if (!EnsureIndexExists(parts, 1, "expected apl num after 'ldapl'")) break;

					il.Add((byte) OpCodes.LdAPL);
					il.Add(byte.Parse(parts[1]));

					break;
				case "ret":
					il.Add((byte) OpCodes.Ret);
					break;
				case "end":
					break;
			}
		}

		return il;
	}

	static void RefString(List<byte> il, ref IEnumerable<byte> stringRef, string str)
	{
		var stringBytes = Encoding.UTF8.GetBytes(str);

		ushort offset = 0;
		ushort length = (ushort) stringBytes.Length;

		// Check if we can reuse an existing reference

		bool foundReuse = false;

		for (ushort i = 0; i <= (stringRef.Count()-stringBytes.Length); i++)
		{
			if (stringRef.Skip(i).Take(stringBytes.Length).SequenceEqual(stringBytes))
			{
				offset = i;
				foundReuse = true;
			}
		}

		if (!foundReuse)
		{
			offset = (ushort) stringRef.Count();
			stringRef = stringRef.Concat(stringBytes);
		}

		AppendShort(il, offset); // string ref data
		AppendShort(il, length);
	}

	static void AppendShort(List<byte> il, ushort value)
	{
		var bytes = BitConverter.GetBytes(value); // use bitconverter because endianness might be different
		
		il.Add(bytes[0]);
		il.Add(bytes[1]);
	}

	static void AppendShort(List<byte> il, short value) => AppendShort(il, (ushort) value);

	static void AppendLong(List<byte> il, uint value)
	{
		var bytes = BitConverter.GetBytes(value); // use bitconverter because endianness might be different
		
		il.Add(bytes[0]);
		il.Add(bytes[1]);
		il.Add(bytes[2]);
		il.Add(bytes[3]);
	}

	static void AppendLong(List<byte> il, int value) => AppendLong(il, (uint) value);

	static void AppendLongLong(List<byte> il, ulong value)
	{
		var bytes = BitConverter.GetBytes(value); // use bitconverter because endianness might be different
		
		il.Add(bytes[0]);
		il.Add(bytes[1]);
		il.Add(bytes[2]);
		il.Add(bytes[3]);
		il.Add(bytes[4]);
		il.Add(bytes[5]);
		il.Add(bytes[6]);
		il.Add(bytes[7]);
	}

	static void AppendLongLong(List<byte> il, long value) => AppendLongLong(il, (ulong) value);

	static void AppendFloat(List<byte> il, float value) => AppendLong(il, (int) value);

	static void AppendDouble(List<byte> il, double value) => AppendLongLong(il, (long) value);

	static bool TryGetAtIndex(string[] arr, int index, string errMessage, out string elem)
	{
		if (index > (arr.Length-1))
		{
			Console.WriteLine($"error: {errMessage}");

			elem = string.Empty;

			return false;
		}

		elem = arr[index];

		return true;
	}

	static bool EnsureIndexExists(string[] arr, int index, string errMessage)
	{
		if (index > (arr.Length-1))
		{
			Console.Error.WriteLine($"error: {errMessage}");

			return false;
		}

		return true;
	}
}