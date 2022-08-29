using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	public class CustomTypeString : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeString() : base(typeof(string))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			writer.WritePrimitive((string)obj);
		}

		public override object Read<T>(Reader reader)
		{
			return reader.Read_string();
		}
	}

	public class CustomTypeStringArray : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeStringArray() : base(typeof(string[]), CustomTypeString.Instance)
		{
			Instance = this;
		}
	}
}