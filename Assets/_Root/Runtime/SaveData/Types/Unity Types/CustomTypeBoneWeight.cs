using System;
using UnityEngine;
using System.Collections.Generic;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	[Properties("boneIndex0", "boneIndex1", "boneIndex2", "boneIndex3", "weight0", "weight1", "weight2", "weight3")]
	public class CustomTypeBoneWeight : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeBoneWeight() : base(typeof(BoneWeight))
		{
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			BoneWeight casted = (BoneWeight)obj;

			writer.WriteProperty("boneIndex0", casted.boneIndex0, CustomTypeINT.Instance);
			writer.WriteProperty("boneIndex1", casted.boneIndex1, CustomTypeINT.Instance);
			writer.WriteProperty("boneIndex2", casted.boneIndex2, CustomTypeINT.Instance);
			writer.WriteProperty("boneIndex3", casted.boneIndex3, CustomTypeINT.Instance);

			writer.WriteProperty("weight0", casted.weight0, CustomTypeFloat.Instance);
			writer.WriteProperty("weight1", casted.weight1, CustomTypeFloat.Instance);
			writer.WriteProperty("weight2", casted.weight2, CustomTypeFloat.Instance);
			writer.WriteProperty("weight3", casted.weight3, CustomTypeFloat.Instance);

		}

		public override object Read<T>(Reader reader)
		{
			var obj = new BoneWeight();

			obj.boneIndex0 = reader.ReadProperty<int>(CustomTypeINT.Instance);
			obj.boneIndex1 = reader.ReadProperty<int>(CustomTypeINT.Instance);
			obj.boneIndex2 = reader.ReadProperty<int>(CustomTypeINT.Instance);
			obj.boneIndex3 = reader.ReadProperty<int>(CustomTypeINT.Instance);

			obj.weight0 = reader.ReadProperty<float>(CustomTypeFloat.Instance);
			obj.weight1 = reader.ReadProperty<float>(CustomTypeFloat.Instance);
			obj.weight2 = reader.ReadProperty<float>(CustomTypeFloat.Instance);
			obj.weight3 = reader.ReadProperty<float>(CustomTypeFloat.Instance);

			return obj;
		}
	}

	public class CustomTypeBoneWeightArray : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeBoneWeightArray() : base(typeof(BoneWeight[]), CustomTypeBoneWeight.Instance)
		{
			Instance = this;
		}
	}
}