using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	public class CustomTypeUshort : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeUshort() : base(typeof(ushort))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			writer.WritePrimitive((ushort)obj);
		}

		public override object Read<T>(Reader reader)
		{
			return (T)(object)reader.Read_ushort();
		}
	}

	public class CustomTypeUshortArray : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeUshortArray() : base(typeof(ushort[]), CustomTypeUshort.Instance)
		{
			Instance = this;
		}
	}
}