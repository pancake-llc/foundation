using System;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	[Properties("dynamicFriction", "staticFriction", "bounciness", "frictionCombine", "bounceCombine")]
	public class CustomTypePhysicMaterial : CustomObjectType
	{
		public static CustomType Instance = null;

		public CustomTypePhysicMaterial() : base(typeof(UnityEngine.PhysicMaterial)){ Instance = this; }

		protected override void WriteObject(object obj, Writer writer)
		{
			var instance = (UnityEngine.PhysicMaterial)obj;
			
			writer.WriteProperty("dynamicFriction", instance.dynamicFriction, CustomTypeFloat.Instance);
			writer.WriteProperty("staticFriction", instance.staticFriction, CustomTypeFloat.Instance);
			writer.WriteProperty("bounciness", instance.bounciness, CustomTypeFloat.Instance);
			writer.WriteProperty("frictionCombine", instance.frictionCombine);
			writer.WriteProperty("bounceCombine", instance.bounceCombine);
		}

		protected override void ReadObject<T>(Reader reader, object obj)
		{
			var instance = (UnityEngine.PhysicMaterial)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "dynamicFriction":
						instance.dynamicFriction = reader.Read<System.Single>(CustomTypeFloat.Instance);
						break;
					case "staticFriction":
						instance.staticFriction = reader.Read<System.Single>(CustomTypeFloat.Instance);
						break;
					case "bounciness":
						instance.bounciness = reader.Read<System.Single>(CustomTypeFloat.Instance);
						break;
					case "frictionCombine":
						instance.frictionCombine = reader.Read<UnityEngine.PhysicMaterialCombine>();
						break;
					case "bounceCombine":
						instance.bounceCombine = reader.Read<UnityEngine.PhysicMaterialCombine>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(Reader reader)
		{
			var instance = new UnityEngine.PhysicMaterial();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}

		public class CustomTypePhysicMaterialArray : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypePhysicMaterialArray() : base(typeof(UnityEngine.PhysicMaterial[]), CustomTypePhysicMaterial.Instance)
		{
			Instance = this;
		}
	}
}