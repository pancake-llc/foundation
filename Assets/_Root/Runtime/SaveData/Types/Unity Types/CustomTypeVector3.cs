using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	[Properties("x", "y", "z")]
	public class CustomTypeVector3 : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeVector3() : base(typeof(Vector3))
		{
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			Vector3 casted = (Vector3)obj;
			writer.WriteProperty("x", casted.x, CustomTypeFloat.Instance);
			writer.WriteProperty("y", casted.y, CustomTypeFloat.Instance);
			writer.WriteProperty("z", casted.z, CustomTypeFloat.Instance);
		}

		public override object Read<T>(Reader reader)
		{
			return new Vector3(	reader.ReadProperty<float>(CustomTypeFloat.Instance),
								reader.ReadProperty<float>(CustomTypeFloat.Instance),
								reader.ReadProperty<float>(CustomTypeFloat.Instance));
		}
	}

		public class CustomTypeVector3Array : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeVector3Array() : base(typeof(Vector3[]), CustomTypeVector3.Instance)
		{
			Instance = this;
		}
	}
}