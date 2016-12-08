using System.Collections.Generic;
using System.IO;
using System.Text;
using Config;

namespace BuildSystem
{
    internal enum AssetType
    {
        assetbundle = 1,
        asset = 2,
        prefab = 3,
        sprite = 4
    }

    internal enum AssetLocation
    {
        www = 1,
        resources = 2
    }

    static class ABToConfig
    {
        private const string policycsvfile = "../config/assetcachepolicy.csv";
        private const string assetcsvfile = "../config/assets.csv";
        private const string assetuicsvfile = "../config/assetui.csv";
        private const string assetscenecsvfile = "../config/assetscene.csv";
        

        private static List<List<string>> policycsv = new List<List<string>>();
        private static readonly List<List<string>> assetcsv = new List<List<string>>();
        private static readonly List<List<string>> assetuicsv = new List<List<string>>();
        private static readonly List<List<string>> assetscenecsv = new List<List<string>>();

        
        public static void CSVInitialize()
        {
            policycsv.Clear();
            assetcsv.Clear();
            assetuicsv.Clear();
            assetscenecsv.Clear();
            
            if (File.Exists(policycsvfile))
            {
                using (var texter = new StreamReader(policycsvfile, Encoding.UTF8))
                {
                    policycsv = CSV.Parse(texter);
                }
            }
            else
            {
                var policydefine = new List<string> {"name", "lruSize"};
                policycsv.Add(policydefine);
                policycsv.Add(policydefine);
            }

            File.Delete(assetcsvfile);
            var assetcomment = new List<string>
            {
                "assetpath",
                "abpath",
                "type(assetbundle:1;asset:2;prefab:3;sprite:4)",
                "location(www:1;resources:2)",
                "cachepolicy"
            };
            var assetdefine = new List<string>
            {
                "assetpath",
                "abpath",
                "type",
                "location",
                "cachepolicy"
            };
            assetcsv.Add(assetcomment);
            assetcsv.Add(assetdefine);

            var uiassetcomment = new List<string>
            {
                "assetname",
                "assetpath",
            };
            var uiassetdefine = new List<string>
            {
                "assetname",
                "assetpath",
            };
            assetuicsv.Add(uiassetcomment);
            assetuicsv.Add(uiassetdefine);

            assetscenecsv.Add(uiassetcomment);
            assetscenecsv.Add(uiassetdefine);
        }

        public static void CSVAddPolicyIf(string name, int lruSize)
        {
            for (int i = 2; i < policycsv.Count; i++)
            {
                var policy = policycsv[i];
                if (policy[0].Equals(name))
                    return;
            }
            policycsv.Add(new List<string> {name, lruSize.ToString()});
        }

        public static void CSVAddAsset(string assetpath, string abpath, AssetType type, AssetLocation location,
            string cachepolicy)
        {
            var asset = new List<string>
            {
                assetpath,
                abpath,
                ((int) type).ToString(),
                ((int) location).ToString(),
                cachepolicy
            };
            assetcsv.Add(asset);
        }

        public static void CSVAddUIEnum(string assetname, string assetpath)
        {
            var ui = new List<string>
            {
                assetname,
                assetpath
            };
            assetuicsv.Add(ui);
        }

        public static void CSVAddSceneEnum(string assetname, string assetpath)
        {
            var scene = new List<string>
            {
                assetname,
                assetpath
            };
            assetscenecsv.Add(scene);
        }


        public static void CSVSave()
        {
            SaveToCsv(policycsv, policycsvfile);
            SaveToCsv(assetcsv, assetcsvfile);
            SaveToCsv(assetuicsv, assetuicsvfile);
            SaveToCsv(assetscenecsv, assetscenecsvfile);
        }

        private static void SaveToCsv(List<List<string>> data, string fn) //假设这里没有需要转义的字符
        {
            using (var texter = new StreamWriter(fn, false)) //no bom
            {
                foreach (var line in data)
                {
                    texter.WriteLine(string.Join(",", line.ToArray()));
                }
            }
        }

        
    }
}