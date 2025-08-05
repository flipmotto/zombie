using UnityEditor;
using UnityEditor.Build.Reporting;

public class BuildScript
{
    [MenuItem("Build/Build Client for Windows")]
    public static void BuildWindowsClient()
    {
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/SampleScene.unity" },
            locationPathName = "Build/Client/client.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        UnityEngine.Debug.Log($"Client build result: {report.summary.result}, size: {report.summary.totalSize} bytes");
    }

    [MenuItem("Build/Build Server for Linux (Headless)")]
    public static void BuildLinuxServer()
    {
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/SampleScene.unity" },
            locationPathName = "Build/Server/server.x86_64",
            target = BuildTarget.StandaloneLinux64,
            options = BuildOptions.None,
            subtarget = (int)StandaloneBuildSubtarget.Server
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        UnityEngine.Debug.Log($"Server build result: {report.summary.result}, size: {report.summary.totalSize} bytes");
    }
}