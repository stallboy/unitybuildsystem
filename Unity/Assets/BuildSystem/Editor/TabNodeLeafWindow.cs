using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;


public abstract class TabNodeLeafWindow : EditorWindow
{
    public abstract class LeafData
    {
        public abstract string GetName();

        public virtual bool Hit(string keyword)
        {
            return GetName().Contains(keyword);
        }
    }

    public class SimpleLeaf : LeafData
    {
        public string name;

        public SimpleLeaf(string name)
        {
            this.name = name;
        }

        public override string GetName()
        {
            return name;
        }
    }

    public abstract class NodeData
    {
        public bool expand = true;
        public abstract string GetName();

        public virtual bool Hit(string keyword)
        {
            return GetName().Contains(keyword);
        }

        public bool IsChildrenHit(string keyword)
        {
            for (int i = 0; i < GetLeafCount(); i++)
            {
                if (GetLeaf(i).Hit(keyword))
                {
                    return true;
                }
            }
            return false;
        }

        public abstract int GetLeafCount();
        public abstract LeafData GetLeaf(int idx);
    }

    public class SimpleNode : NodeData
    {
        public string name;
        public readonly List<LeafData> leafs = new List<LeafData>();
        public object attach;

        public SimpleNode(string name)
        {
            this.name = name;
        }

        public SimpleNode(string name, ICollection<string> leafs)
        {
            this.name = name;
            foreach (var leaf in leafs)
            {
                this.leafs.Add(new SimpleLeaf(leaf));
            }
        }

        public override string GetName()
        {
            return name;
        }

        public override int GetLeafCount()
        {
            return leafs.Count;
        }

        public override LeafData GetLeaf(int idx)
        {
            return leafs[idx];
        }
    }

    public abstract class TabData
    {
        public abstract string GetName();

        public abstract int GetNodeCount();
        public abstract NodeData GetNode(int idx);
    }

    public class SimpleTab : TabData
    {
        public string name;
        public readonly List<NodeData> nodes = new List<NodeData>();

        public SimpleTab(string name)
        {
            this.name = name;
        }

        public override string GetName()
        {
            return name;
        }

        public override int GetNodeCount()
        {
            return nodes.Count;
        }

        public override NodeData GetNode(int idx)
        {
            return nodes[idx];
        }
    }


    public abstract class AllData
    {
        public abstract int GetTabCount();
        public abstract TabData GetTab(int idx);
    }

    public class SimpleAll : AllData
    {
        public readonly List<TabData> tabs = new List<TabData>();

        public override int GetTabCount()
        {
            return tabs.Count;
        }

        public override TabData GetTab(int idx)
        {
            return tabs[idx];
        }
    }

    protected int selectedTabIndex;
    protected string searchKeyword = "";
    private Vector2 scrollPos;
    private int viewCount;

    protected NodeData selectedNodeData;
    protected LeafData selectedLeafData;

    private readonly Stopwatch watcher = new Stopwatch();

    public AllData data;

    public void OnEnable()
    {
        DoInit();
    }

    protected abstract void DoInit();

    protected abstract void OnClick(NodeData node, LeafData nullableLeaf, bool isDoubleClick);


    protected virtual void OnCmd(string cmd)
    {
    }

    protected void OnGUI()
    {
        if (data == null)
        {
            return;
        }

        //TAB
        EditorGUILayout.BeginHorizontal();
        var tabnames = new List<string>();
        for (int i = 0; i < data.GetTabCount(); i++)
        {
            tabnames.Add(data.GetTab(i).GetName());
        }
        if (tabnames.Count == 0)
        {
            return;
        }

        if (selectedTabIndex >= tabnames.Count)
        {
            selectedTabIndex = 0;
        }
        selectedTabIndex = ToolUtils.Tabs(tabnames.ToArray(), selectedTabIndex, false, 50, false);
        searchKeyword = EditorGUILayout.TextField("", searchKeyword, GUILayout.MinWidth(100));
        bool isCmd = searchKeyword.StartsWith("!");
        if (isCmd)
        {
            var cmd = searchKeyword.Substring(1);
            if (cmd.Equals("+"))
            {
                var thisTab = data.GetTab(selectedTabIndex);
                for (int j = 0; j < thisTab.GetNodeCount(); j++)
                {
                    var node = thisTab.GetNode(j);
                    node.expand = true;
                }
            }
            else if (cmd.Equals("-"))
            {
                var thisTab = data.GetTab(selectedTabIndex);
                for (int j = 0; j < thisTab.GetNodeCount(); j++)
                {
                    var node = thisTab.GetNode(j);
                    node.expand = false;
                }
            }
            else
            {
                OnCmd(cmd);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();


        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        EditorGUILayout.BeginVertical();

        //每行占18像素,Tab行也占18像素高度
        int firstIndex = (int) (scrollPos.y/18);
        int excludeLastIndex = firstIndex + viewCount;
        GUILayout.Space(firstIndex*18);

        int index = 0;
        var tab = data.GetTab(selectedTabIndex);
        for (int j = 0; j < tab.GetNodeCount(); j++)
        {
            var node = tab.GetNode(j);
            bool nodeHit = isCmd || node.Hit(searchKeyword);
            if (nodeHit || node.IsChildrenHit(searchKeyword))
            {
                index++;

                if (index > firstIndex && index < excludeLastIndex)
                {
                    var foldpos = GUILayoutUtility.GetRect(18, 18);
                    foldpos.width = 18;
                    var style = selectedNodeData == node ? EditorStyles.whiteLabel : EditorStyles.label;

                    node.expand = EditorGUI.Foldout(foldpos, node.expand, "");
                    var btnpos = new Rect(foldpos.x + foldpos.width, foldpos.y, position.width - foldpos.width,
                        foldpos.height);

                    if (GUI.Button(btnpos, node.GetName(), style))
                    {
                        OnClick(node, null, false);
                        var delta = watcher.ElapsedMilliseconds;
                        if (delta < 500 && selectedNodeData == node)
                        {
                            OnClick(node, null, true);
                        }
                        selectedNodeData = node;
                        selectedLeafData = null;
                        watcher.Reset();
                        watcher.Start();
                    }
                }

                if (node.expand)
                {
                    for (int k = 0; k < node.GetLeafCount(); k++)
                    {
                        var leaf = node.GetLeaf(k);

                        if (nodeHit || leaf.Hit(searchKeyword))
                        {
                            index++;

                            if (index > firstIndex && index < excludeLastIndex)
                            {
                                var estyle = selectedLeafData == leaf ? EditorStyles.whiteLabel : EditorStyles.label;

                                if (GUILayout.Button("    " + leaf.GetName(), estyle, GUILayout.Height(18)))
                                {
                                    OnClick(node, leaf, false);
                                    var delta = watcher.ElapsedMilliseconds;
                                    if (delta < 500 && selectedLeafData == leaf)
                                    {
                                        OnClick(node, leaf, true);
                                    }
                                    selectedNodeData = null;
                                    selectedLeafData = leaf;
                                    watcher.Reset();
                                    watcher.Start();
                                }
                            }
                        }
                    }
                }
            }
        }

        var space = (index - viewCount - firstIndex)*18;
        if (space < 0)
        {
            space = 0;
        }
        GUILayout.Space(space);

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();

        if (Event.current.type == EventType.repaint)
        {
            var viewSize = GUILayoutUtility.GetLastRect();
            int newViewcount = (int) (viewSize.height/18) + 1;
            viewCount = newViewcount;
        }
    }
}