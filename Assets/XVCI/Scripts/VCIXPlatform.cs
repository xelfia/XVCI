// © 2019 XELF

using MoonSharp.Interpreter;
using UnityEngine;

namespace VCI {
	public class VCIXPlatform : MonoBehaviour {
		public void Start() {
			UserData.RegisterAssembly();
			Script.DefaultOptions.DebugPrint += a => {
				Debug.Log(a);
			};
		}
		public static void ToRuntime(GameObject root, VCIObject o) {
			if (o.Scripts != null) {
				root.AddComponent<VCIXRuntimeItem>();
			}
		}
	}
}
