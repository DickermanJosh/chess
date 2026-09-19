using System;
using System.IO;
using System.Linq;
using System.Threading;
using Core;
using Opera;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class OperaIntegrationTools
{
    [MenuItem("Opera/Prepare AI scene")]
    public static void PrepareScene()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/OnlineGame.unity");
        foreach (var root in scene.GetRootGameObjects())
            if (root.GetComponent<Canvas>() != null) UnityEngine.Object.DestroyImmediate(root);
        new GameObject("Opera Game").AddComponent<OperaGameController>();
        if (!EditorSceneManager.SaveScene(scene, "Assets/Scenes/AIGame.unity"))
            throw new IOException("Could not save the Opera scene.");
        AssetDatabase.SaveAssets();
        Debug.Log("[Opera checks] AI scene prepared.");
    }

    public static void PrepareAndValidate()
    {
        PrepareScene();
        Validate();
    }

    [MenuItem("Opera/Validate rules and engine")]
    public static void Validate()
    {
        OperaRulesChecks.Run();
        CheckClient();
        Debug.Log("[Opera checks] All integration checks passed.");
    }

    private static void CheckClient()
    {
        var state = new GameState();
        using (var client = new UciEngineClient())
        {
            client.StartAsync(OperaEngineLocator.FindExecutable(), CancellationToken.None).GetAwaiter().GetResult();
            for (int ply = 0; ply < 4; ply++)
            {
                var history = state.MoveTracker.moves.Select(move => move.ToUci()).ToArray();
                string response = client.GetMoveAsync(history, 50, CancellationToken.None).GetAwaiter().GetResult();
                if (response == null || !state.TryApplyMove(Move.FromUci(state, response)))
                    throw new Exception("UCI response did not apply to the Unity board: " + response);
            }
            int pid = client.ProcessId;
            using (var cancellation = new CancellationTokenSource(100))
            {
                try
                {
                    client.GetMoveAsync(state.MoveTracker.moves.Select(move => move.ToUci()).ToArray(),
                        3000, cancellation.Token).GetAwaiter().GetResult();
                    throw new Exception("Search should have been cancelled.");
                }
                catch (OperationCanceledException) { }
            }
            RequireProcessExit(pid);
        }
        using (var restarted = new UciEngineClient())
        {
            restarted.StartAsync(OperaEngineLocator.FindExecutable(), CancellationToken.None).GetAwaiter().GetResult();
            int pid = restarted.ProcessId;
            restarted.Dispose();
            RequireProcessExit(pid);
        }
        Debug.Log("[Opera checks] Real UCI moves, cancellation, restart and child-process cleanup passed.");
    }

    private static void RequireProcessExit(int pid)
    {
        try
        {
            using (var child = System.Diagnostics.Process.GetProcessById(pid))
                if (!child.WaitForExit(2000)) throw new Exception("Opera child process leaked: " + pid);
        }
        catch (ArgumentException) { } // Already reaped.
    }

    [MenuItem("Opera/Build macOS ARM64 playtest")]
    public static void BuildMac()
    {
        if (!File.Exists(Path.Combine(Application.streamingAssetsPath, "Opera/macOS-arm64/opera-uci")))
            throw new FileNotFoundException("Package the macOS ARM64 engine before building.");
        var target = NamedBuildTarget.Standalone;
        int architecture = PlayerSettings.GetArchitecture(target);
        int width = PlayerSettings.defaultScreenWidth, height = PlayerSettings.defaultScreenHeight;
        var screenMode = PlayerSettings.fullScreenMode;
        string product = PlayerSettings.productName;
        string identifier = PlayerSettings.GetApplicationIdentifier(target);
        try
        {
            PlayerSettings.SetArchitecture(target, 1);
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 800;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.productName = "Opera Chess";
            PlayerSettings.SetApplicationIdentifier(target, "com.operaengine.chessplaytest");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
                locationPathName = "build/Opera Desktop/Opera Chess.app",
                target = BuildTarget.StandaloneOSX,
                subtarget = (int)StandaloneBuildSubtarget.Player,
                options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new Exception("Opera playtest build failed: " + report.summary.result);
            Debug.Log("[Opera checks] Playtest built at " + report.summary.outputPath);
        }
        finally
        {
            PlayerSettings.SetArchitecture(target, architecture);
            PlayerSettings.defaultScreenWidth = width;
            PlayerSettings.defaultScreenHeight = height;
            PlayerSettings.fullScreenMode = screenMode;
            PlayerSettings.productName = product;
            PlayerSettings.SetApplicationIdentifier(target, identifier);
            AssetDatabase.SaveAssets();
        }
    }
}
