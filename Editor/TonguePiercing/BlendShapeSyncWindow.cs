using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using nadena.dev.modular_avatar.core;
using VRC.SDK3.Avatars.Components;

namespace moe.kyre.avatartools
{
    public class BlendShapeSyncWindow : EditorWindow
    {
        private SkinnedMeshRenderer local = null;
        private SkinnedMeshRenderer reference = null;
        private Vector2 scrollPos = Vector2.zero;
        private Dictionary<string, bool> blendShapeSelections = new Dictionary<string, bool>();
        
        public static List<BlendShape> blendShapes = new List<BlendShape>();        
        
        [MenuItem("Tools/tool4tp/BlendShapeSync")]
        public static void ShowWindow()
        {
            GetWindow<BlendShapeSyncWindow>("tool4tp/BlendShapeSync");
        }
    
        private void OnGUI()
        {
            var editorReference = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("元メッシュ", (SkinnedMeshRenderer)reference, typeof(SkinnedMeshRenderer), true);
            var editorLocal = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("舌ピアスなど", (SkinnedMeshRenderer)local, typeof(SkinnedMeshRenderer), true);

            if (local == null && editorLocal != null)
            {
                local = editorLocal;
                blendShapes = BlendShapes.GetBlendShapes(local);
                blendShapeSelections.Clear();
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
                    blendShapes = BlendShapes.GetBlendShapes(local);
                    blendShapeSelections.Clear();
                }
                
                float rowHeight = EditorGUIUtility.singleLineHeight + 4f;
                float listHeight = Mathf.Min(200f, Mathf.Max(rowHeight, blendShapes.Count * rowHeight));
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(listHeight));

                foreach (var bs in blendShapes)
                {
                    EditorGUILayout.BeginHorizontal();
            
                    if (!blendShapeSelections.ContainsKey(bs.name))
                    {
                        blendShapeSelections[bs.name] = true;
                    }

                    blendShapeSelections[bs.name] = EditorGUILayout.Toggle(
                        blendShapeSelections.TryGetValue(bs.name, out var current) ? current : true,
                        GUILayout.Width(20));

                    EditorGUILayout.LabelField(bs.name);
            
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndScrollView();

                if (reference != null)
                {
                    if (GUILayout.Button("設定する"))
                    {
                        var selected = blendShapes.Where(bs => blendShapeSelections.TryGetValue(bs.name, out var state) && state).ToList();
                        if (selected.Count == 0)
                        {
                            EditorUtility.DisplayDialog("BlendShapeSync", "1 つ以上のブレンドシェイプを選択してください。", "OK");
                            return;
                        }
                        Attacher.BlendShapeSync(editorLocal, reference, selected);
                    }
                }
            }
        }
    }
}
