//////////////////////////////////////////////////////
// Copyright (c) BrainFailProductions
//////////////////////////////////////////////////////

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using static BrainFailProductions.PolyFew.MaterialCombiner.CombiningInformation;


namespace BrainFailProductions.PolyFew
{


    public class MaterialCombiner : MonoBehaviour
    {
        private static CombiningInformation staticCombinationInfo;

        #region DATA_STRUCTURES

        public class CombiningInformation
        {

            #region DATA_STRUCTURES

            public enum DiffuseColorSpace
            {
                NON_LINEAR,
                LINEAR
            }

            public enum CompressionType
            {
                UNCOMPRESSED,
                DXT1,
                ETC2_RGB,
                PVRTC_RGB4,
                ASTC_RGB,
                DXT1_CRUNCHED,
            }

            public enum CompressionQuality
            {
                LOW,
                MEDIUM,
                HIGH
            }

            [System.Serializable]
            public struct Resolution
            {
                public int width;
                public int height;
            }

            [System.Serializable]
            public class TextureArrayUserSettings
            {
                public Resolution resolution;
                public FilterMode filteringMode;
                public CompressionType compressionType;
                public CompressionQuality compressionQuality;
                public int anisotropicFilteringLevel;

                #region BATCH FEW INSPECTOR VARS
                public int choiceResolutionW = 4;
                public int choiceResolutionH = 4;
                public int choiceFilteringMode = 0;
                public int choiceCompressionQuality = 1;
                public int choiceCompressionType = 0;
                #endregion BATCH FEW INSPECTOR VARS

                public TextureArrayUserSettings(Resolution resolution, FilterMode filteringMode, CompressionType compressionType, CompressionQuality compressionQuality = CompressionQuality.MEDIUM, int anisotropicFilteringLevel = 1)
                {
                    this.resolution = resolution;
                    this.filteringMode = filteringMode;
                    this.compressionType = compressionType;
                    this.compressionQuality = compressionQuality;
                    this.anisotropicFilteringLevel = anisotropicFilteringLevel;
                }

            }

            [System.Serializable]
            public class TextureArrayGroup
            {
                public TextureArrayUserSettings diffuseArraySettings;
                public TextureArrayUserSettings metallicArraySettings;
                public TextureArrayUserSettings specularArraySettings;
                public TextureArrayUserSettings normalArraySettings;
                public TextureArrayUserSettings heightArraySettings;
                public TextureArrayUserSettings occlusionArraySettings;
                public TextureArrayUserSettings emissiveArraySettings;
                public TextureArrayUserSettings detailMaskArraySettings;
                public TextureArrayUserSettings detailAlbedoArraySettings;
                public TextureArrayUserSettings detailNormalArraySettings;

                public void InitializeDefaultArraySettings(Resolution resolution, FilterMode filteringMode, CompressionType compressionType, CompressionQuality compressionQuality = CompressionQuality.MEDIUM, int anisotropicFilteringLevel = 1)
                {
                    diffuseArraySettings = new TextureArrayUserSettings(resolution, filteringMode, compressionType, compressionQuality, anisotropicFilteringLevel);
                    metallicArraySettings = new TextureArrayUserSettings(resolution, filteringMode, compressionType, compressionQuality, anisotropicFilteringLevel);
                    specularArraySettings = new TextureArrayUserSettings(resolution, filteringMode, compressionType, compressionQuality, anisotropicFilteringLevel);
                    normalArraySettings = new TextureArrayUserSettings(resolution, filteringMode, compressionType, compressionQuality, anisotropicFilteringLevel);
                    heightArraySettings = new TextureArrayUserSettings(resolution, filteringMode, compressionType, compressionQuality, anisotropicFilteringLevel);
                    occlusionArraySettings = new TextureArrayUserSettings(resolution, filteringMode, compressionType, compressionQuality, anisotropicFilteringLevel);
                    emissiveArraySettings = new TextureArrayUserSettings(resolution, filteringMode, compressionType, compressionQuality, anisotropicFilteringLevel);
                    detailMaskArraySettings = new TextureArrayUserSettings(resolution, filteringMode, compressionType, compressionQuality, anisotropicFilteringLevel);
                    detailAlbedoArraySettings = new TextureArrayUserSettings(resolution, filteringMode, compressionType, compressionQuality, anisotropicFilteringLevel);
                    detailNormalArraySettings = new TextureArrayUserSettings(resolution, filteringMode, compressionType, compressionQuality, anisotropicFilteringLevel);
                }
            }

            [System.Serializable]
            public class MaterialProperties
            {
                // BATCHFEW UI
                public bool foldOut = true;
                public int texArrIndex;
                public int matIndex;

                // BATCHFEW UI

                public string materialName;
                public Material originalMaterial;
                public Color albedoTint;
                public Vector4 uvTileOffset = new Vector4(1, 1, 0, 0);
                public float normalIntensity = 1;
                public float occlusionIntensity = 1;
                public float smoothnessIntensity = 1;
                public float glossMapScale = 1;
                public float metalIntensity = 1;
                public Color emissionColor = Color.black;
                public Vector4 detailUVTileOffset = new Vector4(1, 1, 0, 0);
                public float alphaCutoff = 0.5f;
                public Color specularColor = Color.black;
                public float detailNormalScale = 1;
                public float heightIntensity = 0.05f;
                public float uvSec = 0;
                public int alphaMode = 0;
                public bool specularWorkflow = false;

                #region PUBLIC_METHODS

                public bool IsSameAs(MaterialProperties toCompare)
                {
                    if (originalMaterial == toCompare.originalMaterial) { return true; }

                    if (toCompare.albedoTint != albedoTint) { return false; }

                    if (toCompare.normalIntensity != normalIntensity) { return false; }

                    if (toCompare.occlusionIntensity != occlusionIntensity) { return false; }

                    if (toCompare.smoothnessIntensity != smoothnessIntensity) { return false; }

                    if (toCompare.glossMapScale != glossMapScale) { return false; }

                    if (toCompare.uvTileOffset != uvTileOffset) { return false; }

                    if (toCompare.metalIntensity != metalIntensity) { return false; }

                    if (toCompare.emissionColor != emissionColor) { return false; }

                    if (toCompare.detailUVTileOffset != detailUVTileOffset) { return false; }

                    if (toCompare.alphaCutoff != alphaCutoff) { return false; }

                    if (toCompare.specularColor != specularColor) { return false; }

                    if (toCompare.detailNormalScale != detailNormalScale) { return false; }

                    if (toCompare.heightIntensity != heightIntensity) { return false; }

                    if (toCompare.uvSec != uvSec) { return false; }

                    if (toCompare.alphaMode != alphaMode) { return false; }


                    return true;

                }

                public static Texture2D NewTexture()
                {
                    Texture2D tex = new Texture2D(8, 4, TextureFormat.RGBAHalf, false, true);

                    for (int x = 0; x < 8; ++x)
                    {
                        for (int y = 0; y < 4; ++y)
                        {
                            tex.SetPixel(x, y, Color.black);
                        }
                    }

                    tex.Apply();

                    return tex;
                }

                public void BurnAttrToImg(ref Texture2D burnOn, int index, int textureArrayIndex)
                {
                    if (index >= burnOn.height)
                    {
                        var t = new Texture2D(burnOn.width, index + 1, TextureFormat.RGBAHalf, false, true);
                        Color[] colors = burnOn.GetPixels();
                        t.SetPixels(0, 0, burnOn.width, burnOn.height, colors);
                        burnOn = t;
                    }

                    if (burnOn.width < 8)
                    {
                        var t = new Texture2D(8, burnOn.height, TextureFormat.RGBAHalf, false, true);

                        Color[] colors = burnOn.GetPixels();
                        t.SetPixels(0, 0, burnOn.width, burnOn.height, colors);
                        burnOn = t;
                    }

                    burnOn.SetPixel(0, index, (new Color(uvTileOffset.x - 1, uvTileOffset.y - 1, uvTileOffset.z, uvTileOffset.w)));
                    burnOn.SetPixel(1, index, (new Color(normalIntensity, occlusionIntensity, smoothnessIntensity, metalIntensity)));
                    burnOn.SetPixel(2, index, albedoTint);
                    burnOn.SetPixel(3, index, emissionColor);
                    burnOn.SetPixel(4, index, new Color(specularColor.r, specularColor.g, specularColor.b, glossMapScale));
                    burnOn.SetPixel(5, index, (new Color(detailUVTileOffset.x, detailUVTileOffset.y, detailUVTileOffset.z, detailUVTileOffset.w)));
                    burnOn.SetPixel(6, index, (new Color(alphaCutoff, detailNormalScale, heightIntensity, uvSec)));
                    burnOn.SetPixel(7, index, (new Color(textureArrayIndex, textureArrayIndex, textureArrayIndex, textureArrayIndex)));

                    burnOn.Apply();
                }

                public void FillPropertiesFromMaterial(Material material, CombiningInformation combineInfo)
                {
                    materialName = material.name;
                    originalMaterial = material;
                    normalIntensity = 1;
                    occlusionIntensity = 1;
                    smoothnessIntensity = 1;
                    albedoTint = Color.white;
                    metalIntensity = 1;
                    uvTileOffset = new Vector4(1, 1, 0, 0);
                    detailUVTileOffset = new Vector4(1, 1, 0, 0);
                    emissionColor = Color.black;
                    alphaCutoff = 0.5f;
                    specularColor = Color.black;
                    detailNormalScale = 1;
                    heightIntensity = 0.05f;
                    alphaMode = 0;
                    glossMapScale = 0;

                    if (material.shader.name.ToLower() == "standard (specular setup)")
                    {
                        specularWorkflow = true;
                    }

                    if (material.HasProperty("_Color"))
                    {
                        albedoTint = material.GetColor("_Color");
                    }

                    if (material.HasProperty("_MainTex") && material.HasProperty("_MainTex_ST"))
                    {
                        uvTileOffset = material.GetVector("_MainTex_ST");
                    }

                    if (material.HasProperty("_GlossMapScale"))
                    {
                        glossMapScale = material.GetFloat("_GlossMapScale");
                    }

                    if (material.HasProperty("_Glossiness"))
                    {
                        smoothnessIntensity = material.GetFloat("_Glossiness");
                    }

                    if (material.HasProperty("_Smoothness"))
                    {
                        smoothnessIntensity = material.GetFloat("_Smoothness");
                    }

                    if (material.HasProperty("_MetallicGlossMap") && material.GetTexture("_MetallicGlossMap") != null)
                    {
                        smoothnessIntensity = glossMapScale;
                    }

                    if (material.HasProperty("_SpecColor"))
                    {
                        specularColor = material.GetColor("_SpecColor");
                    }

                    if (material.HasProperty("_Metallic"))
                    {
                        metalIntensity = material.GetFloat("_Metallic");
                    }

                    if (material.HasProperty("_OcclusionStrength"))
                    {
                        occlusionIntensity = material.GetFloat("_OcclusionStrength") + 1f;
                    }

                    if (material.HasProperty("_BumpScale"))
                    {
                        normalIntensity = material.GetFloat("_BumpScale");
                    }

                    if (material.HasProperty("_DetailNormalMapScale"))
                    {
                        detailNormalScale = material.GetFloat("_DetailNormalMapScale");
                    }

                    if (material.HasProperty("_EmissionColor") && material.HasProperty("_EmissionMap") && combineInfo.ShouldGenerateEmissionArray())
                    {
                        emissionColor = Color.black;
                    }

                    else if (material.HasProperty("_EmissionColor"))
                    {
                        emissionColor = material.GetColor("_EmissionColor");
                    }

                    if (material.HasProperty("_Parallax"))
                    {
                        heightIntensity = material.GetFloat("_Parallax");
                    }

                    if (material.HasProperty("_UVSec"))
                    {
                        uvSec = material.GetFloat("_UVSec");
                    }

                    if (material.HasProperty("_DetailAlbedoMap") && material.HasProperty("_DetailAlbedoMap_ST"))
                    {
                        detailUVTileOffset = material.GetVector("_DetailAlbedoMap_ST");
                    }

                    if (material.HasProperty("_Mode"))
                    {
                        alphaMode = (int)material.GetFloat("_Mode");
                    }
                }

