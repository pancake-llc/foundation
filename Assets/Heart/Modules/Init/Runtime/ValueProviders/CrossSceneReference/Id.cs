using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Sisus.Init.Internal
{
	/// <summary>
	/// A persistent and globally unique identifier which can be serialized by Unity.
	/// </summary>
	[StructLayout(LayoutKind.Sequential), Serializable]
	public struct Id : IFormattable, IEquatable<Id>
	{
		public static readonly Id Empty = new();

		[SerializeField] private int a;
		[SerializeField] private short b;
		[SerializeField] private short c;
		[SerializeField] private byte d;
		[SerializeField] private byte e;
		[SerializeField] private byte f;
		[SerializeField] private byte g;
		[SerializeField] private byte h;
		[SerializeField] private byte i;
		[SerializeField] private byte j;
		[SerializeField] private byte k;

		private Id(byte[] b)
		{
			a = (b[3] << 24) | (b[2] << 16) | (b[1] << 8) | b[0];
			this.b = (short)((b[5] << 8) | b[4]);
			c = (short)((b[7] << 8) | b[6]);
			d = b[8];
			e = b[9];
			f = b[10];
			g = b[11];
			h = b[12];
			i = b[13];
			j = b[14];
			k = b[15];
		}

		public Id(int a, short b, short c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
		{
			this.a = a;
			this.b = b;
			this.c = c;
			this.d = d;
			this.e = e;
			this.f = f;
			this.g = g;
			this.h = h;
			this.i = i;
			this.j = j;
			this.k = k;
		}

		// Returns an unsigned byte array containing the GUID.
		public byte[] ToByteArray()
		{
			byte[] g = new byte[16];

			g[0] = (byte)(a);
			g[1] = (byte)(a >> 8);
			g[2] = (byte)(a >> 16);
			g[3] = (byte)(a >> 24);
			g[4] = (byte)(b);
			g[5] = (byte)(b >> 8);
			g[6] = (byte)(c);
			g[7] = (byte)(c >> 8);
			g[8] = d;
			g[9] = e;
			g[10] = f;
			g[11] = this.g;
			g[12] = h;
			g[13] = i;
			g[14] = j;
			g[15] = k;

			return g;
		}
		
		public override int GetHashCode() => a ^ ((b << 16) | (ushort)c) ^ ((f << 24) | k);
		public override bool Equals(object obj) => obj is Id other && a == other.a && b == other.b && c == other.c && d == other.d && e == other.e && f == other.f && g == other.g && h == other.h && i == other.i && j == other.j && k == other.k;
		public bool Equals(Id other) => a == other.a && b == other.b && c == other.c && d == other.d && e == other.e && f == other.f && g == other.g && h == other.h && i == other.i && j == other.j && k == other.k;
		public static bool operator ==(Id a, Id b) => a.a == b.a && a.b == b.b && a.c == b.c && a.d == b.d && a.e == b.e && a.f == b.f && a.g == b.g && a.h == b.h && a.i == b.i && a.j == b.j && a.k == b.k;
		public static bool operator !=(Id a, Id b) => !(a == b);
		public static Id NewId() => new Id(Guid.NewGuid().ToByteArray());
		public override string ToString() => ToString("D", null);
		public string ToString(string format) => ToString(format, null);
		public string ToString(string format, IFormatProvider provider) => new Guid(a, b, c, d, e, f, g, h, i, j, k).ToString(format, provider);
	}
}
