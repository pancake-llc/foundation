using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	[Properties("alpha", "time")]
	public class CustomTypeGradientAlphaKey : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeGradientAlphaKey() : base(typeof(UnityEngine.GradientAlphaKey))
		{
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			var instance = (UnityEngine.GradientAlphaKey)obj;
			
			writer.WriteProperty("alpha", instance.alpha, CustomTypeFloat.Instance);
			writer.WriteProperty("time", instance.time, CustomTypeFloat.Instance);
		}

		public override object Read<T>(Reader reader)
		{
			return new UnityEngine.GradientAlphaKey(reader.ReadProperty<float>(CustomTypeFloat.Instance),
													reader.ReadProperty<float>(CustomTypeFloat.Instance));
		}
	}

		public class CustomTypeGradientAlphaKeyArray : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeGradientAlphaKeyArray() : base(typeof(GradientAlphaKey[]), CustomTypeGradientAlphaKey.Instance)
		{
			Instance = this;
		}
	}
}