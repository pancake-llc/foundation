using System;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	[Properties("bounciness", "friction")]
	public class CustomTypePhysicsMaterial2D : CustomObjectType
	{
		public static CustomType Instance = null;

		public CustomTypePhysicsMaterial2D() : base(typeof(UnityEngine.PhysicsMaterial2D)){ Instance = this; }

		protected override void WriteObject(object obj, Writer writer)
		{
			var instance = (UnityEngine.PhysicsMaterial2D)obj;
			
			writer.WriteProperty("bounciness", instance.bounciness, CustomTypeFloat.Instance);
			writer.WriteProperty("friction", instance.friction, CustomTypeFloat.Instance);
		}

		protected override void ReadObject<T>(Reader reader, object obj)
		{
			var instance = (UnityEngine.PhysicsMaterial2D)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "bounciness":
						instance.bounciness = reader.Read<System.Single>(CustomTypeFloat.Instance);
						break;
					case "friction":
						instance.friction = reader.Read<System.Single>(CustomTypeFloat.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(Reader reader)
		{
			var instance = new UnityEngine.PhysicsMaterial2D();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}

		public class CustomTypePhysicsMaterial2DArray : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypePhysicsMaterial2DArray() : base(typeof(UnityEngine.PhysicsMaterial2D[]), CustomTypePhysicsMaterial2D.Instance)
		{
			Instance = this;
		}
	}
}