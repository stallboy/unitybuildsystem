using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SpriteChecker
{
    public static void CheckSpritesTagsAndBundles()
    {
        string[] guids = AssetDatabase.FindAssets("t:sprite");

        Dictionary<string, string> dict = new Dictionary<string, string>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null)
            {
                Debug.LogWarning("Sprite : " + path + " not TextureImporter");
            }
            else if (!dict.ContainsKey(ti.spritePackingTag))
            {
                dict.Add(ti.spritePackingTag, ti.assetBundleName);
            }
            else if (dict[ti.spritePackingTag] != ti.assetBundleName)
            {
                Debug.LogWarning("Sprite : " + ti.assetPath + " should be packed in " + dict[ti.spritePackingTag]);
            }
        }
    }
}