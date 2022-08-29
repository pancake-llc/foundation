using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	public class CustomTypeINT : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeINT() : base(typeof(int))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			writer.WritePrimitive((int)obj);
		}

		public override object Read<T>(Reader reader)
		{
			return (T)(object)reader.Read_int();
		}
	}

	public class CustomTypeINTArray : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeINTArray() : base(typeof(int[]), CustomTypeINT.Instance)
		{
			Instance = this;
		}
	}
}