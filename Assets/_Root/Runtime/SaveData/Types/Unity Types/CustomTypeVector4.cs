using System;
using UnityEngine;
using System.Collections.Generic;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	[Properties("x", "y", "z", "w")]
	public class CustomTypeVector4 : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeVector4() : base(typeof(Vector4))
		{
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			Vector4 casted = (Vector4)obj;
			writer.WriteProperty("x", casted.x, CustomTypeFloat.Instance);
			writer.WriteProperty("y", casted.y, CustomTypeFloat.Instance);
			writer.WriteProperty("z", casted.z, CustomTypeFloat.Instance);
			writer.WriteProperty("w", casted.w, CustomTypeFloat.Instance);
		}

		public override object Read<T>(Reader reader)
		{
			return new Vector4(	reader.ReadProperty<float>(CustomTypeFloat.Instance),
								reader.ReadProperty<float>(CustomTypeFloat.Instance),
								reader.ReadProperty<float>(CustomTypeFloat.Instance),
								reader.ReadProperty<float>(CustomTypeFloat.Instance));
		}

		public static bool Equals(Vector4 a, Vector4 b)
		{
			return (Mathf.Approximately(a.x,b.x) && Mathf.Approximately(a.y,b.y) && Mathf.Approximately(a.z,b.z) && Mathf.Approximately(a.w,b.w));
		}
	}

		public class CustomTypeVector4Array : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeVector4Array() : base(typeof(Vector4[]), CustomTypeVector4.Instance)
		{
			Instance = this;
		}
	}
}