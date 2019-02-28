#pragma warning disable
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif
using UnityEngine;


namespace VCI
{
    public static class VCIExportUnityPackage
    {
        const string DATE_FORMAT = "yyyyMMdd";
        const string PREFIX = "UniVCI";

        static string System(string dir, string fileName, string args)
        {
            // Start the child process.
            var p = new System.Diagnostics.Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.FileName = fileName;
            p.StartInfo.Arguments = args;
            p.StartInfo.WorkingDirectory = dir;
            if (!p.Start())
            {
                return "ERROR";
            }
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // p.WaitForExit();
            // Read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();
            string err = p.StandardError.ReadToEnd();
            p.WaitForExit();

            if (string.IsNullOrEmpty(output))
            {
                return err;
            }
            return output;
        }

        //const string GIT_PATH = "C:\\Program Files\\Git\\mingw64\\bin\\git.exe";
        const string GIT_PATH = "C:\\Program Files\\Git\\bin\\git.exe";

        static string GetGitHash(string path)
        {
            return System(path, "git.exe", "rev-parse HEAD").Trim();
        }

        static string GetPath(string folder)
        {
            //var date = DateTime.Today.ToString(DATE_FORMAT);

            var path = string.Format("{0}/{1}.unitypackage",
                folder,
                VCIVersion.VCI_VERSION
                ).Replace("\\", "/");

            return path;
        }

        static IEnumerable<string> EnumerateFiles(string path, Func<string, bool> isExclude = null)
        {
            path = path.Replace("\\", "/");

            if (Path.GetFileName(path).StartsWith(".git"))
            {
                yield break;
            }

            if (isExclude != null && isExclude(path))
            {
                yield break;
            }

            if (Directory.Exists(path))
            {
                foreach (var child in Directory.GetFileSystemEntries(path))
                {
                    foreach (var x in EnumerateFiles(child, isExclude))
                    {
                        yield return x;
                    }
                }
            }
            else
            {
                if (Path.GetExtension(path).ToLower() == ".meta")
                {
                    yield break;
                }

                yield return path;
            }
        }

        public static bool Build(string[] levels)
        {
            var buildPath = Path.GetFullPath(Application.dataPath + "/../build/build.exe");
            Debug.LogFormat("{0}", buildPath);
            var build = BuildPipeline.BuildPlayer(levels,
                buildPath,
                BuildTarget.StandaloneWindows,
                BuildOptions.None
                );
#if UNITY_2018_1_OR_NEWER
            var iSuccess = build.summary.result != BuildResult.Succeeded;
#else
            var iSuccess = string.IsNullOrEmpty(build);
#endif
            return iSuccess;
        }

        public static bool BuildTestScene()
        {
            var levels = new string[] { "Assets/VRM/VCI/Examples/vci_setup.unity" };
            return Build(levels);
        }

        static bool EndsWith(string path, params string[] exts)
        {
            foreach (var ext in exts)
            {
                if (path.EndsWith(ext))
                {
                    return true;
                }
                if (path.EndsWith(ext + ".meta"))
                {
                    return true;
                }
            }

            return false;
        }

        static bool ExcludeCsProj(string path)
        {
            /*
            if(EndsWith(path, "csproj", "sln", "csproj.user", "psess", "bin", "obj", "vsp", "vspx"))
            {
                return true;
            }
            */
            if (path.EndsWith("/UniJSON/Profiling.meta"))
            {
                return true;
            }
            if (path.EndsWith("/UniJSON/Profiling"))
            {
                return true;
            }
            if (path.EndsWith("/UniGLTF/doc"))
            {
                return true;
            }
            if (path.EndsWith("/UniHumanoid/doc"))
            {
                return true;
            }
            if (path.EndsWith("/VCI-Embedded-Script"))
            {
                return true;
            }
            if (path.EndsWith("/VCI-Embedded-Sample"))
            {
                return true;
            }
            if (path.EndsWith("/ReactiveConsole"))
            {
                return true;
            }
            if (path.EndsWith("/VCIConsole"))
            {
                return true;
            }

            if (path.EndsWith("/VRM/UniHumanoid"))
            {
                return true;
            }

            if (path.StartsWith("Assets/VRM/UniVRM/"))
            {
                if (IncluceFiles.Any(x => x.StartsWith(path)))
                {
                    int a = 0;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        static string[] IncluceFiles = new string[]
        {
            "Assets/VRM/UniVRM/Scripts/Format/glTF_VRM_Material.cs.meta",
            "Assets/VRM/UniVRM/Scripts/Format/VRMMaterialExporter.cs.meta",
            "Assets/VRM/UniVRM/Scripts/Format/VRMMaterialImporter.cs.meta",
        };

#if VRM_DEVELOP
        [MenuItem(VCIVersion.MENU + "/Export unitypackage")]
#endif
        public static void CreateUnityPackage()
        {
            var folder = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

            var path = GetPath(folder);
            if (File.Exists(path))
            {
                Debug.LogErrorFormat("{0} is already exists", path);
                return;
            }

            {
                var files = EnumerateFiles("Assets/VRM", ExcludeCsProj).ToArray();
                Debug.LogFormat("{0}", string.Join("", files.Select((x, i) => string.Format("[{0:##0}] {1}\n", i, x)).ToArray()));
                AssetDatabase.ExportPackage(files
                    , path,
                    ExportPackageOptions.Default);
            }

            Debug.LogFormat("exported: {0}", path);
        }
    }
}
