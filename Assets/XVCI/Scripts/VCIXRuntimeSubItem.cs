// © 2019 XELF

using MoonSharp.Interpreter;
using UnityEngine;

namespace VCI {
	[MoonSharpUserData]
	public class VCIXRuntimeSubItem : MonoBehaviour {
		[ContextMenu("Grab")]
		[MoonSharpHidden]
		public void OnGrab() {
			var runtime = GetComponentInParent<VCIXRuntimeItem>();
			runtime.OnGrab(this);
		}
	}
}
