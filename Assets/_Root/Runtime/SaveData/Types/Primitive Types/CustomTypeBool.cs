using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	public class CustomTypeBool : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeBool() : base(typeof(bool))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			writer.WritePrimitive((bool)obj);
		}

		public override object Read<T>(Reader reader)
		{
			return (T)(object)reader.Read_bool();
		}
	}

	public class CustomTypeBoolArray : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeBoolArray() : base(typeof(bool[]), CustomTypeBool.Instance)
		{
			Instance = this;
		}
	}
}