using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace BuildSystem
{
    public class AnimationTimeExport
    {
        public static void ExportAnimationTime()
        {
            var anim2Time = new Dictionary<string, float>();
            Collect("Assets/Hero", anim2Time);
            Collect("Assets/Monster", anim2Time);
            Collect("Assets/Npc", anim2Time);
            Collect("Assets/Pet", anim2Time);

            using (var sw = new StreamWriter("../config/animationtime.csv", false, Encoding.GetEncoding("GBK")))
            {
                sw.WriteLine("DO NOT EDIT,clip length in second");
                sw.WriteLine("anim,time");

                foreach (var animtime in anim2Time)
                {
                    sw.WriteLine(animtime.Key.ToLower() + "," + animtime.Value);
                }
            }

            EditorLogger.Log("generated animationtime.csv count={0}", anim2Time.Count);
        }

        private static void Collect(string dir, Dictionary<string, float> anim2Time)
        {
            foreach (var animFile in Directory.GetFiles(dir, "*.anim", SearchOption.AllDirectories))
            {
                var anim = animFile.Replace("\\", "/");

                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(anim);
                if (clip != null)
                {
                    if (clip.length > 0)
                    {
                        anim2Time.Add(anim.Substring(7), clip.length);
                    }
                    else
                    {
                        Debug.LogErrorFormat("animation clip length <= 0: {0} {1}", anim, clip.length);
                    }
                }
                else
                {
                    Debug.LogErrorFormat("Can't load animation clip: {0}", anim);
                }
            }
        }

        public static void TrimScaleInfoInAnimation()
        {
            Debug.Log("trim hero anim");
            Trim("Assets/Hero");
            Debug.Log("trim monster anim");
            Trim("Assets/Monster");
            Debug.Log("trim npc anim");
            Trim("Assets/Npc");
            Debug.Log("trim pet anim");
            Trim("Assets/Pet");
        }


        private static void Trim(string dir)
        {
            foreach (var animFile in Directory.GetFiles(dir, "*.anim", SearchOption.AllDirectories))
            {
                var anim = animFile.Replace("\\", "/");
                AnimationClip clip = AssetDatabase.LoadAssetAtPath(anim, typeof(AnimationClip)) as AnimationClip;
                if (clip != null)
                {
                    if (clip.length > 0)
                    {
                        AnimationClip copiedClip = Object.Instantiate(clip);
                        copiedClip.ClearCurves();
                        int trimScale = 0;

                        foreach (var editorCurveBinding in AnimationUtility.GetCurveBindings(clip))
                        {
                            var curve = AnimationUtility.GetEditorCurve(clip, editorCurveBinding);

                            if (editorCurveBinding.propertyName.StartsWith("m_LocalScale"))
                            {
                                trimScale++;
                            }
                            else
                            {
                                copiedClip.SetCurve(editorCurveBinding.path, editorCurveBinding.type,
                                    editorCurveBinding.propertyName, curve);
                            }
                        }

                        if (trimScale > 0)
                        {
                            EditorLogger.Log("{0} trim scale count ={1}", anim, trimScale);
                            AssetDatabase.CreateAsset(copiedClip, anim);
                        }
                    }
                    else
                    {
                        Debug.LogErrorFormat("animation clip length <= 0: {0} {1}", anim, clip.length);
                    }
                }
                else
                {
                    Debug.LogErrorFormat("Can't load animation clip: {0}", anim);
                }
            }
        }
    }
}