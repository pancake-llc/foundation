using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	public class CustomTypeDouble : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeDouble() : base(typeof(double))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			writer.WritePrimitive((double)obj);
		}

		public override object Read<T>(Reader reader)
		{
			return (T)(object)reader.Read_double();
		}
	}

	public class CustomTypeDoubleArray : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeDoubleArray() : base(typeof(double[]), CustomTypeDouble.Instance)
		{
			Instance = this;
		}
	}
}