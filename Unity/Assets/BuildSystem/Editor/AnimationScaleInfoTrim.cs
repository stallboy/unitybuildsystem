using System.IO;
using UnityEditor;
using UnityEngine;

namespace BuildSystem
{
    public class AnimationScaleInfoTrim
    {
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