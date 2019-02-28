using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
using System.Linq;
using System.Text;
using System.IO;
using UniJSON;

namespace VCI
{
    public class VCIExporter : gltfExporter
    {
        protected override IMaterialExporter CreateMaterialExporter()
        {
            return new VRM.VRMMaterialExporter();
        }

        public VCIExporter(glTF gltf) : base(gltf)
        {
            gltf.extensionsUsed.Add(glTF_VCAST_vci_meta.ExtensionName);
        }

        public new static glTF Export(GameObject go)
        {
            var gltf = new glTF();
            using (var exporter = new VCIExporter(gltf)
            {
#if VRM_EXPORTER_USE_SPARSE
                // experimental
                UseSparseAccessorForBlendShape=true
#endif
            })
            {
                _Export(gltf, exporter, go);
            }
            return gltf;
        }

        /// <summary>
        /// workaround. fix next UniVRM
        /// </summary>
        /// <param name="gltf"></param>
        static void RemoveVRM(glTF gltf)
        {
            var ex = gltf.extensions;
            var fi = ex.GetType().GetField("VRM");
            if (fi != null)
            {
                fi.SetValue(ex, null);
            }
        }

        public static void _Export(glTF gltf, VCIExporter exporter, GameObject go)
        {
            exporter.Prepare(go);
            exporter.Export();

            // clear VRM
            //gltf.extensions.VRM = null;
            RemoveVRM(gltf);

            var vciObject = exporter.Copy.GetComponent<VCIObject>();

            // vci interaction
            if (vciObject != null)
            {
                // script
                if (vciObject.Scripts.Any())
                {
                    gltf.extensions.VCAST_vci_embedded_script = new glTF_VCAST_vci_embedded_script
                    {
                        scripts = vciObject.Scripts.Select(x =>
                        {
                            var viewIndex = gltf.ExtendBufferAndGetViewIndex<byte>(0, Utf8String.Encoding.GetBytes(x.source));
                            return new glTF_VCAST_vci_embedded_script_source
                            {
                                name = x.name,
                                mimeType = x.mimeType,
                                targetEngine = x.targetEngine,
                                source = viewIndex,
                            };
                        }).ToList()
                    };
                }

                // meta
                var meta = vciObject.Meta;
                gltf.extensions.VCAST_vci_meta = new glTF_VCAST_vci_meta
                {
                    exporterVCIVersion = VCIVersion.VCI_VERSION,
                    specVersion = VCISpecVersion.Version,

                    title = meta.title,

                    version = meta.version,
                    author = meta.author,
                    contactInformation = meta.contactInformation,
                    reference = meta.reference,
                    description = meta.description,

                    modelDataLicenseType = meta.modelDataLicenseType,
                    modelDataOtherLicenseUrl = meta.modelDataOtherLicenseUrl,
                    scriptLicenseType = meta.scriptLicenseType,
                    scriptOtherLicenseUrl = meta.scriptOtherLicenseUrl,

                    scriptWriteProtected = meta.scriptWriteProtected,
                    scriptEnableDebugging = meta.scriptEnableDebugging,
                    scriptFormat = meta.scriptFormat
                };
                if (meta.thumbnail != null)
                {
                    gltf.extensions.VCAST_vci_meta.thumbnail = TextureIO.ExportTexture(
                        gltf, gltf.buffers.Count - 1, meta.thumbnail, glTFTextureTypes.Unknown);
                }
            }

            // materials

            gltf.extensions.VCAST_vci_material_unity = new glTF_VCAST_vci_material_unity
            {
                materials = exporter.Materials
                .Select(m => VRM.glTF_VRM_Material.CreateFromMaterial(m, exporter.TextureManager.Textures))
                .ToList()
            };

            // collider & rigidbody & joint & item
            for (int i = 0; i < exporter.Nodes.Count; i++)
            {
                var node = exporter.Nodes[i];
                var gltfNode = gltf.nodes[i];

                // 各ノードに複数のコライダーがあり得る
                var colliders = node.GetComponents<Collider>();
                if (colliders.Any())
                {
                    if (gltfNode.extensions == null)
                    {
                        gltfNode.extensions = new glTFNode_extensions();
                    }

                    gltfNode.extensions.VCAST_vci_collider = new glTF_VCAST_vci_colliders();
                    gltfNode.extensions.VCAST_vci_collider.colliders = new List<glTF_VCAST_vci_Collider>();

                    foreach (var collider in colliders)
                    {
                        var gltfCollider = glTF_VCAST_vci_Collider.GetglTfColliderFromUnityCollider(collider);
                        if (gltfCollider == null)
                        {
                            Debug.LogWarningFormat("collider is not supported: {0}", collider.GetType().Name);
                            continue;
                        }
                        gltfNode.extensions.VCAST_vci_collider.colliders.Add(gltfCollider);
                    }
                }

                var rigidbodies = node.GetComponents<Rigidbody>();
                if (rigidbodies.Any())
                {
                    if (gltfNode.extensions == null)
                    {
                        gltfNode.extensions = new glTFNode_extensions();
                    }

                    gltfNode.extensions.VCAST_vci_rigidbody = new glTF_VCAST_vci_rigidbody();
                    gltfNode.extensions.VCAST_vci_rigidbody.rigidbodies = new List<glTF_VCAST_vci_Rigidbody>();

                    foreach (var rigidbody in rigidbodies)
                    {
                        gltfNode.extensions.VCAST_vci_rigidbody.rigidbodies.Add(glTF_VCAST_vci_Rigidbody.GetglTfRigidbodyFromUnityRigidbody(rigidbody));
                    }
                }

                var joints = node.GetComponents<Joint>();
                if (joints.Any())
                {
                    if (gltfNode.extensions == null)
                    {
                        gltfNode.extensions = new glTFNode_extensions();
                    }

                    gltfNode.extensions.VCAST_vci_joints = new glTF_VCAST_vci_joints();
                    gltfNode.extensions.VCAST_vci_joints.joints = new List<glTF_VCAST_vci_joint>();

                    foreach (var joint in joints)
                    {
                        gltfNode.extensions.VCAST_vci_joints.joints.Add(glTF_VCAST_vci_joint.GetglTFJointFromUnityJoint(joint, exporter.Nodes));
                    }
                }

                var item = node.GetComponent<VCISubItem>();
                if (item != null)
                {
                    var warning = item.ExportWarning;
                    if (!string.IsNullOrEmpty(warning))
                    {
                        throw new System.Exception(warning);
                    }

                    if (gltfNode.extensions == null)
                    {
                        gltfNode.extensions = new glTFNode_extensions();
                    }

                    gltfNode.extensions.VCAST_vci_item = new glTF_VCAST_vci_item
                    {
                        grabbable = item.Grabbable,
                        scalable = item.Scalable,
                        uniformScaling = item.UniformScaling,
                        groupId = item.GroupId,
                    };
                }
            }

            var clips = exporter.Copy.GetComponentsInChildren<AudioSource>()
                .Select(x => x.clip)
                .Where(x => x != null)
                .ToArray();
            if (clips.Any())
            {
                var audios = clips.Select(x => FromAudioClip(gltf, x)).Where(x => x != null).ToList();
                gltf.extensions.VCAST_vci_audios = new glTF_VCAST_vci_audios
                {
                    audios = audios
                };
            }
        }


