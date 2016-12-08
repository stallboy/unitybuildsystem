using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BuildSystem
{
    public class ABMarkDupChecker
    {
        private static Dictionary<string, string> markedAsset2Bundle;
        private static Dictionary<string, ABAssetinfos.AssetInfo> allAssetInfos;
        private static ABAssetinfos assetInfos;

        private static bool isMarked(string asset)
        {
            return markedAsset2Bundle.ContainsKey(asset);
        }

        private static void collectUnmarkedAsset(string thisAsset, string bundle)
        {
            foreach (var asset in AssetDatabase.GetDependencies(thisAsset, false))
            {
                if (!asset.EndsWith(".cs") && !isMarked(asset))
                {
                    ABAssetinfos.AssetInfo res;
                    if (allAssetInfos.TryGetValue(asset, out res))
                    {
                        res.containingABs.Add(bundle);
                    }
                    else
                    {
                        res = new ABAssetinfos.AssetInfo {asset = asset, isMarked = false};
                        res.containingABs.Add(bundle);
                        allAssetInfos.Add(asset, res);
                    }
                    collectUnmarkedAsset(asset, bundle);
                }
            }
        }

        private static int calcSize(string asset)
        {
            if (asset.EndsWith(".unity"))
            {
                return 0;
            }
            var objs = AssetDatabase.LoadAllAssetsAtPath(asset);
            var allsize = 0;
            foreach (var obj in objs)
            {
                if (obj != null)
                {
                    var size = Profiler.GetRuntimeMemorySize(obj);
                    allsize += size;

                    if (obj is GameObject || obj is Component)
                    {
                    }
                    else
                    {
                        Resources.UnloadAsset(obj);
                    }
                }
                else
                {
                    Debug.LogError(asset + " load=null");
                }
            }
            return allsize;
        }


        public static void CheckDuplicate(string dupcsvfn)
        {
            markedAsset2Bundle = new Dictionary<string, string>();
            foreach (var bundle in AssetDatabase.GetAllAssetBundleNames())
            {
                foreach (var asset in AssetDatabase.GetAssetPathsFromAssetBundle(bundle))
                {
                    markedAsset2Bundle.Add(asset, bundle);
                }
            }

            Debug.Log("marked asset count=" + markedAsset2Bundle.Count);
            allAssetInfos = new Dictionary<string, ABAssetinfos.AssetInfo>();
            foreach (var kv in markedAsset2Bundle)
            {
                collectUnmarkedAsset(kv.Key, kv.Value);
            }

            foreach (var kv in markedAsset2Bundle)
            {
                var ai = new ABAssetinfos.AssetInfo {asset = kv.Key, isMarked = true};
                ai.containingABs.Add(kv.Value);
                allAssetInfos.Add(kv.Key, ai);
            }
            Debug.Log("all asset count=" + allAssetInfos.Count);


            assetInfos = new ABAssetinfos();
            foreach (var kv in allAssetInfos)
            {
                var ai = kv.Value;
                ai.memSize = calcSize(kv.Key);
                ai.containingABCount = ai.containingABs.Count;
                ai.canSaveMemSize = ai.memSize*(ai.containingABCount - 1);
                assetInfos.sortedAllAssetInfos.Add(ai);
            }
            assetInfos.sortedAllAssetInfos.Sort(ABAssetinfos.assetinfoCmp);
            assetInfos.sum();

            Resources.UnloadUnusedAssets();


            Debug.Log(assetInfos.sumStr);

            assetInfos.SaveToCsv(dupcsvfn);
            Debug.Log("save to " + dupcsvfn);
        }
    }
}