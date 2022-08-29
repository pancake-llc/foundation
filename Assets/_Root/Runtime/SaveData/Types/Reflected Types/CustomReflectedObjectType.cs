using System;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	internal class CustomReflectedObjectType : CustomObjectType
	{
		public CustomReflectedObjectType(Type type) : base(type)
		{
			isReflectedType = true;
			GetMembers(true);
		}

		protected override void WriteObject(object obj, Writer writer)
		{
			WriteProperties(obj, writer);

        }

		protected override object ReadObject<T>(Reader reader)
		{
			var obj = Reflection.CreateInstance(this.type);
			ReadProperties(reader, obj);
			return obj;
		}

		protected override void ReadObject<T>(Reader reader, object obj)
		{
			ReadProperties(reader, obj);
		}
	}
}