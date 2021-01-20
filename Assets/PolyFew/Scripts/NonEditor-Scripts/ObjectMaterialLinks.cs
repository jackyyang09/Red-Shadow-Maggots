#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;


namespace BrainFailProductions.PolyFew
{

    [ExecuteInEditMode]
    public class ObjectMaterialLinks : MonoBehaviour
    {
        public List<MaterialCombiner.CombiningInformation.MaterialEntity> linkedMaterialEntities;

        public Texture2D linkedAttrImg;

        void Start()
        {
            var mr = GetComponent<MeshRenderer>();
            var smr = GetComponent<SkinnedMeshRenderer>();
            Material[] materials;

            if (mr != null)
            {
                materials = mr.sharedMaterials;

                if (materials != null && materials.Length > 0)
                {
                    bool isFeasible = false;

                    foreach (var mat in materials)
                    {
                        if (mat == null) { continue; }

                        string shaderName = mat.shader.name.ToLower();

                        if (shaderName == "batchfewstandard" || shaderName == "batchfewstandardspecular")
                        {
                            isFeasible = true;
                            break;
                        }
                    }

                    if (!isFeasible)
                    {
                        DestroyImmediate(this);
                    }
                }

                else
                {
                    DestroyImmediate(this);
                }
            }

            else if (smr != null)
            {
                materials = smr.sharedMaterials;

                if (materials != null && materials.Length > 0)
                {
                    bool isFeasible = false;

                    foreach (var mat in materials)
                    {
                        if (mat == null) { continue; }

                        string shaderName = mat.shader.name.ToLower();

                        if (shaderName == "batchfewstandard" || shaderName == "batchfewstandardspecular")
                        {
                            isFeasible = true;
                            break;
                        }
                    }

                    if (!isFeasible)
                    {
                        DestroyImmediate(this);
                    }
                }

                else
                {
                    DestroyImmediate(this);
                }
            }

            else
            {
                DestroyImmediate(this);
            }
        }
    }

}

#endif