using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	[Properties("r", "g", "b", "a")]
	public class CustomTypeColor32 : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeColor32() : base(typeof(Color32))
		{
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			Color32 casted = (Color32)obj;
			writer.WriteProperty("r", casted.r, CustomTypeByte.Instance);
			writer.WriteProperty("g", casted.g, CustomTypeByte.Instance);
			writer.WriteProperty("b", casted.b, CustomTypeByte.Instance);
			writer.WriteProperty("a", casted.a, CustomTypeByte.Instance);
		}

		public override object Read<T>(Reader reader)
		{
			return new Color32(	reader.ReadProperty<byte>(CustomTypeByte.Instance),
								reader.ReadProperty<byte>(CustomTypeByte.Instance),
								reader.ReadProperty<byte>(CustomTypeByte.Instance),
								reader.ReadProperty<byte>(CustomTypeByte.Instance));
		}

		public static bool Equals(Color32 a, Color32 b)
		{
			if(a.r != b.r || a.g != b.g || a.b != b.b || a.a != b.a)
				return false;
			return true;
		}
	}

	public class CustomTypeColor32Array : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeColor32Array() : base(typeof(Color32[]), CustomTypeColor32.Instance)
		{
			Instance = this;
		}
	}
}