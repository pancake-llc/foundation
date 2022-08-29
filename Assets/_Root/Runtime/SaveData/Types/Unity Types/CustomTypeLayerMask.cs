using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	[Properties("colorKeys", "alphaKeys", "mode")]
	public class CustomTypeLayerMask : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeLayerMask() : base(typeof(UnityEngine.LayerMask))
		{
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			var instance = (UnityEngine.LayerMask)obj;

			writer.WriteProperty("value", instance.value, CustomTypeINT.Instance);
		}

		public override object Read<T>(Reader reader)
		{
			LayerMask instance = new LayerMask();
			string propertyName;
			while((propertyName = reader.ReadPropertyName()) != null)
			{
				switch(propertyName)
				{
					case "value":
						instance = reader.Read<int>(CustomTypeINT.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
			return instance;
		}
	}
}
