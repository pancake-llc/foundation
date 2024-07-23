using System;

namespace Sisus.Init.Serialization
{
	/// <summary>
	/// Represents a simple wrapper for an <see cref="int"/> value that makes it serializable by Unity's serialization system
	/// when assigned to a <see cref="System.Object"/> type field that has the <see cref="UnityEngine.SerializeReference"/>
	/// attribute.
	/// <para>
	/// Used to serialize <see cref="int"/> values by <see cref="Any{object}"/>.
	/// </para>
	/// </summary>
	[Serializable] public sealed class _Integer : Serializable<int>, IValueProvider<int> { }

	/// <summary>
	/// Represents a simple wrapper for a <see cref="string"/> value that makes it serializable by Unity's serialization system
	/// when assigned to a <see cref="System.Object"/> type field that has the <see cref="UnityEngine.SerializeReference"/>
	/// attribute.
	/// <para>
	/// Used to serialize <see cref="string"/> values by <see cref="Any{object}"/>.
	/// </para>
	/// </summary>
	[Serializable] public sealed class _String : Serializable<string>, IValueProvider<string> { }

	/// <summary>
	/// Represents a simple wrapper for a <see cref="float"/> value that makes it serializable by Unity's serialization system
	/// when assigned to a <see cref="System.Object"/> type field that has the <see cref="UnityEngine.SerializeReference"/>
	/// attribute.
	/// <para>
	/// Used to serialize <see cref="float"/> values by <see cref="Any{object}"/>.
	/// </para>
	/// </summary>
	[Serializable] public sealed class _Float : Serializable<float>, IValueProvider<float> { }

	/// <summary>
	/// Represents a simple wrapper for a <see cref="bool"/> value that makes it serializable by Unity's serialization system
	/// when assigned to a <see cref="System.Object"/> type field that has the <see cref="UnityEngine.SerializeReference"/>
	/// attribute.
	/// <para>
	/// Used to serialize <see cref="bool"/> values by <see cref="Any{object}"/>.
	/// </para>
	/// </summary>
	[Serializable] public sealed class _Boolean : Serializable<bool>, IValueProvider<bool> { }

	/// <summary>
	/// Represents a simple wrapper for a <see cref="double"/> value that makes it serializable by Unity's serialization system
	/// when assigned to a <see cref="System.Object"/> type field that has the <see cref="UnityEngine.SerializeReference"/>
	/// attribute.
	/// <para>
	/// Used to serialize <see cref="double"/> values by <see cref="Any{object}"/>.
	/// </para>
	/// </summary>
	[Serializable] public sealed class _Double : Serializable<double>, IValueProvider<double> { }

	/// <summary>
	/// Represents a simple wrapper for a <see cref="double"/> value that makes it serializable by Unity's serialization system
	/// when assigned to a <see cref="System.Object"/> type field that has the <see cref="UnityEngine.SerializeReference"/>
	/// attribute.
	/// <para>
	/// Used to serialize <see cref="double"/> values by <see cref="Any{object}"/>.
	/// </para>
	/// </summary>
	[Serializable] public sealed class _Char : Serializable<char>, IValueProvider<char> { }

	[Serializable]
	public abstract class Serializable<T> : IValueProvider<object>, IComparable, IComparable<T>, IConvertible, IEquatable<T> where T : IComparable, IComparable<T>, IConvertible, IEquatable<T>
	{
		public T value;
		public T Value => value;
		object IValueProvider<object>.Value => value;
		object IValueProvider.Value => value;

		public int CompareTo(object obj) => value.CompareTo(obj);
		public int CompareTo(T other) => value.CompareTo(other);
		public bool Equals(T other) => value.Equals(other);
		public TypeCode GetTypeCode() => value.GetTypeCode();
		public bool ToBoolean(IFormatProvider provider) => ((IConvertible)value).ToBoolean(provider);
		public byte ToByte(IFormatProvider provider) => ((IConvertible)value).ToByte(provider);
		public char ToChar(IFormatProvider provider) => ((IConvertible)value).ToChar(provider);
		public DateTime ToDateTime(IFormatProvider provider) => ((IConvertible)value).ToDateTime(provider);
		public decimal ToDecimal(IFormatProvider provider) => ((IConvertible)value).ToDecimal(provider);
		public double ToDouble(IFormatProvider provider) => ((IConvertible)value).ToDouble(provider);
		public short ToInt16(IFormatProvider provider) => ((IConvertible)value).ToInt16(provider);
		public int ToInt32(IFormatProvider provider) => ((IConvertible)value).ToInt32(provider);
		public long ToInt64(IFormatProvider provider) => ((IConvertible)value).ToInt64(provider);
		public sbyte ToSByte(IFormatProvider provider) => ((IConvertible)value).ToSByte(provider);
		public float ToSingle(IFormatProvider provider) => ((IConvertible)value).ToSingle(provider);
		public string ToString(IFormatProvider provider) => value.ToString(provider);
		public object ToType(Type conversionType, IFormatProvider provider) => ((IConvertible)value).ToType(conversionType, provider);
		public ushort ToUInt16(IFormatProvider provider) => ((IConvertible)value).ToUInt16(provider);
		public uint ToUInt32(IFormatProvider provider) => ((IConvertible)value).ToUInt32(provider);
		public ulong ToUInt64(IFormatProvider provider) => ((IConvertible)value).ToUInt64(provider);
	}
}
