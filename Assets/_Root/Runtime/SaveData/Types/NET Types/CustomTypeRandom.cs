using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	[Properties("inext", "inextp", "SeedArray")]
	public class CustomTypeRandom : CustomObjectType
	{
		public static CustomType Instance = null;

		public CustomTypeRandom() : base(typeof(System.Random)){ Instance = this; }

		protected override void WriteObject(object obj, Writer writer)
		{
			var instance = (System.Random)obj;
			
			writer.WritePrivateField("inext", instance);
			writer.WritePrivateField("inextp", instance);
			writer.WritePrivateField("SeedArray", instance);
		}

		protected override void ReadObject<T>(Reader reader, object obj)
		{
			var instance = (System.Random)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "inext":
					reader.SetPrivateField("inext", reader.Read<System.Int32>(), instance);
					break;
					case "inextp":
					reader.SetPrivateField("inextp", reader.Read<System.Int32>(), instance);
					break;
					case "SeedArray":
					reader.SetPrivateField("SeedArray", reader.Read<System.Int32[]>(), instance);
					break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(Reader reader)
		{
			var instance = new System.Random();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}

	public class CustomTypeRandomArray : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeRandomArray() : base(typeof(System.Random[]), CustomTypeRandom.Instance)
		{
			Instance = this;
		}
	}
}