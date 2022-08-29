using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	[Properties("r", "g", "b", "a")]
	public class CustomTypeColor : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeColor() : base(typeof(Color))
		{
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			Color casted = (Color)obj;
			writer.WriteProperty("r", casted.r, CustomTypeFloat.Instance);
			writer.WriteProperty("g", casted.g, CustomTypeFloat.Instance);
			writer.WriteProperty("b", casted.b, CustomTypeFloat.Instance);
			writer.WriteProperty("a", casted.a, CustomTypeFloat.Instance);
		}

		public override object Read<T>(Reader reader)
		{
			return new Color(	reader.ReadProperty<float>(CustomTypeFloat.Instance),
								reader.ReadProperty<float>(CustomTypeFloat.Instance),
								reader.ReadProperty<float>(CustomTypeFloat.Instance),
								reader.ReadProperty<float>(CustomTypeFloat.Instance));
		}
	}

	public class CustomTypeColorArray : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeColorArray() : base(typeof(Color[]), CustomTypeColor.Instance)
		{
			Instance = this;
		}
	}
}