using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace BuildSystem
{
    internal static class ModelBoundingBox
    {
        public static void GetAvatarHeight()
        {
            Debug.Log("开始获取模型Npc,Pet,Monster的高度");
            string[] dirs =
            {
                "Assets/Npc/",
                "Assets/Pet/",
                "Assets/Monster/"
            };

            var sb = new StringBuilder();
            foreach (var dir in dirs)
            {
                foreach (var prefab in Directory.GetFiles(dir, "*.prefab"))
                {
                    var obj = AssetDatabase.LoadAssetAtPath(prefab, typeof(Object));
                    var go = Object.Instantiate(obj) as GameObject;
                    var render = go.GetComponentInChildren<SkinnedMeshRenderer>();
                    if (render)
                    {
                        sb.Append(prefab + "\t" + render.bounds.size.y + "\n");
                    }

                    Object.DestroyImmediate(go);
                }
            }
            Debug.Log(sb);
            Debug.Log("获取模型的高度结束");
        }
    }
}