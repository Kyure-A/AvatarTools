using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using nadena.dev.modular_avatar.core;
using VRC.SDK3.Avatars.Components;

namespace moe.kyre.tool4tp
{
    public class TPBlendShapeSyncWindow : EditorWindow
    {
        private SkinnedMeshRenderer local = null;
        private SkinnedMeshRenderer reference = null;
        private Vector2 scrollPos = Vector2.zero;
        
        public static List<BlendShape> blendShapes = new List<BlendShape>();        
        
        [MenuItem("Tools/tool4tp/BlendShapeSync")]
        public static void ShowWindow()
        {
            GetWindow<TPBlendShapeSyncWindow>("tool4tp/BlendShapeSync");
        }
    
        private void OnGUI()
        {
            var editorLocal = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("舌ピアスなど", (SkinnedMeshRenderer)local, typeof(SkinnedMeshRenderer), true);
            var editorReference = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("元メッシュ", (SkinnedMeshRenderer)reference, typeof(SkinnedMeshRenderer), true);

            if (local == null && editorLocal != null)
            {
                local = editorLocal;
                blendShapes = TPBlendShapes.GetBlendShapes(local);
            }

            if (reference == null && editorReference != null)
            {
                reference = editorReference;
            }
            
            if (local != null)
            {
                if (local != editorLocal)
                {
                    local = editorLocal;
                    blendShapes = TPBlendShapes.GetBlendShapes(local);
                }
                
                float rowHeight = EditorGUIUtility.singleLineHeight + 4f;
                float listHeight = Mathf.Min(200f, Mathf.Max(rowHeight, blendShapes.Count * rowHeight));
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(listHeight));

                var toRemove = new List<string>();
                
                foreach (var bs in blendShapes)
                {
                    EditorGUILayout.BeginHorizontal();
            
                    bool state = EditorGUILayout.Toggle(true, GUILayout.Width(20));
            
                    if (!state)
                    {
                        toRemove.Add(bs.name);
                        EditorGUILayout.EndHorizontal();
                        continue;
                    }

                    EditorGUILayout.LabelField(bs.name);
            
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndScrollView();

                foreach (var bs in toRemove) TPBlendShapes.RemoveBlendShapeByName(blendShapes, bs);
                
                if (reference != null)
                {
                    if (GUILayout.Button("設定する"))
                    {
                        TPAttacher.BlendShapeSync(editorLocal, reference);
                    }
                }
            }
        }
    }
}
