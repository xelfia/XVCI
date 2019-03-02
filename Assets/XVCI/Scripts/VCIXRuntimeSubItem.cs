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
		[ContextMenu("Ungrab")]
		[MoonSharpHidden]
		public void OnUngrab() {
			var runtime = GetComponentInParent<VCIXRuntimeItem>();
			runtime.OnUngrab(this);
		}
		public Quaternion GetLocalRotation() =>
			transform.localRotation;
		public void SetLocalRotation(Quaternion value)
			=> transform.localRotation = value;
	}
}
