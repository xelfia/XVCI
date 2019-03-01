// © 2019 XELF

using MoonSharp.Interpreter;
using System.Linq;
using UnityEngine;

namespace VCI {
	[MoonSharpUserData]
	public class VCIXRuntimeItem : MonoBehaviour {
		private VCIObject vciObject;
		private VCIXRuntimeScript[] scripts;

		[MoonSharpHidden]
		public VCIXRuntimeItem() {
			assets = new VCIXAssets(this);
			studio = new VCIXStudio();
		}

		[MoonSharpHidden]
		public void Start() {
			vciObject = GetComponent<VCIObject>();
			scripts = vciObject.Scripts.Select(s => new VCIXRuntimeScript(this, s)).ToArray();
		}
		[MoonSharpHidden]
		public void Update() {
			foreach (var script in scripts) {
				script.Update();
			}
		}
		[MoonSharpHidden]
		public void OnGrab(VCIXRuntimeSubItem target) {
			foreach (var script in scripts) {
				script.OnGrab(target);
			}
		}
		[MoonSharpHidden]
		public void OnUngrab(VCIXRuntimeSubItem target) {
			foreach (var script in scripts) {
				script.OnUngrab(target);
			}
		}
		public VCIXAssets assets { get; private set; }
		public VCIXStudio studio { get; private set; }
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
		public void SetMaterialColorFromIndex(int index, object color) {
		}
		public void _ALL_SetMaterialColorFromIndex(int index, object color) {
		}
	}
	[MoonSharpUserData]
	public class VCIXStudio {
		public VCIXStudio() {
			shared = new VCIXShared();
		}
		public VCIXShared shared;
	}
	[MoonSharpUserData]
	public class VCIXShared {
		public void Bind(string name, object action) { }
		public void Set(string name, object value) { }
	}
	[MoonSharpUserData]
	public class VCIXRuntimeScript {
		private readonly VCIXRuntimeItem runtime;
		private readonly Script script;
		private readonly object update, onGrab, onUngrab, onColliderExit, onUse;

		public VCIXAssets assets => runtime.assets;
		public VCIXStudio studio => runtime.studio;

		[MoonSharpHidden]
		public VCIXRuntimeScript(VCIXRuntimeItem runtime, VCIObject.Script vciScript) {
			this.runtime = runtime;
			script = new Script();
			script.Globals["vci"] = this;
			script.DoString(vciScript.source);
			update = script.Globals["update"];
			onGrab = script.Globals["onGrab"];
			onUngrab = script.Globals["onUngrab"];
			onColliderExit = script.Globals["onColliderExit"];
		}

		[MoonSharpHidden]
		public void Update() {
			if (update != null) {
				script.Call(update);
			}
		}

		[MoonSharpHidden]
		public void OnGrab(VCIXRuntimeSubItem target) {
			if (onGrab != null)
				script.Call(onGrab, target.name);
		}
		[MoonSharpHidden]
		public void OnUngrab(VCIXRuntimeSubItem target) {
			if (onUngrab != null)
				script.Call(onUngrab, target.name);
		}
		[MoonSharpHidden]
		public void OnColliderExit(VCIXRuntimeSubItem target, Collider collider) {
			if (onColliderExit != null)
				script.Call(onColliderExit, target.name, collider.name);
		}
		[MoonSharpHidden]
		public void OnUse(VCIXRuntimeSubItem target) {
			if (onUse != null) {
				script.Call(onUse, target.name);
			}
		}
	}
}
