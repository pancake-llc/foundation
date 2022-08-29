using System;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	[Properties("value")]
	public class CustomTypeGuid : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeGuid() : base(typeof(Guid))
		{
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			Guid casted = (Guid)obj;
			writer.WriteProperty("value", casted.ToString(), CustomTypeString.Instance);
		}

		public override object Read<T>(Reader reader)
		{
			return Guid.Parse(reader.ReadProperty<string>(CustomTypeString.Instance));
		}
	}

	public class CustomTypeGuidArray : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeGuidArray() : base(typeof(Guid[]), CustomTypeGuid.Instance)
		{
			Instance = this;
		}
	}
}