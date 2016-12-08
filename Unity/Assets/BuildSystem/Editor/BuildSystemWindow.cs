using System;
using System.Diagnostics;
using System.IO;
using Tools;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace BuildSystem
{
    public class BuildSystemWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private int tabIndex;

        [MenuItem("Assets/资源系统", false, 202)]
        public static void Init()
        {
            GetWindow<BuildSystemWindow>("资源系统");
        }

        public void OnGUI()
        {
            tabIndex = ToolUtils.Tabs(new[] {"美术", "策划", "程序", "其他"}, tabIndex, true, 70, true, 30, 18);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.BeginVertical();
            if (tabIndex == 0)
            {
                artist();
            }
            else if (tabIndex == 1)
            {
                designer();
            }
            else if (tabIndex == 2)
            {
                coder();
            }
            else
            {
                other();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        private void artist()
        {
            btn("增加特效后，记得填特效时间，sfxtime.csv", opensfxtime);
            btn("Export NavMesh to mesh", ObjFromNavMesh.Export);
            btn("Split and Export NavMesh to mesh", ObjFromNavMesh.SplitAndExport);
            btn("ObjFix 下obj文件修正", ObjFix.Fix);

            btn("导出选中的mesh到obj文件，附带子mesh", ObjFromMeshFilter.ExportWithSubmesh);
            btn("导出选中mesh到obj文件，不带子mesh", ObjFromMeshFilter.ExportWithoutSubmesh);

            btn("动画名字检测", AnimationNameChecker.Check);
            btn("渲染摄像机", RenderImage.Init);
        }


        private void designer()
        {
            btn("检测动画控制器", AnimatorControllerGenerator.checkAnimatorControllers);
            btn("生成动画控制器", AnimatorControllerGenerator.generateAnimatorControllers, false);

            btn("生成资源列表，assets.csv", ABMarkByRule.DoTheMarkAndSaveToCsv);

            btn("精简动画，去掉scale信息", AnimationTimeExport.TrimScaleInfoInAnimation, false);
            btn("生成动画时间，animationtime.csv", AnimationTimeExport.ExportAnimationTime);

            btn("获取宠物怪物Npc等模型高度", ModelBoundingBox.GetAvatarHeight);

            btn("生成客户端配置数据和代码", gencsvcodeanddata);
            btn("生成客户端效果数据", geneffectdata);
            btn("生成客户端地图数据", genmapdata);


            btn("生成效果编辑器配置数据和代码", gencfgdataforeffectoreditor);
 
        }

        private static void opensfxtime()
        {
            runbat("../config", "sfxtime.csv");
        }

        private static void genmapdata()
        {
            runbat("../mapconfig", "生成客户端地图数据.bat");
        }

        private static void geneffectdata()
        {
            runbat("../effectconfig", "生成客户端效果数据.bat");
        }

        private static void gencfgdataforeffectoreditor()
        {
            runbat("../config", "生成效果编辑器配置数据和代码.bat");
        }

        private static void gencsvcodeanddata()
        {
            runbat("../config", "生成客户端配置数据和代码.bat");
        }
        public static void genrefassets()
        {
            runbat("../config", "生成refassets.csv.bat");
        }
        private static void runbat(string dir, string fn)
        {
            var psi = new ProcessStartInfo(fn);
            var di = new DirectoryInfo(dir);
            psi.WorkingDirectory = di.FullName;
            Process.Start(psi);
        }

        private void coder()
        {
        }

        private void other()
        {
            tip("打包By规则：");
            btn("MarkByRule", ABMarkByRule.DoTheMarkAndSaveToCsv);
            btn("检查资源重复", ABMarkByRule_DupWindow.CheckDup, false);
            btn("显示资源重复窗口", ABMarkByRule_DupWindow.ShowDupWindow);


            tip("打包By必须：");
            btn("MarkOnlyNeeded", ABMarkOnlyNeeded.DoTheMarkAndSaveTheOnlyNeededAssetsCsv);
            btn("检查资源重复", ABMarkOnlyNeeded_DupWindow.CheckDup, false);
            btn("显示资源重复窗口", ABMarkOnlyNeeded_DupWindow.ShowDupWindow);

            tip("打包：");
            btn("打包资源到AssetBundles下", ABBuild.BuildAssetBundlesForActiveTarget, false);
            btn("从AssetBundles拷到Assets下", ABBuild.CopyToAssets);

            tip("测试：");
            btn("test TabNodeLeafWindow", TabNodeLeafTest.init);
            btn("Check Sprites TagsAndBundles", SpriteChecker.CheckSpritesTagsAndBundles);
            
            tip("检测贴图：");
            btn("检测 贴图资源导入设置", TextureImportFix.checkTexture);
            btn("保存 贴图资源导入设置", TextureImportFix.saveTexture);

            tip("检查FBX：");
            
            
        }


        private void btn(string txt, Action action, bool sure = true)
        {
            GUILayout.Space(5);
            if (GUILayout.Button(txt, GUILayout.MinHeight(30)))
            {
                bool besure = sure || EditorUtility.DisplayDialog("确认", "确定要 " + txt + " 吗？", "确定", "取消");
                if (besure)
                {
                    action();
                    Debug.Log(txt + " 完成");
                }
            }
        }

        private void tip(string txt)
        {
            GUILayout.Label(txt);
        }
    }
}