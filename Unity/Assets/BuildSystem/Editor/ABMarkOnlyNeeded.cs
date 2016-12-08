using System.Collections.Generic;
using System.IO;
using Config;
using UnityEditor;
using UnityEngine;

namespace BuildSystem
{
    public class ABMarkOnlyNeeded : ABMark
    {
        readonly HashSet<string> neededAssets = new HashSet<string>();
        readonly Dictionary<string, string> allAssetMarks = new Dictionary<string, string>();


        public static void DoTheMarkAndSaveTheOnlyNeededAssetsCsv()
        {
            BuildSystemWindow.genrefassets();
            var marker = new ABMarkOnlyNeeded();
            marker.MarkAllAndSave("markonlyneeded.csv");
        }

        protected override void DoMark()
        {
            neededAssets.Clear();
            allAssetMarks.Clear();

            foreach (var line in CSV.Parse(new StreamReader("refassets.csv")))
            {
                var r = line[0];
                if (r.Length > 0)
                {
                    neededAssets.Add("assets/" + r);
                }
            }

            foreach (var line in CSV.Parse(new StreamReader("refassetscene.csv")))
            {
                var r = line[0];
                neededAssets.Add("assets/scene/" + r + ".unity");
            }

            
            foreach (var line in CSV.Parse(new StreamReader("markbyrule.csv")))
            {
                allAssetMarks.Add(line[0], line[1]);
            }

            HashSet<string> allNeededAssets = new HashSet<string>(neededAssets);
            foreach (var asset in neededAssets)
            {
                foreach (var depasset in AssetDatabase.GetDependencies(asset))
                {
                    if (!allNeededAssets.Contains(depasset))
                    {
                        allNeededAssets.Add(depasset);
                    }
                }
            }

            foreach (var asset in allNeededAssets)
            {
                string briefasset = asset.Substring(7).ToLower();
                string mark;
                if (allAssetMarks.TryGetValue(briefasset, out mark))
                {
                    MarkName(asset, mark);
                }
                else
                {
                    Debug.LogError(briefasset + " no mark");
                }
            }
        }
    }
}