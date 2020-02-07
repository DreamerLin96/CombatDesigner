﻿
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// deprecated
/// </summary>

namespace CombatDesigner.EditorTool
{
    public class ChainEditorPopupWindow : EditorWindow
    {
        static ChainEditorPopupWindow popup;
        public ActorModel model;
        public static void Open()
        {
            popup = GetWindow<ChainEditorPopupWindow>(true, "Node Popup");
        }

        private void OnGUI()
        {
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.BeginVertical();
            EditorGUILayout.LabelField("Create New Graph:", EditorStyles.boldLabel);
            //name = EditorGUILayout.TextField("Enter Name: ", name);
            model = (ActorModel)EditorGUILayout.ObjectField("Model", model, typeof(ActorModel), false);

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Create Graph", GUILayout.Height(40)))
            {
                if (model != null)
                {
                    ChainEditorUtilities.CreateNewGraph(model);
                    popup.Close();
                }
                else
                {
                    EditorUtility.DisplayDialog("Node Message:", "Please select a valid ActorModel!", "OK");
                }
            }
            if (GUILayout.Button("Cancel", GUILayout.Height(40)))
            {
                popup.Close();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.Space(20);
            GUILayout.EndHorizontal();
            GUILayout.Space(20);
        }

    }
}
#endif