using UnityEditor;
using UnityEngine;

namespace BuildSystem
{
    public abstract class ABMarkDupWindow : TabNodeLeafWindow
    {
        protected abstract string GetLoadCsvFile();
        protected abstract string GetTitle();

        protected override void DoInit()
        {
            titleContent = new GUIContent(GetTitle());
            var all = new SimpleAll();
            data = all;
            ABAssetinfos assetInfos = new ABAssetinfos();
            assetInfos.LoadFromCsv(GetLoadCsvFile());

            var tab = new SimpleTab(assetInfos.sumStr);
            all.tabs.Add(tab);

            foreach (var ai in assetInfos.sortedAllAssetInfos)
            {
                var nodename = ai.asset + ", cnt=" + ai.containingABs.Count + ", mem=" +
                               ToolUtils.readableSize(ai.memSize) + ", cansave=" +
                               ToolUtils.readableSize(ai.canSaveMemSize);

                var node = new SimpleNode(nodename, ai.containingABs) {attach = ai};
                tab.nodes.Add(node);
            }
        }

        protected override void OnClick(NodeData node, LeafData nullableLeaf, bool isDoubleClick)
        {
            if (nullableLeaf == null)
            {
                var sn = node as SimpleNode;
                if (sn != null)
                {
                    var di = sn.attach as ABAssetinfos.AssetInfo;
                    if (di != null)
                    {
                        var obj = AssetDatabase.LoadMainAssetAtPath(di.asset);
                        if (isDoubleClick)
                        {
                            EditorGUIUtility.PingObject(obj);
                        }
                        else
                        {
                            Selection.activeObject = obj;
                        }
                    }
                }
            }
        }
    }
}