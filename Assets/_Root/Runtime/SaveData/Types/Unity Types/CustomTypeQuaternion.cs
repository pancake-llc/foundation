using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	[Properties("x", "y", "z", "w")]
	public class CustomTypeQuaternion : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeQuaternion() : base(typeof(Quaternion))
		{
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			var casted = (Quaternion)obj;
			writer.WriteProperty("x", casted.x, CustomTypeFloat.Instance);
			writer.WriteProperty("y", casted.y, CustomTypeFloat.Instance);
			writer.WriteProperty("z", casted.z, CustomTypeFloat.Instance);
			writer.WriteProperty("w", casted.w, CustomTypeFloat.Instance);
		}

		public override object Read<T>(Reader reader)
		{
			return new Quaternion(	reader.ReadProperty<float>(CustomTypeFloat.Instance),
									reader.ReadProperty<float>(CustomTypeFloat.Instance),
									reader.ReadProperty<float>(CustomTypeFloat.Instance),
									reader.ReadProperty<float>(CustomTypeFloat.Instance));
		}
	}

		public class CustomTypeQuaternionArray : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeQuaternionArray() : base(typeof(Quaternion[]), CustomTypeQuaternion.Instance)
		{
			Instance = this;
		}
	}
}
