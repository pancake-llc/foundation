using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	[Properties("color", "time")]
	public class CustomTypeGradientColorKey : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeGradientColorKey() : base(typeof(UnityEngine.GradientColorKey))
		{
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			var instance = (UnityEngine.GradientColorKey)obj;
			
			writer.WriteProperty("color", instance.color, CustomTypeColor.Instance);
			writer.WriteProperty("time", instance.time, CustomTypeFloat.Instance);
		}

		public override object Read<T>(Reader reader)
		{
			return new UnityEngine.GradientColorKey(reader.ReadProperty<Color>(CustomTypeColor.Instance),
													reader.ReadProperty<float>(CustomTypeFloat.Instance));
		}
	}

		public class CustomTypeGradientColorKeyArray : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeGradientColorKeyArray() : base(typeof(GradientColorKey[]), CustomTypeGradientColorKey.Instance)
		{
			Instance = this;
		}
	}
}