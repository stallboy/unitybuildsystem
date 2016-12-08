using UnityEngine;
using UnityEditor;
using System.IO;

public class RenderImage : EditorWindow {

    public Camera cam;
    public int width = 512;
    public int height = 512;
    public Object targetFolder;

    public static void Init()
    {
        RenderImage window = (RenderImage)EditorWindow.GetWindow(typeof(RenderImage), true, "渲染摄像机");
        window.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        cam = EditorGUILayout.ObjectField("渲染用的摄像机：", cam, typeof(Camera), true) as Camera;
        width = EditorGUILayout.IntField("贴图尺寸：", width);
        height = EditorGUILayout.IntField("贴图尺寸：", height);
        EditorGUILayout.HelpBox("指定渲染图片存放文件夹，若不指定则放在Asset根目录下，名称为“RenderImage”", MessageType.Info);
        targetFolder = EditorGUILayout.ObjectField("贴图存放文件夹：", targetFolder, typeof(Object), true) as Object;
        if (GUILayout.Button("Accept"))
        {
            BtnCmd();
        }
        EditorGUILayout.EndVertical();
    }

    void BtnCmd()
    {
        
        RenderTextureToJPG(cam);
        AssetDatabase.SaveAssets();
    }

    void RenderTextureToJPG(Camera cam)
    {
        string path;
        if (targetFolder == null)        
            path = Application.dataPath + @"/RenderImage.jpg";        
        else
            path = string.Join("/", new string[] { AssetDatabase.GetAssetPath(targetFolder), @"/RenderImage.jpg" });
        RenderTexture tempRT = new RenderTexture(width, height, 24);
        RenderTexture.active = tempRT;
        cam.targetTexture = tempRT;

        Texture2D tex = new Texture2D(tempRT.width, tempRT.height);
        Debug.Log("tempRT.width:  " + tempRT.width);
        Debug.Log("tempRT.height:  " + tempRT.height);
        cam.Render();

        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        Debug.Log("tex.width:  " + tex.width);
        Debug.Log("tex.height:  " + tex.height);
        tex.Apply();
        byte[] bytes = tex.EncodeToJPG();

        File.WriteAllBytes(path, bytes);
        AssetDatabase.Refresh();
        #region 贴图导入设置
        //TextureImporter texImporter = AssetImporter.GetAtPath(path) as TextureImporter;
        //texImporter.textureType = TextureImporterType.Advanced;
        //texImporter.isReadable = true;
        //texImporter.textureFormat = TextureImporterFormat.RGB24;
        //texImporter.wrapMode = TextureWrapMode.Clamp;
        #endregion


        
        RenderTexture.active = null;
        cam.targetTexture = null;
        Object.DestroyImmediate(tempRT);
        Object.DestroyImmediate(tex);




    }

}
