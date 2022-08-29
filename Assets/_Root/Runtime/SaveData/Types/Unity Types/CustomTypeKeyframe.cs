using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	[Properties("time", "value", "inTangent", "outTangent")]
	public class CustomTypeKeyframe : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeKeyframe() : base(typeof(UnityEngine.Keyframe))
		{
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			var instance = (UnityEngine.Keyframe)obj;
			
			writer.WriteProperty("time", instance.time, CustomTypeFloat.Instance);
			writer.WriteProperty("value", instance.value, CustomTypeFloat.Instance);
			writer.WriteProperty("inTangent", instance.inTangent, CustomTypeFloat.Instance);
			writer.WriteProperty("outTangent", instance.outTangent, CustomTypeFloat.Instance);
		}

		public override object Read<T>(Reader reader)
		{
			return new UnityEngine.Keyframe(reader.ReadProperty<System.Single>(CustomTypeFloat.Instance),
											reader.ReadProperty<System.Single>(CustomTypeFloat.Instance),
											reader.ReadProperty<System.Single>(CustomTypeFloat.Instance),
											reader.ReadProperty<System.Single>(CustomTypeFloat.Instance));
		}
	}

	public class CustomTypeKeyFrameArray : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeKeyFrameArray() : base(typeof(Keyframe[]), CustomTypeKeyframe.Instance)
		{
			Instance = this;
		}
	}
}
