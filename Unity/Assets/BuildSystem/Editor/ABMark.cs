using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace BuildSystem
{
    public abstract class ABMark
    {
        protected readonly Dictionary<string, string> allMarks = new Dictionary<string, string>();
        protected readonly Dictionary<string, string> notInterestedOldAssets = new Dictionary<string, string>();

        protected abstract void DoMark();

        public void MarkAllAndSave(string saveTo)
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();
            notInterestedOldAssets.Clear();
            allMarks.Clear();

            foreach (var bundle in AssetDatabase.GetAllAssetBundleNames())
            {
                foreach (var assetpath in AssetDatabase.GetAssetPathsFromAssetBundle(bundle))
                {
                    var asset = assetpath.Replace("\\", "/");
                    if (notInterestedOldAssets.ContainsKey(asset))
                    {
                        EditorLogger.Log("duplicate {0}", asset);
                    }
                    else
                    {
                        //EditorLogger.Log("old asset = {0}, bundle = {1}", asset, bundle);
                        notInterestedOldAssets.Add(asset, bundle);
                    }
                }
            }

            DoMark();

            foreach (var old in notInterestedOldAssets)
            {
                var importer = AssetImporter.GetAtPath(old.Key);
                importer.assetBundleName = null;
                EditorLogger.Log("unmark asset = {0}, bundle = {1}", old.Key, old.Value);
            }
            using (var sw = new StreamWriter(saveTo, false)) //no bom
            {
                foreach (var kv in allMarks)
                {
                    sw.WriteLine(kv.Key + "," + kv.Value);
                }
            }
        }

        public void SaveToConfig()
        {
            ABToConfig.CSVInitialize();

            foreach (var kv in allMarks)
            {
                string asset = kv.Key;
                string bundle = kv.Value;

                string cachePolicy = "common";
                int cacheLruSize = 0;
                string ui = null;
                string scene = null;
                AssetType type = AssetType.asset;

                if (asset.EndsWith(".unity.bundle"))
                {
                    string[] sp = asset.Split('/', '.');
                    scene = sp[sp.Length - 3];
                    type = AssetType.assetbundle;
                }
                else if (asset.StartsWith("ui/panel"))
                {
                    string[] splitArray = bundle.Split('/', '.');
                    ui = splitArray[splitArray.Length - 3];
                }
                else if (asset.StartsWith("ui/atlas"))
                {
                    type = AssetType.sprite;
                }
                else if (asset.EndsWith(".prefab"))
                {
                    type = AssetType.prefab;
                }

                if (scene != null)
                {
                    ABToConfig.CSVAddSceneEnum(scene, bundle);
                }
                if (ui != null)
                {
                    ABToConfig.CSVAddUIEnum(ui, asset);
                }
                ABToConfig.CSVAddAsset(asset, bundle, type, AssetLocation.www, cachePolicy);
                ABToConfig.CSVAddPolicyIf(cachePolicy, cacheLruSize);
            }

            ABToConfig.CSVSave();
        }

        protected void MarkDir(string path, string searchPattern,
            SearchOption searchOption = SearchOption.AllDirectories)
        {
            foreach (var file in Directory.GetFiles(path, searchPattern, searchOption))
            {
                Mark(file);
            }
        }

        protected void MarkDirName(string path, string searchPattern, string assetBundleName,
            SearchOption searchOption = SearchOption.AllDirectories)
        {
            foreach (var file in Directory.GetFiles(path, searchPattern, searchOption))
            {
                MarkName(file, assetBundleName);
            }
        }


        protected void Mark(string assetPath)
        {
            MarkName(assetPath, assetPath.Substring(7));
        }

        protected void MarkName(string assetPath, string assetBundleName)
        {
            var asset = assetPath.Replace("\\", "/");
            var bundle = assetBundleName.Replace("\\", "/").ToLower() + ".bundle";
            var brifasset = asset.Substring(7).ToLower();
            //EditorLogger.Log("asset = {0}, bundle = {1}", brifasset, bundle);
            allMarks[brifasset] = bundle;
            string oldBundle;
            if (notInterestedOldAssets.TryGetValue(asset, out oldBundle))
            {
                notInterestedOldAssets.Remove(asset);
                if (oldBundle.Equals(bundle))
                {
                    return;
                }
            }
            var importer = AssetImporter.GetAtPath(asset);
            EditorLogger.Log("mark asset = {0}, bundle = {1}", asset, bundle);
            importer.assetBundleName = bundle;
        }
    }
}