using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BuildSystem
{
    public class ClipPropStat
    {
        public int Count;
        public int KeySum;
    }

    public class AnimationClipInfoWindow : EditorWindow
    {
        private AnimationClip _clip;
        private string _filter = "Position";
        
        public static void Init()
        {
            GetWindow<AnimationClipInfoWindow>("Clip");
        }

        public void OnGUI()
        {
            _clip = EditorGUILayout.ObjectField("Clip", _clip, typeof (AnimationClip), false) as AnimationClip;

            if (_clip != null)
            {
                var bindings = AnimationUtility.GetCurveBindings(_clip);
                EditorGUILayout.LabelField("Curve Count=" + bindings.Length);

                var propertyCount = new Dictionary<string, ClipPropStat>();
                foreach (var binding in bindings)
                {
                    var curve = AnimationUtility.GetEditorCurve(_clip, binding);
                    ClipPropStat old;

                    if (propertyCount.TryGetValue(binding.propertyName, out old))
                    {
                        old.Count += 1;
                        old.KeySum += curve.keys.Length;
                    }
                    else
                    {
                        propertyCount[binding.propertyName] = new ClipPropStat()
                        {
                            Count = 1,
                            KeySum = curve.keys.Length
                        };
                    }
                }

                foreach (var pc in propertyCount)
                {
                    EditorGUILayout.LabelField(pc.Key + " count = " + pc.Value.Count + ", keysum = " + pc.Value.KeySum);
                }

                _filter = EditorGUILayout.TextField("Property Filter", _filter);

                if (GUILayout.Button("Log"))
                {
                    foreach (var binding in bindings)
                    {
                        var curve = AnimationUtility.GetEditorCurve(_clip, binding);
                        if (binding.propertyName.Contains(_filter))
                            EditorLogger.Log(binding.path + ", " + binding.propertyName + ", Keys: " + curve.keys.Length);
                    }
                }
            }
        }
    }
}