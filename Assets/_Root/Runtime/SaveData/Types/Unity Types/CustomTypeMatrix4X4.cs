using System;
using UnityEngine;
using System.Collections.Generic;

namespace Pancake.SaveData
{
	[UnityEngine.Scripting.Preserve]
	[Properties("col0", "col1", "col2", "col3")]
	public class CustomTypeMatrix4X4 : CustomType
	{
		public static CustomType Instance = null;

		public CustomTypeMatrix4X4() : base(typeof(Matrix4x4))
		{
			Instance = this;
		}

		public override void Write(object obj, Writer writer)
		{
			Matrix4x4 casted = (Matrix4x4)obj;
			writer.WriteProperty("col0", casted.GetColumn(0), CustomTypeVector4.Instance);
			writer.WriteProperty("col1", casted.GetColumn(1), CustomTypeVector4.Instance);
			writer.WriteProperty("col2", casted.GetColumn(2), CustomTypeVector4.Instance);
			writer.WriteProperty("col3", casted.GetColumn(3), CustomTypeVector4.Instance);
		}

		public override object Read<T>(Reader reader)
		{
			var matrix = new Matrix4x4();
			matrix.SetColumn(0,	reader.ReadProperty<Vector4>(CustomTypeVector4.Instance));
			matrix.SetColumn(1,	reader.ReadProperty<Vector4>(CustomTypeVector4.Instance));
			matrix.SetColumn(2,	reader.ReadProperty<Vector4>(CustomTypeVector4.Instance));
			matrix.SetColumn(3,	reader.ReadProperty<Vector4>(CustomTypeVector4.Instance));
			return matrix;
		}
	}

		public class CustomTypeMatrix4X4Array : CustomArrayType
	{
		public static CustomType Instance;

		public CustomTypeMatrix4X4Array() : base(typeof(Matrix4x4[]), CustomTypeMatrix4X4.Instance)
		{
			Instance = this;
		}
	}
}