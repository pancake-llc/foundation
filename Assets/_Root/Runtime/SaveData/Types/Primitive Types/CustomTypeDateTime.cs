using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	public class CustomTypeDateTime : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeDateTime() : base(typeof(DateTime))
		{
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			writer.WriteProperty("ticks", ((DateTime)obj).Ticks, CustomTypeLong.Instance);
		}

		public override object Read<T>(Reader reader)
		{
			reader.ReadPropertyName();
			return new DateTime(reader.Read<long>(CustomTypeLong.Instance));
		}
	}

	public class CustomTypeDateTimeArray : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeDateTimeArray() : base(typeof(DateTime[]), CustomTypeDateTime.Instance)
		{
			Instance = this;
		}
	}
}