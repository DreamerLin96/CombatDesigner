﻿
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

namespace CombatDesigner.EditorTool
{
    /// <summary>
    public static class ChainEditorUtilities
    {

        public static string cachedPath;
        /// <summary>
        public static void CreateNewGraph(ActorModel model)
        {
            cachedPath = cachedPath == null ?  Application.dataPath: cachedPath;
            string graphPath = EditorUtility.SaveFilePanelInProject("Create Graph", "NewGraph", "", "Please enter a file name to save", cachedPath);
            if (string.IsNullOrWhiteSpace(graphPath))
                return;
            cachedPath = PathUtilities.GetLeafDirectories(graphPath).ToString();

            ChainGraph graph = ScriptableObject.CreateInstance<ChainGraph>();    // create a scriptableObject
            if (graph == null)
            {
                EditorUtility.DisplayDialog("Node Message", "Unable to create a new graph", "Ok");
            }

            if (!graphPath.Contains(".asset"))
            {
                graphPath += ".asset";
            }

            graph.InitGraph(model);

            AssetDatabase.CreateAsset(graph, graphPath); // create the asset
            AssetDatabase.SaveAssets();// save the asset
            AssetDatabase.Refresh();// after saving refresh to update

            ChainEditorWindow win = EditorWindow.GetWindow<ChainEditorWindow>();
            if (win != null)
            {
                win.graph = graph;
            }

            Vector2 rootNodePos = new Vector2(win._workView.viewRect.width / 2 - 50, win._workView.viewRect.height / 2 + 50);

            CreateNode(graph, NodeType.RootBehaviorNode, rootNodePos);
        }

        static int FindLastIndex(string str, char x)
        {
            int index = -1;
            for (int i = 0; i < str.Length; i++)
                if (str[i] == x)
                    index = i;
            return index;
        }

