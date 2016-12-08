using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace BuildSystem
{
    public class TexImportSetting
    {
        public string path;

        public string format;
        public string colorSpace;

        public TextureImporterFormat _defaultImportFormat;
        public string defaultImportFormat;
        public TextureImporterFormat _platformImportFormat;
        public string platformImportFormat;

        public int defaultCompressionQuality;
        public int platformCompressionQuality;

        public int defaultMaxTextureSize;
        public int platformMaxTextureSize;

        public TextureImporterSettings setting = new TextureImporterSettings();
    }

    public class TextureImportFix
    {
        public static List<TexImportSetting> texSettingList = new List<TexImportSetting>();
        public static List<string> logList = new List<string>();

        public static void checkTexture()
        {
            texSettingList.Clear();
            logList.Clear();

            string platform = Enum.GetName(typeof(BuildTarget), BuildTarget.Android);
            foreach (var guid in AssetDatabase.FindAssets("t:Texture"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter im = AssetImporter.GetAtPath(path) as TextureImporter;
                if (im != null)
                {
                    TexImportSetting ti = new TexImportSetting
                    {
                        path = path.Substring(7),
                        defaultCompressionQuality = im.compressionQuality,
                        defaultMaxTextureSize = im.maxTextureSize,
                        _defaultImportFormat = im.textureFormat,
                        defaultImportFormat = Enum.GetName(typeof(TextureImporterFormat), im.textureFormat),
                    };

                    im.ReadTextureSettings(ti.setting);
                    TextureImporterFormat fmt;
                    if (im.GetPlatformTextureSettings(platform, out ti.platformMaxTextureSize, out fmt,
                        out ti.platformCompressionQuality))
                    {
                        ti._platformImportFormat = fmt;
                        ti.platformImportFormat = Enum.GetName(typeof(TextureImporterFormat), fmt);
                    }

                    ColorSpace colorSpace;
                    int compressionQuality;
                    TextureFormat format;
                    im.ReadTextureImportInstructions(BuildTarget.Android, out format, out colorSpace,
                        out compressionQuality);

                    ti.format = Enum.GetName(typeof(TextureFormat), format);
                    ti.colorSpace = Enum.GetName(typeof(ColorSpace), colorSpace);

                    texSettingList.Add(ti);

                    if (format != TextureFormat.ETC_RGB4 && format != TextureFormat.ETC2_RGB &&
                        format != TextureFormat.ETC2_RGBA1 &&
                        format != TextureFormat.ETC2_RGBA8)
                    {
                        log("{0} format not etc {1} ", path, ti.format);
                    }

                    if (ti.setting.readable)
                    {
                        log("{0} readable", path);
                    }
                }
                else
                {
                    log("{0} importer = null", path);
                }
            }
        }

        public static void saveTexture()
        {
            using (var sw = new StreamWriter("贴图资源导入设置.csv", false, Encoding.UTF8))
            {
                sw.WriteLine(
                    "path,format,colorSpace,platformCompressionQuality,maxTextureSize,filterMode,aniso,readable,grayscaleToAlpha,mipmapEnabled,borderMipmap,generateMipsInLinearSpace,fadeOut,alphaIsTransparency,allowsAlphaSplit,normalMap,convertToNormalMap,lightmap,linearTexture,seamlessCubemap");
                foreach (var ti in texSettingList)
                {
                    var s = ti.setting;
                    var line = string.Join(",", new[]
                    {
                        ti.path, ti.format, ti.colorSpace,
                        ti.platformCompressionQuality.ToString(),
                        s.maxTextureSize.ToString(),
                        Enum.GetName(typeof(FilterMode), s.filterMode),
                        s.aniso.ToString(),
                        s.readable ? "readable" : "",
                        s.grayscaleToAlpha ? "grayscaleToAlpha" : "",
                        s.mipmapEnabled ? "mipmapEnabled" : "",
                        s.borderMipmap ? "borderMipmap" : "",
                        s.generateMipsInLinearSpace ? "generateMipsInLinearSpace" : "",
                        s.fadeOut ? "fadeOut" : "",
                        s.alphaIsTransparency ? "alphaIsTransparency" : "",
                        s.allowsAlphaSplit ? "allowsAlphaSplit" : "",
                        s.normalMap ? "normalMap" : "",
                        s.convertToNormalMap ? "convertToNormalMap" : "",
                        s.lightmap ? "lightmap" : "",
                        s.linearTexture ? "linearTexture" : "",
                        s.seamlessCubemap ? "seamlessCubemap" : ""
                    });
                    sw.WriteLine(line);
                }
            }

            using (var sw = new StreamWriter("贴图资源导入设置log.txt", false, Encoding.UTF8))
            {
                foreach (var log in logList)
                {
                    sw.WriteLine(log);
                }
            }
        }

        public static void setTextureAutomaticCompressed()
        {
            foreach (var ti in texSettingList)
            {
                if (ti._platformImportFormat == TextureImporterFormat.ETC2_RGBA8)
                {
                    var path = "Assets/" + ti.path;
                    TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer != null)
                    {
                        log("{0} set importer.textureFormat = AutomaticCompressed", path);
                        importer.textureFormat = TextureImporterFormat.AutomaticCompressed;
                        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                    }
                    else
                    {
                        log("{0} importer null", path);
                    }
                }
            }
        }

        public static void setTextureEtc1()
        {
            foreach (var ti in texSettingList)
            {
                if (ti._defaultImportFormat != TextureImporterFormat.AutomaticCompressed)
                {
                    var path = "Assets/" + ti.path;
                    TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer != null)
                    {
                        log("{0} set importer.textureFormat = AutomaticCompressed", path);
                        importer.textureFormat = TextureImporterFormat.AutomaticCompressed;
                        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                    }
                    else
                    {
                        log("{0} importer null", path);
                    }
                }
            }
        }


        public static void log(string fmt, params object[] parameters)
        {
            string res = string.Format(fmt, parameters);
            Debug.Log(res);
            logList.Add(res);
        }
    }
}