        static glTF_VCAST_vci_audio FromAudioClip(glTF gltf, AudioClip clip)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
#endif
            {
                var bytes = WaveUtil.GetWaveBinary(clip);
                var viewIndex = gltf.ExtendBufferAndGetViewIndex(0, bytes);
                return new glTF_VCAST_vci_audio
                {
                    name = clip.name,
                    mimeType = "audio/wav",
                    bufferView = viewIndex,
                };
            }
#if UNITY_EDITOR
            else
            {
                var path = UnityPath.FromAsset(clip);
                if (!path.IsUnderAssetsFolder)
                {
                    return null;
                }
                if (path.Extension.ToLower() != ".wav")
                {
                    var bytes = WaveUtil.GetWaveBinary(clip);
                    var viewIndex = gltf.ExtendBufferAndGetViewIndex(0, bytes);
                    return new glTF_VCAST_vci_audio
                    {
                        name = clip.name,
                        mimeType = "audio/wav",
                        bufferView = viewIndex,
                    };
                }
                else
                {
                    var bytes = File.ReadAllBytes(path.FullPath);
                    var viewIndex = gltf.ExtendBufferAndGetViewIndex(0, bytes);
                    return new glTF_VCAST_vci_audio
                    {
                        name = clip.name,
                        mimeType = "audio/wav",
                        bufferView = viewIndex,
                    };
                }
            }
#endif
        }
    }
}
