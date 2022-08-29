using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	public class CustomTypeShort : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeShort() : base(typeof(short))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			writer.WritePrimitive((short)obj);
		}

		public override object Read<T>(Reader reader)
		{
			return (T)(object)reader.Read_short();
		}
	}

	public class CustomTypeShortArray : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeShortArray() : base(typeof(short[]), CustomTypeShort.Instance)
		{
			Instance = this;
		}
	}
}