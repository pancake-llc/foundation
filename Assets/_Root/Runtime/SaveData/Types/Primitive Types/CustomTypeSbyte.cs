using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	public class CustomTypeSbyte : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeSbyte() : base(typeof(sbyte))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			writer.WritePrimitive((sbyte)obj);
		}

		public override object Read<T>(Reader reader)
		{
			return (T)(object)reader.Read_sbyte();
		}
	}

		public class CustomTypeSbyteArray : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeSbyteArray() : base(typeof(sbyte[]), CustomTypeSbyte.Instance)
		{
			Instance = this;
		}
	}
}