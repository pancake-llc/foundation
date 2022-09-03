using System;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	internal class CustomReflectedValueType : CustomType
	{
		public CustomReflectedValueType(Type type) : base(type)
		{
			isReflectedType = true;
			GetMembers(true);
		}

		public override void Write(object obj, Writer writer)
		{
			WriteProperties(obj, writer);
		}

		public override object Read<T>(Reader reader)
		{
			var obj = Reflection.CreateInstance(this.type);

			if(obj == null)
				throw new NotSupportedException("Cannot create an Instance of "+this.type+". However, you may be able to add support for it using a custom CustomType file.");
			// Make sure we return the result of ReadProperties as properties aren't assigned by reference.
			return ReadProperties(reader, obj);
		}

		public override void ReadInto<T>(Reader reader, object obj)
		{
			throw new NotSupportedException("Cannot perform self-assigning load on a value type.");
		}
	}
}