using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	[Properties("x", "y")]
	public class CustomTypeVector2 : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeVector2() : base(typeof(Vector2))
		{
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			Vector2 casted = (Vector2)obj;
			writer.WriteProperty("x", casted.x, CustomTypeFloat.Instance);
			writer.WriteProperty("y", casted.y, CustomTypeFloat.Instance);
		}

		public override object Read<T>(Reader reader)
		{
			return new Vector2(	reader.ReadProperty<float>(CustomTypeFloat.Instance),
								reader.ReadProperty<float>(CustomTypeFloat.Instance));
		}
	}

		public class CustomTypeVector2Array : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeVector2Array() : base(typeof(Vector2[]), CustomTypeVector2.Instance)
		{
			Instance = this;
		}
	}
}