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
            string errMsg = "[BuildApp] -buildTarget cannot be null";
            Debug.LogError(errMsg);
            throw new ArgumentException(errMsg);
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
        const string BUILD_ROOT_DIR = "Build";
        if (!Directory.Exists(BUILD_ROOT_DIR))
        {
            Directory.CreateDirectory(BUILD_ROOT_DIR);
        }
        // TODO Buildディレクトリの中身をクリアしないといけないかもしれない

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
            string errMsg = $"[BuildApp] Unsupported build target: {target}";
            Debug.LogError(errMsg);
            throw new NotSupportedException(errMsg);
        }


        // Execute build
        var buildOptions = new BuildPlayerOptions();
        buildOptions.scenes = sceneList;
        buildOptions.locationPathName = BUILD_ROOT_DIR + Path.DirectorySeparatorChar + Application.productName + fileExt;
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
                {
                    string errMsg = $"[BuildApp] Build failed. (Result: {summary.result})";
                    Debug.LogError(errMsg);
                    Debug.LogError($"[BuildApp] Error: {summary.totalErrors} / Warning: {summary.totalWarnings}");
                    throw new Exception(errMsg);
                }

            case UnityEditor.Build.Reporting.BuildResult.Cancelled:
                {
                    string errMsg = "[BuildApp] Build cancelled.";
                    Debug.LogWarning(errMsg);
                    throw new Exception(errMsg);
                }

            default:
                {
                    string errMsg = $"[BuildApp] Unknown build result: {summary.result}";
                    Debug.LogError(errMsg);
                    throw new Exception(errMsg);
                }
        }
    }
}
