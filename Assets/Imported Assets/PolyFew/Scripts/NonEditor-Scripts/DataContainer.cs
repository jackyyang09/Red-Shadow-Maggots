//////////////////////////////////////////////////////
// Copyright (c) BrainFailProductions
//////////////////////////////////////////////////////


#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using static BrainFailProductions.PolyFew.MaterialCombiner.CombiningInformation;

namespace BrainFailProductions.PolyFew
{

    public class DataContainer : MonoBehaviour
    {


        [System.Serializable]
        public class ObjectsHistory : SerializableDictionary<GameObject, UndoRedoOps> { }

        [System.Serializable]
        public class ObjectMeshPair : SerializableDictionary<GameObject, MeshRendererPair> { }

        [System.Serializable]
        public class MeshRendererPair
        {
            public bool attachedToMeshFilter;
            public Mesh mesh;

            public MeshRendererPair(bool attachedToMeshFilter, Mesh mesh)
            {
                this.attachedToMeshFilter = attachedToMeshFilter;
                this.mesh = mesh;
            }

            public void Destruct()
            {
                if (mesh != null)
                {
                    DestroyImmediate(mesh);
                }
            }
        }


        [System.Serializable]
        public class CustomMeshActionStructure
        {
            public MeshRendererPair meshRendererPair;
            public GameObject gameObject;
            public Action action;

            public CustomMeshActionStructure(MeshRendererPair meshRendererPair, GameObject gameObject, Action action)
            {
                this.meshRendererPair = meshRendererPair;
                this.gameObject = gameObject;
                this.action = action;
            }
        }


        [System.Serializable]
        public class ObjectHistory
        {
            public bool isReduceDeep;
            public ObjectMeshPair objectMeshPairs;


            public ObjectHistory(bool isReduceDeep, ObjectMeshPair objectMeshPairs)
            {
                this.isReduceDeep = isReduceDeep;
                this.objectMeshPairs = objectMeshPairs;
            }

            public void Destruct()
            {

                if (objectMeshPairs == null || objectMeshPairs.Count == 0)
                {
                    return;
                }

                foreach (var item in objectMeshPairs)
                {
                    item.Value.Destruct();
                }

                objectMeshPairs = null;
            }
        }


        [System.Serializable]
        public class UndoRedoOps
        {
            public GameObject gameObject;
            public List<ObjectHistory> undoOperations;
            public List<ObjectHistory> redoOperations;

            public UndoRedoOps(GameObject gameObject, List<ObjectHistory> undoOperations, List<ObjectHistory> redoOperations)
            {
                this.gameObject = gameObject;
                this.undoOperations = undoOperations;
                this.redoOperations = redoOperations;
            }


            public void Destruct()
            {
                if (undoOperations != null && undoOperations.Count > 0)
                {
                    foreach (var operation in undoOperations)
                    {
                        operation.Destruct();
                    }
                }

                if (redoOperations != null && redoOperations.Count > 0)
                {
                    foreach (var operation in redoOperations)
                    {
                        operation.Destruct();
                    }
                }

            }
        }


        [System.Serializable]
        public class LODLevelSettings
        {

            public float reductionStrength;
            public float transitionHeight;
            public bool preserveUVFoldover;
            public bool preserveUVSeams;
            public bool preserveBorders;
            public bool useEdgeSort;
            public bool recalculateNormals;
            public float aggressiveness;
            public int maxIterations;
            public bool regardCurvature;
            public bool regardTolerance;
            public bool combineMeshes;
            public bool simplificationOptionsFoldout;
            public bool intensityFoldout;
            public List<float> sphereIntensities;


            public LODLevelSettings(float reductionStrength, float transitionHeight, bool preserveUVFoldover, bool preserveUVSeams, bool preserveBorders, bool smartLinking, bool recalculateNormals, float aggressiveness, int maxIterations, bool regardTolerance, bool regardCurvature, bool combineMeshes, List<float> preservationStrengths)
            {
                this.reductionStrength = reductionStrength;
                this.transitionHeight = transitionHeight;
                this.preserveUVFoldover = preserveUVFoldover;
                this.preserveUVSeams = preserveUVSeams;
                this.preserveBorders = preserveBorders;
                this.useEdgeSort = smartLinking;
                this.recalculateNormals = recalculateNormals;
                this.aggressiveness = aggressiveness;
                this.maxIterations = maxIterations;
                this.regardTolerance = regardTolerance;
                this.regardCurvature = regardCurvature;
                this.combineMeshes = combineMeshes;
                this.sphereIntensities = preservationStrengths;
            }
        }


        [System.Serializable]
        public class ToleranceSphere: ScriptableObject
        {

            public Vector3 worldPosition;
            public float diameter;
            public Color color;
            public float preservationStrength;
            public bool isHidden;

            public ToleranceSphere(Vector3 worldPosition, float diameter, Color color, float preservationStrength, bool isHidden = false)
            {
                this.worldPosition = worldPosition;
                this.diameter = diameter;
                this.color = color;
                this.preservationStrength = preservationStrength;
                this.isHidden = isHidden;
            }


            public void SetProperties(ToleranceSphereJson tSphereJson)
            {
                worldPosition = tSphereJson.worldPosition;
                diameter = tSphereJson.diameter;
                color = tSphereJson.color;
                preservationStrength = tSphereJson.preservationStrength;
                isHidden = tSphereJson.isHidden;
            }


