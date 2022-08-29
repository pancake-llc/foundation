#if UNITY_2017_2_OR_NEWER
using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	[Properties("x", "y", "z")]
	public class CustomTypeVector3Int : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeVector3Int() : base(typeof(Vector3Int))
		{
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			Vector3Int casted = (Vector3Int)obj;
			writer.WriteProperty("x", casted.x, CustomTypeINT.Instance);
			writer.WriteProperty("y", casted.y, CustomTypeINT.Instance);
			writer.WriteProperty("z", casted.z, CustomTypeINT.Instance);
		}

		public override object Read<T>(Reader reader)
		{
			return new Vector3Int(	reader.ReadProperty<int>(CustomTypeINT.Instance),
									reader.ReadProperty<int>(CustomTypeINT.Instance),
									reader.ReadProperty<int>(CustomTypeINT.Instance));
		}
	}

		public class CustomTypeVector3IntArray : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeVector3IntArray() : base(typeof(Vector3Int[]), CustomTypeVector3Int.Instance)
		{
			Instance = this;
		}
	}
}
#endif