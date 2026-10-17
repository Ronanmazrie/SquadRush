using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SquadRush.Editor
{
    public static class DistributionBuilder
    {
        const string ScenePath = "Assets/Scenes/SquadRush.unity";
        const string Version = "0.14.0";

        [MenuItem("Squad Rush/Distribution/Build Android Download")]
        public static void BuildAndroid()
        {
            PlayerSettings.bundleVersion = Version;
            PlayerSettings.Android.bundleVersionCode = 15;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            EditorUserBuildSettings.buildAppBundle = false;
            Build(BuildTarget.Android, "Builds/Android/SquadRush-Android.apk");
        }

        [MenuItem("Squad Rush/Distribution/Build Mobile Browser Version")]
        public static void BuildWeb()
        {
            PlayerSettings.bundleVersion = Version;
            // GitHub Pages cannot set Content-Encoding for Unity's compressed artifacts.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.template = "PROJECT:SquadRush";
            AssetDatabase.SaveAssets();
            Build(BuildTarget.WebGL, "Builds/Web");
        }

        static void Build(BuildTarget target, string fallback)
        {
            PrototypeBuilder.PrepareVisuals();
            if (!BuildPipeline.IsBuildTargetSupported(BuildPipeline.GetBuildTargetGroup(target), target))
                throw new InvalidOperationException("The requested platform module is not installed: " + target);
            string[] args = Environment.GetCommandLineArgs();
            int outputIndex = Array.IndexOf(args, "-buildOutput");
            string destination = outputIndex >= 0 && outputIndex + 1 < args.Length ? args[outputIndex + 1] : fallback;
            destination = Path.GetFullPath(destination);
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            AssetDatabase.SaveAssets();
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath }, target = target, locationPathName = destination,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Distribution build failed: " + report.summary.result);
            Debug.Log("[SquadRush] Distribution build succeeded: " + target + ", bytes=" + report.summary.totalSize);
        }
    }
}
