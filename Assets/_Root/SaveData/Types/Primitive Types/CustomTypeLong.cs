using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	public class CustomTypeLong : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeLong() : base(typeof(long))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			writer.WritePrimitive((long)obj);
		}

		public override object Read<T>(Reader reader)
		{
			return (T)(object)reader.Read_long();
		}
	}

	public class CustomTypeLongArray : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeLongArray() : base(typeof(long[]), CustomTypeLong.Instance)
		{
			Instance = this;
		}
	}
}