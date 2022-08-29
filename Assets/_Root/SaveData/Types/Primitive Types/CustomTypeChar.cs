namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	public class CustomTypeChar : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeChar() : base(typeof(char))
		{
			isPrimitive = true;
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			writer.WritePrimitive((char)obj);
		}

		public override object Read<T>(Reader reader)
		{
			return (T)(object)reader.Read_char();
		}
	}
		public class CustomTypeCharArray : CustomArrayType
		{
			public static CustomType Instance;

			public CustomTypeCharArray() : base(typeof(char[]), CustomTypeChar.Instance)
			{
				Instance = this;
			}
	}
}