using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	public class CustomTypeUlong : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeUlong() : base(typeof(ulong))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			writer.WritePrimitive((ulong)obj);
		}

		public override object Read<T>(Reader reader)
		{
			return (T)(object)reader.Read_ulong();
		}
	}

	public class CustomTypeUlongArray : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeUlongArray() : base(typeof(ulong[]), CustomTypeUlong.Instance)
		{
			Instance = this;
		}
	}
}