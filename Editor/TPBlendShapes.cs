using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using nadena.dev.modular_avatar.core;
using VRC.SDK3.Avatars.Components;

namespace moe.kyre.tool4tp
{ 
    public struct BlendShape
    {
        public string name;
        public int index;
    }
    
    public static class TPBlendShapes
    {
        public static string[] Visemes = { "sil", "pp", "ff", "th", "dd", "kk", "ch", "ss", "nn", "rr", "aa", "e", "ih", "oh", "ou" };
        
        public static List<BlendShape> GetBlendShapes(SkinnedMeshRenderer local)
        {
            var blendShapes = new List<BlendShape>();
            
            Mesh mesh = local.sharedMesh;
            if (mesh == null) return null;
            
            // for でかくのださいからもうちょい map とか使いたいけど
            for (int i = 0; i < mesh.blendShapeCount; i++)
            {
                string name = mesh.GetBlendShapeName(i);
                
                blendShapes.Add(new BlendShape
                {
                    name = name,
                    index = i,
                });
            }
            
            return blendShapes;
        }
    
        public static void RemoveBlendShapeByName(List<BlendShape> blendShapes, string name)
        {
            blendShapes.RemoveAll(x => x.name == name);
        }

        public static bool isViseme(BlendShape blendShape)
        {
            for (int i = 0; i < Visemes.Length; i++)
            {
                // VRChat SDK (com.vrchat.base/Editor/VRCSDK/Dependencies/VRChat/Components/VRCAvatarDescriptorEditor.cs) を参照
                if (blendShape.name.ToLowerInvariant() == "vrc.v_" + Visemes[i]) return true;
                if (blendShape.name.ToLowerInvariant() == "v_" + Visemes[i]) return true;
                if (blendShape.name.ToLowerInvariant() == Visemes[i]) return true;
            }
            
            return false;
        }
    }
}
