using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	[Properties("x", "y", "width", "height")]
	public class CustomTypeRect : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeRect() : base(typeof(UnityEngine.Rect))
		{
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			var instance = (UnityEngine.Rect)obj;
			
			writer.WriteProperty("x", instance.x, CustomTypeFloat.Instance);
			writer.WriteProperty("y", instance.y, CustomTypeFloat.Instance);
			writer.WriteProperty("width", instance.width, CustomTypeFloat.Instance);
			writer.WriteProperty("height", instance.height, CustomTypeFloat.Instance);
		}

		public override object Read<T>(Reader reader)
		{
			return new Rect(reader.ReadProperty<float>(CustomTypeFloat.Instance), 
							reader.ReadProperty<float>(CustomTypeFloat.Instance), 
							reader.ReadProperty<float>(CustomTypeFloat.Instance), 
							reader.ReadProperty<float>(CustomTypeFloat.Instance));
		}
	}
}
