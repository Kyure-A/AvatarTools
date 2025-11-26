using System;
using System.Linq;
using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using nadena.dev.modular_avatar.core;
using VRC.SDK3.Avatars.Components;

namespace moe.kyre.avatartools
{
    public static class Attacher
    {
        public static void BoneProxy (GameObject obj)
        {
            return;
        }

        public static void BlendShapeSync (SkinnedMeshRenderer local, SkinnedMeshRenderer reference, List<BlendShape> selectedBlendShapes = null)
        {
            GameObject avatar = nadena.dev.ndmf.runtime.RuntimeUtil.FindAvatarInParents(reference.gameObject.transform).gameObject;
            string relativePath = nadena.dev.ndmf.runtime.RuntimeUtil.RelativePath(avatar, reference.gameObject);
            
            var source = selectedBlendShapes ?? BlendShapeSyncWindow.blendShapes;
            
            List<BlendshapeBinding> bindings = source
                .Select(blendshape => new BlendshapeBinding
                {
                    ReferenceMesh = new AvatarObjectReference
                    {
                        referencePath = relativePath
                    },
                    Blendshape = blendshape.name,
                    LocalBlendshape = blendshape.name
                }).ToList();

            var component = local.gameObject.AddComponent<ModularAvatarBlendshapeSync>();

            if (component != null)
            {
                component.Bindings = bindings;
            }

            return;
        }

        public static void MergeAnimator (GameObject obj)
        {
            return;
        }
    }
}
