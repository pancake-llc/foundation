using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	public class CustomTypeDecimal : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeDecimal() : base(typeof(decimal))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			writer.WritePrimitive((decimal)obj);
		}

		public override object Read<T>(Reader reader)
		{
			return (T)(object)reader.Read_decimal();
		}
	}

	public class CustomTypeDecimalArray : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeDecimalArray() : base(typeof(decimal[]), CustomTypeDecimal.Instance)
		{
			Instance = this;
		}
	}
}