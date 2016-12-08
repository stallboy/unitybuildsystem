using System.Collections.Generic;
using UnityEngine;

public class TabNodeLeafTest : TabNodeLeafWindow
{
    public static void init()
    {
        GetWindow<TabNodeLeafTest>("test");
    }
    
    protected override void DoInit()
    {
        SimpleAll adata = new SimpleAll();
        var t = new SimpleTab("iamtab");
        var t2 = new SimpleTab("iamtab");
        var n = new SimpleNode("iamnode", new List<string> {"aa", "bb", "i am a leaf", "i am too"});
        var n2 = new SimpleNode("iamnode2", new List<string> { "cc", "dd", "i am a leaf", "i am too" });
        t.nodes.Add(n);
        t.nodes.Add(n2);
        t.nodes.Add(n);

        t2.nodes.Add(new SimpleNode("bbb"));

        adata.tabs.Add(t);
        adata.tabs.Add(t);
        adata.tabs.Add(t2);

        data = adata;
    }

    protected override void OnClick(NodeData node, LeafData nullableLeaf, bool isDoubleClick)
    {
        if (nullableLeaf == null)
        {
            Debug.Log("click node=" + node.GetName() + ", isDoubleClick=" + isDoubleClick);
        }
        else
        {
            Debug.Log("click leaf=" + nullableLeaf.GetName() + ", isDoubleClick=" + isDoubleClick);
        }
    }
}