                #endregion PUBLIC_METHODS

            }

            [System.Serializable]
            public class MeshData
            {
                public List<MeshFilter> meshFilters;
                public List<MeshRenderer> meshRenderers;
                public List<SkinnedMeshRenderer> skinnedMeshRenderers;
                public Material[] originalMaterials;
                public Mesh[] outputMeshes;
                public Matrix4x4[] outputMatrices;
            }

            [System.Serializable]
            public class CombineMetaData
            {
                public Material material;
                public MaterialProperties materialProperties;

                //For BatchFew UI
                public MaterialProperties tempMaterialProperties;
                //For BatchFew UI

                public List<MeshData> meshesData = new List<MeshData>();
            }

            [System.Serializable]
            public class MaterialEntity
            {
                public List<CombineMetaData> combinedMats = new List<CombineMetaData>();

                // FOR BATCHFEW UI
                public int textArrIndex;
                // FOR BATCHFEW UI


                #region DATA_STRUCTURES
                public Texture2D diffuseMap;
                public Texture2D metallicMap;
                public Texture2D specularMap;
                public Texture2D normalMap;
                public Texture2D heightMap;
                public Texture2D occlusionMap;
                public Texture2D emissionMap;
                public Texture2D detailMaskMap;
                public Texture2D detailAlbedoMap;
                public Texture2D detailNormalMap;
                #endregion DATA_STRUCTURES

                #region PUBLIC_METHODS

                public bool HasAnyTextures()
                {
                    return
                        (
                        diffuseMap != null ||
                        heightMap != null ||
                        normalMap != null ||
                        metallicMap != null ||
                        detailAlbedoMap != null ||
                        detailNormalMap != null ||
                        detailMaskMap != null ||
                        emissionMap != null ||
                        specularMap != null ||
                        occlusionMap != null
                        );
                }

                #endregion PUBLIC_METHODS

            }

            #endregion DATA_STRUCTURES


            #region PUBLIC_METHODS

            public bool ShouldGenerateMetallicArray()
            {
                foreach (var s in materialEntities)
                {
                    if (s.metallicMap != null)
                        return true;
                }
                return false;
            }

            public bool ShouldGenerateSpecularArray()
            {
                foreach (MaterialEntity texture in materialEntities)
                {
                    if (texture.specularMap != null) { return true; }
                }
                return false;
            }

            public bool ShouldGenerateNormalArray()
            {
                foreach (MaterialEntity texture in materialEntities)
                {
                    if (texture.normalMap != null) { return true; }
                }
                return false;
            }

            public bool ShouldGenerateHeightArray()
            {
                foreach (MaterialEntity texture in materialEntities)
                {
                    if (texture.heightMap != null) { return true; }
                }
                return false;
            }

            public bool ShouldGenerateOcclusionArray()
            {
                foreach (MaterialEntity texture in materialEntities)
                {
                    if (texture.occlusionMap != null) { return true; }
                }
                return false;
            }

            public bool ShouldGenerateEmissionArray()
            {
                foreach (MaterialEntity texture in materialEntities)
                {
                    if (texture.emissionMap != null) { return true; }
                }
                return false;
            }

            public bool ShouldGenerateDetailMaskArray()
            {
                foreach (MaterialEntity texture in materialEntities)
                {
                    if (texture.detailMaskMap != null) { return true; }
                }
                return false;
            }

            public bool ShouldGenerateDetailAlbedoArray()
            {
                foreach (MaterialEntity texture in materialEntities)
                {
                    if (texture.detailAlbedoMap != null) { return true; }
                }
                return false;
            }

            public bool ShouldGenerateDetailNormalArray()
            {
                foreach (MaterialEntity texture in materialEntities)
                {
                    if (texture.detailNormalMap != null) { return true; }
                }
                return false;
            }

            #endregion PUBLIC_METHODS


            public List<MaterialEntity> materialEntities = new List<MaterialEntity>();

            public TextureArrayGroup textureArraysSettings = new TextureArrayGroup();

            public DiffuseColorSpace diffuseColorSpace;

            public Material[] combinedMaterials;

        }

        private class Vertices
        {
            List<Vector3> verts = null;
            List<Vector2> uv1 = null;
            List<Vector2> uv2 = null;
            List<Vector2> uv3 = null;
            List<Vector2> uv4 = null;
            List<Vector3> normals = null;
            List<Vector4> tangents = null;
            List<Color32> colors = null;
            List<BoneWeight> boneWeights = null;

            public Vertices()
            {
                verts = new List<Vector3>();
            }

            public Vertices(Mesh aMesh)
            {
                verts = CreateList(aMesh.vertices);

                uv1 = CreateList(aMesh.uv);
                uv2 = CreateList(aMesh.uv2);
                uv3 = CreateList(aMesh.uv3);
                uv4 = CreateList(aMesh.uv4);
                normals = CreateList(aMesh.normals);
                tangents = CreateList(aMesh.tangents);
                colors = CreateList(aMesh.colors32);
                boneWeights = CreateList(aMesh.boneWeights);
            }

            private List<T> CreateList<T>(T[] verticesToList)
            {
                if (verticesToList == null || verticesToList.Length == 0) { return null; }

                return new List<T>(verticesToList);
            }

            private void Copy<T>(ref List<T> destination, List<T> source, int index)
            {
                if (source == null) { return; }

                if (destination == null) { destination = new List<T>(); }

                destination.Add(source[index]);
            }

            public int Add(Vertices other, int index)
            {
                int a = verts.Count;

                Copy(ref verts, other.verts, index);
                Copy(ref uv1, other.uv1, index);
                Copy(ref uv2, other.uv2, index);
                Copy(ref uv3, other.uv3, index);
                Copy(ref uv4, other.uv4, index);
                Copy(ref normals, other.normals, index);
                Copy(ref tangents, other.tangents, index);
                Copy(ref colors, other.colors, index);
                Copy(ref boneWeights, other.boneWeights, index);

                return a;
            }

            public void AssignTo(Mesh target)
            {

                target.SetVertices(verts);

                if (uv1 != null) target.SetUVs(0, uv1);
                if (uv2 != null) target.SetUVs(1, uv2);
                if (uv3 != null) target.SetUVs(2, uv3);
                if (uv4 != null) target.SetUVs(3, uv4);
                if (normals != null) target.SetNormals(normals);
                if (tangents != null) target.SetTangents(tangents);
                if (colors != null) target.SetColors(colors);
                if (boneWeights != null) target.boneWeights = boneWeights.ToArray();

            }
        }

        class CombineKey
        {
            public Material[] materials;
            public Mesh mesh;
            public Mesh finalMesh;
            public MeshFilter targetFilter;
            public SkinnedMeshRenderer targetSkin;
        }

        public class NonfeasibleMaterial
        {
            public bool isOnMeshRenderer = true;
            public int submeshIndex;
            public Renderer renderer;
            public Material material;
            public GameObject gameObject;

            public NonfeasibleMaterial(bool isOnMeshRenderer, int submeshIndex, Renderer renderer, Material material, GameObject gameObject)
            {
                this.isOnMeshRenderer = isOnMeshRenderer;
                this.submeshIndex = submeshIndex;
                this.renderer = renderer;
                this.material = material;
                this.gameObject = gameObject;
            }

            public NonfeasibleMaterial() { }

        }

        private static HashSet<NonfeasibleMaterial> nonFeasibleMaterials;

        #endregion DATA_STRUCTURES


        #region PUBLIC_METHODS

