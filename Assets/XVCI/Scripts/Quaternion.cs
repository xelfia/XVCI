// © 2019 XELF

using MoonSharp.Interpreter;
using T = UnityEngine.Quaternion;

namespace VCI {
	[MoonSharpUserData]
	public struct Quaternion {
		public float x, y, z, w;
		public static Quaternion Euler(float x, float y, float z)
			=> T.Euler(x, y, z);
		public Quaternion @new(float x, float y, float z, float w)
			=> new Quaternion(x, y, z, w);
		public Quaternion(float x, float y, float z, float w)
			=> (this.x, this.y, this.z, this.w) = (x, y, z, w);
		public static Quaternion operator *(Quaternion a, Quaternion b)
			=> (T)a * (T)b;
		public static implicit operator T(Quaternion o)
			=> new T(o.x, o.y, o.z, o.w);
		public static implicit operator Quaternion(T o)
			=> new Quaternion(o.x, o.y, o.z, o.w);
		public override string ToString()
			=> $"({x}, {y}, {z}, {w})";
	}
}
