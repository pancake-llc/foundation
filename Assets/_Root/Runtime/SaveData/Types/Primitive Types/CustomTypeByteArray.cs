using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	public class CustomTypeByteArray : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeByteArray() : base(typeof(byte[]))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			writer.WritePrimitive((byte[])obj);
		}

		public override object Read<T>(Reader reader)
		{
			return (T)(object)reader.Read_byteArray();
		}
	}
}