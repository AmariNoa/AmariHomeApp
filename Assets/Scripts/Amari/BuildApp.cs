using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BuildApp
{
    public static void Build()
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
            return;
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


        // Check or create directory
        const string BUILD_DIR = "Build";
        if (!Directory.Exists(BUILD_DIR))
        {
            Directory.CreateDirectory(BUILD_DIR);
        }


        // Execute build
        var buildOptions = new BuildPlayerOptions();
        buildOptions.scenes = sceneList;
        buildOptions.locationPathName = BUILD_DIR + Path.DirectorySeparatorChar + target.ToString();
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
                return;

            case UnityEditor.Build.Reporting.BuildResult.Cancelled:
                Debug.LogWarning("[BuildApp] Build cancelled.");
                return;

            default:
                Debug.LogError($"[BuildApp] Unknown build result: {summary.result}");
                return;
        }
    }
}
