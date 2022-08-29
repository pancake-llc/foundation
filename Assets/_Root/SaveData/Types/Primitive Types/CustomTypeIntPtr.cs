using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	public class CustomTypeIntPtr : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeIntPtr() : base(typeof(IntPtr))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			writer.WritePrimitive((long)(IntPtr)obj);
		}

		public override object Read<T>(Reader reader)
		{
			return (T)(object)(IntPtr)reader.Read_long();
		}
	}

	public class CustomTypeIntPtrArray : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeIntPtrArray() : base(typeof(IntPtr[]), CustomTypeIntPtr.Instance)
		{
			Instance = this;
		}
	}
}