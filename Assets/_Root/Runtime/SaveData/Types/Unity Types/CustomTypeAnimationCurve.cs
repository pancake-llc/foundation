using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	[Properties("keys", "preWrapMode", "postWrapMode")]
	public class CustomTypeAnimationCurve : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeAnimationCurve() : base(typeof(UnityEngine.AnimationCurve))
		{
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			var instance = (UnityEngine.AnimationCurve)obj;
			
			writer.WriteProperty("keys", instance.keys, CustomTypeKeyFrameArray.Instance);
			writer.WriteProperty("preWrapMode", instance.preWrapMode);
			writer.WriteProperty("postWrapMode", instance.postWrapMode);
		}

		public override object Read<T>(Reader reader)
		{
			var instance = new UnityEngine.AnimationCurve();
			ReadInto<T>(reader, instance);
			return instance;
		}

		public override void ReadInto<T>(Reader reader, object obj)
		{
			var instance = (UnityEngine.AnimationCurve)obj;
			string propertyName;
			while((propertyName = reader.ReadPropertyName()) != null)
			{
				switch(propertyName)
				{
					
					case "keys":
						instance.keys = reader.Read<UnityEngine.Keyframe[]>();
						break;
					case "preWrapMode":
						instance.preWrapMode = reader.Read<UnityEngine.WrapMode>();
						break;
					case "postWrapMode":
						instance.postWrapMode = reader.Read<UnityEngine.WrapMode>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}
}
