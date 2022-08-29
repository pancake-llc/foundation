using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	public class CustomTypeUint : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeUint() : base(typeof(uint))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			writer.WritePrimitive((uint)obj);
		}

		public override object Read<T>(Reader reader)
		{
			return (T)(object)reader.Read_uint();
		}
	}

	public class CustomTypeUintArray : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeUintArray() : base(typeof(uint[]), CustomTypeUint.Instance)
		{
			Instance = this;
		}
	}
}