            public void SetProperties(Vector3 worldPosition, float diameter, Color color, float preservationStrength, bool isHidden = false)
            {
                this.worldPosition = worldPosition;
                this.diameter = diameter;
                this.color = color;
                this.preservationStrength = preservationStrength;
                this.isHidden = isHidden;
            }
        }


        [System.Serializable]
        public class ToleranceSphereJson
        {

            public Vector3 worldPosition;
            public float diameter;
            public Color color;
            public float preservationStrength;
            public bool isHidden;

            public ToleranceSphereJson(Vector3 worldPosition, float diameter, Color color, float preservationStrength, bool isHidden = false)
            {
                this.worldPosition = worldPosition;
                this.diameter = diameter;
                this.color = color;
                this.preservationStrength = preservationStrength;
                this.isHidden = isHidden;
            }

            public ToleranceSphereJson(ToleranceSphere toleranceSphere)
            {
                if (toleranceSphere == null) { return; }

                DumpFromToleranceSphere(toleranceSphere);
            }


            public void SetProperties(Vector3 worldPosition, float diameter, Color color, float preservationStrength, bool isHidden = false)
            {
                this.worldPosition = worldPosition;
                this.diameter = diameter;
                this.color = color;
                this.preservationStrength = preservationStrength;
                this.isHidden = isHidden;
            }


            public void DumpFromToleranceSphere(ToleranceSphere toleranceSphere)
            {
                if (toleranceSphere == null) { return; }

                worldPosition = toleranceSphere.worldPosition;
                diameter = toleranceSphere.diameter;
                color = toleranceSphere.color;
                preservationStrength = toleranceSphere.preservationStrength;
                isHidden = toleranceSphere.isHidden;
            }


            public void DumpToToleranceSphere(ref ToleranceSphere toleranceSphere)
            {
                if (toleranceSphere == null) { return; }

                toleranceSphere.worldPosition = worldPosition;
                toleranceSphere.diameter = diameter;
                toleranceSphere.color = color;
                toleranceSphere.preservationStrength = preservationStrength;
                toleranceSphere.isHidden = isHidden;
            }
        }




        public ObjectsHistory objectsHistory;

        public ObjectMeshPair objectMeshPairs;

        public List<GameObject> historyAddedObjects;   // This is the ordered list of GameObjects for which the undo/redo records are added

        public List<LODLevelSettings> currentLodLevelSettings;

        public List<ToleranceSphere> toleranceSpheres;



        #region BATCH FEW DATA

        public TextureArrayGroup textureArraysSettings;
        public List<MaterialProperties> materialsToRestore;
        public ObjectMaterialLinks lastObjMaterialLinks; // BeforeSerialization
        public bool relocateMaterialLinks;
        public bool reInitializeTempMatProps;

        public int choiceTextureMap = 0;
        public int choiceDiffuseColorSpace = 0;

        
        public readonly string[] textureMapsChoices = new[] { "Albedo", "Metallic", "Specular", "Normal", "Height", "Occlusion", "Emission", "Detail Mask", "Detail Albedo", "Detail Normal" };
        public readonly string[] compressionTypesChoices = new[] { "Uncompressed", "DXT1", "ETC2_RGB", "PVRTC_RGB4", "ASTC_RGB", "DXT1_CRUNCHED" };
        public readonly string[] resolutionsChoices = new[] { "32", "64", "128", "256", "512", "1024", "2048", "4096" };
        public readonly string[] filteringModesChoices = new[] { "Point (no filter)", "Bilinear", "Trilinear" };
        public readonly string[] compressionQualitiesChoices = new[] { "Low", "Medium", "High" };
        public readonly string[] colorSpaceChoices = new[] { "Non_Linear", "Linear" };
        public string batchFewSavePath = "";

        #endregion BATCH FEW DATA


        #region ALTER TEXTURE ARRAYS

        public List<Texture2DArray> existingTextureArrays = new List<Texture2DArray>();
        public bool existingTextureArraysFoldout;
        public int existingTextureArraysSize;
        public bool textureArraysPropsFoldout;
        public TextureArrayUserSettings existingArraysProps;
        public int choiceColorSpace = 0; //non linear

        #endregion ALTER TEXTURE ARRAYS


        #region Inspector Drawer Vars

        public bool preserveBorders;
        public bool preserveUVSeams;
        public bool preserveUVFoldover;
        public bool useEdgeSort = false;
        public bool recalculateNormals;
        public int maxIterations = 100;
        public float aggressiveness = 7;
        public bool regardCurvature = false;
        public bool reduceDeep;
        public bool isPreservationActive;
        public float sphereDiameter = 0.5f;
        public Vector3 oldSphereScale;
        public float reductionStrength;
        public bool reductionPending;
        public GameObject prevFeasibleTarget;
        public Transform prevSelection;
        public bool runOnThreads;
        public int triangleCount;
        [SerializeField]
        public UnityEngine.Object lastDrawer;
        public bool foldoutAutoLOD;
        public bool foldoutBatchFew;
        public bool foldoutAutoLODMultiple;
        public Vector3 objPositionPrevFrame; 
        public Vector3 objScalePrevFrame;
        public bool regardChildren = true;
        public string autoLODSavePath = "";

        public bool foldoutAdditionalOpts;
        public bool generateUV2LODs;
        public bool copyParentStaticFlags;
        public bool copyParentTag;
        public bool copyParentLayer;

        public bool generateUV2meshCombine;

        #endregion Inspector Drawer Vars


    }
}


#endif