using System;
using UnityEngine;
using System.Collections.Generic;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	[Properties("center", "size")]
	public class CustomTypeBounds : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeBounds() : base(typeof(Bounds))
		{
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			Bounds casted = (Bounds)obj;

			writer.WriteProperty("center", casted.center, CustomTypeVector3.Instance);
			writer.WriteProperty("size", casted.size, CustomTypeVector3.Instance);
		}

		public override object Read<T>(Reader reader)
		{
			return new Bounds(	reader.ReadProperty<Vector3>(CustomTypeVector3.Instance), 
								reader.ReadProperty<Vector3>(CustomTypeVector3.Instance));
		}
	}

		public class CustomTypeBoundsArray : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeBoundsArray() : base(typeof(Bounds[]), CustomTypeBounds.Instance)
		{
			Instance = this;
		}
	}
}