        /// <summary>
        public static void LoadGraph()
        {
            string graphPath = EditorUtility.OpenFilePanel("Load Graph", cachedPath, "asset");
            if (string.IsNullOrWhiteSpace(graphPath))
                return;

            int appPathLen = Application.dataPath.Length;
            string finalPath = graphPath.Substring(appPathLen - 6);

            ChainGraph graph = AssetDatabase.LoadAssetAtPath(finalPath, typeof(ChainGraph)) as ChainGraph;
            if (graph != null)
            {
                ChainEditorWindow win = EditorWindow.GetWindow<ChainEditorWindow>();
                if (win != null)
                {
                    win.graph = graph;
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Node Message", "Unable to load selected graph!", "OK");
            }
        }

        /// <summary>
        public static void UnloadGraph()
        {
            ChainEditorWindow win = EditorWindow.GetWindow<ChainEditorWindow>();
            if (win != null)
            {
                win.graph = null;
            }
        }

        /// <summary>
        public static void CreateNode(ChainGraph graph, NodeType nodeType, Vector2 pos)
        {
            if (graph != null)
            {
                ChainBehaviorNode node = null;
                switch (nodeType)
                {
                    case NodeType.ChainBehaviorNode:
                        node = ScriptableObject.CreateInstance<ChainBehaviorNode>();
                        node.nodeName = "Chain Behavior";
                        break;
                    case NodeType.RootBehaviorNode:
                        node = ScriptableObject.CreateInstance<RootBehaviorNode>();
                        node.nodeName = "Root Behavior";
                        break;
                    default:
                        break;
                }

                if (node != null)
                {
                    node.InitNode(graph);
                    node.nodeRect.x = pos.x;
                    node.nodeRect.y = pos.y;
                    node.parentGraph = graph;
                    graph.nodes.Add(node);
                    UpdateChainBehaviorNodeID(graph);

                    node.name = node.nodeName;
                    AssetDatabase.AddObjectToAsset(node, graph);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }

        public static void UpdateGraphToModel(ChainGraph graph)
        {
            if (EditorUtility.DisplayDialog("Confirm Dialogue", "Do you really want to update this graph data to the " + graph.model.name + " ActorModel data?", "Yes", "No"))
            {
                graph.model.chainBehaviors.Clear();
                for (int i = 0; i < graph.nodes.Count; i++)
                {
                    graph.model.chainBehaviors.Add(new ChainBehavior(graph.nodes[i].id, graph.nodes[i].behavior, graph.nodes[i].followUps, graph.nodes[i].priority));
                }

                EditorUtility.SetDirty(graph.model);
                Debug.Log("Saved Chains Successfully");
            }
        }

        /// <summary>
        public static void DeleteNode(int nodeIndex, ChainGraph graph)
        {
            if (graph != null)
            {
                if (graph.nodes.Count >= nodeIndex)
                {
                    ChainBehaviorNode deleteNode = (ChainBehaviorNode)graph.nodes[nodeIndex];
                    if (deleteNode != null)
                    {
                        RemoveNodeAndCleanUp(graph, deleteNode);
                        UpdateChainBehaviorNodeID(graph);
                        UpdateFollowUps(graph);
                        UnityEngine.Object.DestroyImmediate(deleteNode, true);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }
            }
        }












        /// <summary>
        {
            foreach (ChainBehaviorNode n in graph.nodes)
            {
                if (n.output.nodes != null)
                {
                    if (n.output.nodes.Contains(deleteNode))
                    {
                        n.output.nodes.Remove(deleteNode);
                    }
                }
            }
            graph.nodes.Remove(deleteNode);

        }

        /// <summary>
        static void UpdateChainBehaviorNodeID(ChainGraph graph)
        {
            for (int i = 0; i < graph.nodes.Count; i++)
            {
                graph.nodes[i].id = i;
            }
        }

        /// <summary>
        public static void UpdateFollowUps(ChainGraph graph)
        {
            foreach (ChainBehaviorNode n in graph.nodes)
            {
                if (n.output.nodes != null)
                {
                    //ReorderByPriority(n);
                    n.followUps.Clear();
                    for (int i = 0; i < n.output.nodes.Count; i++)
                    {
                        n.followUps.Add(n.output.nodes[i].id);
                    }
                }
            }
        }

        /// <summary>
        public static void ReorderByPriority(ChainBehaviorNode n)
        {
            n.output.nodes = n.output.nodes.OrderBy(o => o.priority).ToList();
        }

        /// <summary>
        public static void DrawGrid(Rect viewRect, float gridSpacing, float gridOpacity, Color gridColor)
        {
            int widthDivs = Mathf.CeilToInt(viewRect.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(viewRect.height / gridSpacing);
            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            for (int x = 0; x < widthDivs; x++)
            {
                Handles.DrawLine(new Vector3(gridSpacing * x, 0, 0), new Vector3(gridSpacing * x, viewRect.height, 0));
            }
            for (int y = 0; y < heightDivs; y++)
            {
                Handles.DrawLine(new Vector3(0, gridSpacing * y, 0), new Vector3(viewRect.width, gridSpacing * y, 0));
            }
            Handles.color = Color.white;
            Handles.EndGUI();
        }

        /// <summary>
        public static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }


        const float k_EditorWindowTabHeight = 20.0f;
        static Matrix4x4 s_prevGuiMatrix;

        /// <summary>
        public static Rect BeginZoomArea(float zoomScale, Rect screenCoordsArea)
        {
            GUI.EndGroup();        // End the group Unity begins automatically for an EditorWindow to clip out the window tab. This allows us to draw outside of the size of the EditorWindow.

            Rect clippedArea = screenCoordsArea.ScaleSizeBy(1.0f / zoomScale, screenCoordsArea.TopLeft());
            clippedArea.y += k_EditorWindowTabHeight;
            GUI.BeginGroup(clippedArea);

            s_prevGuiMatrix = GUI.matrix;
            //translates the clip area's top-left corner to the origin
            Matrix4x4 translation = Matrix4x4.TRS(clippedArea.TopLeft(), Quaternion.identity, Vector3.one);
            //scaling around the origin
            Matrix4x4 scale = Matrix4x4.Scale(new Vector3(zoomScale, zoomScale, 1.0f));
            //apply and translates the zoomed result back to where the clip area is supposed to be
            GUI.matrix = translation * scale * translation.inverse * GUI.matrix;

            return clippedArea;
        }

        /// <summary>
        public static void EndZoomArea()
        {
            GUI.matrix = s_prevGuiMatrix;
            GUI.EndGroup();
            GUI.BeginGroup(new Rect(0.0f, k_EditorWindowTabHeight, Screen.width, Screen.height));
        }

        /// <summary>
        public static Vector2 ConvertScreenCoordsToZoomCoords(Vector2 screenCoords, Rect zoomRect, float zoomScale, Vector2 zoomPivot)
        {
            return (screenCoords - zoomRect.TopLeft()) / zoomScale + zoomPivot;
        }



        #region Debug
        public static void RepaintInspector(System.Type t)
        {
            Editor[] ed = (Editor[])Resources.FindObjectsOfTypeAll<Editor>();
            for (int i = 0; i < ed.Length; i++)
            {
                if (ed[i].GetType() == t)
                {
                    ed[i].Repaint();
                    return;
                }
            }
        }
        #endregion
    }
}

#endif