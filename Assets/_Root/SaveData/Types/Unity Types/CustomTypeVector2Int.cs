#if UNITY_2017_2_OR_NEWER
using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	[Properties("x", "y")]
	public class CustomTypeVector2Int : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeVector2Int() : base(typeof(Vector2Int))
		{
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			Vector2Int casted = (Vector2Int)obj;
			writer.WriteProperty("x", casted.x, CustomTypeINT.Instance);
			writer.WriteProperty("y", casted.y, CustomTypeINT.Instance);
		}

		public override object Read<T>(Reader reader)
		{
			return new Vector2Int(	reader.ReadProperty<int>(CustomTypeINT.Instance),
									reader.ReadProperty<int>(CustomTypeINT.Instance));
		}
	}

		public class CustomTypeVector2IntArray : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeVector2IntArray() : base(typeof(Vector2Int[]), CustomTypeVector2Int.Instance)
		{
			Instance = this;
		}
	}
}
#endif