using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEditor.Build;

public class BuildScript
{
    [MenuItem("Build/Build Client for Windows")]
    public static void BuildWindowsClient()
    {
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Scene.unity" },
            locationPathName = "Build/Client/client.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None,
            subtarget = (int)StandaloneBuildSubtarget.Player
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        UnityEngine.Debug.Log($"Client build result: {report.summary.result}, size: {report.summary.totalSize} bytes");
    }

    [MenuItem("Build/Build Server for Linux (Headless)")]
    public static void BuildLinuxServer()
    {
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Scene.unity" },
            locationPathName = "Build/Server/Linux/server.x86_64",
            target = BuildTarget.StandaloneLinux64,
            options = BuildOptions.None,
            subtarget = (int)StandaloneBuildSubtarget.Server
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        UnityEngine.Debug.Log($"Server build result: {report.summary.result}, size: {report.summary.totalSize} bytes");
    }

    [MenuItem("Build/Build Server for Windows (Headless)")]
    public static void BuildWindowsServer()
    {
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Scene.unity" },
            locationPathName = "Build/Server/Win64/server.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None,
            subtarget = (int)StandaloneBuildSubtarget.Server
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        Debug.Log($"Windows Server build result: {report.summary.result}, size: {report.summary.totalSize} bytes");
    }
}