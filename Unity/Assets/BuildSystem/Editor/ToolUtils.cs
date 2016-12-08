using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class ToolUtils
{
    public static string GetSelectedDirectory()
    {
        string path = null;
        foreach (var obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
                break;
            }
        }
        return path;
    }

    public static string GetSelectedFile()
    {
        string path = null;
        foreach (var obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                break;
            }
        }
        return path;
    }

    public static int Tabs(string[] options, int selected, bool withHorizontal, int minWidth, bool expendWidth,
        int fixedHeight = 0, int fontSize = 23)
    {
        if (withHorizontal)
        {
            GUILayout.BeginHorizontal();
        }

        var style = EditorStyles.toolbarButton;
        if (fixedHeight > 0)
        {
            style = new GUIStyle(style) {fixedHeight = fixedHeight, fontSize = fontSize};
        }
        for (var i = 0; i < options.Length; ++i)
        {
            var isSelected = i == selected;
            if (GUILayout.Toggle(isSelected, options[i], style, GUILayout.MinWidth(minWidth),
                GUILayout.ExpandWidth(expendWidth)))
            {
                selected = i;
            }
        }

        if (withHorizontal)
        {
            GUILayout.EndHorizontal();
        }
        return selected;
    }

    public static string readableSize(long size)
    {
        if (size < 1024)
        {
            return size.ToString();
        }
        else if (size < 1024*1024)
        {
            float sz = size/1024f;
            return sz.ToString("f1") + "k";
        }
        else if (size < 1024*1024*1024)
        {
            float sz = size/1024f/1024f;
            return sz.ToString("f1") + "m";
        }
        else
        {
            float sz = size/1024f/1024f/1024f;
            return sz.ToString("f1") + "g";
        }
    }
}