using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BuildSystem
{
    static class ABBuild
    {
        public static void BuildAssetBundlesForActiveTarget()
        {
            DoBuild(EditorUserBuildSettings.activeBuildTarget);
        }

        public static void CopyToAssets()
        {
            var platform = GetPlatformForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
            var outputPath = "AssetBundles/" + platform;
            var sw = Stopwatch.StartNew();
            EditorLogger.Log("CopyToAssets from {0} Start ========================", outputPath);
            CopyFiles(outputPath, platform, "Assets/StreamingAssets");
            EditorLogger.Log("CopyToAssets from {0} Ok, Used {1}", outputPath, sw.Elapsed);
        }


        private static void DoBuild(BuildTarget target)
        {
            var platform = GetPlatformForAssetBundles(target);
            var outputPath = "AssetBundles/" + platform;
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            Dictionary<string, DateTime> oldFileLastWriteTimes = collectFileLastWriteTimes(outputPath, true);

            var sw = Stopwatch.StartNew();
            EditorLogger.Log("BuildAssetBundles to {0} Start ========================", outputPath);
            var manifest = BuildPipeline.BuildAssetBundles(outputPath,
                BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ChunkBasedCompression |
                BuildAssetBundleOptions.DisableWriteTypeTree,
                target);
            EditorLogger.Log("BuildAssetBundles to {0} Ok, Used {1}", outputPath, sw.Elapsed);

            HashSet<string> neededFiles = collectNeededFiles(manifest, platform);

            CheckFiles(outputPath, neededFiles, oldFileLastWriteTimes);
        }

        private static string GetPlatformForAssetBundles(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.Android:
                    return "Android";
                case BuildTarget.iOS:
                    return "iOS";
                case BuildTarget.WebGL:
                    return "WebGL";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Windows";
                case BuildTarget.StandaloneOSXIntel:
                case BuildTarget.StandaloneOSXIntel64:
                case BuildTarget.StandaloneOSXUniversal:
                    return "OSX";
                default:
                    return null;
            }
        }


        private static Dictionary<string, DateTime> collectFileLastWriteTimes(string outputPath, bool includeManifest)
        {
            Dictionary<string, DateTime> result = new Dictionary<string, DateTime>();
            var prefix = outputPath.Length + 1;
            foreach (var file in Directory.GetFiles(outputPath, "*", SearchOption.AllDirectories))
            {
                if (includeManifest || !file.EndsWith(".manifest"))
                {
                    var fi = new FileInfo(file);
                    var fn = file.Substring(prefix).Replace("\\", "/");
                    result.Add(fn, fi.LastWriteTime);
                }
            }
            return result;
        }

        private static HashSet<string> collectNeededFiles(AssetBundleManifest manifest, string platform)
        {
            var bundles = new HashSet<string>();
            foreach (var assetBundleName in manifest.GetAllAssetBundles())
            {
                bundles.Add(assetBundleName);
                bundles.Add(assetBundleName + ".manifest");
            }
            bundles.Add(platform);
            bundles.Add(platform + ".manifest");
            return bundles;
        }

        private static void CheckFiles(string outputPath, HashSet<string> neededFiles,
            Dictionary<string, DateTime> oldFileLastWriteTimes)
        {
            HashSet<string> missedNeededFiles = new HashSet<string>(neededFiles);
            Dictionary<string, DateTime> okNeededFileLastWriteTimes = new Dictionary<string, DateTime>();
            var prefix = outputPath.Length + 1;
            foreach (var file in Directory.GetFiles(outputPath, "*", SearchOption.AllDirectories))
            {
                var fn = file.Substring(prefix).Replace("\\", "/");
                if (neededFiles.Contains(fn))
                {
                    missedNeededFiles.Remove(fn);
                    var fi = new FileInfo(file);
                    okNeededFileLastWriteTimes.Add(fn, fi.LastWriteTime);
                }
                else
                {
                    File.Delete(file);
                    logDel(fn);
                }
            }

            DeleteEmptyDirectory(outputPath, false);

            foreach (var fn in missedNeededFiles)
            {
                logMiss(fn);
            }

            foreach (var e in okNeededFileLastWriteTimes)
            {
                var fn = e.Key;
                DateTime time = e.Value;
                DateTime oldTime;
                if (oldFileLastWriteTimes.TryGetValue(fn, out oldTime))
                {
                    if (!oldTime.Equals(time))
                    {
                        logModifiy(fn);
                    }
                }
                else
                {
                    logAdd(fn);
                }
            }
        }

        private static bool IsSvnDirOrFile(string fullName)
        {
            return fullName.IndexOf(".svn", StringComparison.Ordinal) > 0;
        }

        private static void DeleteEmptyDirectory(string path, bool deleteMeta)
        {
            foreach (var dir in Directory.GetDirectories(path, "*", SearchOption.AllDirectories))
            {
                if (Directory.Exists(dir))
                {
                    if (Directory.GetFiles(dir, "*", SearchOption.AllDirectories).Length == 0)
                    {
                        if (!IsSvnDirOrFile(dir))
                        {
                            Directory.Delete(dir, true);
                            if (deleteMeta)
                            {
                                File.Delete(dir + ".meta");
                            }
                            logDelDir(dir);
                        }
                    }
                }
            }
        }

        private static void CopyFiles(string abPath, string platform, string toPath)
        {
            Dictionary<string, DateTime> neededFileLastWrites = collectFileLastWriteTimes(abPath, false);
            DateTime manifestTime = neededFileLastWrites[platform];
            neededFileLastWrites.Remove(platform);


            var mfi = new FileInfo(toPath + "/manifest");
            if (mfi.Exists)
            {
                if (!mfi.LastWriteTime.Equals(manifestTime))
                {
                    logModifiy("manifest");
                }
                File.Copy(abPath + "/" + platform, toPath + "/manifest", true);
            }
            else
            {
                File.Copy(abPath + "/" + platform, toPath + "/manifest");
                logAdd("manifest");
            }


            HashSet<string> needAddFiles = new HashSet<string>(neededFileLastWrites.Keys);

            var prefix = toPath.Length + 1;
            foreach (var file in Directory.GetFiles(toPath, "*", SearchOption.AllDirectories))
            {
                var fn = file.Substring(prefix).Replace("\\", "/");
                if (fn.EndsWith(".meta") || fn.Equals("manifest"))
                {
                    continue;
                }
                if (fn.StartsWith("lua.zip") || fn.StartsWith("cfg/") || fn.StartsWith("effectcfg/") ||
                    fn.StartsWith("mapcfg/") || fn.StartsWith("wanmeilogomovie") || fn.StartsWith("logo"))
                {
                    continue;
                }
                if (IsSvnDirOrFile(file))
                {
                    continue;
                }

                DateTime neededFileTime;
                if (neededFileLastWrites.TryGetValue(fn, out neededFileTime))
                {
                    needAddFiles.Remove(fn);
                    var fi = new FileInfo(file);
                    if (!fi.LastWriteTime.Equals(neededFileTime))
                    {
                        logModifiy(fn);
                    }
                    File.Copy(abPath + "/" + fn, file, true);
                }
                else
                {
                    if (!IsSvnDirOrFile(file))
                    {
                        File.Delete(file);
                        File.Delete(file + ".meta");
                        logDel(fn);
                    }
                }
            }

            foreach (var fn in needAddFiles)
            {
                var tofn = toPath + "/" + fn;
                // ReSharper disable once AssignNullToNotNullAttribute
                Directory.CreateDirectory(Path.GetDirectoryName(tofn));
                File.Copy(abPath + "/" + fn, tofn);
                logAdd(fn);
            }

            DeleteEmptyDirectory(toPath, true);
        }

        private static void logAdd(string fn)
        {
            EditorLogger.Log(EditorLogger.AddColor("ADD FILE " + fn, LoggerColor.orange));
        }

        private static void logDel(string fn)
        {
            EditorLogger.Log(EditorLogger.AddColor("DELETE FILE " + fn, LoggerColor.purple));
        }

        private static void logDelDir(string dir)
        {
            EditorLogger.Log(EditorLogger.AddColor("DELETE DIR " + dir, LoggerColor.purple));
        }

        private static void logModifiy(string fn)
        {
            EditorLogger.Log("MODIFY FILE {0}", fn);
        }

        private static void logMiss(string fn)
        {
            EditorLogger.Log(EditorLogger.AddColor("ERROR MISS FILE " + fn, LoggerColor.red));
        }
    }
}