using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Opera
{
    public static class OperaEngineLocator
    {
        public static string FindExecutable()
        {
            string configured = Environment.GetEnvironmentVariable("OPERA_ENGINE_PATH");
            if (!string.IsNullOrWhiteSpace(configured)) return Path.GetFullPath(configured);
            string cpu = RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "arm64" : "x86_64";
            string platform;
            string filename = "opera-uci";
            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer: platform = "macOS"; break;
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer: platform = "Windows"; filename += ".exe"; break;
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.LinuxPlayer: platform = "Linux"; break;
                default: throw new PlatformNotSupportedException("Opera requires a desktop build of this game.");
            }
            return Path.Combine(Application.streamingAssetsPath, "Opera", platform + "-" + cpu, filename);
        }
    }
}