        private static Dictionary<GameObject, List<MaterialEntity>> InitializeFromGameObjects(List<GameObject> gameObjectsList, TextureArrayGroup textureArraysSettings, DiffuseColorSpace diffuseColorSpace, bool regardChildren, Action<string> OnError)
        {

            staticCombinationInfo = new CombiningInformation();
            staticCombinationInfo.textureArraysSettings = textureArraysSettings;
            staticCombinationInfo.diffuseColorSpace = diffuseColorSpace;
            staticCombinationInfo.materialEntities.Clear();
            int textureArraysToGenerate = 0;
            var objectsMaterialLinks = new Dictionary<GameObject, List<MaterialEntity>>();
            nonFeasibleMaterials = new HashSet<NonfeasibleMaterial>();
            HashSet<Material> tempAdded = new HashSet<Material>();


            foreach (GameObject gameObject in gameObjectsList)
            {
                if (gameObject == null)
                {
                    continue;
                }

                Renderer[] renderers = null;

                if (regardChildren)
                {
                    renderers = gameObject.GetComponentsInChildren<Renderer>(true);
                }

                else
                {
                    renderers = new Renderer[] { gameObject.GetComponent<Renderer>() };
                }

                foreach (Renderer renderer in renderers)
                {

                    MeshRenderer meshRenderer = renderer as MeshRenderer;

                    SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;

                    if (meshRenderer == null && skinnedMeshRenderer == null) { continue; }

                    MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();

                    if ((skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh == null) || (meshFilter != null && meshFilter.sharedMesh == null))
                    {
                        Debug.LogWarning("Mesh Filter on GameObject " + gameObject.name + " does not have a mesh and was skipped");
                        continue;
                    }

                    int submeshIndex = -1;

                    // Has Submeshes
                    if (renderer.sharedMaterials != null && renderer.sharedMaterials.Length > 1)
                    {
                        bool skipGameObject = true;

                        foreach (var mat in renderer.sharedMaterials)
                        {
                            submeshIndex++;

                            if (mat == null)
                            {

                                NonfeasibleMaterial nfm = new NonfeasibleMaterial
                                {
                                    isOnMeshRenderer = renderer is MeshRenderer ? true : false,
                                    submeshIndex = submeshIndex,
                                    renderer = renderer,
                                    material = mat,
                                    gameObject = renderer.gameObject
                                };

                                nonFeasibleMaterials.Add(nfm);

                                Material[] s = new Material[renderer.sharedMaterials.Length];

                                Array.Copy(renderer.sharedMaterials, s, s.Length);

                                int a = 0;
                                foreach (var item in s)
                                {
                                    if (item == null)
                                    {
                                        s[a] = new Material(Shader.Find("Unlit/Color"));
                                        s[a].color = Color.black;
                                        tempAdded.Add(s[a]);
                                    }
                                    a++;
                                }
                                renderer.sharedMaterials = s;

                            }

                            else { skipGameObject = false; }
                        }

                        if (skipGameObject)
                        {
                            Debug.LogWarning("Renderer on GameObject " + gameObject.name + " does not have any material associated with any of the submeshes and was skipped");
                            continue;
                        }
                    }

                    else if ((meshRenderer != null && meshRenderer.sharedMaterial == null) || (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMaterial == null))
                    {
                        Debug.LogWarning("Renderer on GameObject " + gameObject.name + " does not have a material and was skipped");
                        continue;
                    }

                    Material[] materials = meshRenderer != null ? meshRenderer.sharedMaterials : skinnedMeshRenderer.sharedMaterials;


                    submeshIndex = -1;

                    foreach (Material material in materials)
                    {
                        submeshIndex++;

                        if (material == null) { continue; }

                        string shaderName = material.shader.name.ToLower();

                        if (shaderName != "standard" && shaderName != "standard (specular setup)")
                        {
                            // Might have submeshes
                            if (renderer.sharedMaterials.Length > 1)
                            {
                                NonfeasibleMaterial nfm = new NonfeasibleMaterial
                                {
                                    isOnMeshRenderer = renderer is MeshRenderer ? true : false,
                                    submeshIndex = submeshIndex,
                                    renderer = renderer,
                                    material = material,
                                    gameObject = renderer.gameObject
                                };

                                nonFeasibleMaterials.Add(nfm);
                            }

                            // Don't log warning for forcefully added nonfeasible materials
                            if (!tempAdded.Contains(material))
                            {
                                Debug.LogWarning($"The material \"{material.name}\" on GameObject \"{renderer.gameObject.name}\" has a Non Standard Shader \"{material.shader.name}\". The material won't be combined.");
                            }

                            continue;
                        }


                        bool hasTint = false;

                        if ((shaderName == "standard" || shaderName == "standard (specular setup)" ) && material.mainTexture == null && renderer.sharedMaterials.Length == 1)
                        {
                            hasTint = true;
                            Color albedoColor = material.color;
                            Texture2D texture = new Texture2D(1, 1);
                            texture.SetPixels(new Color[] { albedoColor });
                            material.mainTexture = texture;
                            texture.Apply(false);
                        }
                       


                        MaterialEntity materialEntity = null;
                        Texture2D mainTexture = material.mainTexture as Texture2D;

                        if (mainTexture == null)
                        {
                            mainTexture = material.GetTexture("_MainTex") as Texture2D;
                        }
                        if (mainTexture != null)
                        {
                            materialEntity = GetMaterialEntityWithDiffuseMap(staticCombinationInfo, mainTexture);
                        }

                        if (materialEntity == null)
                        {
                            materialEntity = new MaterialEntity();
                            materialEntity.diffuseMap = material.mainTexture as Texture2D;
                            staticCombinationInfo.materialEntities.Add(materialEntity);
                        }

                        if (materialEntity.combinedMats == null)
                        {
                            materialEntity.combinedMats = new List<CombineMetaData>();
                        }

                        Mesh sharedMesh = meshFilter != null ? meshFilter.sharedMesh : skinnedMeshRenderer.sharedMesh;

                        var materialProperties = new MaterialProperties();
                        materialProperties.FillPropertiesFromMaterial(material, staticCombinationInfo);

                        AddCombine(sharedMesh, materialProperties, materialEntity, staticCombinationInfo, meshFilter, meshRenderer, skinnedMeshRenderer);

                        SetTextureFromMaterial(ref materialEntity.diffuseMap, material, "_Diffuse");
                        SetTextureFromMaterial(ref materialEntity.diffuseMap, material, "_Albedo");
                        SetTextureFromMaterial(ref materialEntity.diffuseMap, material, "_DiffuseTex");
                        SetTextureFromMaterial(ref materialEntity.diffuseMap, material, "_AlbedoTex");
                        SetTextureFromMaterial(ref materialEntity.specularMap, material, "_SpecGlossMap");
                        SetTextureFromMaterial(ref materialEntity.normalMap, material, "_BumpMap");
                        SetTextureFromMaterial(ref materialEntity.normalMap, material, "_Bump");
                        SetTextureFromMaterial(ref materialEntity.normalMap, material, "_NormalMap");
                        SetTextureFromMaterial(ref materialEntity.normalMap, material, "_Normal");
                        SetTextureFromMaterial(ref materialEntity.metallicMap, material, "_MetallicGlossMap");
                        SetTextureFromMaterial(ref materialEntity.occlusionMap, material, "_OcclusionMap");
                        SetTextureFromMaterial(ref materialEntity.occlusionMap, material, "_AO");
                        SetTextureFromMaterial(ref materialEntity.occlusionMap, material, "_AOMap");
                        SetTextureFromMaterial(ref materialEntity.heightMap, material, "_ParallaxMap");
                        SetTextureFromMaterial(ref materialEntity.heightMap, material, "_HeightMap");
                        SetTextureFromMaterial(ref materialEntity.heightMap, material, "_Height");
                        SetTextureFromMaterial(ref materialEntity.detailAlbedoMap, material, "_DetailAlbedoMap");
                        SetTextureFromMaterial(ref materialEntity.detailNormalMap, material, "_DetailNormalMap");
                        SetTextureFromMaterial(ref materialEntity.detailMaskMap, material, "_DetailMask");
                        SetTextureFromMaterial(ref materialEntity.emissionMap, material, "_EmissiveMap");



                        if (!hasTint && !materialEntity.HasAnyTextures())
                        {
                            // Might have submeshes
                            if (renderer.sharedMaterials.Length > 1)
                            {
                                NonfeasibleMaterial nfm = new NonfeasibleMaterial
                                {
                                    isOnMeshRenderer = renderer is MeshRenderer ? true : false,
                                    submeshIndex = submeshIndex,
                                    renderer = renderer,
                                    material = material,
                                    gameObject = renderer.gameObject
                                };

                                nonFeasibleMaterials.Add(nfm);
                            }

                            Debug.LogWarning($"The material \"{material.name}\" on GameObject \"{renderer.gameObject.name}\" has no textures. The material won't be combined.");
                            continue;
                        }

                        if (objectsMaterialLinks.ContainsKey(renderer.gameObject))
                        {
                            var entititesLinked = objectsMaterialLinks[renderer.gameObject];
                            entititesLinked.Add(materialEntity);
                        }
                        else
                        {
                            var entititesLinked = new List<MaterialEntity>();
                            entititesLinked.Add(materialEntity);
                            objectsMaterialLinks.Add(renderer.gameObject, entititesLinked);
                        }

                        //AddCombine(sharedMesh, materialProperties, materialEntity, staticCombinationInfo, meshFilter, meshRenderer, skinnedMeshRenderer);

                    }
                }
            }

            for (int a = 0; a < staticCombinationInfo.materialEntities.Count; ++a)
            {
                var materialEntity = staticCombinationInfo.materialEntities[a];

                if (!materialEntity.HasAnyTextures())
                {
                    staticCombinationInfo.materialEntities.RemoveAt(a);
                    a--;
                }
                else { textureArraysToGenerate++; }
            }

            if (textureArraysToGenerate <= 1)
            {
                string error = "Failed to combine materials. Not enough feasible materials to combine. You might want to check the \"Consider Children\" option to take nested children into account. If you are trying to combine materials that are not using StandardShaders, then this is not supported";
                OnError?.Invoke(error);
                return null;
            }

            return objectsMaterialLinks;
        }


