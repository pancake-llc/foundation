namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	public class CustomTypeFloat : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeFloat() : base(typeof(float))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			writer.WritePrimitive((float)obj);
		}

		public override object Read<T>(Reader reader)
		{
			return (T)(object)reader.Read_float();
		}
	}

	public class CustomTypeFloatArray : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeFloatArray() : base(typeof(float[]), CustomTypeFloat.Instance)
		{
			Instance = this;
		}
	}
}