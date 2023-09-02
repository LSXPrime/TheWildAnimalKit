using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LWOS
{
    public class RuntimeMaterialChanger : MonoBehaviour
    {
        public SkinnedMeshRenderer meshRenderer;
        public SkinData[] Skins;
        public bool AI;

        public bool isDone;

        void Start()
        {
            meshRenderer = GetComponent<SkinnedMeshRenderer>();
        }
        
        void Update()
        {
            if (isDone)
                enabled = false;
            
            int MaterialIndex = AI ? Random.Range(0, Skins.Length) : LocalPrefs.GetInt("PlayerSkin");
            meshRenderer.sharedMaterials = GetSkin(MaterialIndex).Materials;
            isDone = true;
        }

        public SkinData GetSkin(int MaterialID)
        {
            foreach (SkinData skin in Skins)
            {
                if (skin.SkinID == MaterialID)
                    return skin;
            }

            return Skins[MaterialID];
        }

        [System.Serializable]
        public class SkinData
        {
            public int SkinID;
            public string SkinName;
            public Material[] Materials;
        }
    }
}
