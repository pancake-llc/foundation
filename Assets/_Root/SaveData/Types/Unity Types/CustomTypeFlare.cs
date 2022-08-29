using System;
using UnityEngine;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	[Properties("hideFlags")]
	public class CustomTypeFlare : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeFlare() : base(typeof(UnityEngine.Flare))
		{
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			var instance = (UnityEngine.Flare)obj;

			writer.WriteProperty("hideFlags", instance.hideFlags);
		}

		public override object Read<T>(Reader reader)
		{
			var instance = new UnityEngine.Flare();
			ReadInto<T>(reader, instance);
			return instance;
		}

		public override void ReadInto<T>(Reader reader, object obj)
		{
			var instance = (UnityEngine.Flare)obj;
			string propertyName;
			while((propertyName = reader.ReadPropertyName()) != null)
			{
				switch(propertyName)
				{
					
					case "hideFlags":
						instance.hideFlags = reader.Read<UnityEngine.HideFlags>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}

		public class CustomTypeFlareArray : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeFlareArray() : base(typeof(UnityEngine.Flare[]), CustomTypeFlare.Instance)
		{
			Instance = this;
		}
	}
}