using System.IO;
using UnityEditor;
using UnityEngine;

public class AnimationNameChecker
{
    public static void Check()
    {
        string[] names =
        {
            "arise",
            "attack01",
            "attack02",
            "attack03",
            "attack04",
            "backtocity",
            "battlestand",
            "behit",
            "born",
            "dead",
            "die",
            "fear",
            "flydown",
            "flyloop",
            "flyup",
            "gather",
            "idleforchoose",
            "idleforchooseloop",
            "idleshow",
            "qinggong",
            "rideseat",
            "rideskill01",
            "rideskill02",
            "ridestand",
            "run",
            "seated",
            "sitdown",
            "skill01",
            "skill02",
            "skill03",
            "skill04",
            "skill11",
            "skill12",
            "skill13",
            "skill14",
            "skill21",
            "skill22",
            "skill23",
            "skill24",
            "sprint",
            "standup",
            "stun",
            "win",
            "idlestand",
            "chuidizi",
            "idlewing",
            "openwing",
            "closewing"
        };

        var aniFiles = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
        var path = new string[aniFiles.Length];

        var index = 0;
        foreach (var file in aniFiles)
        {
            path[index] = AssetDatabase.GetAssetPath(file);
            index++;
        }

        foreach (var p in path)
        {
            var i = p.LastIndexOfAny(new[] {'/'}) + 1;
            var file = p.Substring(i);

            var count = 0;

            var aniNames = Directory.GetFiles(p, "*.anim");

            if (aniNames.Length == 0)
            {
                EditorLogger.Log(file + "  There is no Animation.");
            }

            foreach (var f in aniNames)
            {
                var indexs = f.LastIndexOfAny(new[] {'\\'}) + 1;
                var motionname = f.Substring(indexs, f.Length - indexs - 5);

                var isHave = false;
                foreach (var a in names)
                {
                    if (motionname.Equals(a))
                    {
                        isHave = true;
                        break;
                    }
                }
                if (isHave == false)
                {
                    EditorLogger.Log("<color=red><b>" + file + "</b></color>" + "  <color=red><b>" + motionname +
                                     "</b></color>" + "  is  wrong.");
                    count++;
                }
            }

            if (count == 0 && aniNames.Length != 0)
            {
                EditorLogger.Log(file + "   <color=green><b>All  Animation's  name  is  right.</b></color>");
            }
            if (count != 0)
            {
                EditorLogger.Log("<color=red><b>" + file + "</b></color>  There  is  <color=red><b>" + count +
                                 "</b></color>  wrong  names.");
            }
        }
    }
}