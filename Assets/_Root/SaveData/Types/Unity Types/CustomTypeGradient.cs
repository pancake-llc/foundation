using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	[Properties("colorKeys", "alphaKeys", "mode")]
	public class CustomTypeGradient : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeGradient() : base(typeof(UnityEngine.Gradient))
		{
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			var instance = (UnityEngine.Gradient)obj;
			writer.WriteProperty("colorKeys", instance.colorKeys, CustomTypeGradientColorKeyArray.Instance);
			writer.WriteProperty("alphaKeys", instance.alphaKeys, CustomTypeGradientAlphaKeyArray.Instance);
			writer.WriteProperty("mode", instance.mode);
		}

		public override object Read<T>(Reader reader)
		{
			var instance = new UnityEngine.Gradient();
			ReadInto<T>(reader, instance);
			return instance;
		}

		public override void ReadInto<T>(Reader reader, object obj)
		{
			var instance = (UnityEngine.Gradient)obj;
			instance.SetKeys(
				reader.ReadProperty<UnityEngine.GradientColorKey[]>(CustomTypeGradientColorKeyArray.Instance),
				reader.ReadProperty<UnityEngine.GradientAlphaKey[]>(CustomTypeGradientAlphaKeyArray.Instance)
			);

			string propertyName;
			while((propertyName = reader.ReadPropertyName()) != null)
			{
				switch(propertyName)
				{
					case "mode":
						instance.mode = reader.Read<UnityEngine.GradientMode>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}
}
