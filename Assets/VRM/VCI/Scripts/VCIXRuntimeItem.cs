// © 2019 XELF

using MoonSharp.Interpreter;
using System.Linq;
using UnityEngine;

namespace VCI {
	[MoonSharpUserData]
	public class VCIXRuntimeItem : MonoBehaviour {
		private VCIXRuntimeScript[] scripts;

		public VCIXRuntimeItem() {
			assets = new VCIXAssets(this);
		}

		[MoonSharpHidden]
		public void Start() {
			var vci = GetComponent<VCIObject>();
			scripts = vci.Scripts.Select(s => new VCIXRuntimeScript(this, s)).ToArray();
		}
		[MoonSharpHidden]
		public void OnGrab(VCIXRuntimeSubItem target) {
			foreach (var script in scripts) {
				script.OnGrab(target);
			}
		}
		public VCIXAssets assets { get; private set; }
	}
	[MoonSharpUserData]
	public class VCIXAssets {
		public VCIXAssets(VCIXRuntimeItem runtime) => this.runtime = runtime;
		private VCIXRuntimeItem runtime;
		public VCIXRuntimeSubItem GetSubItem(string name) {
			var transform = runtime.transform.Find(name);
			if (transform == null)
				return null;
			return transform.GetComponent<VCIXRuntimeSubItem>();
		}
	}
	[MoonSharpUserData]
	public class VCIXRuntimeScript {
		private readonly VCIXRuntimeItem runtime;
		private readonly Script script;
		private readonly object onGrab;

		[MoonSharpHidden]
		public VCIXRuntimeScript(VCIXRuntimeItem runtime, VCIObject.Script vciScript) {
			this.runtime = runtime;
			script = new Script();
			script.Globals["vci"] = this;
			script.DoString(vciScript.source);
			onGrab = script.Globals["onGrab"];
		}

		[MoonSharpHidden]
		public void OnGrab(VCIXRuntimeSubItem target) {
			if (script != null && onGrab != null)
				script.Call(onGrab, target);
		}
		public VCIXAssets assets => runtime.assets;
	}
}
