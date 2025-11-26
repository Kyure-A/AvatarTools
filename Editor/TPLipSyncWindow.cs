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
        public static List<BlendShape> blendShapes = new List<BlendShape>();
        
        [MenuItem("Tools/tool4tp/LipSync")]
        public static void ShowWindow()
        {
            GetWindow<TPLipSyncWindow>("tool4tp/LipSync");
        }

        private string GetRelativePath(Transform target, Transform root)
        {
            if (target == root)
                return "";
            var parentPath = GetRelativePath(target.parent, root);
            return string.IsNullOrEmpty(parentPath) ? target.name : $"{parentPath}/{target.name}";
        }
        
        private void OnGUI()
        {
            var avatarLocal = (VRCAvatarDescriptor)EditorGUILayout.ObjectField("Avatar", (VRCAvatarDescriptor)avatar, typeof(VRCAvatarDescriptor), true);
            var editorLocal = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("舌ピアスなど", (SkinnedMeshRenderer)local, typeof(SkinnedMeshRenderer), true);
            
            if (avatarLocal != null && avatar == null) avatar = avatarLocal;
            if (editorLocal != null && local == null) local = editorLocal;
            
            if (local != null)
            {
                blendShapes = TPBlendShapes.GetBlendShapes(local);

                var visemes = blendShapes.Where(s => TPBlendShapes.isViseme(s)).ToList();
                
                foreach (var bs in visemes)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    bool state = EditorGUILayout.Toggle(true, GUILayout.Width(20));
                    
                    EditorGUILayout.LabelField(bs.name);
                    
                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("LipSync を生成する"))
                {
                    string fullPath = EditorUtility.SaveFolderPanel("フォルダを選択", "", "");
                    string path = "Assets" + fullPath.Replace(Application.dataPath, "");
                    
                    if (!string.IsNullOrEmpty(path))
                    {
                        string rendererPath = GetRelativePath(editorLocal.transform, avatarLocal.transform);
                        string controllerPath = TPLipSyncAnim.Create(rendererPath, path, visemes);
                        var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
                        if (!controller) return;

                        var mergeAnimator = local.GetComponent<ModularAvatarMergeAnimator>();
                        if (!mergeAnimator)
                        {
                            mergeAnimator = local.gameObject.AddComponent<ModularAvatarMergeAnimator>();
                        }

                        mergeAnimator.animator = controller;
                        mergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
                        mergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
                        mergeAnimator.mergeAnimatorMode = MergeAnimatorMode.Append;
                        mergeAnimator.deleteAttachedAnimator = true;
                        EditorUtility.SetDirty(mergeAnimator);
                    }
                }
            }
        }
    }
}
