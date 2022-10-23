using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BuildApp
{
    public static bool Build()
    {
        var target = BuildTarget.NoTarget;
        var isDevelopment = false;

        var args = Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length; i++)
        {
            switch(args[i])
            {
                case "-buildTarget":
                    target = Enum.Parse<BuildTarget>(args[i + 1]);
                    break;
                case "-development":
                    isDevelopment = true;
                    break;
            }
        }

        if (target == BuildTarget.NoTarget)
        {
            Debug.LogError("[BuildApp] -buildTarget cannot be null");
            return false;
        }


        // TODO ビルド対象のシーンが増えたら自動で列挙するようにしたほうがいいかもしれない
        // Build scene list
        string[] sceneList =
        {
            "Assets/Scenes/HomeApp.unity"
        };

        // Build options
        BuildOptions options = BuildOptions.None;
        if (isDevelopment)
        {
            options |= BuildOptions.Development;
            options |= BuildOptions.AllowDebugging;
        }


        /*
        // Check or create directory
        const string BUILD_ROOT_DIR = "Build";
        if (!Directory.Exists(BUILD_ROOT_DIR))
        {
            Directory.CreateDirectory(BUILD_ROOT_DIR);
        }
        string BuildDir = BUILD_ROOT_DIR + Path.DirectorySeparatorChar + target.ToString();
        if(!Directory.Exists(BuildDir))
        {
            Directory.CreateDirectory(BuildDir);
        }
        */

        string fileExt = "";

        switch (target)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                fileExt = ".exe";
                break;

            case BuildTarget.Android:
                fileExt = ".apk";
                break;
        }
        if (string.IsNullOrEmpty(fileExt))
        {
            Debug.LogError($"[BuildApp] Unsupported build target: {target}");
            return false;
        }


        // Execute build
        var buildOptions = new BuildPlayerOptions();
        buildOptions.scenes = sceneList;
        buildOptions.locationPathName = Application.productName + fileExt;
        buildOptions.target = target;
        buildOptions.options = options;

        var report = BuildPipeline.BuildPlayer(buildOptions);
        var summary = report.summary;

        switch (summary.result)
        {
            case UnityEditor.Build.Reporting.BuildResult.Succeeded:
                Debug.Log($"[BuildApp] Build succeeded. (Target: {target})");
                break;
            
            case UnityEditor.Build.Reporting.BuildResult.Failed:
            case UnityEditor.Build.Reporting.BuildResult.Unknown:
                Debug.LogError($"[BuildApp] Build failed. (Result: {summary.result})");
                Debug.LogError($"[BuildApp] Error: {summary.totalErrors} / Warning: {summary.totalWarnings}");
                return false;

            case UnityEditor.Build.Reporting.BuildResult.Cancelled:
                Debug.LogWarning("[BuildApp] Build cancelled.");
                return false;

            default:
                Debug.LogError($"[BuildApp] Unknown build result: {summary.result}");
                return false;
        }

        return true;
    }
}
