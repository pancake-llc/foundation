using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	public class CustomTypeByte : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeByte() : base(typeof(byte))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			writer.WritePrimitive((byte)obj);
		}

		public override object Read<T>(Reader reader)
		{
			return (T)(object)reader.Read_byte();
		}
	}
}