        public static bool CombineMaterials(GameObject[] gameObjectsList, string savePath, TextureArrayGroup textureArraysSettings, DiffuseColorSpace diffuseColorSpace, bool regardChildren, bool displayErrorBox, Action<string> OnError)
        {

            try
            {

                #region FIX SAVE PATH

                if (string.IsNullOrWhiteSpace(savePath)) { savePath = UtilityServices.BATCHFEW_ASSETS_DEFAULT_SAVE_PATH; }

                string originalSavePath = savePath;

                char last = savePath[savePath.Length - 1];

                if (last == '/') { savePath = savePath.Remove(savePath.Length - 1, 1); }
                if (last == '\\') { savePath = savePath.Remove(savePath.Length - 1, 1); }

                if (!AssetDatabase.IsValidFolder(savePath))
                {
                    savePath = UtilityServices.BATCHFEW_ASSETS_DEFAULT_SAVE_PATH;

                    last = savePath[savePath.Length - 1];

                    if (last == '/') { savePath = savePath.Remove(savePath.Length - 1, 1); }
                    if (last == '\\') { savePath = savePath.Remove(savePath.Length - 1, 1); }

                    //"Assets/BATCHFEW_COMBINED_ASSETS";
                    // The folder at default path doesn't exist so create it
                    if (!AssetDatabase.IsValidFolder(savePath))
                    {
                        string folderName = savePath.Substring(savePath.LastIndexOf("/") + 1);
                        string parentPath = savePath.Split(new string[] { "/" + folderName }, StringSplitOptions.None)[0];
                        AssetDatabase.CreateFolder(parentPath, folderName);
                        AssetDatabase.CreateFolder(savePath, "BatchFew_CombinedStuff");
                        savePath += "/BatchFew_CombinedStuff";
                    }

                    //"Assets/BATCHFEW_COMBINED_ASSETS";
                    // The folder at default path exists so create a unique sub folder
                    else
                    {
                        savePath += "/BatchFew_CombinedStuff";
                        savePath = AssetDatabase.GenerateUniqueAssetPath(savePath);
                        string folderName = savePath.Substring(savePath.LastIndexOf("/") + 1);
                        string parentPath = savePath.Split(new string[] { "/" + folderName }, StringSplitOptions.None)[0];
                        AssetDatabase.CreateFolder(parentPath, folderName);
                    }

                    Debug.LogWarning($"The save path: \"{originalSavePath}\" is not valid or does not exist. A default path \"{savePath}\" will be used to save the combined assets.");

                    savePath += "/";

                }

                else
                {
                    last = savePath[savePath.Length - 1];

                    if (last == '/') { savePath = savePath.Remove(savePath.Length - 1, 1); }
                    if (last == '\\') { savePath = savePath.Remove(savePath.Length - 1, 1); }

                    savePath = AssetDatabase.GenerateUniqueAssetPath(savePath + "/BatchFew_CombinedStuff");

                    string uniqueName = savePath.Substring(savePath.LastIndexOf("/") + 1);
                    string parentPath = savePath.Split(new string[] { "/" + uniqueName }, StringSplitOptions.None)[0];
                    AssetDatabase.CreateFolder(parentPath, uniqueName);
                    savePath += "/";
                }

                #endregion FIX SAVE PATH


                #region PRE CHECKS

                if (gameObjectsList == null)
                {
                    string error = $"Failed to combine materials. " + new System.ArgumentNullException(nameof(gameObjectsList)).Message;

                    OnError?.Invoke(error);

                    if (displayErrorBox)
                    {
                        EditorUtility.DisplayDialog("Operation Failed", error, "Ok");
                    }

                    return false;
                }

                if (gameObjectsList.Length == 0)
                {
                    string error = "Failed to combine materials. The provided GameObjects list is empty";

                    OnError?.Invoke(error);

                    if (displayErrorBox)
                    {
                        EditorUtility.DisplayDialog("Operation Failed", error, "Ok");
                    }

                    return false;
                }

                HashSet<Transform> allSelected = new HashSet<Transform>();
                List<GameObject> feasibleSelections = new List<GameObject>();

                if (regardChildren)
                {
                    foreach (var selection in gameObjectsList)
                    {
                        if (selection != null) { allSelected.Add(selection.transform); }
                    }

                    for (int a = 0; a < gameObjectsList.Length; a++)
                    {
                        Transform selectedObject = gameObjectsList[a].transform;

                        Transform topSelectedParent = UtilityServices.GetTopLevelSelectedParent(selectedObject, allSelected);

                        if (!topSelectedParent.Equals(selectedObject))
                        {
                            gameObjectsList[a] = null;
                        }
                    }
                }


                foreach (var item in gameObjectsList)
                {
                    if (item != null) { feasibleSelections.Add(item); }
                }


                if (feasibleSelections.Count == 0)
                {
                    string error = $"Failed to combine materials. The provided GameObjects list is empty";
                    OnError?.Invoke(error);

                    return false;
                }

                #endregion PRE CHECKS


                bool didFail = false;

                var objectsMaterialLinks = InitializeFromGameObjects(feasibleSelections, textureArraysSettings, diffuseColorSpace, regardChildren, (string error) =>
                {
                    OnError(error);

                    if (displayErrorBox)
                    {
                        EditorUtility.DisplayDialog("Operation Failed", error, "Ok");
                    }

                    didFail = true;
                });


                if (didFail)
                {
                    RestoreNonFeasibleMats();
                    return false;
                }

                CombiningInformation objectsData = staticCombinationInfo;
                List<MaterialEntity> srcMatEntitites = objectsData.materialEntities;


                if (objectsData == null || objectsData.materialEntities == null || objectsData.materialEntities.Count == 0)
                {
                    string error = $"Failed to combine materials. Please check console for any clues.";

                    OnError?.Invoke(error);

                    if (displayErrorBox)
                    {
                        EditorUtility.DisplayDialog("Operation Failed", error, "Ok");
                    }

                    RestoreNonFeasibleMats();
                    return false;
                }


                objectsData.combinedMaterials = null;

                var settings = objectsData.textureArraysSettings;
#pragma warning disable
                HashSet<string> meshNames = new HashSet<string>();

                int textureArraysCount = srcMatEntitites.Count;
                if (textureArraysCount < 1) { textureArraysCount = 1; }


                bool diffuseHasAlpha = DiffuseMapHasAlpha(objectsData);


                bool createNormalArr = objectsData.ShouldGenerateNormalArray();
                bool createOcclusionArr = objectsData.ShouldGenerateOcclusionArray();
                bool createMetalArr = objectsData.ShouldGenerateMetallicArray();
                bool createDetailMaskArr = objectsData.ShouldGenerateDetailMaskArray();
                bool createDetailAlbedoArr = objectsData.ShouldGenerateDetailAlbedoArray();
                bool createDetailNormalArr = objectsData.ShouldGenerateDetailNormalArray();
                bool createEmissionArr = objectsData.ShouldGenerateEmissionArray();
                bool createHeightArr = objectsData.ShouldGenerateHeightArray();
                bool createSpecularArr = objectsData.ShouldGenerateSpecularArray();

                Texture2DArray diffuseArray = AllocateArray(settings.diffuseArraySettings, textureArraysCount, objectsData.diffuseColorSpace, diffuseHasAlpha);
                Texture2DArray normalArray = createNormalArr ? AllocateArray(settings.normalArraySettings, textureArraysCount, DiffuseColorSpace.LINEAR, true) : null;
                Texture2DArray metallicArray = createMetalArr ? AllocateArray(settings.metallicArraySettings, textureArraysCount, DiffuseColorSpace.LINEAR, true) : null;
                Texture2DArray occlusionArray = createOcclusionArr ? AllocateArray(settings.occlusionArraySettings, textureArraysCount, DiffuseColorSpace.LINEAR, false) : null;
                Texture2DArray heightArray = createHeightArr ? AllocateArray(settings.heightArraySettings, textureArraysCount, DiffuseColorSpace.LINEAR, false) : null;
                Texture2DArray emisArray = createEmissionArr ? AllocateArray(settings.emissiveArraySettings, textureArraysCount, objectsData.diffuseColorSpace, false) : null;
                Texture2DArray detailAlbedoArray = createDetailAlbedoArr ? AllocateArray(settings.detailAlbedoArraySettings, textureArraysCount, objectsData.diffuseColorSpace, false) : null;
                Texture2DArray detailNormalArray = createDetailNormalArr ? AllocateArray(settings.detailNormalArraySettings, textureArraysCount, DiffuseColorSpace.LINEAR, true) : null;
                Texture2DArray detailMaskArray = createDetailMaskArr ? AllocateArray(settings.detailMaskArraySettings, textureArraysCount, DiffuseColorSpace.LINEAR, false) : null;
                Texture2DArray specularArray = createSpecularArr ? AllocateArray(settings.specularArraySettings, textureArraysCount, DiffuseColorSpace.LINEAR, true) : null;


                Texture2D blankBump = new Texture2D(1, 1, TextureFormat.RGBA32, true, true);
                blankBump.SetPixel(0, 0, new Color(0, 0.5f, 0, 0.5f));
                blankBump.Apply();

                Texture2D blankGrey = new Texture2D(1, 1, TextureFormat.RGBA32, true, true);
                blankBump.SetPixel(0, 0, new Color(0.5f, 0.5f, 0.5f, 1.0f));
                blankBump.Apply();

                Texture2D attrImg = MaterialProperties.NewTexture();

                bool isSpecularShader = false;

                for (int a = 0; a < srcMatEntitites.Count; ++a)
                {
                    try
                    {

                        EditorUtility.DisplayProgressBar("Combining Materials", $"Preparing Source Materials {a + 1}/{srcMatEntitites.Count}", (float)(a + 1) / srcMatEntitites.Count);


                        MaterialEntity arrayToGenerate = srcMatEntitites[a];
                        Texture2D diffuse = arrayToGenerate.diffuseMap;

                        if (diffuse == null)
                        {
                            diffuse = Texture2D.whiteTexture;
                        }

                        Texture2D diffuseTexture = GetResizedTexture(arrayToGenerate.diffuseMap != null ? arrayToGenerate.diffuseMap : Texture2D.whiteTexture, settings.diffuseArraySettings.resolution, false);
                        Texture2D normalTexture = GetResizedTexture(arrayToGenerate.normalMap != null ? arrayToGenerate.normalMap : blankBump, settings.normalArraySettings.resolution, true);
                        Texture2D metallicTexture = GetResizedTexture(arrayToGenerate.metallicMap != null ? arrayToGenerate.metallicMap : Texture2D.blackTexture, settings.metallicArraySettings.resolution, true);
                        Texture2D occlusionTexture = GetResizedTexture(arrayToGenerate.occlusionMap != null ? arrayToGenerate.occlusionMap : Texture2D.whiteTexture, settings.occlusionArraySettings.resolution, true);
                        Texture2D heightTexture = GetResizedTexture(arrayToGenerate.heightMap != null ? arrayToGenerate.heightMap : Texture2D.blackTexture, settings.heightArraySettings.resolution, true);
                        Texture2D emissionTexture = GetResizedTexture(arrayToGenerate.emissionMap != null ? arrayToGenerate.emissionMap : Texture2D.blackTexture, settings.emissiveArraySettings.resolution, false);
                        Texture2D detailAlbedoTexture = GetResizedTexture(arrayToGenerate.detailAlbedoMap != null ? arrayToGenerate.detailAlbedoMap : blankGrey, settings.detailAlbedoArraySettings.resolution, false);
                        Texture2D detailNormalTexture = GetResizedTexture(arrayToGenerate.detailNormalMap != null ? arrayToGenerate.detailNormalMap : blankBump, settings.detailNormalArraySettings.resolution, true);
                        Texture2D detailMaskTexture = GetResizedTexture(arrayToGenerate.detailMaskMap != null ? arrayToGenerate.detailMaskMap : Texture2D.whiteTexture, settings.detailMaskArraySettings.resolution, true);
                        Texture2D specularTexture = GetResizedTexture(arrayToGenerate.specularMap != null ? arrayToGenerate.specularMap : Texture2D.blackTexture, settings.specularArraySettings.resolution, false);


                        CompressTexture(diffuseTexture, settings.diffuseArraySettings, diffuseHasAlpha);
                        CompressTexture(normalTexture, settings.normalArraySettings, true);
                        CompressTexture(metallicTexture, settings.metallicArraySettings, true);
                        CompressTexture(occlusionTexture, settings.occlusionArraySettings, false);
                        CompressTexture(heightTexture, settings.heightArraySettings, false);
                        CompressTexture(emissionTexture, settings.emissiveArraySettings, false);
                        CompressTexture(detailAlbedoTexture, settings.detailAlbedoArraySettings, false);
                        CompressTexture(detailNormalTexture, settings.detailNormalArraySettings, true);
                        CompressTexture(detailMaskTexture, settings.detailMaskArraySettings, false);
                        CompressTexture(specularTexture, settings.specularArraySettings, true);


                        WriteTextureToTextureArray(diffuseTexture, diffuseArray, a, SizeToMipCount(settings.diffuseArraySettings));
                        WriteTextureToTextureArray(normalTexture, normalArray, a, SizeToMipCount(settings.normalArraySettings));
                        WriteTextureToTextureArray(metallicTexture, metallicArray, a, SizeToMipCount(settings.metallicArraySettings));
                        WriteTextureToTextureArray(occlusionTexture, occlusionArray, a, SizeToMipCount(settings.occlusionArraySettings));
                        WriteTextureToTextureArray(heightTexture, heightArray, a, SizeToMipCount(settings.heightArraySettings));
                        WriteTextureToTextureArray(emissionTexture, emisArray, a, SizeToMipCount(settings.emissiveArraySettings));
                        WriteTextureToTextureArray(detailAlbedoTexture, detailAlbedoArray, a, SizeToMipCount(settings.detailAlbedoArraySettings));
                        WriteTextureToTextureArray(detailNormalTexture, detailNormalArray, a, SizeToMipCount(settings.detailNormalArraySettings));
                        WriteTextureToTextureArray(detailMaskTexture, detailMaskArray, a, SizeToMipCount(settings.detailMaskArraySettings));
                        WriteTextureToTextureArray(specularTexture, specularArray, a, SizeToMipCount(settings.specularArraySettings));


                        if (diffuseTexture != null) { DestroyImmediate(diffuseTexture); }
                        if (normalTexture != null) { DestroyImmediate(normalTexture); }
                        if (metallicTexture != null) { DestroyImmediate(metallicTexture); }
                        if (occlusionTexture != null) { DestroyImmediate(occlusionTexture); }
                        if (emissionTexture != null) { DestroyImmediate(emissionTexture); }
                        if (detailAlbedoTexture != null) { DestroyImmediate(detailAlbedoTexture); }
                        if (detailNormalTexture != null) { DestroyImmediate(detailNormalTexture); }
                        if (detailMaskTexture != null) { DestroyImmediate(detailMaskTexture); }
                        if (specularTexture != null) { DestroyImmediate(specularTexture); }



                        foreach (var comb in arrayToGenerate.combinedMats)
                        {
                            foreach (var meshData in comb.meshesData)
                            {
                                Mesh sourceMesh = null;
                                Material[] sourceMaterials = null;
                                Matrix4x4 sourceMatrix = Matrix4x4.identity;

                                if (meshData.meshFilters[0] != null)
                                {
                                    sourceMesh = meshData.meshFilters[0].sharedMesh;
                                    sourceMaterials = meshData.meshRenderers[0].sharedMaterials;
                                    sourceMatrix = meshData.meshFilters[0].gameObject.transform.localToWorldMatrix;
                                }

                                if (meshData.skinnedMeshRenderers[0] != null)
                                {
                                    sourceMesh = meshData.skinnedMeshRenderers[0].sharedMesh;
                                    sourceMaterials = meshData.skinnedMeshRenderers[0].sharedMaterials;
                                    sourceMatrix = meshData.skinnedMeshRenderers[0].gameObject.transform.localToWorldMatrix;
                                    //sourceMatrix = meshData.skinnedMeshRenderers[0].rootBone.worldToLocalMatrix *  meshData.skinnedMeshRenderers[0].gameObject.transform.localToWorldMatrix;
                                }

                                if (sourceMaterials == null) { continue; }


                                for (int b = 0; b < sourceMaterials.Length; ++b)
                                {
                                    Material material = sourceMaterials[b];

                                    if (material == null)
                                    {
                                        sourceMaterials[b] = new Material(Shader.Find("Standard"));
                                    }
                                    else
                                    {
                                        if (material.shader != null && material.shader.name.ToLower() == "standard (specular setup)")
                                        {
                                            isSpecularShader = true;
                                        }
                                    }
                                }

#pragma warning disable
                                Mesh[] outputMeshes = new Mesh[sourceMesh.subMeshCount];
                                Matrix4x4[] outputMatrix = new Matrix4x4[sourceMesh.subMeshCount];

                                //for (int c = 0; c < sourceMesh.subMeshCount; ++c)
                                //{
                                //    outputMeshes[c] = GetSubmesh(sourceMesh, c, meshData.skinnedMeshRenderers[0]);
                                //    outputMatrix[c] = sourceMatrix;
                                //    Color[] colors = outputMeshes[c].colors;

                                //    if (colors == null || colors.Length == 0)
                                //    {
                                //        colors = new Color[outputMeshes[c].vertexCount];
                                //    }

                                //    int textureArrayIndex = FindTexArrIndexForMaterial(sourceMaterials[c], objectsData);
                                //    int attrImgIndex = FindAttrImgIndex(sourceMaterials[c], objectsData);

                                //    for (int d = 0; d < outputMeshes[c].vertexCount; ++d)
                                //    {
                                //        Color color = colors[d];
                                //        color.r = attrImgIndex / 255.0f;
                                //        color.g = 1;
                                //        color.b = 1;
                                //        color.a = textureArrayIndex / 255.0f;
                                //        colors[d] = color;
                                //    }

                                //    outputMeshes[c].colors = colors;

                                //    foreach (var link in objectsMaterialLinks)
                                //    {
                                //        List<MaterialEntity> materialEntitiesLinked = link.Value;

                                //        foreach (var materialEntity in materialEntitiesLinked)
                                //        {
                                //            if (materialEntity.GetHashCode() == arrayToGenerate.GetHashCode())
                                //            {
                                //                foreach (var comMats in materialEntity.combinedMats)
                                //                {
                                //                    if (sourceMaterials[c] == null) { continue; }

                                //                    if (comMats.material.GetHashCode() == sourceMaterials[c].GetHashCode())
                                //                    {
                                //                        comMats.materialProperties.matIndex = attrImgIndex;
                                //                        comMats.materialProperties.texArrIndex = a;
                                //                    }
                                //                }
                                //            }
                                //        }
                                //    }


                                //    string name = sourceMesh.name;
                                //    int nameIdx = 1;

                                //    while (meshNames.Contains(name))
                                //    {
                                //        name = sourceMesh.name + "_" + nameIdx;
                                //        nameIdx++;
                                //    }

                                //    outputMeshes[c].name = name;
                                //    meshNames.Add(name);
                                //    comb.materialProperties.BurnAttrToImg(ref attrImg, attrImgIndex, a);

                                //}

                                Mesh toSave = Instantiate(sourceMesh);
                                Color[] stamped = sourceMesh.colors.Length == 0 ? new Color[sourceMesh.vertexCount] : sourceMesh.colors;


                                for (int c = 0; c < sourceMesh.subMeshCount; ++c)
                                {
                                    var vertIndices = GetSubmeshVerts(sourceMesh, c, meshData.skinnedMeshRenderers[0]);
                                    outputMatrix[c] = sourceMatrix;


                                    int textureArrayIndex = FindTexArrIndexForMaterial(sourceMaterials[c], objectsData);
                                    int attrImgIndex = FindAttrImgIndex(sourceMaterials[c], objectsData);

                                    foreach (int index in vertIndices)
                                    {
                                        Color color = stamped[index];
                                        color.r = attrImgIndex / 255.0f;
                                        color.g = 1;
                                        color.b = 1;
                                        color.a = textureArrayIndex / 255.0f;
                                        stamped[index] = color;
                                    }


                                    foreach (var link in objectsMaterialLinks)
                                    {
                                        List<MaterialEntity> materialEntitiesLinked = link.Value;

                                        foreach (var materialEntity in materialEntitiesLinked)
                                        {
                                            if (materialEntity.GetHashCode() == arrayToGenerate.GetHashCode())
                                            {
                                                foreach (var comMats in materialEntity.combinedMats)
                                                {
                                                    if (sourceMaterials[c] == null) { continue; }

                                                    if (comMats.material.GetHashCode() == sourceMaterials[c].GetHashCode())
                                                    {
                                                        comMats.materialProperties.matIndex = attrImgIndex;
                                                        comMats.materialProperties.texArrIndex = a;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    comb.materialProperties.BurnAttrToImg(ref attrImg, attrImgIndex, a);

                                }



                                toSave.name = sourceMesh.name;
                                toSave.colors = stamped;
                                meshData.outputMeshes = new Mesh[] { toSave };
                                meshData.outputMatrices = new Matrix4x4[] { sourceMatrix };

                            }
                        }

                    }
                    finally
                    {
                        //EditorUtility.ClearProgressBar();
                    }
                }



                int totalTexArray = 0;
                if (diffuseArray != null) { totalTexArray++; }
                if (normalArray != null) { totalTexArray++; }
                if (metallicArray != null) { totalTexArray++; }
                if (heightArray != null) { totalTexArray++; }
                if (occlusionArray != null) { totalTexArray++; }
                if (emisArray != null) { totalTexArray++; }
                if (detailAlbedoArray != null) { totalTexArray++; }
                if (detailNormalArray != null) { totalTexArray++; }
                if (detailMaskArray != null) { totalTexArray++; }
                if (specularArray != null) { totalTexArray++; }

                int counter = 1;

                if (diffuseArray != null)
                {
                    EditorUtility.DisplayProgressBar("Combining Materials", $"Writing Texture Arrays To Disk {counter}/{totalTexArray}", (float)(1) / totalTexArray);
                    counter++;
                }
                string path = AssetDatabase.GenerateUniqueAssetPath(savePath + "DiffuseMapsTextureArray.asset");
                WriteTextureArrayToDisk(ref diffuseArray, path);

                if (normalArray != null)
                {
                    EditorUtility.DisplayProgressBar("Combining Materials", $"Writing Texture Arrays To Disk {counter}/{totalTexArray}", (float)(2) / totalTexArray);
                    counter++;
                }
                path = AssetDatabase.GenerateUniqueAssetPath(savePath + "NormalMapsTextureArray.asset");
                WriteTextureArrayToDisk(ref normalArray, path);

                if (metallicArray != null)
                {
                    EditorUtility.DisplayProgressBar("Combining Materials", $"Writing Texture Arrays To Disk {counter}/{totalTexArray}", (float)(3) / totalTexArray);
                    counter++;
                }
                path = AssetDatabase.GenerateUniqueAssetPath(savePath + "MetallicMapsTextureArray.asset");
                WriteTextureArrayToDisk(ref metallicArray, path);

                if (heightArray != null)
                {
                    EditorUtility.DisplayProgressBar("Combining Materials", $"Writing Texture Arrays To Disk {counter}/{totalTexArray}", (float)(4) / totalTexArray);
                    counter++;
                }
                path = AssetDatabase.GenerateUniqueAssetPath(savePath + "HeightMapsTextureArray.asset");
                WriteTextureArrayToDisk(ref heightArray, path);

                if (occlusionArray != null)
                {
                    EditorUtility.DisplayProgressBar("Combining Materials", $"Writing Texture Arrays To Disk {counter}/{totalTexArray}", (float)(5) / totalTexArray);
                    counter++;
                }
                path = AssetDatabase.GenerateUniqueAssetPath(savePath + "OcclusionMapsTextureArray.asset");
                WriteTextureArrayToDisk(ref occlusionArray, path);

                if (emisArray != null)
                {
                    EditorUtility.DisplayProgressBar("Combining Materials", $"Writing Texture Arrays To Disk {counter}/{totalTexArray}", (float)(6) / totalTexArray);
                    counter++;
                }
                path = AssetDatabase.GenerateUniqueAssetPath(savePath + "EmissionMapsTextureArray.asset");
                WriteTextureArrayToDisk(ref emisArray, path);

                if (detailAlbedoArray != null)
                {
                    EditorUtility.DisplayProgressBar("Combining Materials", $"Writing Texture Arrays To Disk {counter}/{totalTexArray}", (float)(7) / totalTexArray);
                    counter++;
                }
                path = AssetDatabase.GenerateUniqueAssetPath(savePath + "DetailAlbedoMapsTextureArray.asset");
                WriteTextureArrayToDisk(ref detailAlbedoArray, path);

                if (detailNormalArray != null)
                {
                    EditorUtility.DisplayProgressBar("Combining Materials", $"Writing Texture Arrays To Disk {counter}/{totalTexArray}", (float)(8) / totalTexArray);
                    counter++;
                }
                path = AssetDatabase.GenerateUniqueAssetPath(savePath + "DetailNormalMapsTextureArray.asset");
                WriteTextureArrayToDisk(ref detailNormalArray, path);

                if (detailMaskArray != null)
                {
                    EditorUtility.DisplayProgressBar("Combining Materials", $"Writing Texture Arrays To Disk {counter}/{totalTexArray}", (float)(9) / totalTexArray);
                    counter++;
                }

                path = AssetDatabase.GenerateUniqueAssetPath(savePath + "DetailMaskMapsTextureArray.asset");
                WriteTextureArrayToDisk(ref detailMaskArray, path);

                if (specularArray != null)
                {
                    EditorUtility.DisplayProgressBar("Combining Materials", $"Writing Texture Arrays To Disk {counter}/{totalTexArray}", (float)(10) / totalTexArray);
                    counter++;
                }

                path = AssetDatabase.GenerateUniqueAssetPath(savePath + "SpecularMapsTextureArray.asset");
                WriteTextureArrayToDisk(ref specularArray, path);





                List<CombineKey> generatedMeshes = new List<CombineKey>();

                if (attrImg != null)
                {
                    string attrImgPath = savePath + "_MatAttributes.atim";
                    AttributesImage.BurnToAttributesImg(attrImg, attrImgPath);
                    AssetDatabase.Refresh();
                    attrImg = AssetDatabase.LoadAssetAtPath<Texture2D>(attrImgPath);
                }

                Material[] generatedMaterials = GenerateMaterials(objectsData, isSpecularShader, savePath);
                HashSet<Material> usedMaterials = new HashSet<Material>();


                foreach (var material in generatedMaterials)
                {

                    material.SetFloat("_PackingMode", 0);

                    material.SetTexture("_MainTex", diffuseArray);
                    material.SetTexture("_BumpMap", normalArray);

                    if (metallicArray == null && specularArray != null)
                    {
                        material.SetTexture("_SpecGlossMap", specularArray);
                    }
                    else
                    {
                        material.SetTexture("_MetallicGlossMap", metallicArray);
                    }

                    material.SetTexture("_AttrImg", attrImg);
                    material.SetTexture("_ParallaxMap", heightArray);
                    material.SetTexture("_OcclusionMap", occlusionArray);
                    material.SetTexture("_EmissionMap", emisArray);
                    material.SetTexture("_DetailAlbedoMap", detailAlbedoArray);
                    material.SetTexture("_DetailNormalMap", detailNormalArray);
                    material.SetTexture("_DetailMaskMap", detailMaskArray);

                    EditorUtility.SetDirty(material);
                }



                objectsData.combinedMaterials = generatedMaterials;


                string meshPath = AssetDatabase.GenerateUniqueAssetPath(savePath + "TextureIndexed_Meshes");
                string uniqueN = meshPath.Substring(meshPath.LastIndexOf("/") + 1);
                string parentP = meshPath.Split(new string[] { "/" + uniqueN }, StringSplitOptions.None)[0];
                AssetDatabase.CreateFolder(parentP, uniqueN);
                meshPath += "/";


                int totalCount = 0;
                int completed = 0;


                List<CombineKey> tempGenMeshes = new List<CombineKey>();

                foreach (var textureArray in objectsData.materialEntities)
                {
                    foreach (var comb in textureArray.combinedMats)
                    {
                        foreach (var meshData in comb.meshesData)
                        {
                            for (int a = 0; a < meshData.meshFilters.Count; ++a)
                            {
                                MeshFilter meshFilter = meshData.meshFilters[a];
                                MeshRenderer meshRenderer = meshData.meshRenderers[a];
                                SkinnedMeshRenderer skinnedMeshRenderer = meshData.skinnedMeshRenderers[a];

                                Mesh sharedMesh = null;

                                if (meshFilter != null)
                                {
                                    sharedMesh = meshFilter.sharedMesh;
                                }

                                if (skinnedMeshRenderer != null)
                                {
                                    sharedMesh = skinnedMeshRenderer.sharedMesh;
                                }

                                var finalMesh = new Mesh();


                                Mesh previous = GetPreviouslyCombined(sharedMesh, meshData.originalMaterials, tempGenMeshes);

                                if (previous != null) { finalMesh = previous; }

                                CombineKey combineKey = new CombineKey();
                                combineKey.mesh = sharedMesh;
                                combineKey.materials = meshData.originalMaterials;
                                combineKey.finalMesh = finalMesh;
                                combineKey.targetFilter = meshFilter;
                                combineKey.targetSkin = skinnedMeshRenderer;

                                tempGenMeshes.Add(combineKey);

                                if (previous == null)
                                {
                                    totalCount++;
                                }
                            }

                        }
                    }
                }


                foreach (var textureArray in objectsData.materialEntities)
                {
                    foreach (var comb in textureArray.combinedMats)
                    {
                        foreach (var meshData in comb.meshesData)
                        {
                            for (int a = 0; a < meshData.meshFilters.Count; ++a)
                            {
                                MeshFilter meshFilter = meshData.meshFilters[a];
                                MeshRenderer meshRenderer = meshData.meshRenderers[a];
                                SkinnedMeshRenderer skinnedMeshRenderer = meshData.skinnedMeshRenderers[a];

                                Mesh sharedMesh = null;

                                if (meshFilter != null)
                                {
                                    sharedMesh = meshFilter.sharedMesh;
                                }

                                if (skinnedMeshRenderer != null)
                                {
                                    sharedMesh = skinnedMeshRenderer.sharedMesh;
                                }


                                Material[] combinedMaterials = null;
                                Material[] sourceMaterials = null;
                                int length = 0;

                                if (meshRenderer != null)
                                {
                                    Material usedMat = objectsData.combinedMaterials[comb.materialProperties.alphaMode];
                                    usedMaterials.Add(usedMat);
                                    meshRenderer.sharedMaterial = usedMat;
                                    length = meshRenderer.sharedMaterials.Length;
                                    sourceMaterials = meshRenderer.sharedMaterials;

                                }
                                if (skinnedMeshRenderer != null)
                                {
                                    Material usedMat = objectsData.combinedMaterials[comb.materialProperties.alphaMode];
                                    usedMaterials.Add(usedMat);
                                    skinnedMeshRenderer.sharedMaterial = usedMat;
                                    length = skinnedMeshRenderer.sharedMaterials.Length;
                                    sourceMaterials = skinnedMeshRenderer.sharedMaterials;
                                }

                                combinedMaterials = new Material[length];

                                for (int b = 0; b < length; ++b)
                                {
                                    Material usedMat = objectsData.combinedMaterials[comb.materialProperties.alphaMode];
                                    usedMaterials.Add(usedMat);
                                    combinedMaterials[b] = usedMat;
                                }

                                bool canCombine = CanCombineMaterials(meshData.originalMaterials);
                                canCombine = false;

                                if (canCombine && combinedMaterials.Length > 0)
                                {
                                    combinedMaterials = new Material[1] { combinedMaterials[0] };
                                }

                                if (meshRenderer != null)
                                {
                                    meshRenderer.sharedMaterials = combinedMaterials;
                                    EditorUtility.SetDirty(meshRenderer.gameObject);
                                }

                                if (skinnedMeshRenderer != null)
                                {
                                    skinnedMeshRenderer.sharedMaterials = combinedMaterials;
                                    EditorUtility.SetDirty(skinnedMeshRenderer.gameObject);
                                }

                                Mesh[] outputMeshes = meshData.outputMeshes;
                                Matrix4x4[] outputMatrix = meshData.outputMatrices;

                                CombineInstance[] combines = new CombineInstance[outputMeshes.Length];

                                for (int c = 0; c < outputMeshes.Length; ++c)
                                {
                                    combines[c].mesh = outputMeshes[c];
                                    combines[c].transform = outputMatrix[c];
                                }

                                var finalMesh = new Mesh();

                                if (outputMeshes != null && outputMeshes.Length > 0 && outputMeshes[0] != null)
                                {
                                    finalMesh.name = outputMeshes[0].name;
                                }

                                int totalVerticesCount = 0;

                                foreach (var c in combines)
                                {
                                    totalVerticesCount += c.mesh.vertexCount;
                                }

#if UNITY_2017_3_OR_NEWER
                                if (totalVerticesCount > 64000)
                                {
                                    finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                                }
#endif
                                //finalMesh.CombineMeshes(combines, canCombine, false);
                                //finalMesh.CombineMeshes(combines, canCombine, true);

                                finalMesh = outputMeshes[0];
                                finalMesh.UploadMeshData(false);

                                Mesh previous = GetPreviouslyCombined(sharedMesh, meshData.originalMaterials, generatedMeshes);
                                if (previous != null) { finalMesh = previous; }

                                CombineKey combineKey = new CombineKey();
                                combineKey.mesh = sharedMesh;
                                combineKey.finalMesh = finalMesh;
                                combineKey.materials = meshData.originalMaterials;
                                combineKey.targetFilter = meshFilter;
                                combineKey.targetSkin = skinnedMeshRenderer;

                                generatedMeshes.Add(combineKey);

                                if (previous == null)
                                {
                                    completed++;
                                    EditorUtility.DisplayProgressBar("Combining Materials", $"Writing modified meshes to disk {completed}/{totalCount}", (float)(completed) / (totalCount));

                                    string meshName = UtilityServices.MakePathSafe(finalMesh.name) + "_TexIndexed.mesh";
                                    AssetDatabase.CreateAsset(finalMesh, meshPath + meshName);
                                }

                                else
                                {
                                    finalMesh = previous;
                                }

                                foreach (var mat in combinedMaterials)
                                {
                                    EditorUtility.SetDirty(mat);
                                }
                            }
                        }
                    }
                }


                EditorUtility.DisplayProgressBar("Combining Materials", $"Finishing up {0}/{1}", (float)(0) / (1));


                foreach (var mesh in generatedMeshes)
                {
                    if (mesh.targetFilter != null)
                    {
                        mesh.targetFilter.sharedMesh = mesh.finalMesh;
                    }
                    if (mesh.targetSkin != null)
                    {
                        mesh.targetSkin.sharedMesh = mesh.finalMesh;
                    }
                }


                AssetDatabase.Refresh();

                DestroyImmediate(blankBump);
                DestroyImmediate(blankGrey);
                SceneView.RepaintAll();


                //Delete Unused materials

                foreach (Material material in objectsData.combinedMaterials)
                {
                    if (!usedMaterials.Contains(material))
                    {
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(material));
                    }
                }


                // Add material info to objects
                foreach (var link in objectsMaterialLinks)
                {
                    GameObject gameObject = link.Key;
                    List<MaterialEntity> materialEntitiesLinked = link.Value;

                    ObjectMaterialLinks objMaterialLinks = gameObject.GetComponent<ObjectMaterialLinks>();

                    if (objMaterialLinks != null) { DestroyImmediate(objMaterialLinks); }

                    objMaterialLinks = gameObject.AddComponent<ObjectMaterialLinks>();

                    objMaterialLinks.linkedMaterialEntities = materialEntitiesLinked;

                    objMaterialLinks.linkedAttrImg = attrImg;

                    objMaterialLinks.hideFlags = HideFlags.HideInInspector;

                    string attrImgPath = AssetDatabase.GetAssetPath(attrImg);

                    foreach (var matEntity in materialEntitiesLinked)
                    {
                        foreach (var combMat in matEntity.combinedMats)
                        {
                            MaterialProperties props = combMat.materialProperties;
                            props.BurnAttrToImg(ref attrImg, props.matIndex, props.texArrIndex);
                            //Debug.Log($"GameObject {gameObject.name}  is linked to Material: {combMat.material.name}");
                        }
                    }

                    AttributesImage.BurnToAttributesImg(attrImg, attrImgPath);
                }


                EditorUtility.DisplayProgressBar("Combining Materials", $"Finishing up {1}/{1}", (float)(1) / (1));

                // Restore Nonfeasible Materials to initial state
                RestoreNonFeasibleMats();



                // Bug in some newer versions of Unity doesn't load generated materials unless they are selected
                // This can cause objects to look different

                generatedMaterials = new Material[usedMaterials.Count];
                usedMaterials.CopyTo(generatedMaterials);


                if (generatedMaterials != null)
                {
                    //for (int a = 0; a < generatedMaterials.Length; a++)
                    //{
                    //    var item = generatedMaterials[a];
                    //    if (item == null) { continue; }
                    //    Selection.activeObject = item;
                    //}

                    //for (int a = generatedMaterials.Length - 1; a > -1; a--)
                    //{
                    //    var item = generatedMaterials[a];
                    //    if (item == null) { continue; }
                    //    Selection.activeObject = item;
                    //}

                    Selection.objects = generatedMaterials;

                }

                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();
                return true;
            }

            catch (Exception ex)
            {
                RestoreNonFeasibleMats();
                EditorUtility.ClearProgressBar();

                if (savePath.ToLower().Equals("assets") || savePath.ToLower().Equals("assets/") || savePath.ToLower().Equals("assets\\") || String.IsNullOrWhiteSpace(savePath))
                {
                }
                else
                {
                    FileUtil.DeleteFileOrDirectory(savePath);
                }

                AssetDatabase.Refresh();
                throw ex;
            }
        }

        #endregion PUBLIC_METHODS


        #region HELPERS


        private static MaterialEntity GetMaterialEntityWithDiffuseMap(CombiningInformation combineInfo, Texture2D diffuseMap)
        {
            foreach (MaterialEntity materialEntity in combineInfo.materialEntities)
            {
                if (materialEntity.diffuseMap == diffuseMap)
                {
                    return materialEntity;
                }
            }

            return null;
        }


        private static void AddCombine(Mesh sharedMesh, MaterialProperties materialProperties, MaterialEntity materialEntity, CombiningInformation combineInfo, MeshFilter meshFilter, MeshRenderer meshRenderer, SkinnedMeshRenderer skinnedMeshRenderer)
        {
            var comb = FindCombine(materialProperties.originalMaterial, materialEntity);

            if (comb == null)
            {
                comb = new CombineMetaData
                {
                    material = materialProperties.originalMaterial,
                    materialProperties = materialProperties,
                    meshesData = new List<MeshData>()
                };

                materialEntity.combinedMats.Add(comb);
            }

            var meshData = FindMeshDataForMesh(comb, sharedMesh);

            if (meshData == null)
            {
                meshData = new CombiningInformation.MeshData();
                meshData.meshFilters = new List<MeshFilter>();
                meshData.meshRenderers = new List<MeshRenderer>();
                meshData.skinnedMeshRenderers = new List<SkinnedMeshRenderer>();
                meshData.meshFilters.Add(meshFilter);
                meshData.meshRenderers.Add(meshRenderer);
                meshData.skinnedMeshRenderers.Add(skinnedMeshRenderer);

                if (meshRenderer != null)
                {
                    meshData.originalMaterials = (Material[])meshRenderer.sharedMaterials.Clone();
                }

                if (skinnedMeshRenderer != null)
                {
                    meshData.originalMaterials = (Material[])skinnedMeshRenderer.sharedMaterials.Clone();
                }

                comb.meshesData.Add(meshData);
            }

            else
            {
                meshData.meshFilters.Add(meshFilter);
                meshData.meshRenderers.Add(meshRenderer);
                meshData.skinnedMeshRenderers.Add(skinnedMeshRenderer);
            }
        }


        private static void SetTextureFromMaterial(ref Texture2D texture, Material material, string texturePropertyName)
        {
            if (texture == null && material.HasProperty(texturePropertyName))
            {
                texture = material.GetTexture(texturePropertyName) as Texture2D;
            }
        }


        private static CombineMetaData FindCombine(Material material, CombiningInformation.MaterialEntity textureArray)
        {
            foreach (var comb in textureArray.combinedMats)
            {
                if (comb.material == material)
                {
                    return comb;
                }
            }
            return null;
        }


        private static CombiningInformation.MeshData FindMeshDataForMesh(CombiningInformation.CombineMetaData comb, Mesh originalMesh)
        {
            foreach (var meshdata in comb.meshesData)
            {
                foreach (MeshFilter meshFilter in meshdata.meshFilters)
                {
                    if (meshFilter != null && meshFilter.sharedMesh == originalMesh) { return meshdata; }
                }
                foreach (var skinnedRenderer in meshdata.skinnedMeshRenderers)
                {
                    if (skinnedRenderer != null && skinnedRenderer.sharedMesh == originalMesh) { return meshdata; }
                }
            }

            return null;
        }


        private static bool DiffuseMapHasAlpha(CombiningInformation combineInfo)
        {
            foreach (var textureArray in combineInfo.materialEntities)
            {
                if (textureArray.diffuseMap == null) { continue; }

                TextureImporter importer = (TextureImporter)(AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(textureArray.diffuseMap)));

                if (importer != null && importer.DoesSourceTextureHaveAlpha()) { return true; }
            }

            return false;
        }


        public static Texture2DArray AllocateArray(TextureArrayUserSettings arraySettings, int texturesCount, DiffuseColorSpace diffuseColorSpace, bool hasAlphaChannel)
        {
            int width = arraySettings.resolution.width;
            int height = arraySettings.resolution.height;
            var textureFormat = GetTextureFormat(arraySettings.compressionType, arraySettings.compressionQuality, hasAlphaChannel);
            bool isLinearColorSpace = diffuseColorSpace == DiffuseColorSpace.NON_LINEAR ? false : true;

            Texture2DArray array = new Texture2DArray(width, height, texturesCount, textureFormat, true, isLinearColorSpace);

            array.wrapMode = TextureWrapMode.Repeat;
            array.filterMode = arraySettings.filteringMode;
            array.anisoLevel = arraySettings.anisotropicFilteringLevel;

            return array;
        }


        private static TextureFormat GetTextureFormat(CombiningInformation.CompressionType cmp, CombiningInformation.CompressionQuality quality, bool hasAlphaChannel)
        {
            if (hasAlphaChannel)
            {
                if (cmp == CombiningInformation.CompressionType.ETC2_RGB)
                {
                    return TextureFormat.ETC2_RGBA8;
                }

                else if (cmp == CombiningInformation.CompressionType.PVRTC_RGB4)
                {
                    return TextureFormat.PVRTC_RGBA4;
                }

                else if (cmp == CombiningInformation.CompressionType.ASTC_RGB)
                {
                    if (quality == CombiningInformation.CompressionQuality.HIGH)
                    {
                        return TextureFormat.ASTC_RGBA_4x4;
                    }

                    else if (quality == CombiningInformation.CompressionQuality.MEDIUM)
                    {
                        return TextureFormat.ASTC_RGBA_8x8;
                    }

                    //Compression quality Low
                    else
                    {
                        return TextureFormat.ASTC_RGBA_12x12;
                    }
                }

                else if (cmp == CombiningInformation.CompressionType.DXT1)
                {
                    return TextureFormat.DXT5;
                }

                else if (cmp == CombiningInformation.CompressionType.DXT1_CRUNCHED)
                {
                    return TextureFormat.DXT5Crunched;
                }

                //UNCOMPRESSED
                else
                {
                    return TextureFormat.ARGB32;
                }

            }

            else
            {
                if (cmp == CombiningInformation.CompressionType.ETC2_RGB)
                {
                    return TextureFormat.ETC2_RGB;
                }

                else if (cmp == CombiningInformation.CompressionType.PVRTC_RGB4)
                {
                    return TextureFormat.PVRTC_RGB4;
                }

                else if (cmp == CombiningInformation.CompressionType.ASTC_RGB)
                {
                    if (quality == CombiningInformation.CompressionQuality.HIGH)
                    {
                        return TextureFormat.ASTC_RGB_4x4;
                    }

                    else if (quality == CombiningInformation.CompressionQuality.MEDIUM)
                    {
                        return TextureFormat.ASTC_RGB_8x8;
                    }

                    //Compression quality Low
                    else
                    {
                        return TextureFormat.ASTC_RGB_12x12;
                    }
                }

                else if (cmp == CombiningInformation.CompressionType.DXT1)
                {
                    return TextureFormat.DXT1;
                }

                else if (cmp == CombiningInformation.CompressionType.DXT1_CRUNCHED)
                {
                    return TextureFormat.DXT1Crunched;
                }

                //UNCOMPRESSED
                else
                {
                    return TextureFormat.RGB24;
                }

            }
        }



        public static bool IsAlphaCompressed(Texture2DArray textureArray)
        {

            var format = textureArray.format;


            if (format == TextureFormat.ETC2_RGBA8) { return true; }

            else if (format == TextureFormat.PVRTC_RGBA4) { return true; }

            else if (format == TextureFormat.ASTC_RGBA_4x4) { return true; }

            else if (format == TextureFormat.ASTC_RGBA_8x8) { return true; }

            else if (format == TextureFormat.ASTC_RGBA_12x12) { return true; }

            else if (format == TextureFormat.DXT5) { return true; }

            else if (format == TextureFormat.DXT5Crunched) { return true; }

            else if (format == TextureFormat.ARGB32) { return true; }



            else if (format == TextureFormat.ETC2_RGB) { return false; }

            else if (format == TextureFormat.PVRTC_RGB4) { return false; }

            else if (format == TextureFormat.ASTC_RGB_4x4) { return false; }

            else if (format == TextureFormat.ASTC_RGB_8x8) { return false; }

            else if (format == TextureFormat.ASTC_RGB_12x12) { return false; }

            else if (format == TextureFormat.DXT1Crunched) { return false; }

            else if (format == TextureFormat.RGB24) { return false; }

            else if (format == TextureFormat.DXT1) { return false; }



            else { return true; }

        }


        public static Texture2D GetResizedTexture(Texture2D toResize, CombiningInformation.Resolution newSize, bool hasNonColorData)
        {

            RenderTexture renderTexture = new RenderTexture(newSize.width, newSize.height, 0, RenderTextureFormat.ARGB32, hasNonColorData ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
            renderTexture.DiscardContents();

            GL.sRGBWrite = (QualitySettings.activeColorSpace == ColorSpace.Linear) && !hasNonColorData;
            Graphics.Blit(toResize, renderTexture);

            GL.sRGBWrite = false;
            RenderTexture.active = renderTexture;
            Texture2D resized = new Texture2D(newSize.width, newSize.height, TextureFormat.ARGB32, true, hasNonColorData);
            resized.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            resized.Apply(true);

            RenderTexture.active = null;
            renderTexture.Release();
            DestroyImmediate(renderTexture);

            return resized;
        }


        private static void CopyColorChannelFromSrcToDst(Texture2D srcTexture, Texture2D destTexture, int srcChannel, int destChannel)
        {
            if (srcTexture.width != destTexture.width || srcTexture.height != destTexture.height)
            {
                Debug.LogError("Failed to copy color channel, textures are of different resolution.");
                return;
            }

            Color[] srcColors = srcTexture.GetPixels();
            Color[] destColors = destTexture.GetPixels();

            for (int a = 0; a < srcColors.Length; ++a)
            {
                Color pixel = destColors[a];
                pixel[destChannel] = srcColors[a][destChannel];
                destColors[a] = pixel;
            }

            destTexture.SetPixels(destColors);
            destTexture.Apply();
        }


        public static void CompressTexture(Texture2D toCompress, TextureArrayUserSettings arraySettings, bool hasAlphaChannel)
        {
            if (toCompress == null) { return; }

            TextureFormat textureFormat = GetTextureFormat(arraySettings.compressionType, arraySettings.compressionQuality, hasAlphaChannel);
            EditorUtility.CompressTexture(toCompress, textureFormat, 50);
            toCompress.Apply();
        }


        public static void WriteTextureToTextureArray(Texture2D toWrite, Texture2DArray dstTextureArray, int index, int mipCount)
        {
            if (toWrite == null || dstTextureArray == null) { return; }

            if (toWrite.width != dstTextureArray.width || toWrite.height != dstTextureArray.height)
            {
                Debug.LogError("Cannot copy Texture to Texture Array. Resolution mismatch");
            }
            if (toWrite.format != dstTextureArray.format)
            {
                Debug.LogError("Cannot copy Texture to Texture Array. Format Mismatch " + toWrite.format + " != " + dstTextureArray.format);
            }
            for (int mip = 0; mip < mipCount; ++mip)
            {
                Graphics.CopyTexture(toWrite, 0, mip, dstTextureArray, index, mip);
            }
        }


        public static int SizeToMipCount(TextureArrayUserSettings arrayProperties)
        {
            int textureWidth = arrayProperties.resolution.width;
            int textureHeight = arrayProperties.resolution.height;
            int size = textureWidth;

            if (textureWidth > textureHeight) { size = textureHeight; }

            int mips = 11;

            if (size == 4096) { mips = 13; }

            else if (size == 2048) { mips = 12; }

            else if (size == 1024) { mips = 11; }

            else if (size == 512) { mips = 10; }

            else if (size == 256) { mips = 9; }

            else if (size == 128) { mips = 8; }

            else if (size == 64) { mips = 7; }

            else if (size == 32) { mips = 6; }

            return mips;
        }


        private static int FindTexArrIndexForMaterial(Material material, CombiningInformation combineInfo)
        {
            int a = 0;

            foreach (var textureArray in combineInfo.materialEntities)
            {
                if (material.HasProperty("_MainTex"))
                {
                    if (textureArray.diffuseMap == material.GetTexture("_MainTex") as Texture2D)
                    {
                        return a;
                    }
                }

                a++;
            }

            return 0;
        }


        private static int FindAttrImgIndex(Material currentMaterial, CombiningInformation combineInfo)
        {
            int a = 0;
            MaterialProperties materialProperties = new MaterialProperties();
            materialProperties.FillPropertiesFromMaterial(currentMaterial, combineInfo);

            foreach (var textureArray in combineInfo.materialEntities)
            {
                if (currentMaterial.HasProperty("_MainTex"))
                {
                    if (textureArray.diffuseMap == currentMaterial.GetTexture("_MainTex") as Texture2D)
                    {
                        foreach (var comb in textureArray.combinedMats)
                        {
                            if (comb.materialProperties.IsSameAs(materialProperties))
                            {
                                return a;
                            }
                            a++;
                        }
                    }
                    else
                    {
                        a += textureArray.combinedMats.Count;
                    }
                }
            }
            return 0;
        }


        private static void WriteTextureArrayToDisk(ref Texture2DArray textureArray, string path)
        {
            if (textureArray == null) { return; }

            textureArray.Apply(false, true);

            Texture2DArray existing = AssetDatabase.LoadAssetAtPath<Texture2DArray>(path);

            if (existing != null)
            {
                EditorUtility.CopySerialized(textureArray, existing);
                textureArray = existing;
            }

            else
            {
                AssetDatabase.CreateAsset(textureArray, path);
            }
        }


        private static Material[] GenerateMaterials(CombiningInformation combineInfo, bool isSpecularWorkflow, string savePath)
        {
            Material[] generatedMaterials = new Material[4];
            string[] materialPaths = new string[4];

            // Generate only for Opaque blend mode
            for (int a = 0; a < 4; ++a)
            {
                materialPaths[a] = GetMaterialsPath(combineInfo, a, savePath);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(materialPaths[a]);

                if (mat == null)
                {
                    if (isSpecularWorkflow)
                    {
                        mat = new Material(Shader.Find("BatchFewStandardSpecular"));
                        AssetDatabase.CreateAsset(mat, materialPaths[a]);
                        mat.SetFloat("_Mode", a);
                    }
                    else
                    {
                        mat = new Material(Shader.Find("BatchFewStandard"));
                        AssetDatabase.CreateAsset(mat, materialPaths[a]);
                    }
                }
                if (isSpecularWorkflow)
                {
                    mat.shader = Shader.Find("BatchFewStandardSpecular");
                }
                else
                {
                    mat.shader = Shader.Find("BatchFewStandard");
                }

                mat.SetFloat("_Mode", (float)a);

                EditorUtility.SetDirty(mat);

                generatedMaterials[a] = mat = AssetDatabase.LoadAssetAtPath<Material>(materialPaths[a]);
            }

            return generatedMaterials;
        }


        private static string GetMaterialsPath(CombiningInformation combineInfo, int alphaMode, string basePath)
        {
            if (alphaMode == 0) { basePath += "CombineMaterial_OpaqueMode.mat"; }
            if (alphaMode == 1) { basePath += "CombineMaterial_CutoutMode.mat"; }
            else if (alphaMode == 2) { basePath += "CombineMaterial_FadeMode.mat"; }
            else if (alphaMode == 3) { basePath += "CombineMaterial_TransparentMode.mat"; }

            return AssetDatabase.GenerateUniqueAssetPath(basePath);
        }


        private static bool CanCombineMaterials(Material[] materials)
        {
            if (materials == null || materials.Length <= 1) { return true; }

            var material = materials[0];

            for (int a = 1; a < materials.Length; ++a)
            {
                if (material.HasProperty("_Mode") && materials[a].HasProperty("_Mode") && material.GetFloat("_Mode") == materials[a].GetFloat("_Mode"))
                {
                    continue;
                }

                if (material.shader != materials[a].shader) { return false; }
            }

            return true;
        }

        //mightdelete
        private static Mesh GetPreviouslyCombined(Mesh srcMesh, Material[] mats, List<CombineKey> keys)
        {
            foreach (var key in keys)
            {
                if (srcMesh == key.mesh)
                {
                    if (mats.Length == key.materials.Length)
                    {
                        bool match = true;
                        for (int a = 0; a < mats.Length; ++a)
                        {
                            if (mats[a] != key.materials[a])
                            {
                                match = false;
                            }
                        }
                        if (match)
                        {
                            return key.finalMesh;
                        }
                    }
                }
            }
            return null;
        }


        private static Mesh GetSubmesh(Mesh mesh, int submeshIndex, SkinnedMeshRenderer smr = null)
        {
            if (submeshIndex < 0 || submeshIndex >= mesh.subMeshCount) { return null; }

            int[] indices = mesh.GetTriangles(submeshIndex);
            Vertices source = new Vertices(mesh);
            Vertices dest = new Vertices();
            Dictionary<int, int> map = new Dictionary<int, int>();
            int[] newIndices = new int[indices.Length];

            for (int a = 0; a < indices.Length; a++)
            {
                int o = indices[a];
                int n;

                if (!map.TryGetValue(o, out n))
                {
                    n = dest.Add(source, o);
                    map.Add(o, n);
                }

                newIndices[a] = n;
            }

            Mesh newMesh = new Mesh();
            dest.AssignTo(newMesh);
            newMesh.triangles = newIndices;
            newMesh.bindposes = mesh.bindposes;

            //if(smr == null)
            //{
            //    newMesh.bindposes = mesh.bindposes;
            //}
            //else
            //{
            //    Transform[] bones = smr.bones;
            //    Matrix4x4[] bindPoses = new Matrix4x4[bones.Length];

            //    for(int a = 0; a < bindPoses.Length; a++)
            //    {
            //        // The bind pose is bone's inverse transformation matrix
            //        // In this case the matrix we also make this matrix relative to the root
            //        // So that we can move the root game object around freely
            //        bindPoses[a] = bones[a].worldToLocalMatrix * smr.rootBone.transform.localToWorldMatrix;
            //    }

            //    newMesh.bindposes = bindPoses;
            //}

            return newMesh;
        }


        private static HashSet<int> GetSubmeshVerts(Mesh mesh, int submeshIndex, SkinnedMeshRenderer smr = null)
        {
            if (submeshIndex < 0 || submeshIndex >= mesh.subMeshCount) { return null; }

            int[] indices = mesh.GetTriangles(submeshIndex);
            Vertices source = new Vertices(mesh);
            HashSet<int> map = new HashSet<int>();

            for (int a = 0; a < indices.Length; a++)
            {
                int o = indices[a];

                if (!map.Contains(o))
                {
                    map.Add(o);
                }
            }

            return map;
        }



        private static void RestoreNonFeasibleMats()
        {
            try
            {
                foreach (var nfm in nonFeasibleMaterials)
                {
                    Renderer renderer = nfm.renderer;

                    if (renderer != null && renderer.sharedMaterials != null && renderer.sharedMaterials.Length > 0)
                    {
                        try
                        {
                            Material[] s = new Material[renderer.sharedMaterials.Length];
                            Array.Copy(renderer.sharedMaterials, s, s.Length);
                            s[nfm.submeshIndex] = nfm.material;
                            renderer.sharedMaterials = s;
                        }

#pragma warning disable
                        catch (Exception ex) { }
                    }
                }

            }

            catch (Exception ex)
            {
                Debug.LogWarning("Failed to restore nfms");
            }

        }

        #endregion HELPERS

    }

}


#endif