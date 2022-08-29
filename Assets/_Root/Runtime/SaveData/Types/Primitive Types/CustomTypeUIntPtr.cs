using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	public class CustomTypeUIntPtr : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeUIntPtr() : base(typeof(UIntPtr))
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
			return (object)reader.Read_ulong();
		}
	}

	public class CustomTypeUIntPtrArray : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeUIntPtrArray() : base(typeof(UIntPtr[]), CustomTypeUIntPtr.Instance)
		{
			Instance = this;
		}
	}
}