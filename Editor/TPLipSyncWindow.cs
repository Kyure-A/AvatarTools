using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using nadena.dev.modular_avatar.core;
using VRC.SDK3.Avatars.Components;

namespace moe.kyre.tool4tp
{
    public class TPLipSyncWindow : EditorWindow
    {
        private VRCAvatarDescriptor avatar = null;
        private SkinnedMeshRenderer local = null;
        private Vector2 scrollPos = Vector2.zero;
        private Dictionary<string, bool> visemeSelections = new Dictionary<string, bool>();
        public static List<BlendShape> blendShapes = new List<BlendShape>();
        
        [MenuItem("Tools/tool4tp/LipSync")]
        public static void ShowWindow()
        {
            GetWindow<TPLipSyncWindow>("tool4tp/LipSync");
        }

        private void OnGUI()
        {
            var avatarLocal = (VRCAvatarDescriptor)EditorGUILayout.ObjectField("Avatar", (VRCAvatarDescriptor)avatar, typeof(VRCAvatarDescriptor), true);
            var editorLocal = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("舌ピアスなど", (SkinnedMeshRenderer)local, typeof(SkinnedMeshRenderer), true);
            
            if (avatarLocal != null && avatar == null) avatar = avatarLocal;
            if (editorLocal != null && local == null) local = editorLocal;
            
            if (local != null)
            {
                if (local != editorLocal)
                {
                    local = editorLocal;
                    visemeSelections.Clear();
                }
                
                blendShapes = TPBlendShapes.GetBlendShapes(local);

                var visemes = blendShapes.Where(s => TPBlendShapes.isViseme(s)).ToList();

                foreach (var bs in visemes)
                {
                    if (!visemeSelections.ContainsKey(bs.name))
                    {
                        visemeSelections[bs.name] = true;
                    }
                }
                
                float rowHeight = EditorGUIUtility.singleLineHeight + 4f;
                float listHeight = Mathf.Min(200f, Mathf.Max(rowHeight, visemes.Count * rowHeight));
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(listHeight));
                foreach (var bs in visemes)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    visemeSelections[bs.name] = EditorGUILayout.Toggle(
                        visemeSelections.TryGetValue(bs.name, out var current) ? current : true,
                        GUILayout.Width(20));
                    
                    EditorGUILayout.LabelField(bs.name);
                    
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();

                if (GUILayout.Button("LipSync を生成する"))
                {
                    var selectedVisemes = visemes.Where(v => visemeSelections.TryGetValue(v.name, out var state) && state).ToList();
                    if (selectedVisemes.Count == 0)
                    {
                        EditorUtility.DisplayDialog("LipSync", "1 つ以上の Viseme を選択してください。", "OK");
                        return;
                    }

                    string fullPath = EditorUtility.SaveFolderPanel("フォルダを選択", "", "");
                    string path = "Assets" + fullPath.Replace(Application.dataPath, "");
                    
                    if (!string.IsNullOrEmpty(path))
                    {
                        string controllerPath = TPLipSyncAnim.Create(string.Empty, path, selectedVisemes);
                        var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
                        if (!controller) return;

                        var mergeAnimator = local.GetComponent<ModularAvatarMergeAnimator>();
                        if (!mergeAnimator)
                        {
                            mergeAnimator = local.gameObject.AddComponent<ModularAvatarMergeAnimator>();
                        }

                        mergeAnimator.animator = controller;
                        mergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
                        mergeAnimator.pathMode = MergeAnimatorPathMode.Relative;
                        mergeAnimator.relativePathRoot = new AvatarObjectReference();
                        mergeAnimator.mergeAnimatorMode = MergeAnimatorMode.Append;
                        mergeAnimator.deleteAttachedAnimator = true;
                        EditorUtility.SetDirty(mergeAnimator);
                    }
                }
            }
        }
    }
}
