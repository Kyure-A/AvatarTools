using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace moe.kyre.tool4tp
{
    public static class TPLipSyncAnim
    {
        private static void CreateClip(string rendererPath, string dirPath, List<BlendShape> visemes)
        {
            var targetPath = string.IsNullOrEmpty(rendererPath) ? "" : rendererPath;
            for (int i = 0; i < visemes.Count; i++)
            {
                AnimationClip clip = new AnimationClip();

                for (int j = 0; j < visemes.Count; j++)
                {
                    var curve = new AnimationCurve();

                    if (i == j) curve.AddKey(0f, 100f);
                    else curve.AddKey(0f, 0f);

                    clip.SetCurve(targetPath, typeof(SkinnedMeshRenderer), $"blendShape.{visemes[j].name}", curve);
                }

                AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = false;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
                
                AssetDatabase.CreateAsset(clip, Path.Combine(dirPath, $"{visemes[i].name}.anim").Replace("\\", "/"));
            }

            AssetDatabase.SaveAssets();
        }
        
        public static string Create(string rendererPath, string dirPath, List<BlendShape> visemes)
        {
            CreateClip(rendererPath, dirPath, visemes);

            string createPath = Path.Combine(dirPath, "LipSync.controller").Replace("\\", "/");
            
            AnimatorController ac = AnimatorController.CreateAnimatorControllerAtPath(createPath);
            ac.AddParameter("Viseme", AnimatorControllerParameterType.Int);
            ac.AddParameter("Voice", AnimatorControllerParameterType.Float);
            
            AnimatorStateMachine stateMachine = ac.layers[0].stateMachine;

            var canonicalVisemes = TPBlendShapes.Visemes;
            AnimatorState defaultState = null;

            for (int i = 0; i < canonicalVisemes.Length; i++)
            {
                var canonical = canonicalVisemes[i];
                var match = visemes.FirstOrDefault(v =>
                    !string.IsNullOrEmpty(v.name) &&
                    v.name.IndexOf(canonical, StringComparison.OrdinalIgnoreCase) >= 0);
                
                if (string.IsNullOrEmpty(match.name)) continue;

                string clipPath = Path.Combine(dirPath, $"{match.name}.anim").Replace("\\", "/");
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                if (!clip) continue;

                AnimatorState state = stateMachine.AddState(canonical.ToUpperInvariant());
                state.motion = clip;

                state.timeParameter = "Voice";
                state.timeParameterActive = true;
                state.writeDefaultValues = false;

                var transition = stateMachine.AddAnyStateTransition(state);
                transition.hasExitTime = false;
                transition.duration = 0f;
                transition.AddCondition(AnimatorConditionMode.Equals, i, "Viseme");

                if (canonical == "sil" || defaultState == null)
                {
                    defaultState = state;
                    stateMachine.defaultState = defaultState;
                }
            }

            AssetDatabase.SaveAssets();

            return createPath; 
        }
    }
}
