//////////////////////////////////////////////////////
// Copyright (c) BrainFailProductions
//////////////////////////////////////////////////////


using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using UnityEditor.Callbacks;
using static BrainFailProductions.PolyFew.UtilityServices;
using UnityEditor.SceneManagement;
using static BrainFailProductions.PolyFew.MaterialCombiner.CombiningInformation;
using System.IO;
using Resolution = BrainFailProductions.PolyFew.MaterialCombiner.CombiningInformation.Resolution;
using CompressionType = BrainFailProductions.PolyFew.MaterialCombiner.CombiningInformation.CompressionType;

namespace BrainFailProductions.PolyFew
{
    
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PolyFewHost))]
    public class InspectorDrawer: Editor
    {

        private static bool PreserveBorders { get { return dataContainer.preserveBorders; } set { dataContainer.preserveBorders = value; } }
        private static bool PreserveUVSeams { get { return dataContainer.preserveUVSeams; } set { dataContainer.preserveUVSeams = value; } }
        private static bool PreserveUVFoldover { get { return dataContainer.preserveUVFoldover; } set { dataContainer.preserveUVFoldover = value; } }
        private static bool UseEdgeSort { get { return dataContainer.useEdgeSort; } set { dataContainer.useEdgeSort = value; } }
        private static bool RecalculateNormals { get { return dataContainer.recalculateNormals; } set { dataContainer.recalculateNormals = value; } }
        private static int MaxIterations { get { return dataContainer.maxIterations; } set { dataContainer.maxIterations = value; } }
        private static float Aggressiveness { get { return dataContainer.aggressiveness; } set { dataContainer.aggressiveness = value; } }
        private static bool ReduceDeep { get { return dataContainer.reduceDeep; } set { dataContainer.reduceDeep = value; } }
        private static bool RegardCurvature { get { return dataContainer.regardCurvature; } set { dataContainer.regardCurvature = value; } }


        private static int TriangleCount { get { return dataContainer.triangleCount; } set { dataContainer.triangleCount = value; } }
        private static float ReductionStrength { get { return dataContainer.reductionStrength; } set { dataContainer.reductionStrength = value; } }
        private static bool FoldoutAutoLOD { get { return dataContainer.foldoutAutoLOD; } set { dataContainer.foldoutAutoLOD = value; } }
        private static bool FoldoutBatchFew { get { return dataContainer.foldoutBatchFew; } set { dataContainer.foldoutBatchFew = value; } }
        private static bool IsPreservationActive { get { return dataContainer.isPreservationActive; } set { dataContainer.isPreservationActive = value; } }
        private static float SphereDefaultDiameter { get { return dataContainer.sphereDiameter; } set { dataContainer.sphereDiameter = value; } }
        private static Transform PrevSelection { get { return dataContainer.prevSelection; } set { dataContainer.prevSelection = value; } }

        private static bool isFeasibleTargetForPolyFew;
        private static string sphereColHex = "#FBFF00C8";
        private static Color sphereDefaultColor = UtilityServices.HexToColor(sphereColHex);
        private static Material sphereMat;

        private static Texture icon;
        private static bool toolMainFoldout = true;
        private const string ICON_PATH = "Assets/PolyFew/icons/";
        private const string SPHERE_PRESETS_PATH = "Assets/PolyFew/SpherePresets/";

#pragma warning disable
        private bool isVersionOk = true;
        private GameObject thisGameObject;
        private static UnityEngine.Object LastDrawer { get { if (dataContainer == null) { return null; }; return dataContainer.lastDrawer; } set { dataContainer.lastDrawer = value; } }
        private bool areAllMeshesSaved;
        private bool applyForOptionsChange;
        private bool ReductionPending { get { return dataContainer.reductionPending; } set { dataContainer.reductionPending = value; } }
        private GameObject PrevFeasibleTarget { get { return dataContainer.prevFeasibleTarget; } set { dataContainer.prevFeasibleTarget = value; } }
        private static bool RunOnThreads { get { return dataContainer.runOnThreads; } set { dataContainer.runOnThreads = value; } }
        private static Vector3 objPositionPrevFrame { get { return dataContainer.objPositionPrevFrame; } set { dataContainer.objPositionPrevFrame = value; } }
        private static Vector3 objScalePrevFrame { get { return dataContainer.objScalePrevFrame; } set { dataContainer.objScalePrevFrame = value; } }
        private static bool RegardChildren { get { return dataContainer.regardChildren; } set { dataContainer.regardChildren = value; } }

        private static bool FoldoutAdditonalOpts { get { return dataContainer.foldoutAdditionalOpts; } set { dataContainer.foldoutAdditionalOpts = value; } }
        private static bool GenerateUV2LODs { get { return dataContainer.generateUV2LODs; } set { dataContainer.generateUV2LODs = value; } }
        private static bool CopyParentStaticFlags { get { return dataContainer.copyParentStaticFlags; } set { dataContainer.copyParentStaticFlags = value; } }
        private static bool CopyParentTag { get { return dataContainer.copyParentTag; } set { dataContainer.copyParentTag = value; } }
        private static bool CopyParentLayer { get { return dataContainer.copyParentLayer; } set { dataContainer.copyParentLayer = value; } }

        private static bool GenerateUV2MeshCombine { get { return dataContainer.generateUV2meshCombine; } set { dataContainer.generateUV2meshCombine = value; } }






        private static List<Texture2DArray> existingTextureArrays { get { return dataContainer.existingTextureArrays; } set { dataContainer.existingTextureArrays = value; } }
        private static bool existingTextureArraysFoldout { get { return dataContainer.existingTextureArraysFoldout; } set { dataContainer.existingTextureArraysFoldout = value; } }
        private static int existingTextureArraysSize { get { return dataContainer.existingTextureArraysSize; } set { dataContainer.existingTextureArraysSize = value; } }
        private static bool textureArraysPropsFoldout { get { return dataContainer.textureArraysPropsFoldout; } set { dataContainer.textureArraysPropsFoldout = value; } }
        private static TextureArrayUserSettings existingArraysProps { get { return dataContainer.existingArraysProps; } set { dataContainer.existingArraysProps = value; } }


#pragma warning disable
        private readonly System.Object threadLock1 = new System.Object();
#pragma warning disable
        private readonly System.Object threadLock2 = new System.Object();
        private Func<bool> CheckOnThreads = new Func<bool>(() => { return (TriangleCount >= 1500 && dataContainer.objectMeshPairs.Count >= 2); });
        private static bool areMultiObjectsSelected;
        private static bool FoldAutoLODMultiple { get { return dataContainer.foldoutAutoLODMultiple; } set { dataContainer.foldoutAutoLODMultiple = value; } }
        private static string UnityVersion { get { return Application.unityVersion.Trim(); } }
        private static bool didPressButton;
        private static Color originalColor;
        private static List<GameObject> lastMultiSelectedObjects;
        private int width;
        private GUIStyle style;
        private GUIContent content;
        private RectOffset prevPadding;



        void OnEnable()
        {

            //Debug.Log("OnEnable called on InspectorDrawer for POLYFEW");
            if (Selection.gameObjects != null && Selection.gameObjects.Length > 1)
            {                
                isFeasibleTargetForPolyFew = false;

                foreach (GameObject selected in Selection.gameObjects)
                {
                    if (selected.activeSelf && selected.GetComponent<PolyFewHost>() == null)
                    {
                        selected.AddComponent<PolyFewHost>();
                    }
                }


                containerObject = UtilityServices.FindObjectFromTags("fcfab0d594624ba9acfb3155f4bd061b", "EditorOnly");

                if (!containerObject)
                {
                    //Debug.Log("Creating container object");
                    containerObject = new GameObject
                    {
                        name = "fcfab0d594624ba9acfb3155f4bd061b",
                        tag = "EditorOnly",
                        hideFlags = HideFlags.HideAndDontSave
                    };
                    containerObject.AddComponent<DataContainer>();
                }

                else if (containerObject.GetComponent<DataContainer>() == null)
                {
                    containerObject.AddComponent<DataContainer>();
                }


                UtilityServices.dataContainer = containerObject.GetComponent<DataContainer>();

                if (dataContainer.objectsHistory == null)
                {
                    dataContainer.objectsHistory = new DataContainer.ObjectsHistory();
                }

                if (dataContainer.historyAddedObjects == null)
                {
                    dataContainer.historyAddedObjects = new List<GameObject>();
                }


                if (dataContainer.toleranceSpheres == null)
                {
                    dataContainer.toleranceSpheres = new List<DataContainer.ToleranceSphere>();
                }


                return;
            }


            isFeasibleTargetForPolyFew = UtilityServices.CheckIfFeasible(Selection.activeTransform);
            UtilityServices.maxConcurrentThreads = SystemInfo.processorCount * 2;

            isVersionOk = UnityVersion.Contains("2017.1") || UnityVersion.Contains("2017.2") ? false : true;
            if (UnityVersion.Contains("2015")) { isVersionOk = false; }

            Selection.selectionChanged -= SelectionChanged;
            Selection.selectionChanged += SelectionChanged;


            thisGameObject = Selection.activeGameObject;


            containerObject = UtilityServices.FindObjectFromTags("fcfab0d594624ba9acfb3155f4bd061b", "EditorOnly");

            if (!containerObject)
            {
                //Debug.Log("Creating container object");
                containerObject = new GameObject
                {
                    name = "fcfab0d594624ba9acfb3155f4bd061b",
                    tag = "EditorOnly",
                    hideFlags = HideFlags.HideAndDontSave
                };
                containerObject.AddComponent<DataContainer>();
            }

            else if(containerObject.GetComponent<DataContainer>() == null)
            {
                containerObject.AddComponent<DataContainer>();
            }


            UtilityServices.dataContainer = containerObject.GetComponent<DataContainer>();

            if (dataContainer.objectsHistory == null)
            {
                dataContainer.objectsHistory = new DataContainer.ObjectsHistory();
            }

            if (dataContainer.historyAddedObjects == null)
            {
                dataContainer.historyAddedObjects = new List<GameObject>();
            }


            if (dataContainer.toleranceSpheres == null)
            {
                dataContainer.toleranceSpheres = new List<DataContainer.ToleranceSphere>();
            }


            if(dataContainer.textureArraysSettings == null)
            {
                ResetTextureArrays();
            }
            

            #region Restoring persistent data 

            UtilityServices.AutoLODSavePath = EditorPrefs.HasKey("autoLODSavePath") ? EditorPrefs.GetString("autoLODSavePath") : SetAndReturnStringPref("autoLODSavePath", "");
            UtilityServices.BatchFewSavePath = EditorPrefs.HasKey("batchFewSavePath") ? EditorPrefs.GetString("batchFewSavePath") : SetAndReturnStringPref("batchFewSavePath", "");
            string hex = EditorPrefs.HasKey("sphereColHex") ? EditorPrefs.GetString("sphereColHex") : SetAndReturnStringPref("sphereColHex", sphereColHex);
            sphereDefaultColor = UtilityServices.HexToColor(hex);

            #endregion Restoring persistent data 


            LastDrawer = this;

            
            SelectionChanged();

  
            if(isFeasibleTargetForPolyFew)
            {
                objPositionPrevFrame = Selection.activeTransform.position;
                objScalePrevFrame = Selection.activeTransform.lossyScale;
            }

            if (Selection.gameObjects == null || Selection.gameObjects.Length == 1)
            {
                isFeasibleTargetForPolyFew = UtilityServices.CheckIfFeasible(Selection.activeTransform);
            }


        }




        void OnDisable()
        {
            //Debug.Log("OnDisable called on InspectorDrawer for POLYFEW");
            Selection.selectionChanged -= SelectionChanged;
        }



        public void OnDestroy()
        {
            //Debug.Log("OnDestroy called on InspectorDrawer for POLYFEW");

            bool sameTarget = PrevSelection && Selection.activeTransform && Selection.activeTransform.GetHashCode().Equals(PrevSelection.GetHashCode());

            if(!sameTarget)
            {
                DestroyPolyFewHost();
            }

            if (Application.isEditor && thisGameObject == null)
            {
                //Debug.Log("Getting into history");
                GameObject key = thisGameObject;

                try
                {
                    if (dataContainer.objectsHistory.ContainsKey(key))
                    {
                        //Debug.Log("Destroyed Undo Histroy for object");
                        DataContainer.UndoRedoOps ops = dataContainer.objectsHistory[key];

                        ops.Destruct();
                        ops = null;
                        dataContainer.objectsHistory.Remove(key);
                    }
                }

                catch(Exception ex)
                {

                }

            }

        }



        void OnSceneGUI()
        {

            if (isFeasibleTargetForPolyFew && Selection.activeTransform != null)
            {

                PrevFeasibleTarget = Selection.activeTransform.gameObject;


                #region Draw custom handles for preservation sphere


                if (dataContainer.toleranceSpheres != null && IsPreservationActive)
                {
                    foreach (var toleranceSphere in dataContainer.toleranceSpheres)
                    {

                        //Handles.color = toleranceSphere.color;
                        //Handles.DrawSphere(-1, toleranceSphere.worldPosition, Quaternion.identity, toleranceSphere.diameter);

                        //Gizmos.color = toleranceSphere.color;
                        //Gizmos.DrawSphere(toleranceSphere.worldPosition, toleranceSphere.diameter / 2f);
                        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

                        Handles.color = toleranceSphere.color;

                        if (!toleranceSphere.isHidden)
                        {

                            Handles.SphereHandleCap(-1, toleranceSphere.worldPosition, Quaternion.identity, toleranceSphere.diameter, EventType.Repaint);

                            if (Tools.current == Tool.Move)
                            {

                                // the reference to the scriptable object might be null
                                if (toleranceSphere != null)
                                {
                                    Undo.RecordObject(toleranceSphere, "Tolerance Sphere Change Move");
                                }

                                Vector3 objCurrentPos = Selection.activeTransform.position;
                                Vector3 change = objCurrentPos - objPositionPrevFrame;
                                toleranceSphere.worldPosition += change;

                                Vector3 prevPos = toleranceSphere.worldPosition;
                                toleranceSphere.worldPosition = Handles.DoPositionHandle(toleranceSphere.worldPosition, Quaternion.identity);

                            }

                            else if (Tools.current == Tool.Scale)
                            {
                                // the reference to the scriptable object might be null
                                if (toleranceSphere != null)
                                {
                                    Undo.RecordObject(toleranceSphere, "Tolerance Sphere Change Scale");
                                }

                                Vector3 originalPos = toleranceSphere.worldPosition;
                                Vector3 objCurrentScale = Selection.activeTransform.lossyScale;
                                Vector3 change = objCurrentScale - objScalePrevFrame;
                                float oldDiameter = toleranceSphere.diameter;
                                Vector3 newScale = new Vector3(oldDiameter, oldDiameter, oldDiameter) + change;
                                toleranceSphere.diameter = UtilityServices.Average(newScale.x, newScale.y, newScale.z);
                                newScale = new Vector3(toleranceSphere.diameter, toleranceSphere.diameter, toleranceSphere.diameter);


                                newScale = Handles.DoScaleHandle(newScale, toleranceSphere.worldPosition, Quaternion.identity, HandleUtility.GetHandleSize(toleranceSphere.worldPosition));
                                toleranceSphere.diameter = UtilityServices.Average(newScale.x, newScale.y, newScale.z);

                                toleranceSphere.worldPosition = originalPos;
                            }

                        }
                    }
                }

                #endregion Draw custom handles for preservation sphere



                objPositionPrevFrame = Selection.activeTransform.position;
                objScalePrevFrame = Selection.activeTransform.lossyScale;

            }


        }



        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (Selection.activeGameObject == null) { return; }

            if(!Selection.activeGameObject.activeSelf) { return; }
            
            if (isFeasibleTargetForPolyFew)
            {
                toolMainFoldout = EditorGUILayout.Foldout(toolMainFoldout, "");

                EditorGUILayout.BeginVertical("GroupBox");


                #region Title Header

                EditorGUILayout.BeginHorizontal();
                content = new GUIContent();

                string saveMeshPath = ICON_PATH + "icon.png";
                icon = EditorGUIUtility.Load(saveMeshPath) as Texture;
                if (icon) GUILayout.Label(icon, GUILayout.Width(30), GUILayout.MaxHeight(30));
                GUILayout.Space(6);

                EditorGUILayout.BeginVertical();
                GUILayout.Space(7);
                var style = GUI.skin.label;
                style.richText = true;  // #FF6347ff4

                //EditorGUILayout.LabelField("<size=13><b><color=#A52A2AFF>POLY FEW</color></b></size>", style);
                if (GUILayout.Button("<size=13><b><color=#A52A2AFF>POLY FEW</color></b></size>", style)) { toolMainFoldout = !toolMainFoldout; }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                #endregion Title Header


                if (toolMainFoldout)
                {

                    UtilityServices.DrawHorizontalLine(Color.black, 1, 8);


                    #region Section Header


                    GUILayout.Space(10);

                    EditorGUILayout.BeginHorizontal();


                    #region Go Deep

                    content = new GUIContent();
                    style = GUI.skin.textField;
                    style.richText = true;
                    RectOffset oldPadding = new RectOffset(style.padding.left, style.padding.right, style.padding.top, style.padding.bottom);

                    style.padding = new RectOffset(18, style.padding.right, 3, style.padding.bottom);
                    content.text = "<b>Reduce Deep</b>";
                    content.tooltip = "Check this option to apply reduction with the current settings to this mesh and all of the children meshes. If this option is unchecked the reduction is only applied to the currently selected object, if it has a mesh.This might be slow for complex object hierarchies containing lots of meshes.";

                    originalColor = new Color(GUI.backgroundColor.r, GUI.backgroundColor.g, GUI.backgroundColor.b);
                    GUI.backgroundColor = UtilityServices.HexToColor("#EFEEEF"); //E1E1E1


#if UNITY_2019_1_OR_NEWER
                
                    GUILayout.Space(2);
                    EditorGUILayout.LabelField(content, style, GUILayout.Width(118), GUILayout.Height(20));                
#else
                    EditorGUILayout.LabelField(content, style, GUILayout.Width(120), GUILayout.Height(20));

#endif

                    GUI.backgroundColor = originalColor;

                    GUILayout.Space(10);

                    bool prevValue = ReduceDeep;
                    ReduceDeep = EditorGUILayout.Toggle(ReduceDeep, GUILayout.Width(28), GUILayout.ExpandWidth(false));
                    style.padding = oldPadding;

                    if (prevValue != ReduceDeep == true)
                    {
                        TriangleCount = UtilityServices.CountTriangles(ReduceDeep, dataContainer.objectMeshPairs, Selection.activeGameObject);
                        RunOnThreads = CheckOnThreads();
                        applyForOptionsChange = true;
                    }


                    #endregion Go Deep


                    GUILayout.Space(14);


                    #region Undo / Redo buttons

                    content = new GUIContent();

                    //GUILayout.FlexibleSpace();
                    content.tooltip = "Undo the last reduction operation. Please note that you will have to save the scene to keep these changes persistent";


                    GameObject kee = Selection.activeGameObject;

                    bool flag1 = true;


                    flag1 = !dataContainer.objectsHistory.ContainsKey(kee)
                            || dataContainer.objectsHistory[kee] == null
                            || dataContainer.objectsHistory[kee].undoOperations == null
                            || dataContainer.objectsHistory[kee].undoOperations.Count == 0;

                    
                    EditorGUI.BeginDisabledGroup(flag1);
                    bool hasLods = false;

                    if (!flag1)
                    {
                        hasLods = UtilityServices.HasLODs(Selection.activeGameObject);
                    }

                    content.text = "";
                    content.image = EditorGUIUtility.Load(ICON_PATH + "undo.png") as Texture;
                    style = GUI.skin.button;

                    if (GUILayout.Button(content, style, GUILayout.Width(20), GUILayout.MaxHeight(24), GUILayout.ExpandWidth(true)))
                    {
                        if (hasLods)
                        {
                            EditorUtility.DisplayDialog("LODs found under this object", "This object appears to have an LOD group or LOD assets generated. Please remove them first before trying to undo the last reduction operation", "Ok");
                        }
                        else
                        {
                            // undo
                            UtilityServices.ApplyUndoRedoOperation(kee, true);
                            TriangleCount = UtilityServices.CountTriangles(ReduceDeep, dataContainer.objectMeshPairs, Selection.activeGameObject);
                            EditorSceneManager.MarkSceneDirty(Selection.activeGameObject.scene);
                            //EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                            GUIUtility.ExitGUI();
                        }

                    }

                    EditorGUI.EndDisabledGroup();



                    GUILayout.Space(1);

                    content.tooltip = "Redo the last undo operation. Please note that you will have to save the scene to keep these changes persistent";
                    content.image = EditorGUIUtility.Load(ICON_PATH + "redo.png") as Texture;



                    flag1 = !dataContainer.objectsHistory.ContainsKey(kee)
                            || dataContainer.objectsHistory[kee] == null
                            || dataContainer.objectsHistory[kee].redoOperations == null
                            || dataContainer.objectsHistory[kee].redoOperations.Count == 0;


                    EditorGUI.BeginDisabledGroup(flag1);

                    if (!flag1)
                    {
                        hasLods = UtilityServices.HasLODs(Selection.activeGameObject);
                    }


                    if (GUILayout.Button(content, style, GUILayout.Width(20), GUILayout.MaxHeight(24), GUILayout.ExpandWidth(true)))
                    {
                        if (hasLods)
                        {
                            EditorUtility.DisplayDialog("LODs found under this object", "This object appears to have an LOD group or LOD assets generated. Please remove them first before trying to redo the last undo operation", "Ok");
                        }
                        else
                        {
                            //redo
                            UtilityServices.ApplyUndoRedoOperation(kee, false);
                            TriangleCount = UtilityServices.CountTriangles(ReduceDeep, dataContainer.objectMeshPairs, Selection.activeGameObject);
                            //EditorSceneManager.MarkSceneDirty(Selection.activeGameObject.scene); //baw did
                            //EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                            GUIUtility.ExitGUI();
                        }

                    }

                    EditorGUI.EndDisabledGroup();



                    #endregion Undo / Redo buttons



                    GUILayout.Space(10);


                    #region Apply Changes here



                    style = GUI.skin.button;
                    style.richText = true;

                    originalColor = new Color(GUI.backgroundColor.r, GUI.backgroundColor.g, GUI.backgroundColor.b);
                    //# ffc14d   60%
                    //# F0FFFF   73%
                    //# F5F5DC   75%
                    GUI.backgroundColor = UtilityServices.HexToColor("#F5F5DC");

                    content = new GUIContent();
                    content.text = "<size=11> <b><color=#000000>Reduce</color></b> </size>";
                    content.tooltip = "Apply reduction to this object with the current settings. If you don't reduce the object the changes will be lost when this object gets out of focus. Please note that you must save this scene after reducing the object otherwise the reduce operation will be reset on Editor restart.";

                    EditorGUI.BeginDisabledGroup(!ReductionPending || Mathf.Approximately(ReductionStrength, 0));

                    didPressButton = GUILayout.Button(content, style, GUILayout.Width(92), GUILayout.Height(24), GUILayout.ExpandWidth(true));

                    EditorGUI.EndDisabledGroup();

                    GUI.backgroundColor = originalColor;



                    if (didPressButton)
                    {

                        bool prevRedPendingVal = ReductionPending;

                        // Must save the meshes as assets before applying reduction operations
                        if (ReduceDeep)
                        {

                            List<Mesh> originalMeshes = UtilityServices.GetMeshesFromPairs(dataContainer.objectMeshPairs);

                            // The unsaved reduced meshes are the those which have their original meshes in (dataContainer.objectMeshPairs) unsaved as .mesh file
                            //HashSet<Mesh> unsavedReducedMeshes = UtilityServices.GetUnsavedReducedMeshes(dataContainer.objectMeshPairs);

                            Tuple<HashSet<Mesh>, DataContainer.ObjectMeshPair> unsavedReducedMeshesPairs = UtilityServices.GetUnsavedReducedMeshes(dataContainer.objectMeshPairs);
                            HashSet<Mesh> unsavedReducedMeshes = unsavedReducedMeshesPairs?.Item1;

                            // Contains copies of the original meshes that will be saved in the undo operations for this object
                            DataContainer.ObjectMeshPair originalMeshesClones = new DataContainer.ObjectMeshPair();


                            bool areMeshesSaved = UtilityServices.AreMeshesSavedAsAssets(originalMeshes);


                            // Indicates if the reduction operation is successfully applied to all the target meshes.
                            // If this value is true we can then add this reduce operation to the list of undo operations for this object.
                            bool fullySucceeded = true;


                            try
                            {

                                bool savedJustNow = false;

                                if (!areMeshesSaved)
                                {
                                    //Debug.Log("Saving meshes as the meshes weren't saved");

                                    int option = EditorUtility.DisplayDialogComplex("Unsaved Meshes",
                                                "The reduce operation won't be applied unless you save the modified meshes under this object. This is also required for keeping the changes persistent and for making prefabs workable for the modified objects. You only have to save the meshes once for an object. Please note that any changes to prefabs must be manually applied. You must save this scene after saving the object.",
                                                "Save",
                                                "Cancel",
                                                "Don't Save");

                                    List<Mesh> tempUnsavedMeshes = unsavedReducedMeshes.ToList();

                                    switch (option)
                                    {

                                        case 0:

                                            if (!IsPathInAssetsDir(AutoLODSavePath))
                                            {
                                                UtilityServices.AutoLODSavePath = UtilityServices.SetAndReturnStringPref("autoLODSavePath", "Assets/");
                                            }

                                            bool passed = UtilityServices.SaveAllMeshes(tempUnsavedMeshes, AutoLODSavePath, true, (error) =>
                                            {
                                                EditorUtility.DisplayDialog("Cannot Save Meshes", error, "Ok");
                                                areMeshesSaved = false;
                                            });

                                            if (passed)
                                            {
                                                //AssetDatabase.Refresh();
                                                areMeshesSaved = true;

                                                if (!(UnityVersion.Contains("2017") || UnityVersion.Contains("2018")))
                                                {
                                                    UtilityServices.RestoreMeshesFromPairs(unsavedReducedMeshesPairs.Item2);
                                                }
                                                
                                            }

                                            break;

                                        case 1:
                                        case 2:
                                            areMeshesSaved = false;
                                            savedJustNow = false;
                                            break;
                                    }


                                    if (UtilityServices.AreMeshesSavedAsAssets(tempUnsavedMeshes))
                                    {
                                        areMeshesSaved = true;
                                        savedJustNow = true;
                                        ReductionPending = false;
                                        ReductionStrength = 0;
                                    }

                                    else
                                    {
                                        areMeshesSaved = false;
                                        savedJustNow = false;
                                    }

                                }


                                fullySucceeded = areMeshesSaved;
                                
                                // After successfully saving the original meshes copy the modified properties from the modded meshes to the original meshes in the dataContainer list and add the meshes to the objects
                                if (areMeshesSaved)
                                {


                                    int toOverwrite = 0;
                                    int done = 0;
                                    foreach (var kvp in dataContainer.objectMeshPairs) { toOverwrite++; }


                                    //Debug.Log("Original mesh are saved so copying properties");
                                    foreach (var kvp in dataContainer.objectMeshPairs)
                                    {

                                        GameObject gameObject = kvp.Key;

                                        if (gameObject == null) { continue; }

                                        DataContainer.MeshRendererPair mRendererPair = kvp.Value;

                                        if (mRendererPair.mesh == null) { continue; }


                                        if (mRendererPair.attachedToMeshFilter)
                                        {
                                            MeshFilter filter = gameObject.GetComponent<MeshFilter>();


                                            if (filter != null)
                                            {
                                                Mesh moddedMesh = filter.sharedMesh;

                                                // Do this for those meshes that are just saved and exclude those that aren't saved now and had their original meshes saved before
                                                if (savedJustNow && unsavedReducedMeshes.Contains(moddedMesh))
                                                {
                                                    //Debug.Log("Mesh was SavedJustNow  " + moddedMesh.name);

                                                    Mesh prevMeshCopy = Instantiate(mRendererPair.mesh);
                                                    prevMeshCopy.name = mRendererPair.mesh.name;
                                                    DataContainer.MeshRendererPair mRenderPair = new DataContainer.MeshRendererPair(true, prevMeshCopy);
                                                    originalMeshesClones.Add(gameObject, mRenderPair);
                                                    //Debug.Log("Created mesh copy for undo  Triangles count  " + mRenderPair.mesh.triangles.Length / 3 + "  modded tris length  " + moddedMesh.triangles.Length / 3 + "  moddedMesh.HashCode  " + moddedMesh.GetHashCode() + "  created undo mesh hashcode   " + mRenderPair.mesh.GetHashCode());
                                                    
                                                    mRendererPair.mesh = moddedMesh;
                                                }
                                                else
                                                {

                                                    EditorUtility.DisplayProgressBar("Saving Changes", $"Writing mesh changes to existing files {++done}/{toOverwrite}", ((float)done / toOverwrite));

                                                    Mesh prevMeshCopy = Instantiate(mRendererPair.mesh);
                                                    prevMeshCopy.name = mRendererPair.mesh.name;
                                                    DataContainer.MeshRendererPair mRenderPair = new DataContainer.MeshRendererPair(true, prevMeshCopy);
                                                    originalMeshesClones.Add(gameObject, mRenderPair);


                                                    //mRendererPair.mesh.Clear();
                                                    //EditorUtility.CopySerialized(moddedMesh, mRendererPair.mesh);

                                                    //Overwrites the mesh assets and keeps references intact
                                                    if (OverwriteAssetAtPath(mRendererPair.mesh, moddedMesh)) { }
                                                    else
                                                    {
                                                        mRendererPair.mesh.Clear();
                                                        EditorUtility.CopySerialized(moddedMesh, mRendererPair.mesh);
                                                        DestroyImmediate(moddedMesh);
                                                    }

                                                    //mRendererPair.mesh.MakeSimilarToOtherMesh(moddedMesh);
                                                    
                                                }

                                                filter.sharedMesh = mRendererPair.mesh;

                                            }
                                        }

                                        else
                                        {
                                            SkinnedMeshRenderer sRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

                                            if (sRenderer != null)
                                            {
                                                Mesh moddedMesh = sRenderer.sharedMesh;

                                                // Do this for those meshes that are just saved and exclude those that aren't saved now and had their original meshes saved before
                                                if (savedJustNow && unsavedReducedMeshes.Contains(moddedMesh))
                                                {
                                                    //Debug.Log("Mesh was SavedJustNow  " + moddedMesh.name);

                                                    Mesh prevMeshCopy = Instantiate(mRendererPair.mesh);
                                                    prevMeshCopy.name = mRendererPair.mesh.name;
                                                    DataContainer.MeshRendererPair mRenderPair = new DataContainer.MeshRendererPair(false, prevMeshCopy);
                                                    originalMeshesClones.Add(gameObject, mRenderPair);

                                                    mRendererPair.mesh = moddedMesh;
                                                }
                                                else
                                                {
                                                    EditorUtility.DisplayProgressBar("Saving Changes", $"Writing mesh changes to existing files {++done}/{toOverwrite}", ((float)done / toOverwrite));

                                                    Mesh prevMeshCopy = Instantiate(mRendererPair.mesh);
                                                    prevMeshCopy.name = mRendererPair.mesh.name;
                                                    DataContainer.MeshRendererPair mRenderPair = new DataContainer.MeshRendererPair(false, prevMeshCopy);
                                                    originalMeshesClones.Add(gameObject, mRenderPair);

                                                    //mRendererPair.mesh.Clear();
                                                    //EditorUtility.CopySerialized(moddedMesh, mRendererPair.mesh);
                                                    //mRendererPair.mesh.MakeSimilarToOtherMesh(moddedMesh);

                                                    //Overwrites the mesh assets and keeps references intact
                                                    if (OverwriteAssetAtPath(mRendererPair.mesh, moddedMesh)) { }
                                                    else
                                                    {
                                                        mRendererPair.mesh.Clear();
                                                        EditorUtility.CopySerialized(moddedMesh, mRendererPair.mesh);
                                                        DestroyImmediate(moddedMesh);
                                                    }

                                                }

                                                sRenderer.sharedMesh = mRendererPair.mesh;
                                            }

                                        }


                                    }


                                    EditorUtility.ClearProgressBar();


                                    // Required inorder to force unity to record changes on the modified object
                                    var tempO = Selection.activeGameObject.AddComponent<RefreshEnforcer>();
                                    EditorSceneManager.MarkSceneDirty(Selection.activeGameObject.scene);
                                    UtilityServices.dataContainer.objectMeshPairs = UtilityServices.GetObjectMeshPairs(Selection.activeGameObject, true, true);
                                    bool didSave = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

                                    ReductionPending = false;
                                    ReductionStrength = 0;

                                }


                            }

#pragma warning disable

                            catch (Exception ex)
                            {
                                ReductionPending = prevRedPendingVal;
                                fullySucceeded = false;
                            }


                            // Add this operation to the undo ops for this object if reduction operation succeeded          
                            if (fullySucceeded)
                            {
                                // Add here the undo record
                                UtilityServices.SaveRecord(Selection.activeGameObject, true, true, originalMeshesClones);
                            }

                            // Destroy the mesh copies if failed to reduce.
                            else if (originalMeshesClones.Count > 0)
                            {
                                foreach (var item in originalMeshesClones)
                                {
                                    item.Value.Destruct();
                                }

                                originalMeshesClones = null;
                            }

                        }

                        else
                        {

                            GameObject gameObject = Selection.activeGameObject;
                            // Contains the copy of the original mesh that will be saved in the undo operations for this object
                            DataContainer.ObjectMeshPair originalMeshClone = new DataContainer.ObjectMeshPair();

                            DataContainer.MeshRendererPair mRendererPair = dataContainer.objectMeshPairs[gameObject];

                            Mesh moddedMesh = UtilityServices.GetReducedMesh(gameObject, mRendererPair);
                            bool isMeshPresent = mRendererPair.mesh == null ? false : true;
                            bool isMeshSaved = UtilityServices.IsMeshSavedAsAsset(mRendererPair.mesh);

                            bool savedJustNow = false;

                            // Indicates if the reduction operation is successfully applied to the target mesh.
                            // If this value is true we can then add this reduce operation to the list of undo operations for this object.
                            bool fullySucceeded = true;


                            if (isMeshPresent)
                            {
                                try
                                {
                                    if (!isMeshSaved)
                                    {
                                        //Debug.Log("Saving mesh as the mesh wasn't saved");
                                        int option = EditorUtility.DisplayDialogComplex("Unsaved Mesh",
                                                    "The reduce operation won't be applied unless you save the modified mesh of this object. This is also required for keeping the changes persistent and for making prefabs workable for the modified object. You only have to save the mesh once for an object. You must save this scene after saving the object.",
                                                    "Save",
                                                    "Cancel",
                                                    "Don't Save");


                                        switch (option)
                                        {
                                            case 0:
                                                if (!IsPathInAssetsDir(AutoLODSavePath))
                                                {
                                                    UtilityServices.AutoLODSavePath = UtilityServices.SetAndReturnStringPref("autoLODSavePath", "Assets/");
                                                }

                                                bool isSuccess = SaveMesh(moddedMesh, AutoLODSavePath, true, (error) =>
                                                {
                                                    EditorUtility.DisplayDialog("Cannot Save Mesh", error, "Ok");
                                                    isMeshSaved = false;
                                                });

                                                if (isSuccess)
                                                {
                                                    isMeshSaved = true;

                                                    if (!(UnityVersion.Contains("2017") || UnityVersion.Contains("2018")))
                                                    {
                                                        DataContainer.ObjectMeshPair reducedMeshPair = new DataContainer.ObjectMeshPair();
                                                        DataContainer.MeshRendererPair mPair = new DataContainer.MeshRendererPair(mRendererPair.attachedToMeshFilter, moddedMesh);
                                                        reducedMeshPair.Add(gameObject, mPair);
                                                        UtilityServices.RestoreMeshesFromPairs(reducedMeshPair);
                                                    }
                                              
                                                }

                                                break;

                                            case 1:
                                            case 2:
                                                isMeshSaved = false;
                                                break;
                                        }

                                        if (UtilityServices.IsMeshSavedAsAsset(moddedMesh))
                                        {
                                            isMeshSaved = true;
                                            savedJustNow = true;
                                            ReductionPending = false;
                                            ReductionStrength = 0;
                                        }

                                        else
                                        {
                                            isMeshSaved = false;
                                            savedJustNow = false;
                                        }


                                    }

                                    fullySucceeded = isMeshSaved;


                                    // After successfully saving the modded mesh copy the modified properties from the modded mesh to the original mesh in the dataContainer list and add the mesh to the object
                                    if (isMeshSaved)
                                    {
                                        //Debug.Log("Object saved so now applying to original mesh");
                                        if (mRendererPair.attachedToMeshFilter)
                                        {
                                            MeshFilter filter = gameObject.GetComponent<MeshFilter>();

                                            if (filter != null)
                                            {
                                                if (savedJustNow)
                                                {
                                                    //Debug.Log("SavedJustNow");

                                                    Mesh prevMeshCopy = Instantiate(mRendererPair.mesh);
                                                    prevMeshCopy.name = mRendererPair.mesh.name;
                                                    DataContainer.MeshRendererPair mRenderPair = new DataContainer.MeshRendererPair(true, prevMeshCopy);
                                                    originalMeshClone.Add(gameObject, mRenderPair);
                                                    //Debug.Log("Created mesh copy for undo  Triangles count  " + mRenderPair.mesh.triangles.Length / 3 + "  modded tris length  " + moddedMesh.triangles.Length / 3 + "  moddedMesh.HashCode  " + moddedMesh.GetHashCode() + "  created undo mesh hashcode   " + mRenderPair.mesh.GetHashCode());
                                                    
                                                    mRendererPair.mesh = moddedMesh;
                                                }
                                                else
                                                {
                                                    EditorUtility.DisplayProgressBar("Saving Changes", $"Writing mesh changes to existing file {1}/{1}", ((float)1 / 1));

                                                    Mesh prevMeshCopy = Instantiate(mRendererPair.mesh);
                                                    prevMeshCopy.name = mRendererPair.mesh.name;
                                                    DataContainer.MeshRendererPair mRenderPair = new DataContainer.MeshRendererPair(true, prevMeshCopy);
                                                    originalMeshClone.Add(gameObject, mRenderPair);

                                                    //mRendererPair.mesh.Clear();
                                                    //EditorUtility.CopySerialized(moddedMesh, mRendererPair.mesh);
                                                    //mRendererPair.mesh.MakeSimilarToOtherMesh(moddedMesh);

                                                    //Overwrites the mesh assets and keeps references intact
                                                    if (OverwriteAssetAtPath(mRendererPair.mesh, moddedMesh)) { }
                                                    else
                                                    {
                                                        mRendererPair.mesh.Clear();
                                                        EditorUtility.CopySerialized(moddedMesh, mRendererPair.mesh);
                                                        DestroyImmediate(moddedMesh);
                                                    }

                                                }

                                                filter.sharedMesh = mRendererPair.mesh;

                                            }
                                        }

                                        else
                                        {
                                            SkinnedMeshRenderer sRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

                                            if (sRenderer != null)
                                            {
                                                if (savedJustNow)
                                                {
                                                    Mesh prevMeshCopy = Instantiate(mRendererPair.mesh);
                                                    prevMeshCopy.name = mRendererPair.mesh.name;
                                                    DataContainer.MeshRendererPair mRenderPair = new DataContainer.MeshRendererPair(false, prevMeshCopy);
                                                    originalMeshClone.Add(gameObject, mRenderPair);

                                                    mRendererPair.mesh = moddedMesh;
                                                }
                                                else
                                                {
                                                    EditorUtility.DisplayProgressBar("Saving Changes", $"Writing mesh changes to existing file {1}/{1}", ((float)1 / 1));

                                                    Mesh prevMeshCopy = Instantiate(mRendererPair.mesh);
                                                    prevMeshCopy.name = mRendererPair.mesh.name;
                                                    DataContainer.MeshRendererPair mRenderPair = new DataContainer.MeshRendererPair(false, prevMeshCopy);
                                                    originalMeshClone.Add(gameObject, mRenderPair);

                                                    //mRendererPair.mesh.Clear();
                                                    //EditorUtility.CopySerialized(moddedMesh, mRendererPair.mesh);
                                                    //mRendererPair.mesh.MakeSimilarToOtherMesh(moddedMesh);

                                                    //Overwrites the mesh assets and keeps references intact
                                                    if (OverwriteAssetAtPath(mRendererPair.mesh, moddedMesh)) { }
                                                    else
                                                    {
                                                        mRendererPair.mesh.Clear();
                                                        EditorUtility.CopySerialized(moddedMesh, mRendererPair.mesh);
                                                        DestroyImmediate(moddedMesh);
                                                    }

                                                }

                                                sRenderer.sharedMesh = mRendererPair.mesh;
                                            }

                                        }


                                        EditorUtility.ClearProgressBar();

                                        // Required inorder to force unity to record changes on the modified object
                                        var tempO = gameObject.AddComponent<RefreshEnforcer>();
                                        EditorSceneManager.MarkSceneDirty(Selection.activeGameObject.scene);
                                        UtilityServices.dataContainer.objectMeshPairs = UtilityServices.GetObjectMeshPairs(Selection.activeGameObject, true, true);
                                        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                                        ReductionPending = false;
                                        ReductionStrength = 0;
                                    }


                                }
#pragma warning disable

                                catch (Exception ex)
                                {
                                    ReductionPending = prevRedPendingVal;
                                    fullySucceeded = false;
                                }



                                // Add this operation to the undo ops for this object if reduction operation succeeded          
                                if (fullySucceeded)
                                {
                                    // Add here the undo record
                                    UtilityServices.SaveRecord(Selection.activeGameObject, false, true, originalMeshClone);
                                }

                                // Destroy the mesh copies if failed to reduce. This might fail as DestroyImmediate() might not be allowed from a secondary thread.
                                else if (originalMeshClone.Count > 0)
                                {
                                    // There will be just one value in this case because it's not a reduceDeep operation
                                    originalMeshClone[gameObject].Destruct();
                                    originalMeshClone = null;
                                }

                            }

                        }

                    }


                    #endregion Apply Changes here



                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(4);

                    EditorGUILayout.BeginHorizontal();



                    GUILayout.Space(180);


                    EditorGUILayout.EndHorizontal();


                    #endregion Section Header


                    #region Section body

                    GUILayout.Space(6);
                    UtilityServices.DrawHorizontalLine(new Color(105 / 255f, 105 / 255f, 105 / 255f), 1, 5);

                    #region  Reduction options

                    GUILayout.Space(8);
                    content = new GUIContent();
                    style = GUI.skin.label;
                    style.richText = true;




                    EditorGUILayout.BeginHorizontal();

                    bool previousValue;

                    content.text = "Preserve UV Foldover";
                    content.tooltip = "Check this option to preserve UV foldover areas. Usually these are the areas where sharp edges, corners or dents are formed in the mesh or simply the areas where the mesh folds over.";

                    width = 130;

                    EditorGUILayout.LabelField(content, style, GUILayout.Width(130));
                    previousValue = PreserveUVFoldover;
                    PreserveUVFoldover = EditorGUILayout.Toggle(PreserveUVFoldover, GUILayout.Width(28), GUILayout.ExpandWidth(false));

                    if (previousValue != PreserveUVFoldover && !applyForOptionsChange)
                    {
                        RunOnThreads = CheckOnThreads();
                        applyForOptionsChange = true;
                    }

                    GUILayout.Space(12);

                    content.text = "Preserve UV Seams";
                    content.tooltip = "Preserve the mesh areas where the UV seams are made.These are the areas where different UV islands are formed (usually the shallow polygon conjested areas).";


#if UNITY_2019_1_OR_NEWER
                
                    width = 124;                 
#else
                    width = 120;
#endif

                    EditorGUILayout.LabelField(content, style, GUILayout.Width(width));
                    previousValue = PreserveUVSeams;
                    PreserveUVSeams = EditorGUILayout.Toggle(PreserveUVSeams, GUILayout.Width(20), GUILayout.ExpandWidth(false));

                    if (previousValue != PreserveUVSeams && !applyForOptionsChange)
                    {
                        RunOnThreads = CheckOnThreads();
                        applyForOptionsChange = true;
                    }

                    EditorGUILayout.EndHorizontal();


                    EditorGUILayout.BeginHorizontal();


                    content.text = "Preserve Borders";
                    content.tooltip = "Check this option to preserve border edges of the mesh. Border edges are the edges that are unconnected and open. Preserving border edges might lead to lesser polygon reduction but can be helpful where you see serious mesh and texture distortions.";


                    EditorGUILayout.LabelField(content, style, GUILayout.Width(130));
                    previousValue = PreserveBorders;
                    PreserveBorders = EditorGUILayout.Toggle(PreserveBorders, GUILayout.Width(28), GUILayout.ExpandWidth(false));

                    if (previousValue != PreserveBorders && !applyForOptionsChange)
                    {
                        RunOnThreads = CheckOnThreads();
                        applyForOptionsChange = true;
                    }

                    GUILayout.Space(12);

                    //content.text = "Smart Linking";
                    //content.tooltip = "Smart linking links vertices that are very close to each other. This helps in the mesh simplification process where holes or other serious issues could arise. Disabling this (where not needed) can cause a minor performance gain.";
                    content.text = "Use Edge Sort";
                    content.tooltip = "Using edge sort can result in very good quality mesh simplification in some cases but can be a little slow to run.";




#if UNITY_2019_1_OR_NEWER

                    width = 124;               
                
#else
                    width = 120;
#endif

                    EditorGUILayout.LabelField(content, style, GUILayout.Width(width));

                    previousValue = UseEdgeSort;
                    UseEdgeSort = EditorGUILayout.Toggle(UseEdgeSort, GUILayout.Width(20), GUILayout.ExpandWidth(false));

                    if (previousValue != UseEdgeSort && !applyForOptionsChange)
                    {
                        RunOnThreads = CheckOnThreads();
                        applyForOptionsChange = true;
                    }


                    EditorGUILayout.EndHorizontal();


                    EditorGUILayout.BeginHorizontal();

                    
                    content.text = "Regard Curvature";
                    content.tooltip = "Check this option to take into account the discrete curvature of mesh surface during simplification. Taking surface curvature into account can result in very good quality mesh simplification, but it can slow the simplification process significantly.";


                    EditorGUILayout.LabelField(content, style, GUILayout.Width(130));


                    previousValue = RegardCurvature;

                    RegardCurvature = EditorGUILayout.Toggle(RegardCurvature, GUILayout.Width(28), GUILayout.ExpandWidth(false));

                    if (previousValue != RegardCurvature && !applyForOptionsChange)
                    {
                        RunOnThreads = CheckOnThreads();
                        applyForOptionsChange = true;
                    }


                    GUILayout.Space(12);

                    content.text = "Recalculate Normals";
                    content.tooltip = "Recalculate mesh normals after simplification. Use this option if you see incorrect lighting or dark regions on the simplified mesh(es). This also recalculates the tangents afterwards.";



#if UNITY_2019_1_OR_NEWER

                    width = 124;               
                
#else
                    width = 120;
#endif

                    EditorGUILayout.LabelField(content, style, GUILayout.Width(width));

                    previousValue = RecalculateNormals;
                    RecalculateNormals = EditorGUILayout.Toggle(RecalculateNormals, GUILayout.Width(20), GUILayout.ExpandWidth(false));

                    if (previousValue != RecalculateNormals && !applyForOptionsChange)
                    {
                        RunOnThreads = CheckOnThreads();
                        applyForOptionsChange = true;
                    }



                    EditorGUILayout.EndHorizontal();


                    GUILayout.Space(10);

                    GUILayout.BeginHorizontal();


                    content.text = "Aggressiveness";
                    content.tooltip = "The aggressiveness of the reduction algorithm. Higher number equals higher quality, but more expensive to run. Lowest value is 7. Only valid if \"Use Edge Sort\" is unchecked.";


                    EditorGUI.BeginDisabledGroup(UseEdgeSort);


#if UNITY_2019_1_OR_NEWER

                    GUILayout.Space(1);
                    EditorGUILayout.LabelField(content, GUILayout.Width(129));              
                
#else
                    EditorGUILayout.LabelField(content, GUILayout.Width(131));
#endif


                    content.text = "";
                    //aggressiveness = Mathf.Abs(EditorGUILayout.FloatField(content, aggressiveness, GUILayout.Width(168), GUILayout.ExpandWidth(false)));
                    float previous = Aggressiveness;
                    Aggressiveness = Mathf.Abs(EditorGUILayout.FloatField(content, Aggressiveness, GUILayout.Width(168), GUILayout.ExpandWidth(true)));

                    if (Aggressiveness < 7) { Aggressiveness = 7; }

                    if (!Mathf.Approximately(previous, Aggressiveness) && !applyForOptionsChange)
                    {
                        RunOnThreads = CheckOnThreads();
                        applyForOptionsChange = true;
                    }


                    EditorGUI.EndDisabledGroup();

                    GUILayout.EndHorizontal();


                    GUILayout.Space(2);


                    GUILayout.BeginHorizontal();


                    content.text = "Max Iterations";
                    content.tooltip = "The maximum passes the reduction algorithm does. Higher number is more expensive but can bring you closer to your target quality. 100 is the lowest allowed value. Only valid if \"Use Edge Sort\" is unchecked.";

                    EditorGUI.BeginDisabledGroup(UseEdgeSort);


#if UNITY_2019_1_OR_NEWER

                    GUILayout.Space(1);
                    EditorGUILayout.LabelField(content, GUILayout.Width(129));               
                
#else
                    EditorGUILayout.LabelField(content, GUILayout.Width(131));
#endif


                    content.text = "";
                    //maxIterations = Mathf.Abs(EditorGUILayout.IntField(content, maxIterations, GUILayout.Width(168), GUILayout.ExpandWidth(false)));
                    int temp = MaxIterations;
                    MaxIterations = Mathf.Abs(EditorGUILayout.IntField(content, MaxIterations, GUILayout.Width(168), GUILayout.ExpandWidth(true)));

                    if (MaxIterations < 100) { MaxIterations = 100; }

                    if (!Mathf.Approximately(temp, MaxIterations) && !applyForOptionsChange)
                    {
                        RunOnThreads = CheckOnThreads();
                        applyForOptionsChange = true;
                    }


                    EditorGUI.EndDisabledGroup();

                    GUILayout.EndHorizontal();


                    GUILayout.Space(10);


                    #region Preservation Sphere

                    EditorGUILayout.BeginHorizontal();

                    content.text = "Tolerance Spheres";
                    content.tooltip = "Check this option to enable the tolerance spheres. Adding a tolerance sphere allows you to encompass specific areas of the Mesh that you want to preserve polygons of during the reduction process. This can leave such areas of the mesh with the original quality by ignoring them during the reduction process. Please note that reduction with preservation spheres might get slow.";

                    EditorGUILayout.LabelField(content, style, GUILayout.Width(130));

                    previousValue = IsPreservationActive;

                    IsPreservationActive = EditorGUILayout.Toggle(IsPreservationActive, GUILayout.Width(28), GUILayout.ExpandWidth(false));


                    if (previousValue != IsPreservationActive && !applyForOptionsChange)
                    {
                        RunOnThreads = CheckOnThreads();
                        applyForOptionsChange = true;
                    }



#if UNITY_2019_1_OR_NEWER

                    GUILayout.Space(15);                
                
#else
                    GUILayout.Space(15);
#endif


                    EditorGUI.BeginDisabledGroup(!IsPreservationActive);


                    #region LOAD PRESET

                    originalColor = GUI.backgroundColor;
                    GUI.backgroundColor = UtilityServices.HexToColor("#F5F5DC");

                    content = new GUIContent();

                    //GUILayout.FlexibleSpace();
                    style = GUI.skin.button;
                    content.tooltip = "Load an already saved tolerance sphere preset";

                    content.text = "<size=11><b>Load Preset</b></size>";


                    if (GUILayout.Button(content, style, GUILayout.Width(20), GUILayout.MaxHeight(24), GUILayout.ExpandWidth(true)))
                    {

                        string path = EditorUtility.OpenFilePanel("Open Tolerance Spheres Preset", "Assets/", "spp");
                        
                        // User pressed the cancel button
                        if (string.IsNullOrWhiteSpace(path)) { }

                        else
                        {
                            string presetJson = File.ReadAllText(path);
                            DataContainer.ToleranceSphereJson[] spheresJsonable = null;
                            bool failed = false;

                            try
                            {
                                spheresJsonable = JsonUtilityArrays.FromJson<DataContainer.ToleranceSphereJson>(presetJson);
                            }

                            catch(Exception ex)
                            {
                                failed = true;
                            }

                            if(spheresJsonable == null || failed)
                            {
                                EditorUtility.DisplayDialog("Failed", "Failed to load preset. Please check that the specfied file is a valid tolerance spheres preset file", "Ok");
                                failed = true;
                            }

                            if (!failed)
                            {
        
                                dataContainer.toleranceSpheres = new List<DataContainer.ToleranceSphere>();

                                foreach(var sphere in spheresJsonable)
                                {
                                    var toleranceSphere = ScriptableObject.CreateInstance(typeof(DataContainer.ToleranceSphere)) as DataContainer.ToleranceSphere;
                                    toleranceSphere.SetProperties(sphere);

                                    dataContainer.toleranceSpheres.Add(toleranceSphere);
                                }

                                if (dataContainer.currentLodLevelSettings != null && dataContainer.currentLodLevelSettings.Count > 0)
                                {
                                    foreach (var lodLevel in dataContainer.currentLodLevelSettings)
                                    {
                                        lodLevel.sphereIntensities = new List<float>();

                                        foreach(var sphere in spheresJsonable)
                                        {
                                            lodLevel.sphereIntensities.Add(sphere.preservationStrength);
                                        }

                                        if (lodLevel.sphereIntensities.Count == 0) { lodLevel.intensityFoldout = false; }
                                    }
                                }
                            }


                        }

                    }

                    GUI.backgroundColor = originalColor;

                    #endregion LOAD PRESET

                    GUILayout.Space(4);

                    #region SAVE PRESET

                    EditorGUI.BeginDisabledGroup(dataContainer.toleranceSpheres == null || dataContainer.toleranceSpheres.Count == 0);


                    originalColor = GUI.backgroundColor;
                    GUI.backgroundColor = UtilityServices.HexToColor("#F5F5DC");

                    content = new GUIContent();

                    //GUILayout.FlexibleSpace();
                    style = GUI.skin.button;
                    content.tooltip = "Save these tolerance sphere settings as a new preset which can be loaded later on";

                    content.text = "<size=11><b>Save Preset</b></size>";


                    if (GUILayout.Button(content, style, GUILayout.Width(20), GUILayout.MaxHeight(24), GUILayout.ExpandWidth(true)))
                    {

                        string path = EditorUtility.SaveFilePanel("Save Tolerance Spheres Preset", "Assets/", "PRESET_NAME", "spp");
                        
                        // User pressed the cancel button
                        if (string.IsNullOrWhiteSpace(path)) { }

                        else
                        {
                            DataContainer.ToleranceSphereJson[] spheresJsonable = new DataContainer.ToleranceSphereJson[dataContainer.toleranceSpheres.Count];

                            for (int a = 0; a < dataContainer.toleranceSpheres.Count; a++)
                            {
                                var toleranceSphere = dataContainer.toleranceSpheres[a];
                                spheresJsonable[a] = new DataContainer.ToleranceSphereJson(toleranceSphere);
                            }


                            string preset = JsonUtilityArrays.ToJson<DataContainer.ToleranceSphereJson>(spheresJsonable, true);

                            System.IO.File.WriteAllText(path, preset);
                        }

                    }

                    GUI.backgroundColor = originalColor;


                    EditorGUI.EndDisabledGroup();

                    #endregion SAVE PRESET


                    GUILayout.Space(30);

                    #region ADD TOLERANCE SPHERE

                    content = new GUIContent();

                    //GUILayout.FlexibleSpace();
                    style = GUI.skin.button;
                    content.tooltip = "Add a new tolerance sphere";

                    content.text = "<b>Add Sphere</b>";

                    if (GUILayout.Button(content, style, GUILayout.Width(20), GUILayout.MaxHeight(24), GUILayout.ExpandWidth(true)))
                    {
                        Transform selected = Selection.activeTransform;

                        Vector3 lossyScale = selected.lossyScale;
                        float avg = UtilityServices.Average(lossyScale.x, lossyScale.y, lossyScale.z);
                        float sphereDiameter = SphereDefaultDiameter * avg;

                        Vector3 worldPosition = new Vector3(selected.position.x + (lossyScale.x / 2f + sphereDiameter / 2f), selected.position.y, selected.position.z);

                        //DataContainer.ToleranceSphere sphere = new DataContainer.ToleranceSphere(worldPosition, sphereDiameter, sphereDefaultColor, selected.gameObject, 100f);

                        DataContainer.ToleranceSphere sphere = ScriptableObject.CreateInstance(typeof(DataContainer.ToleranceSphere)) as DataContainer.ToleranceSphere;
                        sphere.SetProperties(worldPosition, sphereDiameter, sphereDefaultColor, 100f);

                        dataContainer.toleranceSpheres.Add(sphere);
                        
                        foreach (var lodLevel in dataContainer.currentLodLevelSettings)
                        {
                            lodLevel.sphereIntensities.Add(100f);
                        }

                    }



                    #endregion ADD TOLERANCE SPHERE


                    EditorGUILayout.EndHorizontal();


                    GUILayout.Space(2);


                    
                    #region Draw Tolerance Spheres Settings


                    for (int a = 0; a < dataContainer.toleranceSpheres.Count; a++)
                    {

                        var toleranceSphere = dataContainer.toleranceSpheres[a];

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                        content = new GUIContent();  //FF6347ff  //006699
                        content.text = String.Format("<b><color=#3e2723>Sphere {0}</color></b>", a + 1);

                        style = GUI.skin.label;
                        style.richText = true;

                        GUILayout.Label(content, style);

                        GUILayout.Space(190);

                        #region DUPLICATE TOLERANCE SPHERE

                        style = GUI.skin.button;
                        style.richText = true;

                        originalColor = new Color(GUI.backgroundColor.r, GUI.backgroundColor.g, GUI.backgroundColor.b);

                        content = new GUIContent();
                        content.text = "<size=11><color=#006699><b>Duplicate</b></color></size>";
                        content.tooltip = $"Creates a duplicate of this tolerance sphere";


                        didPressButton = GUILayout.Button(content, style, GUILayout.Width(20), GUILayout.Height(17), GUILayout.ExpandWidth(true));

                        GUI.backgroundColor = originalColor;


                        if (didPressButton)
                        {
                            Transform selected = Selection.activeTransform;

                            DataContainer.ToleranceSphere sphere = ScriptableObject.CreateInstance(typeof(DataContainer.ToleranceSphere)) as DataContainer.ToleranceSphere;
                            sphere.SetProperties(toleranceSphere.worldPosition, toleranceSphere.diameter, toleranceSphere.color, toleranceSphere.preservationStrength, toleranceSphere.isHidden);
                            
                            dataContainer.toleranceSpheres.Add(sphere);

                            foreach (var lodLevel in dataContainer.currentLodLevelSettings)
                            {
                                lodLevel.sphereIntensities.Add(toleranceSphere.preservationStrength);
                            }

                        }

                        #endregion DUPLICATE TOLERANCE SPHERE


                        GUILayout.Space(10);


                        #region HIDE/UNHIDE SPHERE

                        originalColor = GUI.backgroundColor;
                        //GUI.backgroundColor = UtilityServices.HexToColor("#EEFAFF");

                        string text = toleranceSphere.isHidden ? "Unhide this tolerance sphere" : "Hide this tolerance sphere";
                        GUIContent HideTogglecontent = new GUIContent();
                        HideTogglecontent.tooltip = text;
                        HideTogglecontent.image = toleranceSphere.isHidden ? EditorGUIUtility.Load(ICON_PATH + "hide.png") as Texture : EditorGUIUtility.Load(ICON_PATH + "unhide.png") as Texture;

                        if (GUILayout.Button(HideTogglecontent, GUILayout.Width(38), GUILayout.Height(17)))
                        {
                            toleranceSphere.isHidden = !toleranceSphere.isHidden;
                        }

                        GUI.backgroundColor = originalColor;

                        #endregion HIDE/UNHIDE SPHERE

                        GUILayout.Space(2);

                        var previousBackgroundColor = GUI.backgroundColor;
                        GUI.backgroundColor = new Color(Color.gray.r, Color.gray.g, Color.gray.b, 0.8f);
                        GUIContent deleteSphereButtonContent = new GUIContent("<b><color=#FFFFFFD2>X</color></b>", "Remove this tolerance sphere.");
                        style = GUI.skin.button;
                        style.richText = true;

                        if (GUILayout.Button(deleteSphereButtonContent, GUILayout.Width(20)))
                        {
                            dataContainer.toleranceSpheres.RemoveAt(a);

                            foreach (var lodLevel in dataContainer.currentLodLevelSettings)
                            {
                                lodLevel.sphereIntensities.RemoveAt(a);

                                if(lodLevel.sphereIntensities.Count == 0) { lodLevel.intensityFoldout = false; }
                            }

                            a--;
                            continue;
                        }

                        GUI.backgroundColor = previousBackgroundColor;

                        EditorGUILayout.EndHorizontal();


                        GUILayout.Space(6);



                        GUILayout.BeginHorizontal();


                        style = GUI.skin.label;
                        style.richText = true;
                        content.text = "Position";
                        content.tooltip = "The current position values of this tolerance sphere in world space.";


                        EditorGUILayout.LabelField(content, style, GUILayout.Width(124));

                        // the reference to the scriptable object might be null
                        if(toleranceSphere != null)
                        {
                            Undo.RecordObject(toleranceSphere, "Tolerance Sphere inspector change");
                        }
                        


                        toleranceSphere.worldPosition = EditorGUILayout.Vector3Field("", toleranceSphere.worldPosition, GUILayout.Width(140), GUILayout.ExpandWidth(true));

                        

                        GUILayout.Space(21);


                        GUILayout.EndHorizontal();


                        GUILayout.BeginHorizontal();


                        content.text = "Sphere Size";
                        content.tooltip = "The diameter of this tolerance sphere.";

                        

#if UNITY_2019_1_OR_NEWER
                
                        GUILayout.Space(1);
                        EditorGUILayout.LabelField(content, GUILayout.Width(124));               
#else
                        EditorGUILayout.LabelField(content, GUILayout.Width(126));

#endif

                        content.text = "";

                        float newDiameter = 0;


#if UNITY_2019_1_OR_NEWER
                
                        newDiameter = Mathf.Abs(EditorGUILayout.FloatField(content, toleranceSphere.diameter, GUILayout.Width(42), GUILayout.ExpandWidth(true)));
             
#else
                        newDiameter = Mathf.Abs(EditorGUILayout.FloatField(content, toleranceSphere.diameter, GUILayout.Width(45), GUILayout.ExpandWidth(true)));

#endif


                        if (!Mathf.Approximately(newDiameter, 0))
                        {
                            toleranceSphere.diameter = newDiameter;
                        }


                        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                        GUILayout.EndHorizontal();


                        style = GUI.skin.label;
                        style.richText = true;
                        content.text = "Colour";
                        content.tooltip = "Change the color of the tolerance sphere.";


#if UNITY_2019_1_OR_NEWER
                
                        EditorGUILayout.LabelField(content, style, GUILayout.Width(51));
             
#else
                        EditorGUILayout.LabelField(content, style, GUILayout.Width(51));

#endif


#if UNITY_2019_1_OR_NEWER
                
                        GUILayout.Space(3);
                        toleranceSphere.color = EditorGUILayout.ColorField(toleranceSphere.color, GUILayout.Width(48), GUILayout.ExpandWidth(true));             
#else
                        toleranceSphere.color = EditorGUILayout.ColorField(toleranceSphere.color, GUILayout.Width(52), GUILayout.ExpandWidth(true));
#endif


                        GUILayout.Space(3);



                        GUILayout.EndHorizontal();





                        EditorGUILayout.BeginHorizontal();

                        EditorGUILayout.EndHorizontal();


#if UNITY_2019_1_OR_NEWER
                
                        GUILayout.Space(2);
           
#else
                        GUILayout.Space(1);
#endif

                        #region Preservation Strength Slider

                        GUILayout.BeginHorizontal();
                        
                        content = new GUIContent();
                        style = GUI.skin.label;
                        style.richText = true;

                        content.text = "Sphere Intensity";
                        content.tooltip = "The percentage of triangles to preserve in the region enclosed by this preservation sphere.";



                        EditorGUILayout.LabelField(content, style, GUILayout.Width(125));


                        width = 137;


                        float oldValue = toleranceSphere.preservationStrength;
                        toleranceSphere.preservationStrength = Mathf.Abs(GUILayout.HorizontalSlider(toleranceSphere.preservationStrength, 0, 100, GUILayout.Width(width), GUILayout.ExpandWidth(true)));
                        style = GUI.skin.textField;
                        
                        //if (!Mathf.Approximately(oldValue, toleranceSphere.preservationStrength) && !applyForOptionsChange)
                        //{
                        //    RunOnThreads = CheckOnThreads();
                        //    applyForOptionsChange = true;
                        //}


                        GUILayout.Space(5);

                        content.text = "";

                        oldValue = toleranceSphere.preservationStrength;
                        toleranceSphere.preservationStrength = Mathf.Abs(EditorGUILayout.DelayedFloatField(content, toleranceSphere.preservationStrength, style, GUILayout.Width(10), GUILayout.ExpandWidth(true)));

                        
                        if ((int)toleranceSphere.preservationStrength > 100)
                        {
                            toleranceSphere.preservationStrength = GetFirstNDigits((int)toleranceSphere.preservationStrength, 2);
                        }


                        //if (!Mathf.Approximately(oldValue, toleranceSphere.preservationStrength) && !applyForOptionsChange)
                        //{
                        //    RunOnThreads = CheckOnThreads();
                        //    applyForOptionsChange = true;
                        //}


#if UNITY_2019_1_OR_NEWER
                
                        width = 15;
           
#else
                        width = 20;
#endif

                        style = GUI.skin.label;
                        content.text = "<b><size=13>%</size></b>";
                        EditorGUILayout.LabelField(content, style, GUILayout.Width(width));



                        GUILayout.EndHorizontal();

                        #endregion Preservation Strength Slider



                        EditorGUILayout.EndVertical();


                    }



                    #endregion Draw Tolerance Spheres Settings


                    #endregion Reduction options

                    EditorGUI.EndDisabledGroup();

                    #region Reduction section


                    GUILayout.Space(8);
                    UtilityServices.DrawHorizontalLine(new Color(105 / 255f, 105 / 255f, 105 / 255f), 1, 5);
                    GUILayout.Space(8);


                    GUILayout.BeginHorizontal();

                    content = new GUIContent();
                    style = GUI.skin.label;
                    style.richText = true;
                    prevPadding = new RectOffset(style.padding.left, style.padding.right, style.padding.top, style.padding.bottom);

                    style.padding.left = -2;

                    content.text = "Reduction Strength";
                    content.tooltip = "The intensity of the reduction process. This is the amount in percentage to reduce the model by.";



#if UNITY_2019_1_OR_NEWER
                
                    GUILayout.Space(2);
           
#else
                    GUILayout.Space(4);
#endif

                    EditorGUILayout.LabelField(content, style, GUILayout.Width(127));
                    style.padding = prevPadding;


                    if (Mathf.Approximately(ReductionStrength, 0)) { applyForOptionsChange = false; }

                    float oldStrength = ReductionStrength;
                    bool isMeshless = ReduceDeep ? false : UtilityServices.IsMeshless(Selection.activeTransform);
                    hasLods = UtilityServices.HasLODs(Selection.activeGameObject);




                    ReductionStrength = Mathf.Abs(GUILayout.HorizontalSlider(ReductionStrength, 0, 100, GUILayout.Width(138), GUILayout.ExpandWidth(true)));


                    float quality = 1f - (ReductionStrength / 100f);
                    bool isFeasible1 = !Mathf.Approximately(oldStrength, ReductionStrength) && (!isMeshless && !hasLods);
                    bool isFeasible2 = applyForOptionsChange && (!isMeshless && !hasLods);


                    //Debug.Log("IsFeasible1?   "  +isFeasible1 + " !Mathf.Approximately(oldStrength, reductionStrength)?  " + !Mathf.Approximately(oldStrength, reductionStrength) + "  !Mathf.Approximately(reductionStrength, 0)?  " + !Mathf.Approximately(reductionStrength, 0) + "  Flag?  " + flag + "  ReductionStrenght is  " +reductionStrength);
                    //Debug.Log("IsFeasibl2?   "  +isFeasible2 + " applyForReduceDeep?  " + applyForReduceDeep);

                    if (!Mathf.Approximately(oldStrength, ReductionStrength))
                    {

                        if (isMeshless)
                        {
                            EditorUtility.DisplayDialog("Meshless Object", "This object appears to have no feasible mesh for reduction. You might want to enable \"Reduce Deep\" to consider the nested children for reduction.", "Ok");
                            ReductionStrength = oldStrength;
                        }
                        else if (hasLods)
                        {
                            EditorUtility.DisplayDialog("LODs found under this object", "This object appears to have an LOD group or LOD assets generated. Please remove them first before trying to simplify the mesh for this object", "Ok");
                            ReductionStrength = oldStrength;
                        }
                    }

                    if (isFeasible1 || isFeasible2)
                    {
                        ReductionPending = true;


                        try
                        {
                            // PRESERVATION SPHERE CAUSING SLOW BEHAVIOUR ON REDUCTION
                            if (ReduceDeep)
                            {
                                int prevTriangleCount = TriangleCount;

                                //System.Diagnostics.Stopwatch w = new System.Diagnostics.Stopwatch();
                                //w.Start();

                                bool isToleranceActive = false;

                                if (dataContainer.toleranceSpheres == null || dataContainer.toleranceSpheres.Count == 0)
                                {
                                    isToleranceActive = false;
                                }
                                else if (IsPreservationActive)
                                {
                                    isToleranceActive = true;
                                }


                                try
                                {
                                    //var w = new System.Diagnostics.Stopwatch();
                                    //w.Start();
                                    TriangleCount = UtilityServices.SimplifyObjectDeep(dataContainer.objectMeshPairs, dataContainer.toleranceSpheres, RunOnThreads, isToleranceActive, quality, (string err) =>
                                    {
                                        applyForOptionsChange = false;
                                        Debug.LogError(err);
                                        TriangleCount = prevTriangleCount;
                                    });

                                    //w.Stop();
                                    //Debug.Log("OVERALL Time ellapsed =  " + w.ElapsedMilliseconds);
                                }

                                catch (Exception ex)
                                {
                                    Debug.LogError(ex.ToString());
                                }


                            }


                            else
                            {
                                DataContainer.MeshRendererPair meshRendererPair;
                                GameObject selectedObject = Selection.activeGameObject;

                                //EditorUtility.DisplayProgressBar("Reducing Mesh", "Simplifying selected object's mesh. Depending on the mesh complexity this might take some time.", 0);

                                if (dataContainer.objectMeshPairs.TryGetValue(selectedObject, out meshRendererPair))
                                {
                                    bool isToleranceActive = false;

                                    if (dataContainer.toleranceSpheres == null || dataContainer.toleranceSpheres.Count == 0)
                                    {
                                        isToleranceActive = false;
                                    }
                                    else if (IsPreservationActive)
                                    {
                                        isToleranceActive = true;
                                    }

                                    TriangleCount = SimplifyObjectShallow(meshRendererPair, dataContainer.toleranceSpheres, selectedObject, isToleranceActive, quality);
                                }


                                if (applyForOptionsChange)
                                {
                                    //Debug.Log("reduce deep was unchecked so restoring other meshes quality is:   " +quality + "  ISFeasible1?  " + isFeasible1 +  "IsFeasible2  " + isFeasible2 + " !Mathf.Approximately(quality, 0)?  " + !Mathf.Approximately(quality, 0));
                                    UtilityServices.RestoreMeshesFromPairs(dataContainer.objectMeshPairs, Selection.activeGameObject);
                                }

                            }


                        }

                        catch (Exception ex)
                        {
                            //EditorUtility.ClearProgressBar();
                            applyForOptionsChange = false;
                        }

                        //areAllMeshesSaved = AreAllMeshesSaved(Selection.activeGameObject, true); Might not need this

                        applyForOptionsChange = false;
                        //EditorUtility.ClearProgressBar();
                    }


                    style = GUI.skin.textField;

                    GUILayout.Space(5);

                    content.text = "";

                    oldStrength = ReductionStrength;
                    
                    ReductionStrength = Mathf.Abs(EditorGUILayout.DelayedFloatField(content, ReductionStrength, style, GUILayout.Width(10), GUILayout.ExpandWidth(true)));

                    if ((int)ReductionStrength > 100)
                    {
                        ReductionStrength = GetFirstNDigits((int)ReductionStrength, 2);
                    }

                    if (!Mathf.Approximately(oldStrength, ReductionStrength))
                    {
                        if (isMeshless)
                        {
                            EditorUtility.DisplayDialog("Meshless Object", "This object appears to have no feasible mesh for reduction. You might want to enable \"Reduce Deep\" to consider the nested children for reduction.", "Ok");
                            ReductionStrength = oldStrength;
                        }
                        else if (hasLods)
                        {
                            EditorUtility.DisplayDialog("LODs found under this object", "This object appears to have an LOD group or LOD assets generated. Please remove them first before trying to simplify the mesh for this object", "Ok");
                            ReductionStrength = oldStrength;
                        }
                        else
                        {
                            applyForOptionsChange = true;
                        }


                        if (Mathf.Approximately(ReductionStrength, 0))
                        {
                            UtilityServices.RestoreMeshesFromPairs(dataContainer.objectMeshPairs);
                            TriangleCount = UtilityServices.CountTriangles(ReduceDeep, dataContainer.objectMeshPairs, Selection.activeGameObject);
                        }
                    }

                    //GUILayout.Space(2);

                    style = GUI.skin.label;
                    content.text = "<b><size=13>%</size></b>";
                    EditorGUILayout.LabelField(content, style, GUILayout.Width(20));



                    GUILayout.EndHorizontal();

                    GUILayout.Space(2);

                    GUILayout.BeginHorizontal();

                    content = new GUIContent();
                    style = GUI.skin.label;
                    style.richText = true;
                    prevPadding = new RectOffset(style.padding.left, style.padding.right, style.padding.top, style.padding.bottom);

                    style.padding.left = -2;

                    content.text = "Triangles Count";
                    content.tooltip = "The current number of triangles in the selected mesh.";

                    if (ReduceDeep)
                    {
                        content.tooltip = "The total number of triangles in the selected object. Includes the triangles of this mesh as well as all of its children meshes.";
                    }


#if UNITY_2019_1_OR_NEWER
                
                    GUILayout.Space(2);
           
#else
                    GUILayout.Space(4);
#endif


                    EditorGUILayout.LabelField(content, style, GUILayout.Width(127));
                    style.padding = prevPadding;

                    style = GUI.skin.textField;
                    content.text = TriangleCount.ToString();


                    //trianglesCount = Mathf.Abs(EditorGUILayout.IntField(content, trianglesCount, style, GUILayout.Width(50), GUILayout.ExpandWidth(true)));
                    EditorGUILayout.LabelField(content, style, GUILayout.Width(50), GUILayout.ExpandWidth(true));


                    GUILayout.EndHorizontal();


                    #endregion Reduction section


                    #endregion Section body


                    #endregion Section body

                    #region AUTO LOD

                    GUILayout.Space(12);

                    UtilityServices.DrawHorizontalLine(Color.black, 1, 8);

                    #region TITLE HEADER

                    GUILayout.Space(4);

                    EditorGUILayout.BeginHorizontal();

                    content = new GUIContent();
                    content.text = "<size=13><b><color=#A52A2AFF>AUTOMATIC LOD</color></b></size>";
                    content.tooltip = "Expand this section to see options for automatic LOD generation.";

                    style = EditorStyles.foldout;
                    style.richText = true;  // #FF6347ff  //A52A2AFF
                    prevPadding = new RectOffset(style.padding.left, style.padding.right, style.padding.top, style.padding.bottom);
                    style.padding = new RectOffset(20, 0, -1, 0);

                    GUILayout.Space(19);
                    FoldoutAutoLOD = EditorGUILayout.Foldout(FoldoutAutoLOD, content, true, style);

                    style.padding = prevPadding;

                    style = new GUIStyle();
                    style.richText = true;

                    EditorGUILayout.EndHorizontal();


                    #endregion TITLE HEADER



                    if (FoldoutAutoLOD)
                    {

                        UtilityServices.DrawHorizontalLine(Color.black, 1, 8);
                        GUILayout.Space(6);

                        #region Section Header


                        GUILayout.Space(6);

                        EditorGUILayout.BeginHorizontal();


                        #region Change Save Path


                        style = GUI.skin.button;
                        style.richText = true;


                        content = new GUIContent();
                        content.text = "<b><size=11><color=#006699>Change Save Path</color></size></b>";
                        content.tooltip = "Change the path where the generated LODs mesh assets will be saved. If you don't select a path the default path will be used. Please note that the chosen path will be used for saving LOD mesh assets in the future, unless changed.";

                        if (GUILayout.Button(content, style, GUILayout.Width(134), GUILayout.Height(20), GUILayout.ExpandWidth(false)))
                        {
                            //sphereDefaultColor

                            string toOpen = String.IsNullOrWhiteSpace(AutoLODSavePath) ? "Assets/" : AutoLODSavePath;

                            if(!String.IsNullOrWhiteSpace(toOpen))
                            {
                                if (toOpen.EndsWith("/")) { toOpen.Remove(toOpen.Length - 1, 1); }

                                if (!AssetDatabase.IsValidFolder(toOpen))
                                {
                                    toOpen = "Assets/";
                                }
                            }


                            string path = EditorUtility.OpenFolderPanel("Choose LOD Assets Save path", toOpen, "");

                            //Validate the save path. It might be outside the assets folder   

                            // User pressed the cancel button
                            if (string.IsNullOrWhiteSpace(path)) { }

                            else if (!UtilityServices.IsPathInAssetsDir(path))
                            {
                                EditorUtility.DisplayDialog("Invalid Path", "The path you chose is not valid.Please choose a path that points to a directory that exists in the project's Assets folder.", "Ok");
                            }

                            else
                            {
                                path = UtilityServices.GetValidFolderPath(path);

                                if (!string.IsNullOrWhiteSpace(path))
                                {
                                    UtilityServices.AutoLODSavePath = UtilityServices.SetAndReturnStringPref("autoLODSavePath", path);
                                }
                            }

                        }


                        EditorGUI.EndDisabledGroup();

                        #endregion Change Save Path

                        GUILayout.Space(40);

                        #region Add LOD Level

                        content = new GUIContent();

                        //GUILayout.FlexibleSpace();
                        content.tooltip = "Add an LOD level.";

                        content.text = "<b>Add</b>";

                        if (GUILayout.Button(content, style, GUILayout.Width(40), GUILayout.MaxHeight(24), GUILayout.ExpandWidth(true)))
                        {
                            //if (dataContainer.currentLodLevelSettings.Count < UtilityServices.MAX_LOD_COUNT)
                            //{
                            List<float> strengths = new List<float>();
                            foreach(var sphere in dataContainer.toleranceSpheres) { strengths.Add(100f); }

                            dataContainer.currentLodLevelSettings.Add(new DataContainer.LODLevelSettings(0, 0, false, false, false, true, false, 7, 100, false, false, false, strengths));
                            //}
                        }


                        #endregion Add LOD Level

                        GUILayout.Space(2);

                        #region Generate LODs


                        style = GUI.skin.button;
                        style.richText = true;

                        originalColor = new Color(GUI.backgroundColor.r, GUI.backgroundColor.g, GUI.backgroundColor.b);
                        //# ffc14d   60%
                        //# F0FFFF   73%
                        //# F5F5DC   75%
                        GUI.backgroundColor = UtilityServices.HexToColor("#F5F5DC");

                        content = new GUIContent();
                        content.text = "<size=11> <b><color=#000000>Generate LODS</color></b> </size>";
                        content.tooltip = "Generate LODs for this GameObject with the settings specified. Please note that you must save the scene after successful generation of LODs and apply changes to any prefabs manually.";

                        didPressButton = GUILayout.Button(content, style, GUILayout.Width(120), GUILayout.Height(24), GUILayout.ExpandWidth(true));

                        GUI.backgroundColor = originalColor;


                        if (didPressButton)
                        {
                            UtilityServices.RestoreMeshesFromPairs(dataContainer.objectMeshPairs);
                            dataContainer.objectMeshPairs = UtilityServices.GetObjectMeshPairs(Selection.activeGameObject, true, true);
                            ReductionPending = false;
                            ReductionStrength = 0;


                            try
                            {

                                // Delete LOD levels that have 0 screen relative height and 0 reduction strength(Excluding the 1st one)

                                List<DataContainer.LODLevelSettings> levelsToDelete = new List<DataContainer.LODLevelSettings>();

                                if (dataContainer.currentLodLevelSettings.Count > 1)
                                {

                                    for (int a = 1; a < dataContainer.currentLodLevelSettings.Count; a++)
                                    {
                                        var lodLevel = dataContainer.currentLodLevelSettings[a];

                                        if (Mathf.Approximately(lodLevel.transitionHeight, 0))
                                        {
                                            levelsToDelete.Add(lodLevel);
                                        }

                                        if (Mathf.Approximately(lodLevel.reductionStrength, 0))
                                        {
                                            levelsToDelete.Add(lodLevel);
                                        }
                                    }
                                }

                                foreach (var toDelete in levelsToDelete)
                                {
                                    dataContainer.currentLodLevelSettings.Remove(toDelete);
                                }


                                bool isSuccess = UtilityServices.GenerateLODS(Selection.activeGameObject, dataContainer.toleranceSpheres, dataContainer.currentLodLevelSettings, UtilityServices.AutoLODSavePath, null, true, GenerateUV2LODs);
                                EditorUtility.ClearProgressBar();
                                
                                if (isSuccess)
                                {
                                    EditorSceneManager.MarkSceneDirty(Selection.activeGameObject.scene);
                                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

                                    ApplyExtraOptions(Selection.activeGameObject);
                                }

                                else
                                {
                                    EditorUtility.DisplayDialog("Failed", "Failed to generate LODs", "Ok");
                                }


                            }

                            catch (Exception error)
                            {
                                EditorUtility.ClearProgressBar();
                                EditorUtility.DisplayDialog("Failed to generate LODs. The LODs might be partially generated", error.ToString(), "Ok");
                            }

                            GUIUtility.ExitGUI();
                        }


                        #endregion Generate LODs


                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(2);

                        EditorGUILayout.BeginHorizontal();

#if UNITY_2019_1_OR_NEWER
                
                        //GUILayout.Space(177);
           
#else
                        //GUILayout.Space(174);
#endif



                        #region  Additional options

                        //enumMaskField
                        //style = EditorStyles.foldout;
                        //style = EditorStyles.toolbar;
                        //style = EditorStyles.popup;

                        style = EditorStyles.toolbarDropDown;
                        //style = GUI.skin.button;
                        style.richText = true;
                        
                        content = new GUIContent();
                        content.text = "<size=11><b>Set Extra Options</b></size>";
                        content.tooltip = "Set extra options for the LOD generation process";


                        if (GUILayout.Button(content, style, GUILayout.Width(134), GUILayout.Height(17), GUILayout.ExpandWidth(false)))
                        {

                            GenericMenu menu = new GenericMenu();


                            content = new GUIContent();
                            content.text = "Copy static flags to new objects";
                            content.tooltip = "Copy the static flags from this object to the newly created LOD objects";

                            menu.AddItem(content, CopyParentStaticFlags, (object obj) => { CopyParentStaticFlags = !CopyParentStaticFlags; }, "");


                            content = new GUIContent();
                            content.text = "Copy layer to new objects";
                            content.tooltip = "Copy layer from this object to the newly created LOD objects";

                            menu.AddItem(content, CopyParentLayer, (object obj) => { CopyParentLayer = !CopyParentLayer; }, "");


                            content = new GUIContent();
                            content.text = "Copy tag to new objects";
                            content.tooltip = "Copy tag from this object to the newly created LOD objects";

                            menu.AddItem(content, CopyParentTag, (object obj) => { CopyParentTag = !CopyParentTag; }, "");


                            content = new GUIContent();
                            content.text = "Generate UV2";
                            content.tooltip = "Should we generate uv2 with default settings for each mesh, and fill them in?. Note that generating uv2 can cause the LOD generation process to get slow";

                            menu.AddItem(content, GenerateUV2LODs, (object obj) => { GenerateUV2LODs = !GenerateUV2LODs; }, "");


                            menu.ShowAsContext();            

                        } 


                        #endregion  Additional options



#if UNITY_2019_1_OR_NEWER
                
                        GUILayout.Space(40);
#else
                        GUILayout.Space(40);
#endif



                        #region Copy Preview Settings

                        style = GUI.skin.button;
                        style.richText = true;

                        originalColor = new Color(GUI.backgroundColor.r, GUI.backgroundColor.g, GUI.backgroundColor.b);
                        //# ffc14d   60%
                        //# F0FFFF   73%
                        //# F5F5DC   75%
                        //GUI.backgroundColor = UtilityServices.HexToColor("#f9f5f5");

                        content = new GUIContent();
                        content.text = "<size=11><color=#006699><b>Copy Preview</b></color></size>";
                        content.tooltip = $"Copies all the settings from the preview above into each LOD level";


                        didPressButton = GUILayout.Button(content, style, GUILayout.Width(20), GUILayout.Height(17), GUILayout.ExpandWidth(true));

                        GUI.backgroundColor = originalColor;


                        if (didPressButton)
                        {

                            for (int a = 0; a < dataContainer.currentLodLevelSettings.Count; a++)
                            {
                                var lodLevel = dataContainer.currentLodLevelSettings[a];

                                // in Base level don't copy over any settings if reduction is 0
                                if(a == 0)
                                {
                                    if(!Mathf.Approximately(lodLevel.reductionStrength, 0))
                                    {
                                        CopyOverPreviewSettings(lodLevel);
                                    }
                                }

                                else
                                {
                                    CopyOverPreviewSettings(lodLevel);
                                }
                                
                            }

                        }

                        #endregion Copy Preview Settings


                        GUILayout.Space(2);

                        #region Destroy LODs


                        style = GUI.skin.button;
                        style.richText = true;

                        originalColor = new Color(GUI.backgroundColor.r, GUI.backgroundColor.g, GUI.backgroundColor.b);
                        //# ffc14d   60%
                        //# F0FFFF   73%
                        //# F5F5DC   75%
                        //GUI.backgroundColor = UtilityServices.HexToColor("#f9f5f5");

                        content = new GUIContent();
                        content.text = "<size=11><color=#006699><b>Destroy LODs</b></color></size>";
                        content.tooltip = $"Destroy the generated LODs for this mesh. This will also delete the \".mesh\" files in the folder \"{UtilityServices.LOD_ASSETS_DEFAULT_SAVE_PATH}\" that were created for this object during the LOD generation process. Please note that you will have to delete the empty folders manually.";

                        bool hasLODs = UtilityServices.HasLODs(Selection.activeGameObject);

                        EditorGUI.BeginDisabledGroup(!hasLODs);


                        didPressButton = GUILayout.Button(content, style, GUILayout.Height(17), GUILayout.ExpandWidth(true));


                        EditorGUI.EndDisabledGroup();

                        GUI.backgroundColor = originalColor;


                        if (didPressButton)
                        {
                            bool didSucceed = UtilityServices.DestroyLODs(Selection.activeGameObject);

                            if (didSucceed)
                            {
                                EditorUtility.DisplayDialog("Success", $"Successfully destroyed the LODS and deleted the associated mesh assets. Please note that you must delete the empty folders in the path {LOD_ASSETS_DEFAULT_SAVE_PATH} manually.", "Ok");
                                EditorSceneManager.MarkSceneDirty(Selection.activeGameObject.scene);
                                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                            }

                            GUIUtility.ExitGUI();
                        }


                        #endregion Destroy LODs


                        EditorGUILayout.EndHorizontal();


                        #endregion Section Header



                        GUILayout.Space(14);


                        #region Draw LOD Level


                        for (int a = 0; a < dataContainer.currentLodLevelSettings.Count; a++)
                        {

                            var lodLevel = dataContainer.currentLodLevelSettings[a];

                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                            content = new GUIContent();  //FF6347ff  //006699
                            content.text = String.Format("<b><color=#3e2723>Level {0}</color></b>", a + 1);

                            if (a == 0)
                            {
                                content.text = String.Format("<b><color=#3e2723>Level {0} (Base)</color></b>", a + 1);
                            }

                            style = GUI.skin.label;
                            style.richText = true;

                            GUILayout.Label(content, style);

                            var previousBackgroundColor = GUI.backgroundColor;
                            GUI.backgroundColor = new Color(Color.gray.r, Color.gray.g, Color.gray.b, 0.8f);
                            GUIContent deleteLevelButtonContent = new GUIContent("<b><color=#FFFFFFD2>X</color></b>", "Delete this LOD level.");
                            style = GUI.skin.button;
                            style.richText = true;

                            if (GUILayout.Button(deleteLevelButtonContent, GUILayout.Width(20)))
                            {
                                if (dataContainer.currentLodLevelSettings.Count > 1)
                                {
                                    dataContainer.currentLodLevelSettings.RemoveAt(a);
                                    a--;
                                }
                            }

                            GUI.backgroundColor = previousBackgroundColor;

                            EditorGUILayout.EndHorizontal();


                            GUILayout.Space(6);


                            #region Reduction Strength Slider

                            GUILayout.BeginHorizontal();

                            content = new GUIContent();
                            style = GUI.skin.label;
                            style.richText = true;
                            prevPadding = new RectOffset(style.padding.left, style.padding.right, style.padding.top, style.padding.bottom);

                            content.text = "Reduction Strength";
                            content.tooltip = "The intensity of the reduction process. This is the amount in percentage to reduce the model by in this LOD level. The lower this value the higher will be the quality of this LOD level. For the base level or level 1 you should keep this to 0.";


                            GUILayout.Space(16);

                            EditorGUILayout.LabelField(content, style, GUILayout.Width(115));


                            lodLevel.reductionStrength = Mathf.Abs(GUILayout.HorizontalSlider(lodLevel.reductionStrength, 0, 100, GUILayout.Width(130), GUILayout.ExpandWidth(true)));
                            style = GUI.skin.textField;

                            GUILayout.Space(5);

                            content.text = "";

                            lodLevel.reductionStrength = Mathf.Abs(EditorGUILayout.FloatField(content, lodLevel.reductionStrength, style, GUILayout.Width(10), GUILayout.ExpandWidth(true)));


                            if ((int)lodLevel.reductionStrength > 100)
                            {
                                lodLevel.reductionStrength = GetFirstNDigits((int)lodLevel.reductionStrength, 2);
                            }

                            style = GUI.skin.label;
                            content.text = "<b><size=13>%</size></b>";
                            EditorGUILayout.LabelField(content, style, GUILayout.Width(20));



                            GUILayout.EndHorizontal();

                            #endregion   Reduction Strength Slider


                            #region Screen relative transition height


                            GUILayout.BeginHorizontal();

                            content = new GUIContent();
                            style = GUI.skin.label;
                            style.richText = true;
                            prevPadding = new RectOffset(style.padding.left, style.padding.right, style.padding.top, style.padding.bottom);

                            content.text = "Transition Height";
                            content.tooltip = "The screen relative height controls how far the viewing camera must be from the object before a transition to the next LOD level is made.";


                            GUILayout.Space(16);

                            EditorGUILayout.LabelField(content, style, GUILayout.Width(115));

                            float oldHeight = lodLevel.transitionHeight;
                            lodLevel.transitionHeight = Mathf.Abs(GUILayout.HorizontalSlider(lodLevel.transitionHeight, 0, 1, GUILayout.Width(130), GUILayout.ExpandWidth(true)));
                            style = GUI.skin.textField;

                            GUILayout.Space(5);

                            content.text = "";

                            lodLevel.transitionHeight = Mathf.Abs(EditorGUILayout.FloatField(content, lodLevel.transitionHeight, style, GUILayout.Width(10), GUILayout.ExpandWidth(true)));
                            lodLevel.transitionHeight = Mathf.Clamp01(lodLevel.transitionHeight);

                            if (!Mathf.Approximately(oldHeight, lodLevel.transitionHeight) && a != 0)
                            {
                                float lastLevelHeight = dataContainer.currentLodLevelSettings[a - 1].transitionHeight;
                                float currentLevelHeight = lodLevel.transitionHeight;

                                if ((lastLevelHeight - currentLevelHeight) <= 0.05f)
                                {
                                    //Debug.Log($"Last level height  {lastLevelHeight}  currentLevelHeight = {currentLevelHeight}  Mathf.Abs(lastLevelHeight - currentLevelHeight)   " +(Mathf.Abs(lastLevelHeight - currentLevelHeight) + "  a is  " + a));
                                    lodLevel.transitionHeight = lastLevelHeight - 0.05f;
                                    lodLevel.transitionHeight = Mathf.Clamp01(lodLevel.transitionHeight);
                                }
                            }


                            if (!Mathf.Approximately(oldHeight, lodLevel.transitionHeight) && a != (dataContainer.currentLodLevelSettings.Count - 1))
                            {
                                float nextLevelHeight = dataContainer.currentLodLevelSettings[a + 1].transitionHeight;
                                float currentLevelHeight = lodLevel.transitionHeight;

                                if ((currentLevelHeight - nextLevelHeight) <= 0.05f)
                                {
                                    //Debug.Log($"Next level height  {nextLevelHeight}  currentLevelHeight = {currentLevelHeight}  Mathf.Abs(lastLevelHeight - currentLevelHeight)   " +(Mathf.Abs(lastLevelHeight - currentLevelHeight) + "  a is  " + a));
                                    lodLevel.transitionHeight = nextLevelHeight + 0.05f;
                                    lodLevel.transitionHeight = Mathf.Clamp01(lodLevel.transitionHeight);
                                }
                            }



                            GUILayout.Space(24);


                            GUILayout.EndHorizontal();


                            #endregion   Screen relative transition height


                            #region Reduction extra options

                            GUILayout.Space(2);

                            EditorGUILayout.BeginHorizontal();

                            GUILayout.Space(16);
                            content = new GUIContent();
                            content.text = "Reduction Options";
                            content.tooltip = "Expand this section to see options for mesh simplification for this LOD level.";




#if UNITY_2019_1_OR_NEWER
                
                            GUILayout.Space(1);
#endif

                            lodLevel.simplificationOptionsFoldout = EditorGUILayout.Foldout(lodLevel.simplificationOptionsFoldout, content, true);


                            EditorGUILayout.EndHorizontal();

                            if (lodLevel.simplificationOptionsFoldout)
                            {
                                EditorGUILayout.BeginHorizontal();
                                GUILayout.Space(6);
                                UtilityServices.DrawHorizontalLine(Color.black, 1, 8, 14);
                                EditorGUILayout.EndHorizontal();
                            }

                            EditorGUILayout.BeginHorizontal();

                            GUILayout.Space(16);


                            if (lodLevel.simplificationOptionsFoldout)
                            {

                                style = GUI.skin.label;

                                content.text = "Preserve UV Foldover";
                                content.tooltip = "Check this option to preserve UV foldover for this LOD level.";

                                EditorGUILayout.LabelField(content, style, GUILayout.Width(135));
                                lodLevel.preserveUVFoldover = EditorGUILayout.Toggle(lodLevel.preserveUVFoldover, GUILayout.Width(18), GUILayout.ExpandWidth(false));

                                GUILayout.Space(8);

                                content.text = "Preserve UV Seams";
                                content.tooltip = "Preserve the mesh areas where the UV seams are made.These are the areas where different UV islands are formed (usually the shallow polygon conjested areas).";

                                EditorGUILayout.LabelField(content, style, GUILayout.Width(135));
                                lodLevel.preserveUVSeams = EditorGUILayout.Toggle(lodLevel.preserveUVSeams, GUILayout.Width(20), GUILayout.ExpandWidth(false));


                                EditorGUILayout.EndHorizontal();



                                EditorGUILayout.BeginHorizontal();


                                GUILayout.Space(16);

                                content.text = "Preserve Borders";
                                content.tooltip = "Check this option to preserve border edges for this LOD level. Border edges are the edges that are unconnected and open. Preserving border edges might lead to lesser polygon reduction but can be helpful where you see serious mesh and texture distortions.";


                                EditorGUILayout.LabelField(content, style, GUILayout.Width(114));
                                GUILayout.Space(21);


                                lodLevel.preserveBorders = EditorGUILayout.Toggle(lodLevel.preserveBorders, GUILayout.Width(15), GUILayout.ExpandWidth(false));

                                GUILayout.Space(11);

                                //content.text = "Smart Linking";
                                //content.tooltip = "Smart linking links vertices that are very close to each other. This helps in the mesh simplification process where holes or other serious issues could arise. Disabling this (where not needed) can cause a minor performance gain.";
                                content.text = "Use Edge Sort";
                                content.tooltip = "Using edge sort can result in very good quality mesh simplification in some cases but can be a little slow to run.";


                                EditorGUILayout.LabelField(content, style, GUILayout.Width(114));
                                GUILayout.Space(21);


                                lodLevel.useEdgeSort = EditorGUILayout.Toggle(lodLevel.useEdgeSort, GUILayout.Width(10), GUILayout.ExpandWidth(false));


                                EditorGUILayout.EndHorizontal();



                                EditorGUILayout.BeginHorizontal();


                                content.text = "Regard Curvature";
                                content.tooltip = "Check this option to take into account the discrete curvature of mesh surface during simplification. Taking surface curvature into account can result in very good quality mesh simplification, but it can slow the simplification process significantly.";

                                GUILayout.Space(16);
                                EditorGUILayout.LabelField(content, style, GUILayout.Width(135));

                                lodLevel.regardCurvature = EditorGUILayout.Toggle(lodLevel.regardCurvature, GUILayout.Width(15));

                                
                                GUILayout.Space(11);

                                content.text = "Recalculate Normals";
                                content.tooltip = "Recalculate mesh normals after simplification in this LOD level. Use this option if you see incorrect lighting or dark regions on the simplified mesh(es). This also recalculates the tangents afterwards.";


                                EditorGUILayout.LabelField(content, style, GUILayout.Width(135));


                                lodLevel.recalculateNormals = EditorGUILayout.Toggle(lodLevel.recalculateNormals, GUILayout.Width(15), GUILayout.ExpandWidth(false));


                                EditorGUILayout.EndHorizontal();


                                GUILayout.BeginHorizontal();



#if UNITY_2019_1_OR_NEWER
                
                                GUILayout.Space(17);
              
#else
                                GUILayout.Space(16);

#endif

                                content.text = "Aggressiveness";
                                content.tooltip = "The agressiveness of the reduction algorithm to use for this LOD level. Higher number equals higher quality, but more expensive to run. Lowest value is 7.";



#if UNITY_2019_1_OR_NEWER
                
                                EditorGUILayout.LabelField(content, GUILayout.Width(134));
              
#else
                                EditorGUILayout.LabelField(content, GUILayout.Width(134));
                                GUILayout.Space(2);

#endif

                                content.text = "";

                                lodLevel.aggressiveness = Mathf.Abs(EditorGUILayout.FloatField(content, lodLevel.aggressiveness, GUILayout.Width(168), GUILayout.ExpandWidth(true)));

                                if (lodLevel.aggressiveness < 7) { lodLevel.aggressiveness = 7; }


                                GUILayout.EndHorizontal();



                                GUILayout.BeginHorizontal();



                                GUILayout.Space(16);

                                content.text = "Max Iterations";
                                content.tooltip = "The maximum passes the reduction algorithm does for this LOD level. Higher number is more expensive but can bring you closer to your target quality. 100 is the lowest allowed value.";



#if UNITY_2019_1_OR_NEWER
                
                                GUILayout.Space(1);
                                EditorGUILayout.LabelField(content, GUILayout.Width(134));
              
#else
                                EditorGUILayout.LabelField(content, GUILayout.Width(136));

#endif


                                content.text = "";



#if UNITY_2019_1_OR_NEWER
                
                                lodLevel.maxIterations = Mathf.Abs(EditorGUILayout.IntField(content, lodLevel.maxIterations, GUILayout.Width(168), GUILayout.ExpandWidth(true)));

              
#else
                                lodLevel.maxIterations = Mathf.Abs(EditorGUILayout.IntField(content, lodLevel.maxIterations, GUILayout.Width(168), GUILayout.ExpandWidth(true)));

#endif

                                if (lodLevel.maxIterations < 100) { lodLevel.maxIterations = 100; }

                            }


                            GUILayout.EndHorizontal();


                            #endregion Reduction extra options


                            #region Regard Tolerance Sphere And Combine Meshes

                            if (lodLevel.simplificationOptionsFoldout)
                            {
                                EditorGUILayout.BeginHorizontal();
                                GUILayout.Space(6);
                                UtilityServices.DrawHorizontalLine(Color.black, 1, 8, 14);
                                EditorGUILayout.EndHorizontal();
                            }


                            EditorGUILayout.BeginHorizontal();

                            content.text = "Regard Tolerance";
                            content.tooltip = "Check this option if you want this LOD level to regard the tolerance sphere and retain the original quality of the mesh area enclosed within the tolerance sphere. Please note that the LOD generation for this level with preservation sphere might get slow.";
                            style = GUI.skin.label;

                            GUILayout.Space(16);

                            EditorGUILayout.LabelField(content, style, GUILayout.Width(114));

                            lodLevel.regardTolerance = EditorGUILayout.Toggle(lodLevel.regardTolerance, GUILayout.Width(28), GUILayout.ExpandWidth(false));


                            content.text = "Combine Meshes";
                            content.tooltip = "[Deprecated] Combine all renderers and meshes under this level into one, where possible. Please note that this option is present just in case if someone has any special use case where they need to generate LODs with some levels having combined meshes and some having uncombined meshes. You can now use BatchFew to combine meshes exclusively.";



#if UNITY_2019_1_OR_NEWER
                
                            width = 108;

              
#else
                            GUILayout.Space(2);
                            width = 109;
#endif

                            // Added it to Batch few
                            EditorGUILayout.LabelField(content, style, GUILayout.Width(width));

                            lodLevel.combineMeshes = EditorGUILayout.Toggle(lodLevel.combineMeshes, GUILayout.Width(28), GUILayout.ExpandWidth(false));


                            EditorGUILayout.EndHorizontal();


                            #endregion Regard Tolerance Sphere And Combine Meshes


                            if(lodLevel.regardTolerance)
                            {

                                #region Tolerance Spheres Intensities

                                GUILayout.Space(2);

                                EditorGUILayout.BeginHorizontal();

                                GUILayout.Space(16);
                                content = new GUIContent();
                                content.text = "Spheres Intensities";
                                content.tooltip = "Expand this section to adjust the intensities of tolerance spheres for this LOD level.";


#if UNITY_2019_1_OR_NEWER
                
                                GUILayout.Space(1);
                                lodLevel.intensityFoldout = EditorGUILayout.Foldout(lodLevel.intensityFoldout, content, true);
   
#else
                                lodLevel.intensityFoldout = EditorGUILayout.Foldout(lodLevel.intensityFoldout, content, true);

#endif


                                EditorGUILayout.EndHorizontal();

                                if (lodLevel.intensityFoldout && lodLevel.regardTolerance)
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    GUILayout.Space(6);
                                    UtilityServices.DrawHorizontalLine(Color.black, 1, 8, 14);
                                    EditorGUILayout.EndHorizontal();


                                    for (int b = 0; b < lodLevel.sphereIntensities.Count; b++)
                                    {

                                        EditorGUILayout.BeginHorizontal();

                                        GUILayout.Space(14);


                                        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                                        content = new GUIContent();  //FF6347ff  //006699 // 3e2723 //006699
                                        content.text = String.Format("<color=#006699>Sphere {0} Intensity</color>", b + 1);
                                        content.tooltip = "The percentage of triangles to preserve in the region enclosed by this preservation sphere, for this LOD level.";

                                        style = GUI.skin.label;
                                        style.richText = true;


                                        EditorGUILayout.LabelField(content, style, GUILayout.Width(114));


                                        width = 122;

                                        float oldValue = lodLevel.sphereIntensities[b];
                                        lodLevel.sphereIntensities[b] = Mathf.Abs(GUILayout.HorizontalSlider(lodLevel.sphereIntensities[b], 0, 100, GUILayout.Width(width), GUILayout.ExpandWidth(true)));
                                        style = GUI.skin.textField;

                                        GUILayout.Space(6);

                                        content.text = "";

                                        oldValue = lodLevel.sphereIntensities[b];
                                        lodLevel.sphereIntensities[b] = Mathf.Abs(EditorGUILayout.FloatField(content, lodLevel.sphereIntensities[b], style, GUILayout.Width(3), GUILayout.ExpandWidth(true)));


                                        if ((int)lodLevel.sphereIntensities[b] > 100)
                                        {
                                            lodLevel.sphereIntensities[b] = GetFirstNDigits((int)lodLevel.sphereIntensities[b], 2);
                                        }



#if UNITY_2019_1_OR_NEWER
                
                                        width = 16;
   
#else
                                        width = 20;
#endif

                                        style = GUI.skin.label;
                                        content.text = "<b><size=13>%</size></b>";
                                        EditorGUILayout.LabelField(content, style, GUILayout.Width(width));

                                        EditorGUILayout.EndHorizontal();


                                        GUILayout.EndHorizontal();

                                    }


                                }


                                #endregion Tolerance Spheres Intensities

                            }




                            EditorGUILayout.EndVertical();


                        }



                        #endregion Draw LOD Level


                    }


                    #endregion AUTO LOD


                    #region BATCH FEW

                    DrawBatchFewUI();

                    #endregion BATCH FEW

                }


                EditorGUILayout.EndVertical();

            }

           
            else if (Selection.gameObjects != null && Selection.gameObjects.Length > 1)
            {
           

                #region AUTO LOD


                    EditorGUILayout.BeginVertical("GroupBox");


                    #region TITLE HEADER

                    GUILayout.Space(4);

                    EditorGUILayout.BeginHorizontal();

                    content = new GUIContent();
                    content.text = "<size=13><b><color=#A52A2AFF>AUTOMATIC LOD</color></b></size>";
                    content.tooltip = "Expand this section to see options for automatic LOD generation.";

                    style = EditorStyles.foldout;
                    style.richText = true;  // #FF6347ff  //A52A2AFF
                    prevPadding = new RectOffset(style.padding.left, style.padding.right, style.padding.top, style.padding.bottom);
                    style.padding = new RectOffset(20, 0, -1, 0);

                    GUILayout.Space(19);
                    FoldAutoLODMultiple = !EditorGUILayout.Foldout(!FoldAutoLODMultiple, content, true, style);

                    style.padding = prevPadding;

                    style = new GUIStyle();
                    style.richText = true;

                    EditorGUILayout.EndHorizontal();


                    #endregion TITLE HEADER


                    if (!FoldAutoLODMultiple)
                    {
                        UtilityServices.DrawHorizontalLine(Color.black, 1, 8);
                        GUILayout.Space(6);

                        #region Section Header


                        GUILayout.Space(6);

                        EditorGUILayout.BeginHorizontal();


                        #region Change Save Path


                        style = GUI.skin.button;
                        style.richText = true;


                        content = new GUIContent();
                        content.text = "<b><size=11><color=#006699>Change Save Path</color></size></b>";
                        content.tooltip = "Change the path where the generated LODs mesh assets will be saved. If you don't select a path the default path will be used. Please note that the chosen path will be used for saving LOD mesh assets in the future, unless changed.";

                        if (GUILayout.Button(content, style, GUILayout.Width(134), GUILayout.Height(20), GUILayout.ExpandWidth(false)))
                        {

                            string toOpen = String.IsNullOrWhiteSpace(AutoLODSavePath) ? "Assets/" : AutoLODSavePath;

                            if (!String.IsNullOrWhiteSpace(toOpen))
                            {
                                if (toOpen.EndsWith("/")) { toOpen.Remove(toOpen.Length - 1, 1); }

                                if (!AssetDatabase.IsValidFolder(toOpen))
                                {
                                    toOpen = "Assets/";
                                }
                            }


                            string path = EditorUtility.OpenFolderPanel("Choose LOD Assets Save path", toOpen, "");


                            //Validate the save path. It might be outside the assets folder   

                            // User pressed the cancel button
                            if (string.IsNullOrWhiteSpace(path)) { }

                            else if (!UtilityServices.IsPathInAssetsDir(path))
                            {
                                EditorUtility.DisplayDialog("Invalid Path", "The path you chose is not valid.Please choose a path that points to a directory that exists in the project's Assets folder.", "Ok");
                            }

                            else
                            {
                                path = UtilityServices.GetValidFolderPath(path);

                                if (!string.IsNullOrWhiteSpace(path))
                                {
                                    UtilityServices.AutoLODSavePath = UtilityServices.SetAndReturnStringPref("autoLODSavePath", path);
                                }
                            }

                        }

                        EditorGUI.EndDisabledGroup();

                        #endregion Change Save Path


                        GUILayout.Space(40);

                        #region Add LOD Level

                        content = new GUIContent();

                        //GUILayout.FlexibleSpace();
                        content.tooltip = "Add an LOD level.";

                        content.text = "<b>Add</b>";

                        if (GUILayout.Button(content, style, GUILayout.Width(40), GUILayout.MaxHeight(24), GUILayout.ExpandWidth(true)))
                        {

                            if (dataContainer.currentLodLevelSettings == null)
                            {
                                dataContainer.currentLodLevelSettings = new List<DataContainer.LODLevelSettings>();
                            }


                            List<float> strengths = new List<float>();
                            foreach (var sphere in dataContainer.toleranceSpheres) { strengths.Add(100f); }


                            dataContainer.currentLodLevelSettings.Add(new DataContainer.LODLevelSettings(0, 0, false, false, false, true, false, 7, 100, false, false, false, strengths));
                        }
                        

                        #endregion Add LOD Level

                        GUILayout.Space(2);

                        #region Generate LODs


                        style = GUI.skin.button;
                        style.richText = true;

                        originalColor = new Color(GUI.backgroundColor.r, GUI.backgroundColor.g, GUI.backgroundColor.b);
                        //# ffc14d   60%
                        //# F0FFFF   73%
                        //# F5F5DC   75%
                        GUI.backgroundColor = UtilityServices.HexToColor("#F5F5DC");

                        content = new GUIContent();
                        content.text = "<size=11> <b><color=#000000>Generate LODS</color></b> </size>";
                        content.tooltip = "Generate LODs for the selected GameObjects with the common settings specified. Please note that you must save the scene after successful generation of LODs and apply changes to any prefabs manually. Any errors will be silently ignored.";

                        didPressButton = GUILayout.Button(content, style, GUILayout.Width(120), GUILayout.Height(24), GUILayout.ExpandWidth(true));

                        GUI.backgroundColor = originalColor;


                        if (didPressButton)
                        {

                            bool anySuccess = false;

                            // Delete LOD levels that have 0 screen relative height and 0 reduction strength(Excluding the 1st one)

                            List<DataContainer.LODLevelSettings> levelsToDelete = new List<DataContainer.LODLevelSettings>();

                            if (dataContainer.currentLodLevelSettings.Count > 1)
                            {

                                for (int a = 1; a < dataContainer.currentLodLevelSettings.Count; a++)
                                {
                                    var lodLevel = dataContainer.currentLodLevelSettings[a];

                                    if (Mathf.Approximately(lodLevel.transitionHeight, 0))
                                    {
                                        levelsToDelete.Add(lodLevel);
                                    }

                                    if (Mathf.Approximately(lodLevel.reductionStrength, 0))
                                    {
                                        levelsToDelete.Add(lodLevel);
                                    }
                                }
                            }

                            foreach (var toDelete in levelsToDelete)
                            {
                                dataContainer.currentLodLevelSettings.Remove(toDelete);
                            }


                            foreach (GameObject selected in Selection.gameObjects)
                            {

                                dataContainer.objectMeshPairs = UtilityServices.GetObjectMeshPairs(selected, true, true);
                                ReductionPending = false;
                                ReductionStrength = 0;

                                try
                                {
                                    string error = "";

                                    bool isSuccess = UtilityServices.GenerateLODS(selected, dataContainer.toleranceSpheres, dataContainer.currentLodLevelSettings, UtilityServices.AutoLODSavePath, (string err) =>
                                    {
                                        error = err;

                                    }, false, GenerateUV2LODs);

                                    EditorUtility.ClearProgressBar();

                                    if (isSuccess)
                                    {
                                        anySuccess = true;

                                        ApplyExtraOptions(selected);
                                    }

                                    else
                                    {
                                        Debug.LogWarning($"Failed to generate LODs for GameObject \"{selected.name}\". {error}");
                                    }

                                }

                                catch (Exception error)
                                {
                                    EditorUtility.ClearProgressBar();
                                    Debug.LogWarning($"Failed to generate LODs for GameObject \"{selected.name}\". The LODs for this object might be partially generated. {error.ToString()}");
                                }

                            }

                            if (anySuccess)
                            {
                                EditorSceneManager.MarkSceneDirty(Selection.activeGameObject.scene);
                                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                            }


                            GUIUtility.ExitGUI();

                        }


                        #endregion Generate LODs


                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space(2);

                        EditorGUILayout.BeginHorizontal();


#if UNITY_2019_1_OR_NEWER
                
                        //GUILayout.Space(177);
   
#else
                    //GUILayout.Space(178);
#endif


                    #region  Additional options

                    //enumMaskField
                    //style = EditorStyles.foldout;
                    //style = EditorStyles.toolbar;
                    //style = EditorStyles.popup;

                    style = EditorStyles.toolbarDropDown;
                    //style = GUI.skin.button;
                    style.richText = true;

                    content = new GUIContent();
                    content.text = "<size=11><b>Set Extra Options</b></size>";
                    content.tooltip = "Set extra options for the LOD generation process";


                    if (GUILayout.Button(content, style, GUILayout.Width(134), GUILayout.Height(17), GUILayout.ExpandWidth(false)))
                    {

                        GenericMenu menu = new GenericMenu();


                        content = new GUIContent();
                        content.text = "Copy static flags to new objects";
                        content.tooltip = "Copy the static flags from this object to the newly created LOD objects";

                        menu.AddItem(content, CopyParentStaticFlags, (object obj) => { CopyParentStaticFlags = !CopyParentStaticFlags; }, "");


                        content = new GUIContent();
                        content.text = "Copy layer to new objects";
                        content.tooltip = "Copy layer from this object to the newly created LOD objects";

                        menu.AddItem(content, CopyParentLayer, (object obj) => { CopyParentLayer = !CopyParentLayer; }, "");


                        content = new GUIContent();
                        content.text = "Copy tag to new objects";
                        content.tooltip = "Copy tag from this object to the newly created LOD objects";

                        menu.AddItem(content, CopyParentTag, (object obj) => { CopyParentTag = !CopyParentTag; }, "");


                        content = new GUIContent();
                        content.text = "Generate UV2";
                        content.tooltip = "Should we generate uv2 with default settings for each mesh, and fill them in?. Note that generating uv2 can cause the LOD generation process to get slow";

                        menu.AddItem(content, GenerateUV2LODs, (object obj) => { GenerateUV2LODs = !GenerateUV2LODs; }, "");


                        menu.ShowAsContext();

                    }


                    #endregion  Additional options



#if UNITY_2019_1_OR_NEWER
                
                    GUILayout.Space(40);
#else
                    GUILayout.Space(40);
#endif



                    #region Copy Preview Settings

                        style = GUI.skin.button;
                        style.richText = true;

                        originalColor = new Color(GUI.backgroundColor.r, GUI.backgroundColor.g, GUI.backgroundColor.b);
                        //# ffc14d   60%
                        //# F0FFFF   73%
                        //# F5F5DC   75%
                        //GUI.backgroundColor = UtilityServices.HexToColor("#f9f5f5");

                        content = new GUIContent();
                        content.text = "<size=11><color=#006699><b>Copy Preview</b></color></size>";
                        content.tooltip = $"Copies all the settings from the preview above into each LOD level";


                        didPressButton = GUILayout.Button(content, style, GUILayout.Width(20), GUILayout.Height(17), GUILayout.ExpandWidth(true));

                        GUI.backgroundColor = originalColor;


                        if (didPressButton)
                        {

                            for (int a = 0; a < dataContainer.currentLodLevelSettings.Count; a++)
                            {
                                var lodLevel = dataContainer.currentLodLevelSettings[a];

                                // in Base level don't copy over any settings if reduction is 0
                                if(a == 0)
                                {
                                    if(!Mathf.Approximately(lodLevel.reductionStrength, 0))
                                    {
                                        CopyOverPreviewSettings(lodLevel);
                                    }
                                }

                                
                                else
                                {
                                    CopyOverPreviewSettings(lodLevel);
                                }

                            }

                        }

                        #endregion Copy Preview Settings


                        GUILayout.Space(2);


                        #region Destroy LODs

                        style = GUI.skin.button;
                        style.richText = true;

                        originalColor = new Color(GUI.backgroundColor.r, GUI.backgroundColor.g, GUI.backgroundColor.b);

                        content = new GUIContent();
                        content.text = "<size=11><color=#006699><b>Destroy LODs</b></color></size>";
                        content.tooltip = $"Destroy the generated LODs for all the selected objects. This will also delete the \".mesh\" files in the folder \"{UtilityServices.LOD_ASSETS_DEFAULT_SAVE_PATH}\" that were created for these objects during the LOD generation process. Please note that you will have to delete the empty folders manually.";


                        didPressButton = GUILayout.Button(content, style, GUILayout.Height(17), GUILayout.ExpandWidth(true));


                        GUI.backgroundColor = originalColor;
                        bool didAnySucceed = false;

                        if (didPressButton)
                        {

                            foreach (GameObject selected in Selection.gameObjects)
                            {
                                if (UtilityServices.HasLODs(selected))
                                {
                                    try
                                    {
                                        didAnySucceed = UtilityServices.DestroyLODs(selected);

                                        if (!didAnySucceed)
                                        {
                                            Debug.LogWarning($"Failed to delete LODs on GameObject \"{selected.name}\"");
                                        }
                                    }

                                    catch (Exception ex)
                                    {
                                        Debug.LogWarning($"Failed to delete LODs on GameObject \"{selected.name}\". {ex.ToString()}");
                                    }

                                }
                            }

                            if (didAnySucceed)
                            {
                                EditorSceneManager.MarkSceneDirty(Selection.activeGameObject.scene);
                                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                            }

                            GUIUtility.ExitGUI();
                        }


                        #endregion Destroy LODs


                        EditorGUILayout.EndHorizontal();



                        #endregion Section Header


                        GUILayout.Space(14);



                        #region Draw LOD Level


                        for (int a = 0; a < dataContainer.currentLodLevelSettings.Count; a++)
                        {

                            var lodLevel = dataContainer.currentLodLevelSettings[a];

                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                            content = new GUIContent();  //FF6347ff  //006699
                            content.text = String.Format("<b><color=#3e2723>Level {0}</color></b>", a + 1);

                            if (a == 0)
                            {
                                content.text = String.Format("<b><color=#3e2723>Level {0} (Base)</color></b>", a + 1);
                            }

                            style = GUI.skin.label;
                            style.richText = true;

                            GUILayout.Label(content, style);

                            var previousBackgroundColor = GUI.backgroundColor;
                            GUI.backgroundColor = new Color(Color.gray.r, Color.gray.g, Color.gray.b, 0.8f);
                            GUIContent deleteLevelButtonContent = new GUIContent("<b><color=#FFFFFFD2>X</color></b>", "Delete this LOD level.");
                            style = GUI.skin.button;
                            style.richText = true;

                            if (GUILayout.Button(deleteLevelButtonContent, GUILayout.Width(20)))
                            {
                                if (dataContainer.currentLodLevelSettings.Count > 1)
                                {
                                    dataContainer.currentLodLevelSettings.RemoveAt(a);
                                    a--;
                                }
                            }

                            GUI.backgroundColor = previousBackgroundColor;

                            EditorGUILayout.EndHorizontal();


                            GUILayout.Space(6);


                            #region Reduction Strength Slider

                            GUILayout.BeginHorizontal();

                            content = new GUIContent();
                            style = GUI.skin.label;
                            style.richText = true;
                            prevPadding = new RectOffset(style.padding.left, style.padding.right, style.padding.top, style.padding.bottom);

                            content.text = "Reduction Strength";
                            content.tooltip = "The intensity of the reduction process. This is the amount in percentage to simplify the selected GameObjects by in this LOD level. The lower this value the higher will be the quality of this LOD level. For the base level or level 1 you should keep this to 0.";


                            GUILayout.Space(16);

                            EditorGUILayout.LabelField(content, style, GUILayout.Width(115));


                            lodLevel.reductionStrength = Mathf.Abs(GUILayout.HorizontalSlider(lodLevel.reductionStrength, 0, 100, GUILayout.Width(130), GUILayout.ExpandWidth(true)));
                            style = GUI.skin.textField;

                            GUILayout.Space(5);

                            content.text = "";

                            lodLevel.reductionStrength = Mathf.Abs(EditorGUILayout.FloatField(content, lodLevel.reductionStrength, style, GUILayout.Width(10), GUILayout.ExpandWidth(true)));


                            if ((int)lodLevel.reductionStrength > 100)
                            {
                                lodLevel.reductionStrength = GetFirstNDigits((int)lodLevel.reductionStrength, 2);
                            }

                            style = GUI.skin.label;
                            content.text = "<b><size=13>%</size></b>";
                            EditorGUILayout.LabelField(content, style, GUILayout.Width(20));



                            GUILayout.EndHorizontal();

                            #endregion   Reduction Strength Slider


                            #region Screen relative transition height


                            GUILayout.BeginHorizontal();

                            content = new GUIContent();
                            style = GUI.skin.label;
                            style.richText = true;
                            prevPadding = new RectOffset(style.padding.left, style.padding.right, style.padding.top, style.padding.bottom);

                            content.text = "Transition Height";
                            content.tooltip = "The screen relative height controls how far the viewing camera must be from an object before a transition to the next LOD level is made.";


                            GUILayout.Space(16);

                            EditorGUILayout.LabelField(content, style, GUILayout.Width(115));

                            float oldHeight = lodLevel.transitionHeight;
                            lodLevel.transitionHeight = Mathf.Abs(GUILayout.HorizontalSlider(lodLevel.transitionHeight, 0, 1, GUILayout.Width(130), GUILayout.ExpandWidth(true)));
                            style = GUI.skin.textField;

                            GUILayout.Space(5);

                            content.text = "";

                            lodLevel.transitionHeight = Mathf.Abs(EditorGUILayout.FloatField(content, lodLevel.transitionHeight, style, GUILayout.Width(10), GUILayout.ExpandWidth(true)));
                            lodLevel.transitionHeight = Mathf.Clamp01(lodLevel.transitionHeight);

                            if (!Mathf.Approximately(oldHeight, lodLevel.transitionHeight) && a != 0)
                            {
                                float lastLevelHeight = dataContainer.currentLodLevelSettings[a - 1].transitionHeight;
                                float currentLevelHeight = lodLevel.transitionHeight;

                                if ((lastLevelHeight - currentLevelHeight) <= 0.05f)
                                {
                                    //Debug.Log($"Last level height  {lastLevelHeight}  currentLevelHeight = {currentLevelHeight}  Mathf.Abs(lastLevelHeight - currentLevelHeight)   " +(Mathf.Abs(lastLevelHeight - currentLevelHeight) + "  a is  " + a));
                                    lodLevel.transitionHeight = lastLevelHeight - 0.05f;
                                    lodLevel.transitionHeight = Mathf.Clamp01(lodLevel.transitionHeight);
                                }
                            }


                            if (!Mathf.Approximately(oldHeight, lodLevel.transitionHeight) && a != (dataContainer.currentLodLevelSettings.Count - 1))
                            {
                                float nextLevelHeight = dataContainer.currentLodLevelSettings[a + 1].transitionHeight;
                                float currentLevelHeight = lodLevel.transitionHeight;

                                if ((currentLevelHeight - nextLevelHeight) <= 0.05f)
                                {
                                    //Debug.Log($"Next level height  {nextLevelHeight}  currentLevelHeight = {currentLevelHeight}  Mathf.Abs(lastLevelHeight - currentLevelHeight)   " +(Mathf.Abs(lastLevelHeight - currentLevelHeight) + "  a is  " + a));
                                    lodLevel.transitionHeight = nextLevelHeight + 0.05f;
                                    lodLevel.transitionHeight = Mathf.Clamp01(lodLevel.transitionHeight);
                                }
                            }



                            GUILayout.Space(24);


                            GUILayout.EndHorizontal();


                            #endregion   Screen relative transition height



                            GUILayout.Space(2);

                            EditorGUILayout.BeginHorizontal();


                            GUILayout.Space(16);
                            content = new GUIContent();
                            content.text = "Reduction Options";
                            content.tooltip = "Expand this section to see options for mesh simplification for this LOD level.";


#if UNITY_2019_1_OR_NEWER
                
                        GUILayout.Space(1);
                        lodLevel.simplificationOptionsFoldout = EditorGUILayout.Foldout(lodLevel.simplificationOptionsFoldout, content, true);
   
#else
                        lodLevel.simplificationOptionsFoldout = EditorGUILayout.Foldout(lodLevel.simplificationOptionsFoldout, content, true);
#endif


                        #region Combine Meshes



#if UNITY_2019_1_OR_NEWER
                
                            GUILayout.Space(66);

#else
                            GUILayout.Space(40);
#endif

                            EditorGUILayout.BeginHorizontal();


                            style = GUI.skin.label;
                            content.text = "Combine Meshes";
                            content.tooltip = "[Deprecated] Combine all renderers and meshes under this level into one, where possible. Please note that this option is present just in case if someone has any special use case where they need to generate LODs with some levels having combined meshes and some having uncombined meshes. You can now use BatchFew to combine meshes exclusively.";

                            // Added it to Batch few
                            EditorGUILayout.LabelField(content, style, GUILayout.Width(109));

                            lodLevel.combineMeshes = EditorGUILayout.Toggle(lodLevel.combineMeshes, GUILayout.Width(28), GUILayout.ExpandWidth(false));


                            EditorGUILayout.EndHorizontal();

                            #endregion Combine Meshes


                            EditorGUILayout.EndHorizontal();



                            if (lodLevel.simplificationOptionsFoldout)
                            {
                                EditorGUILayout.BeginHorizontal();
                                GUILayout.Space(6);
                                UtilityServices.DrawHorizontalLine(Color.black, 1, 8, 14);
                                EditorGUILayout.EndHorizontal();
                            }


                            #region Reduction extra options


                            EditorGUILayout.BeginHorizontal();

                            GUILayout.Space(16);


                            if (lodLevel.simplificationOptionsFoldout)
                            {

                                style = GUI.skin.label;

                                content.text = "Preserve UV Foldover";
                                content.tooltip = "Check this option to preserve UV foldover for this LOD level.";

                                EditorGUILayout.LabelField(content, style, GUILayout.Width(135));
                                lodLevel.preserveUVFoldover = EditorGUILayout.Toggle(lodLevel.preserveUVFoldover, GUILayout.Width(18), GUILayout.ExpandWidth(false));

                                GUILayout.Space(8);

                                content.text = "Preserve UV Seams";
                                content.tooltip = "Preserve the mesh areas where the UV seams are made.These are the areas where different UV islands are formed (usually the shallow polygon conjested areas).";

                                EditorGUILayout.LabelField(content, style, GUILayout.Width(135));
                                lodLevel.preserveUVSeams = EditorGUILayout.Toggle(lodLevel.preserveUVSeams, GUILayout.Width(20), GUILayout.ExpandWidth(false));


                                EditorGUILayout.EndHorizontal();



                                EditorGUILayout.BeginHorizontal();


                                GUILayout.Space(16);

                                content.text = "Preserve Borders";
                                content.tooltip = "Check this option to preserve border edges for this LOD level. Border edges are the edges that are unconnected and open. Preserving border edges might lead to lesser polygon reduction but can be helpful where you see serious mesh and texture distortions.";


                                EditorGUILayout.LabelField(content, style, GUILayout.Width(114));
                                GUILayout.Space(21);


                                lodLevel.preserveBorders = EditorGUILayout.Toggle(lodLevel.preserveBorders, GUILayout.Width(15), GUILayout.ExpandWidth(false));

                                GUILayout.Space(11);

                                //content.text = "Smart Linking";
                                //content.tooltip = "Smart linking links vertices that are very close to each other. This helps in the mesh simplification process where holes or other serious issues could arise. Disabling this (where not needed) can cause a minor performance gain.";
                                content.text = "Use Edge Sort";
                                content.tooltip = "Using edge sort can result in very good quality mesh simplification in some cases but can be a little slow to run.";


                                EditorGUILayout.LabelField(content, style, GUILayout.Width(114));
                                GUILayout.Space(21);


                                lodLevel.useEdgeSort = EditorGUILayout.Toggle(lodLevel.useEdgeSort, GUILayout.Width(10), GUILayout.ExpandWidth(false));


                                EditorGUILayout.EndHorizontal();



                                EditorGUILayout.BeginHorizontal();


                                content.text = "Regard Curvature";
                                content.tooltip = "Check this option to take into account the discrete curvature of mesh surface during simplification. Taking surface curvature into account can result in very good quality mesh simplification, but it can slow the simplification process significantly.";

                                GUILayout.Space(16);
                                EditorGUILayout.LabelField(content, style, GUILayout.Width(135));

                                lodLevel.regardCurvature = EditorGUILayout.Toggle(lodLevel.regardCurvature, GUILayout.Width(50));


                                EditorGUILayout.EndHorizontal();


                                GUILayout.BeginHorizontal();



#if UNITY_2019_1_OR_NEWER
                
                                GUILayout.Space(17);

#else
                                GUILayout.Space(16);
#endif

                                content.text = "Aggressiveness";
                                content.tooltip = "The agressiveness of the reduction algorithm to use for this LOD level. Higher number equals higher quality, but more expensive to run. Lowest value is 7.";


#if UNITY_2019_1_OR_NEWER
                
                                EditorGUILayout.LabelField(content, GUILayout.Width(134));

#else
                                EditorGUILayout.LabelField(content, GUILayout.Width(134));
                                GUILayout.Space(2);
#endif


                                content.text = "";

                                lodLevel.aggressiveness = Mathf.Abs(EditorGUILayout.FloatField(content, lodLevel.aggressiveness, GUILayout.Width(168), GUILayout.ExpandWidth(true)));

                                if (lodLevel.aggressiveness < 7) { lodLevel.aggressiveness = 7; }


                                GUILayout.EndHorizontal();



                                GUILayout.BeginHorizontal();



                                GUILayout.Space(16);

                                content.text = "Max Iterations";
                                content.tooltip = "The maximum passes the reduction algorithm does for this LOD level. Higher number is more expensive but can bring you closer to your target quality. 100 is the lowest allowed value.";



#if UNITY_2019_1_OR_NEWER
                
                                GUILayout.Space(1);
                                EditorGUILayout.LabelField(content, GUILayout.Width(134));

#else
                                EditorGUILayout.LabelField(content, GUILayout.Width(136));

#endif


                                content.text = "";



#if UNITY_2019_1_OR_NEWER
                
                            lodLevel.maxIterations = Mathf.Abs(EditorGUILayout.IntField(content, lodLevel.maxIterations, GUILayout.Width(168), GUILayout.ExpandWidth(true)));


#else
                            lodLevel.maxIterations = Mathf.Abs(EditorGUILayout.IntField(content, lodLevel.maxIterations, GUILayout.Width(168), GUILayout.ExpandWidth(true)));

#endif

                            if (lodLevel.maxIterations < 100) { lodLevel.maxIterations = 100; }

                            }


                            #endregion Reduction extra options


                            EditorGUILayout.EndHorizontal();


                            EditorGUILayout.EndVertical();

                        }



                        #endregion Draw LOD Level


                    }


                    #endregion AUTO LOD
                

                #region BATCH FEW

                DrawBatchFewUI();

                #endregion BATCH FEW


                EditorGUILayout.EndVertical();
            }


            else if(!isFeasibleTargetForPolyFew && !areMultiObjectsSelected)
            {

                EditorGUILayout.BeginVertical("GroupBox");

                #region BATCH FEW

                DrawBatchFewUI(true);

                #endregion BATCH FEW

                EditorGUILayout.EndVertical();
            }

        }





        public void DrawBatchFewUI(bool boxDrawnAlready = false)
        {

            if (FoldoutAutoLOD)
            {
                GUILayout.Space(12);
            }


            #region TITLE HEADER

            if (!boxDrawnAlready)
            {
                UtilityServices.DrawHorizontalLine(Color.black, 1, 8);
            }

            GUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();

            content = new GUIContent();
            content.text = "<size=13><b><color=#A52A2AFF>BATCH FEW</color></b></size>";
            content.tooltip = "Expand this section to see options for combining materials and meshes and for generating texture atlasses (Texture Arrays).";

            style = EditorStyles.foldout;
            style.richText = true;  // #FF6347ff  //A52A2AFF
            prevPadding = new RectOffset(style.padding.left, style.padding.right, style.padding.top, style.padding.bottom);
            style.padding = new RectOffset(20, 0, -1, 0);

            GUILayout.Space(19);
            FoldoutBatchFew = EditorGUILayout.Foldout(FoldoutBatchFew, content, true, style);

            style.padding = prevPadding;

            style = new GUIStyle();
            style.richText = true;

            EditorGUILayout.EndHorizontal();


            #endregion TITLE HEADER


            if (FoldoutBatchFew)
            {
                UtilityServices.DrawHorizontalLine(Color.black, 1, 8);
                GUILayout.Space(6);

                #region Section Header


                GUILayout.Space(6);

                EditorGUILayout.BeginHorizontal();


                #region Change Save Path


                style = GUI.skin.button;
                style.richText = true;


                content = new GUIContent();
                content.text = "<b><size=11><color=#006699>Change Save Path</color></size></b>";
                content.tooltip = "Change the path where the new combined meshes, texture atlases and materials will be saved. If you don't select a path the default path will be used. Please note that the chosen path will be used for saving such assets in the future, unless changed.";

                if (GUILayout.Button(content, style, GUILayout.Width(134), GUILayout.Height(20), GUILayout.ExpandWidth(false)))
                {

                    string toOpen = String.IsNullOrWhiteSpace(BatchFewSavePath) ? "Assets/" : BatchFewSavePath;

                    if (!String.IsNullOrWhiteSpace(toOpen))
                    {
                        if (toOpen.EndsWith("/")) { toOpen.Remove(toOpen.Length - 1, 1); }

                        if (!AssetDatabase.IsValidFolder(toOpen))
                        {
                            toOpen = "Assets/";
                        }
                    }


                    string path = EditorUtility.OpenFolderPanel("Choose Mesh and Material Combiner assets save path", toOpen, "");

                    //Validate the save path. It might be outside the assets folder   

                    // User pressed the cancel button
                    if (string.IsNullOrWhiteSpace(path)) { }

                    else if (!UtilityServices.IsPathInAssetsDir(path))
                    {
                        EditorUtility.DisplayDialog("Invalid Path", "The path you chose is not valid.Please choose a path that points to a directory that exists in the project's Assets folder.", "Ok");
                    }

                    else
                    {
                        path = UtilityServices.GetValidFolderPath(path);

                        if (!string.IsNullOrWhiteSpace(path))
                        {
                            UtilityServices.BatchFewSavePath = UtilityServices.SetAndReturnStringPref("batchFewSavePath", path);
                        }
                    }

                }

                EditorGUI.EndDisabledGroup();

                #endregion Change Save Path


                GUILayout.Space(40);

                GUILayout.Space(2);

                #region Combine Materials


                style = GUI.skin.button;
                style.richText = true;

                originalColor = new Color(GUI.backgroundColor.r, GUI.backgroundColor.g, GUI.backgroundColor.b);
                //# ffc14d   60%
                //# F0FFFF   73%
                //# F5F5DC   75%
                GUI.backgroundColor = UtilityServices.HexToColor("#F5F5DC");

                content = new GUIContent();
                content.text = "<size=11> <b><color=#000000>Combine Materials</color></b> </size>";
                content.tooltip = "Combine all the materials in the selected objects and generate Texture Atlases with the settings specified. Materials that don't use the Standard Shader or it's variants (Standard Specular etc) will be ignored. Please note that you must save the scene after successful operations and apply changes to any prefabs manually.";

                didPressButton = GUILayout.Button(content, style, GUILayout.Width(120), GUILayout.Height(24), GUILayout.ExpandWidth(true));

                GUI.backgroundColor = originalColor;


                if (didPressButton)
                {

                    var cS = dataContainer.colorSpaceChoices[dataContainer.choiceDiffuseColorSpace];
                    var colorSpace = (MaterialCombiner.CombiningInformation.DiffuseColorSpace)Enum.Parse(typeof(MaterialCombiner.CombiningInformation.DiffuseColorSpace), cS.ToUpper());

                    try
                    {
                        PolyFewResetter.ResetToInitialState();
                        GameObject forObject = Selection.activeGameObject;

                        bool success = MaterialCombiner.CombineMaterials(Selection.gameObjects, BatchFewSavePath, dataContainer.textureArraysSettings, colorSpace, RegardChildren, true, (string error) =>
                        {
                            // Do something on error
                        });

                        if (success)
                        {
                            EditorUtility.DisplayDialog("Operation Successfull", "Successfully combined materials in the selected objects. Please note that changes to any prefabs must be applied manually", "Ok");
                            PolyFewResetter.RefreshObjectMeshPairs(forObject);
                            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                        }
                    }
                    
                    catch (Exception ex)
                    {
                        string error = $"Failed to combine materials due to unknown reasons. Please check console for any clues.";
                        EditorUtility.DisplayDialog("Operation Failed", error, "Ok");
                        Debug.LogError(ex);
                    }


                }


                #endregion Combine Materials

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(2);

                EditorGUILayout.BeginHorizontal();


                #region Consider Children

                content = new GUIContent();
                style = GUI.skin.textField;
                style.richText = true;
                RectOffset oldPadding = new RectOffset(style.padding.left, style.padding.right, style.padding.top, style.padding.bottom);

#if !UNITY_2019_1_OR_NEWER

                style.padding = new RectOffset(10, style.padding.right, 0, 0);
#else
                style.padding = new RectOffset(18, style.padding.right, 0, 0);
#endif
                content.text = "<b><size=11>Consider Children</size></b>";
                content.tooltip = "Checking this option will automatically take into account all of the deep nested children of the selected object(s) while combining materials without the need for explicitly selecting each child object. If this option is unchecked then only the selected objects(Without their children) are considered while combining the materials.";

                originalColor = new Color(GUI.backgroundColor.r, GUI.backgroundColor.g, GUI.backgroundColor.b);
                GUI.backgroundColor = UtilityServices.HexToColor("#EFEEEF");//E1E1E1


                EditorGUILayout.LabelField(content, style, GUILayout.Width(134), GUILayout.Height(17));
                int tWidth = 0;

#if UNITY_2019_1_OR_NEWER
                
                tWidth = 33;
#else
                tWidth = 28;
#endif


                GUI.backgroundColor = originalColor;

                GUILayout.Space(6);

                RegardChildren = EditorGUILayout.Toggle(RegardChildren, GUILayout.Width(tWidth), GUILayout.ExpandWidth(false));
                style.padding = oldPadding;


                #endregion Consider Children



                #region COMBINE MESHES


                bool canCombineMeshes = false;

                if (Selection.gameObjects.Length == 1 && Selection.activeTransform != null && Selection.activeTransform.childCount > 0)
                {
                    canCombineMeshes = true;
                }

                else if (Selection.gameObjects.Length > 1)
                {
                    foreach (var selection in Selection.gameObjects)
                    {
                        if (selection.transform.childCount > 0)
                        {
                            canCombineMeshes = true;
                            break;
                        }
                    }
                }

                EditorGUI.BeginDisabledGroup(!canCombineMeshes);

                style = GUI.skin.button;
                style.richText = true;

                originalColor = new Color(GUI.backgroundColor.r, GUI.backgroundColor.g, GUI.backgroundColor.b);

                GUI.backgroundColor = UtilityServices.HexToColor("#F5F5DC");

                content = new GUIContent();
                content.text = "<size=11><color=#000000><b>Combine Meshes</b></color></size>";
                content.tooltip = "Combine all renderers and meshes nested under the selected object(s). Select a top level parent root object or multiple root objects to start with.";


                didPressButton = GUILayout.Button(content, style, GUILayout.Height(20), GUILayout.ExpandWidth(true));

                GUI.backgroundColor = originalColor;


                if (didPressButton)
                {

                    int option = EditorUtility.DisplayDialogComplex("Choose Combine Target",
                        "What kind of meshes/renderers do you want to combine?. Both skinned and static?, Static meshes only? or Skinned meshes only?. Closing this window will only combine static meshes.",
                        "Skinned And Static",
                        "Static Only",
                        "Skinned Only");

                    UtilityServices.MeshCombineTarget combineTarget = MeshCombineTarget.SkinnedAndStatic;

                    switch (option)
                    {
                        case 0:
                            combineTarget = MeshCombineTarget.SkinnedAndStatic;
                            break;

                        case 1:
                            combineTarget = MeshCombineTarget.StaticOnly;
                            break;

                        case 2:
                            combineTarget = MeshCombineTarget.SkinnedOnly;
                            break;

                        default:
                            combineTarget = MeshCombineTarget.SkinnedAndStatic;
                            break;
                    }



                    option = EditorUtility.DisplayDialogComplex("Make child",
                        "Make the newly created combined GameObject a child of this object?. Selecting yes will also disable all renderers in the current object. Chosing no will simply disable the current object and create a new object in the hierarchy with the combined mesh.",
                        "No",
                        "Yes",
                        "Cancel");

                    bool makeChild = false;

                    switch (option)
                    {
                        case 0:
                            makeChild = false;
                            break;

                        case 1:
                            makeChild = true;
                            break;

                        case 2:
                            makeChild = false;
                            break;

                        default:
                            makeChild = false;
                            break;
                    }


                    if (Selection.gameObjects.Length == 1)
                    {
                        bool didSucceed = true;
                        GameObject toDuplicate = Selection.activeGameObject;
                        var duplicated = GameObject.Instantiate(toDuplicate);
                        duplicated.name = toDuplicate.name;

                        var origPos = duplicated.transform.position;
                        var origRot = duplicated.transform.rotation;
                        var origScale = duplicated.transform.localScale;

                        duplicated.transform.position = Vector3.one;
                        duplicated.transform.rotation = Quaternion.identity;
                        duplicated.transform.localScale = Vector3.one;

                        try
                        {
                           UtilityServices.CombineMeshes(duplicated, toDuplicate.name, BatchFewSavePath, (errorTitle, error) => 
                           {
                               didSucceed = false;
                               EditorUtility.DisplayDialog(errorTitle, error, "Ok");

                           }, combineTarget, GenerateUV2MeshCombine);
                        }

                        catch (Exception ex)
                        {
                            didSucceed = false;
                            EditorUtility.DisplayDialog("Operation Failed", $"Failed to combine meshes due to unknown reasons. Please check console for any clues.", "Ok");
                            Debug.LogError(ex.ToString());
                        }

                        if (didSucceed)
                        {
       
                            EditorUtility.DisplayDialog("Successfully Combined Meshes", $"The original GameObject has been disabled. A copy of the original object \"{duplicated.name}\" with combined meshes has been created. Please note that the root bones for the combined skinned mesh renderers should not be deleted in the new object. Any changes to prefabs must be manually applied.", "Ok");
                            EditorSceneManager.MarkSceneDirty(Selection.activeGameObject.scene);
                            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                            duplicated.name = toDuplicate.name + "Combined_Meshes";

                            duplicated.transform.position = origPos;
                            duplicated.transform.rotation = origRot;
                            duplicated.transform.localScale = origScale;

                            if (makeChild)
                            {
                                foreach (var renderer in toDuplicate.GetComponentsInChildren<Renderer>())
                                {
                                    renderer.enabled = false;
                                }

                                duplicated.transform.parent = toDuplicate.transform;
                            }

                            else
                            {
                                toDuplicate.SetActive(false);
                            }
                        }

                        else
                        {
                            DestroyImmediate(duplicated);
                        }


                        EditorUtility.ClearProgressBar();

                    }

                    else if (Selection.gameObjects.Length > 1)
                    {
                        bool anySuccess = false;

                        foreach (var selection in Selection.gameObjects)
                        {
                            bool didSucceed = true;
                            GameObject duplicated = GameObject.Instantiate(selection);
                            duplicated.name = selection.name;

                            var origPos = duplicated.transform.position;
                            var origRot = duplicated.transform.rotation;
                            var origScale = duplicated.transform.localScale;

                            duplicated.transform.position = Vector3.one;
                            duplicated.transform.rotation = Quaternion.identity;
                            duplicated.transform.localScale = Vector3.one;

                            try
                            {
                                UtilityServices.CombineMeshes(duplicated, selection.name, BatchFewSavePath, (errorTitle, error) =>
                                {
                                    didSucceed = false;
                                    Debug.LogWarning(error);

                                }, combineTarget, GenerateUV2MeshCombine);

                            }
                            catch (Exception ex)
                            {
                                didSucceed = false;
                                Debug.LogError(ex);
                                EditorUtility.ClearProgressBar();
                            }

                            if (didSucceed)
                            {
                                anySuccess = true;
                                selection.SetActive(false);
                                duplicated.name = selection.name + "Combined_Meshes";

                                duplicated.transform.position = origPos;
                                duplicated.transform.rotation = origRot;
                                duplicated.transform.localScale = origScale;
                            }
                            else
                            {
                                DestroyImmediate(duplicated);
                            }

                        }

                        if (anySuccess)
                        {
                            EditorUtility.DisplayDialog("Successfully Combined Meshes", $"The original GameObjects have been disabled. Copy of the original objects with combined meshes have been created. Please note that the root bones for the combined skinned mesh renderers should not be deleted in the new objects. Any changes to prefabs must be manually applied.", "Ok");
                            EditorSceneManager.MarkSceneDirty(Selection.activeGameObject.scene);
                            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                        }

                        else
                        {
                            EditorUtility.DisplayDialog("Operation Failed", "No meshes combined, see the console for more information.", "Ok");
                        }
                    }

                }

                EditorGUI.EndDisabledGroup();


                #endregion COMBINE MESHES



                #region Generate UV2

                content = new GUIContent();
                style = GUI.skin.textField;
                style.richText = true;
                oldPadding = new RectOffset(style.padding.left, style.padding.right, style.padding.top, style.padding.bottom);



#if !UNITY_2019_1_OR_NEWER

                style.padding = new RectOffset(10, style.padding.right, 1, 0);

#else
                style.padding = new RectOffset(18, style.padding.right, 0, 0);
#endif


                content.text = "<b><size=11>Generate UV2</size></b>";
                content.tooltip = "Should we generate uv2 with default settings for each mesh, and fill them in?. Note that generating uv2 can cause the mesh combining process to get slow.";

                originalColor = new Color(GUI.backgroundColor.r, GUI.backgroundColor.g, GUI.backgroundColor.b);
                GUI.backgroundColor = UtilityServices.HexToColor("#EFEEEF"); //E1E1E1


                EditorGUILayout.LabelField(content, style, GUILayout.Width(134), GUILayout.Height(17));
                tWidth = 0;

#if UNITY_2019_1_OR_NEWER
                
                tWidth = 18;
#else
                tWidth = 13;
#endif


                GUI.backgroundColor = originalColor;

                GUILayout.Space(6);

                GenerateUV2MeshCombine = EditorGUILayout.Toggle(GenerateUV2MeshCombine, GUILayout.Width(tWidth), GUILayout.ExpandWidth(false));
                style.padding = oldPadding;

                #endregion Generate UV2


                EditorGUILayout.EndHorizontal();


#endregion Section Header


                GUILayout.Space(10);


#region DRAW TEXTURE ARRAY SETTINGS


                EditorGUILayout.BeginVertical(EditorStyles.helpBox);


                UtilityServices.DrawHorizontalLine(Color.black, 1, 2, 7, 4);

                content = new GUIContent();//TEXTURE ARRAYS SETTINGS //2F4F4F //008080 //191970 //006699
                content.text = "<b><color=#006699>Texture Arrays Settings</color></b>";
                content.tooltip = "Settings for the generated Texture Arrays.";

                style = GUI.skin.label;
                style.richText = true;
                TextAnchor OldAlignment = style.alignment;
                style.alignment = TextAnchor.MiddleCenter;

                EditorGUILayout.LabelField(content, style);

                style.alignment = OldAlignment;

                UtilityServices.DrawHorizontalLine(Color.black, 1, 1, 7, 4);

                GUILayout.Space(4);

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                content = new GUIContent();  //FF6347ff  //006699 //008080
                content.text = "<b><color=#2F4F4F>Texture Map Array :</color></b>";
                content.tooltip = "Choose a Texture map to adjust its generated Texture Array's settings.";


                style = GUI.skin.label;
                style.richText = true;

                GUILayout.Label(content, style, GUILayout.Width(137), GUILayout.ExpandWidth(false));


                dataContainer.choiceTextureMap = EditorGUILayout.Popup("", dataContainer.choiceTextureMap, dataContainer.textureMapsChoices, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));

                style = GUI.skin.button;
                style.richText = true;

                content = new GUIContent();
                content.text = "<b><size=11><color=#D2691E>Reset</color></size></b>";
                content.tooltip = "Reset all settings to default.";

                if (GUILayout.Button(content, style, GUILayout.Width(80), GUILayout.Height(20)))
                {
                    ResetTextureArrays();
                }


                EditorGUILayout.EndHorizontal();


                GUILayout.Space(6);


                var selectedTextureArraySettings = GetTexArrSettingsFromName(dataContainer.textureMapsChoices[dataContainer.choiceTextureMap]);

#region TEXTURES RESOLUTION
                GUILayout.BeginHorizontal();


                content = new GUIContent();
                style = GUI.skin.label;
                style.richText = true;
                prevPadding = new RectOffset(style.padding.left, style.padding.right, style.padding.top, style.padding.bottom);

                content.text = "Textures Resolution";
                content.tooltip = "The resolution of each texture in the selected Texture Map Array. Every texture in the selected Texture Map Array will be resized to this resolution. Texture Arrays have this inherent limitation that they must have same resolution textures in them.";


                GUILayout.Space(12);

                EditorGUILayout.LabelField(content, style, GUILayout.Width(130));

                selectedTextureArraySettings.choiceResolutionW = EditorGUILayout.Popup("", selectedTextureArraySettings.choiceResolutionW, dataContainer.resolutionsChoices, GUILayout.Width(60), GUILayout.ExpandWidth(true));

                content.text = "<b>x</b>";
                GUILayout.Space(2);
                EditorGUILayout.LabelField(content, style, GUILayout.Width(11));
                GUILayout.Space(2);

                selectedTextureArraySettings.choiceResolutionH = EditorGUILayout.Popup("", selectedTextureArraySettings.choiceResolutionH, dataContainer.resolutionsChoices, GUILayout.Width(60), GUILayout.ExpandWidth(true));


                var res = new MaterialCombiner.CombiningInformation.Resolution();
                res.width = Int32.Parse(dataContainer.resolutionsChoices[selectedTextureArraySettings.choiceResolutionW]);
                res.height = Int32.Parse(dataContainer.resolutionsChoices[selectedTextureArraySettings.choiceResolutionH]);
                selectedTextureArraySettings.resolution = res;

                EditorGUILayout.LabelField("", style, GUILayout.Width(0)); // Blank label to stop UI field from bleeding ro getting too close to the edge

                GUILayout.EndHorizontal();
#endregion TEXTURES RESOLUTION


#region FILTERING MODE
                GUILayout.BeginHorizontal();

                content.text = "Filtering Mode";
                content.tooltip = "The filtering mode for the textures in the selected Texture Map Array.";


                GUILayout.Space(13);

                EditorGUILayout.LabelField(content, style, GUILayout.Width(129));

                selectedTextureArraySettings.choiceFilteringMode = EditorGUILayout.Popup("", selectedTextureArraySettings.choiceFilteringMode, dataContainer.filteringModesChoices, GUILayout.Width(142), GUILayout.ExpandWidth(true));

                var fMode = dataContainer.filteringModesChoices[selectedTextureArraySettings.choiceFilteringMode];
                FilterMode filteringMode;

                if (fMode.ToLower().Contains("point")) { filteringMode = FilterMode.Point; }
                else if (fMode.ToLower().Contains("Bilinear")) { filteringMode = FilterMode.Bilinear; }
                else { filteringMode = FilterMode.Trilinear; }

                selectedTextureArraySettings.filteringMode = filteringMode;

                EditorGUILayout.LabelField("", style, GUILayout.Width(0)); // Blank label to stop UI field from bleeding ro getting too close to the edge

                GUILayout.EndHorizontal();
#endregion FILTERING MODE


#region ANISOTROPIC FILTERING
                GUILayout.Space(-2);

                GUILayout.BeginHorizontal();

                content = new GUIContent();
                prevPadding = new RectOffset(style.padding.left, style.padding.right, style.padding.top, style.padding.bottom);

                content.text = "Anisotropic Level";
                content.tooltip = "The level of the anisotropic filtering for the textures in the selected Texture Map Array.";


                GUILayout.Space(13);

                EditorGUILayout.LabelField(content, style, GUILayout.Width(129));

                float floatLevel = Mathf.Abs(GUILayout.HorizontalSlider(selectedTextureArraySettings.anisotropicFilteringLevel, 0, 16, GUILayout.Width(142), GUILayout.ExpandWidth(true)));
                style = GUI.skin.textField;
                selectedTextureArraySettings.anisotropicFilteringLevel = Mathf.RoundToInt(floatLevel);

                GUILayout.Space(5);

                content.text = "";
                selectedTextureArraySettings.anisotropicFilteringLevel = Mathf.Abs(EditorGUILayout.IntField(content, selectedTextureArraySettings.anisotropicFilteringLevel, style, GUILayout.Width(40)));
                selectedTextureArraySettings.anisotropicFilteringLevel = Mathf.Clamp(selectedTextureArraySettings.anisotropicFilteringLevel, (int)0, (int)16);

                EditorGUILayout.LabelField("", GUILayout.Width(0)); // Blank label to stop UI field from bleeding ro getting too close to the edge

                GUILayout.EndHorizontal();
#endregion ANISOTROPIC FILTERING


#region COMPRESSION QUALITY
                GUILayout.Space(1);

                GUILayout.BeginHorizontal();

                content.text = "Compression Quality";
                content.tooltip = "The compression quality for the textures in the selected Texture Map Array. This option is only valid if the compression type selected is \"ASTC RGB\" ";

                style = GUI.skin.label;
                GUILayout.Space(12);


                EditorGUI.BeginDisabledGroup(dataContainer.compressionTypesChoices[selectedTextureArraySettings.choiceCompressionType] != "ASTC_RGB");

                EditorGUILayout.LabelField(content, style, GUILayout.Width(129));

                selectedTextureArraySettings.choiceCompressionQuality = EditorGUILayout.Popup("", selectedTextureArraySettings.choiceCompressionQuality, dataContainer.compressionQualitiesChoices, GUILayout.Width(142), GUILayout.ExpandWidth(true));

                EditorGUI.EndDisabledGroup();


                var cQ = dataContainer.compressionQualitiesChoices[selectedTextureArraySettings.choiceCompressionQuality];
                var compQuality = (MaterialCombiner.CombiningInformation.CompressionQuality)Enum.Parse(typeof(MaterialCombiner.CombiningInformation.CompressionQuality), cQ.ToUpper());

                selectedTextureArraySettings.compressionQuality = compQuality;

                EditorGUILayout.LabelField("", GUILayout.Width(0)); // Blank label to stop UI field from bleeding ro getting too close to the edge

                GUILayout.EndHorizontal();
#endregion COMPRESSION QUALITY


#region COMPRESSION TYPE

                GUILayout.BeginHorizontal();

                content.text = "Compression Type";
                content.tooltip = "The compression type for the textures in the selected Texture Map Array.";

                style = GUI.skin.label;
                GUILayout.Space(12);

                EditorGUILayout.LabelField(content, style, GUILayout.Width(129));

                selectedTextureArraySettings.choiceCompressionType = EditorGUILayout.Popup("", selectedTextureArraySettings.choiceCompressionType, dataContainer.compressionTypesChoices, GUILayout.Width(142), GUILayout.ExpandWidth(true));

                var cT = dataContainer.compressionTypesChoices[selectedTextureArraySettings.choiceCompressionType];
                var compType = (MaterialCombiner.CombiningInformation.CompressionType)Enum.Parse(typeof(MaterialCombiner.CombiningInformation.CompressionType), cT.ToUpper());
                selectedTextureArraySettings.compressionType = compType;

                EditorGUILayout.LabelField("", GUILayout.Width(0)); // Blank label to stop UI field from bleeding ro getting too close to the edge

                GUILayout.EndHorizontal();
#endregion COMPRESSION TYPE


#region DIFFUSE COLOR SPACE

                GUILayout.BeginHorizontal();

                content.text = "Diffuse Color Space";
                content.tooltip = "The color space diffuse maps are in. This should only be changed to \"Linear\" if you're generating Texture arrays on a platform where linear rendering mode can cause diffuse maps to be too dark, Occulus Quest is an example of such a platform.";

                style = GUI.skin.label;
                GUILayout.Space(12);

                EditorGUILayout.LabelField(content, style, GUILayout.Width(129));

                dataContainer.choiceDiffuseColorSpace = EditorGUILayout.Popup("", dataContainer.choiceDiffuseColorSpace, dataContainer.colorSpaceChoices, GUILayout.Width(142), GUILayout.ExpandWidth(true));

                EditorGUILayout.LabelField("", GUILayout.Width(0)); // Blank label to stop UI field from bleeding ro getting too close to the edge

                GUILayout.EndHorizontal();
#endregion DIFFUSE COLOR SPACE


                GUILayout.Space(6);



                EditorGUILayout.EndVertical();



#endregion RAW TEXTURE ARRAY SETTINGS




#region DRAW MATERIALS PROPERTIES


                if (Selection.gameObjects != null && Selection.gameObjects.Length == 1)
                {

                    ObjectMaterialLinks objMaterialLinks = Selection.gameObjects[0].GetComponent<ObjectMaterialLinks>();

                    GUILayout.Space(4);

                    if (objMaterialLinks != null && objMaterialLinks.linkedMaterialEntities != null && !dataContainer.relocateMaterialLinks)
                    {
                        int a = -1;
                        bool wasFeasible = false;

                        foreach (var materialEntity in objMaterialLinks.linkedMaterialEntities)
                        {
                            a++;

#region INITIALIZATION AND PRECHECKS

                            bool isFeasible = false;
                            Texture2D attrImg = null;
                            List<CombineMetaData> combinedMats = null;

                            var comb = materialEntity.combinedMats;

                            if (comb != null && comb.Count != 0)
                            {
                                isFeasible = true;
                                attrImg = objMaterialLinks.linkedAttrImg;
                                combinedMats = comb;
                            }

                            if (attrImg == null) { isFeasible = false; }

                            if (attrImg != null && isFeasible && dataContainer.reInitializeTempMatProps && !dataContainer.relocateMaterialLinks)
                            {
                                foreach (var combMat in combinedMats)
                                {
                                    MaterialProperties origMatProps = combMat.materialProperties;
                                    combMat.tempMaterialProperties = new MaterialProperties();
                                    MaterialProperties tempProps = combMat.tempMaterialProperties;

                                    tempProps.albedoTint = origMatProps.albedoTint;
                                    tempProps.uvTileOffset = origMatProps.uvTileOffset;
                                    tempProps.normalIntensity = origMatProps.normalIntensity;
                                    tempProps.occlusionIntensity = origMatProps.occlusionIntensity;
                                    tempProps.smoothnessIntensity = origMatProps.smoothnessIntensity;
                                    tempProps.glossMapScale = origMatProps.glossMapScale;
                                    tempProps.metalIntensity = origMatProps.metalIntensity;
                                    tempProps.emissionColor = origMatProps.emissionColor;
                                    tempProps.detailUVTileOffset = origMatProps.detailUVTileOffset;
                                    tempProps.alphaCutoff = origMatProps.alphaCutoff;
                                    tempProps.specularColor = origMatProps.specularColor;
                                    tempProps.detailNormalScale = origMatProps.detailNormalScale;
                                    tempProps.heightIntensity = origMatProps.heightIntensity;
                                    tempProps.uvSec = origMatProps.uvSec;

                                }
                            }
                            
#endregion INITIALIZATION AND PRECHECKS


                            // Only the first time
                            if (isFeasible && a == 0)
                            {
                                wasFeasible = true;

                                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

#region SECTION HEADER

                                UtilityServices.DrawHorizontalLine(Color.black, 1, 2, 7, 4);

                                content = new GUIContent(); //2F4F4F //008080 //191970 //006699
                                content.text = "<b><color=#006699>Materials Properties</color></b>";
                                content.tooltip = "Adjust the materials properties for this GameObject. This won't change the old materials properties but it will allow you to adjust properties of the combined material exclusively for each object.";

                                style = GUI.skin.label;
                                style.richText = true;
                                OldAlignment = style.alignment;
                                style.alignment = TextAnchor.MiddleCenter;

                                EditorGUILayout.LabelField(content, style);

                                style.alignment = OldAlignment;

                                UtilityServices.DrawHorizontalLine(Color.black, 1, 1, 7, 4);

#endregion SECTION HEADER
                            }

                            int matIndex = -1;
                            int texArrIndex = -1;

                            if (isFeasible)
                            {
                                matIndex = combinedMats[0].materialProperties.matIndex;
                                texArrIndex = combinedMats[0].materialProperties.texArrIndex;
                            }

                            for (int b = 0; (isFeasible && b < combinedMats.Count); b++, matIndex++)
                            {
                                GUILayout.Space(6);

                                MaterialProperties originalMatProps = combinedMats[b].materialProperties;
                                MaterialProperties tempMatProps = combinedMats[b].tempMaterialProperties;

                                originalMatProps.matIndex = tempMatProps.matIndex = matIndex;


#region MATERIAL NAME AND APPLY CHANGES
                                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                                GUILayout.Space(12);

                                content = new GUIContent();  //FF6347ff  //006699 //008080
                                content.text = $"<b><color=#2F4F4F>Material : {originalMatProps.materialName}</color></b>";
                                content.tooltip = "Click to toggle displaying properties for this material.";

                                style = GUI.skin.label;
                                style.richText = true;

                                originalMatProps.foldOut = EditorGUILayout.Foldout(originalMatProps.foldOut, content, true);


                                style = GUI.skin.button;
                                style.richText = true;


                                content = new GUIContent();
                                content.text = "<b><size=11><color=#D2691E>Reset</color></size></b>";
                                content.tooltip = "Reset this material's properties to the ones after the last apply operation)";

                                if (GUILayout.Button(content, style, GUILayout.Width(60), GUILayout.Height(20)))
                                {
                                    tempMatProps = new MaterialProperties();
                                    combinedMats[b].tempMaterialProperties = tempMatProps;

                                    tempMatProps.albedoTint = originalMatProps.albedoTint;
                                    tempMatProps.uvTileOffset = originalMatProps.uvTileOffset;
                                    tempMatProps.normalIntensity = originalMatProps.normalIntensity;
                                    tempMatProps.occlusionIntensity = originalMatProps.occlusionIntensity;
                                    tempMatProps.smoothnessIntensity = originalMatProps.smoothnessIntensity;
                                    tempMatProps.glossMapScale = originalMatProps.glossMapScale;
                                    tempMatProps.metalIntensity = originalMatProps.metalIntensity;
                                    tempMatProps.emissionColor = originalMatProps.emissionColor;
                                    tempMatProps.detailUVTileOffset = originalMatProps.detailUVTileOffset;
                                    tempMatProps.alphaCutoff = originalMatProps.alphaCutoff;
                                    tempMatProps.specularColor = originalMatProps.specularColor;
                                    tempMatProps.detailNormalScale = originalMatProps.detailNormalScale;
                                    tempMatProps.heightIntensity = originalMatProps.heightIntensity;
                                    tempMatProps.uvSec = originalMatProps.uvSec;


                                    try
                                    {
                                        tempMatProps.BurnAttrToImg(ref attrImg, matIndex, texArrIndex);

                                        int index = dataContainer.materialsToRestore.IndexOf(originalMatProps);
                                        if (index == -1)
                                        {
                                            dataContainer.materialsToRestore.Add(originalMatProps);
                                        }
                                    }

                                    catch (Exception ex) { }

                                }

                                GUILayout.Space(2);

                                content = new GUIContent();
                                content.text = "<b><size=11><color=#D2691E>Apply</color></size></b>";
                                content.tooltip = "Apply all property changes on this material. This is important if you want to keep the changes persistent. Please do save the scene after applying the changes and before existing the editor, otherwise you will get data inconsistencies";

                                if (GUILayout.Button(content, style, GUILayout.Width(80), GUILayout.Height(20)))
                                {
                                    bool failed = false;

                                    try
                                    {
                                        tempMatProps.BurnAttrToImg(ref attrImg, matIndex, texArrIndex);

                                        string path = AssetDatabase.GetAssetPath(attrImg);
                                        AttributesImage.BurnToAttributesImg(attrImg, path);
                                        int index = dataContainer.materialsToRestore.IndexOf(originalMatProps);
                                        if (index != -1) { dataContainer.materialsToRestore.RemoveAt(index); }
                                    }

                                    catch (Exception ex)
                                    {
                                        failed = true;
                                    }

                                    if (!failed)
                                    {
                                        EditorSceneManager.MarkSceneDirty(Selection.activeGameObject.scene);

                                        originalMatProps.albedoTint = tempMatProps.albedoTint;
                                        originalMatProps.uvTileOffset = tempMatProps.uvTileOffset;
                                        originalMatProps.normalIntensity = tempMatProps.normalIntensity;
                                        originalMatProps.occlusionIntensity = tempMatProps.occlusionIntensity;
                                        originalMatProps.smoothnessIntensity = tempMatProps.smoothnessIntensity;
                                        originalMatProps.glossMapScale = tempMatProps.glossMapScale;
                                        originalMatProps.metalIntensity = tempMatProps.metalIntensity;
                                        originalMatProps.emissionColor = tempMatProps.emissionColor;
                                        originalMatProps.detailUVTileOffset = tempMatProps.detailUVTileOffset;
                                        originalMatProps.alphaCutoff = tempMatProps.alphaCutoff;
                                        originalMatProps.specularColor = tempMatProps.specularColor;
                                        originalMatProps.detailNormalScale = tempMatProps.detailNormalScale;
                                        originalMatProps.heightIntensity = tempMatProps.heightIntensity;
                                    }

                                }

                                EditorGUILayout.EndHorizontal();
#endregion MATERIAL NAME AND APPLY CHANGES

                                GUILayout.Space(6);

                                if (originalMatProps.foldOut)
                                {
                                    EditorGUI.BeginChangeCheck();

#region ALBEDO TINT COLOR
                                    GUILayout.BeginHorizontal();

                                    content = new GUIContent();
                                    style = GUI.skin.label;
                                    style.richText = true;
                                    prevPadding = new RectOffset(style.padding.left, style.padding.right, style.padding.top, style.padding.bottom);

                                    content.text = "Albedo Tint Color";
                                    content.tooltip = "Adjust the Albedo tint color";

                                    GUILayout.Space(24);

                                    EditorGUILayout.LabelField(content, style, GUILayout.Width(141));
                                    tempMatProps.albedoTint = EditorGUILayout.ColorField(tempMatProps.albedoTint, GUILayout.Width(110), GUILayout.ExpandWidth(true));

                                    EditorGUILayout.LabelField("", GUILayout.Width(0)); // Blank label to stop UI field from bleeding ro getting too close to the edge

                                    GUILayout.EndHorizontal();
#endregion ALBEDO TINT COLOR


#region METALLIC INTENSITY

                                    GUILayout.BeginHorizontal();

                                    content = new GUIContent();
                                    prevPadding = new RectOffset(style.padding.left, style.padding.right, style.padding.top, style.padding.bottom);

                                    content.text = "Metallic Intensity";
                                    content.tooltip = "Adjust the metal intensity/strength";


                                    GUILayout.Space(24);

                                    EditorGUILayout.LabelField(content, style, GUILayout.Width(141));

                                    floatLevel = Mathf.Abs(GUILayout.HorizontalSlider(tempMatProps.metalIntensity, 0, 10, GUILayout.ExpandWidth(true)));
                                    style = GUI.skin.textField;

                                    GUILayout.Space(5);

                                    content.text = "";
                                    floatLevel = Mathf.Abs(EditorGUILayout.FloatField(content, floatLevel, style, GUILayout.Width(40)));
                                    tempMatProps.metalIntensity = Mathf.Clamp(floatLevel, 0f, 10f);

                                    EditorGUILayout.LabelField("", GUILayout.Width(0)); // Blank label to stop UI field from bleeding ro getting too close to the edge

                                    GUILayout.EndHorizontal();
#endregion METALLIC INTENSITY

#region SMOOTHNESS INTENSITY

                                    GUILayout.BeginHorizontal();

                                    style = GUI.skin.label;
                                    content = new GUIContent();
                                    prevPadding = new RectOffset(style.padding.left, style.padding.right, style.padding.top, style.padding.bottom);

                                    content.text = "Smoothness Intensity";
                                    content.tooltip = "Adjust the smoothness intensity/strength";


                                    GUILayout.Space(24);

                                    EditorGUILayout.LabelField(content, style, GUILayout.Width(141));

                                    floatLevel = Mathf.Abs(GUILayout.HorizontalSlider(tempMatProps.smoothnessIntensity, 0, 10, GUILayout.ExpandWidth(true)));
                                    style = GUI.skin.textField;

                                    GUILayout.Space(5);

                                    content.text = "";
                                    floatLevel = Mathf.Abs(EditorGUILayout.FloatField(content, floatLevel, style, GUILayout.Width(40)));
                                    tempMatProps.smoothnessIntensity = Mathf.Clamp(floatLevel, 0f, 10f);

                                    EditorGUILayout.LabelField("", GUILayout.Width(0)); // Blank label to stop UI field from bleeding ro getting too close to the edge

                                    GUILayout.EndHorizontal();
#endregion SMOOTHNESS INTENSITY

#region NORMAL INTENSITY

                                    GUILayout.BeginHorizontal();

                                    style = GUI.skin.label;
                                    content = new GUIContent();
                                    prevPadding = new RectOffset(style.padding.left, style.padding.right, style.padding.top, style.padding.bottom);

                                    content.text = "Normal Intensity";
                                    content.tooltip = "Adjust the normal intensity/strength";


                                    GUILayout.Space(24);

                                    EditorGUILayout.LabelField(content, style, GUILayout.Width(141));

                                    floatLevel = Mathf.Abs(GUILayout.HorizontalSlider(tempMatProps.normalIntensity, 0, 10, GUILayout.ExpandWidth(true)));
                                    style = GUI.skin.textField;

                                    GUILayout.Space(5);

                                    content.text = "";
                                    floatLevel = Mathf.Abs(EditorGUILayout.FloatField(content, floatLevel, style, GUILayout.Width(40)));
                                    tempMatProps.normalIntensity = Mathf.Clamp(floatLevel, 0f, 10f);

                                    EditorGUILayout.LabelField("", GUILayout.Width(0)); // Blank label to stop UI field from bleeding ro getting too close to the edge

                                    GUILayout.EndHorizontal();
#endregion NORMAL INTENSITY

#region HEIGHT INTENSITY

                                    GUILayout.BeginHorizontal();

                                    style = GUI.skin.label;
                                    content = new GUIContent();
                                    prevPadding = new RectOffset(style.padding.left, style.padding.right, style.padding.top, style.padding.bottom);

                                    content.text = "Height Intensity";
                                    content.tooltip = "Adjust the parallax height intensity/strength";


                                    GUILayout.Space(24);

                                    EditorGUILayout.LabelField(content, style, GUILayout.Width(141));

                                    floatLevel = Mathf.Abs(GUILayout.HorizontalSlider(tempMatProps.heightIntensity, 0, 0.2f, GUILayout.ExpandWidth(true)));
                                    style = GUI.skin.textField;

                                    GUILayout.Space(5);

                                    content.text = "";
                                    floatLevel = Mathf.Abs(EditorGUILayout.FloatField(content, floatLevel, style, GUILayout.Width(40)));
                                    tempMatProps.heightIntensity = Mathf.Clamp(floatLevel, 0f, 0.2f);

                                    EditorGUILayout.LabelField("", GUILayout.Width(0)); // Blank label to stop UI field from bleeding ro getting too close to the edge

                                    GUILayout.EndHorizontal();
#endregion HEIGHT INTENSITY

#region OCCLUSION INTENSITY

                                    GUILayout.BeginHorizontal();

                                    style = GUI.skin.label;
                                    content = new GUIContent();
                                    prevPadding = new RectOffset(style.padding.left, style.padding.right, style.padding.top, style.padding.bottom);

                                    content.text = "Occlusion Intensity";
                                    content.tooltip = "Adjust the parallax occlusion intensity/strength";


                                    GUILayout.Space(24);

                                    EditorGUILayout.LabelField(content, style, GUILayout.Width(141));

                                    floatLevel = Mathf.Abs(GUILayout.HorizontalSlider(tempMatProps.occlusionIntensity, 0, 10, GUILayout.ExpandWidth(true)));
                                    style = GUI.skin.textField;

                                    GUILayout.Space(5);

                                    content.text = "";
                                    floatLevel = Mathf.Abs(EditorGUILayout.FloatField(content, floatLevel, style, GUILayout.Width(40)));
                                    tempMatProps.occlusionIntensity = Mathf.Clamp(floatLevel, 0f, 10f);

                                    EditorGUILayout.LabelField("", GUILayout.Width(0)); // Blank label to stop UI field from bleeding ro getting too close to the edge

                                    GUILayout.EndHorizontal();
#endregion OCCLUSION INTENSITY

#region DETAIL NORMAL INTENSITY

                                    GUILayout.BeginHorizontal();

                                    style = GUI.skin.label;
                                    content = new GUIContent();
                                    prevPadding = new RectOffset(style.padding.left, style.padding.right, style.padding.top, style.padding.bottom);

                                    content.text = "Detail Normal Intensity";
                                    content.tooltip = "Adjust the detail normal intensity/strength";


                                    GUILayout.Space(24);

                                    EditorGUILayout.LabelField(content, style, GUILayout.Width(141));

                                    floatLevel = Mathf.Abs(GUILayout.HorizontalSlider(tempMatProps.detailNormalScale, 0, 10, GUILayout.ExpandWidth(true)));
                                    style = GUI.skin.textField;

                                    GUILayout.Space(5);

                                    content.text = "";
                                    floatLevel = Mathf.Abs(EditorGUILayout.FloatField(content, floatLevel, style, GUILayout.Width(40)));
                                    tempMatProps.detailNormalScale = Mathf.Clamp(floatLevel, 0f, 10f);

                                    EditorGUILayout.LabelField("", GUILayout.Width(0)); // Blank label to stop UI field from bleeding ro getting too close to the edge

                                    GUILayout.EndHorizontal();
#endregion DETAIL NORMAL INTENSITY

#region GLOSS INTENSITY

                                    GUILayout.BeginHorizontal();

                                    style = GUI.skin.label;
                                    content = new GUIContent();
                                    prevPadding = new RectOffset(style.padding.left, style.padding.right, style.padding.top, style.padding.bottom);

                                    content.text = "Gloss Intensity";
                                    content.tooltip = "Adjust the gloss intensity/strength";


                                    GUILayout.Space(24);

                                    EditorGUILayout.LabelField(content, style, GUILayout.Width(141));

                                    floatLevel = Mathf.Abs(GUILayout.HorizontalSlider(tempMatProps.glossMapScale, 0, 1, GUILayout.ExpandWidth(true)));
                                    style = GUI.skin.textField;

                                    GUILayout.Space(5);

                                    content.text = "";
                                    floatLevel = Mathf.Abs(EditorGUILayout.FloatField(content, floatLevel, style, GUILayout.Width(40)));
                                    tempMatProps.glossMapScale = Mathf.Clamp(floatLevel, 0f, 1f);

                                    EditorGUILayout.LabelField("", GUILayout.Width(0)); // Blank label to stop UI field from bleeding ro getting too close to the edge

                                    GUILayout.EndHorizontal();
#endregion GLOSS INTENSITY

#region ALPHA CUTOFF

                                    GUILayout.BeginHorizontal();

                                    style = GUI.skin.label;
                                    content = new GUIContent();
                                    prevPadding = new RectOffset(style.padding.left, style.padding.right, style.padding.top, style.padding.bottom);

                                    content.text = "Alpha Cutoff";
                                    content.tooltip = "Adjust the Alpha Cutoff value";


                                    GUILayout.Space(24);

                                    EditorGUILayout.LabelField(content, style, GUILayout.Width(141));

                                    floatLevel = Mathf.Abs(GUILayout.HorizontalSlider(tempMatProps.alphaCutoff, 0, 1, GUILayout.ExpandWidth(true)));
                                    style = GUI.skin.textField;

                                    GUILayout.Space(5);

                                    content.text = "";
                                    floatLevel = Mathf.Abs(EditorGUILayout.FloatField(content, floatLevel, style, GUILayout.Width(40)));
                                    tempMatProps.alphaCutoff = Mathf.Clamp(floatLevel, 0f, 1f);

                                    EditorGUILayout.LabelField("", GUILayout.Width(0)); // Blank label to stop UI field from bleeding ro getting too close to the edge

                                    GUILayout.EndHorizontal();
#endregion ALPHA CUTOFF

#region SPECULAR COLOR
                                    GUILayout.BeginHorizontal();

                                    style = GUI.skin.label;
                                    content.text = "Specular Color";
                                    content.tooltip = "Adjust the specular color";

                                    GUILayout.Space(24);

                                    EditorGUILayout.LabelField(content, style, GUILayout.Width(141));
                                    tempMatProps.specularColor = EditorGUILayout.ColorField(tempMatProps.specularColor, GUILayout.Width(110), GUILayout.ExpandWidth(true));

                                    EditorGUILayout.LabelField("", GUILayout.Width(0)); // Blank label to stop UI field from bleeding ro getting too close to the edge

                                    GUILayout.EndHorizontal();
#endregion SPECULAR COLOR

#region EMISSION COLOR
                                    GUILayout.BeginHorizontal();

                                    style = GUI.skin.label;
                                    content.text = "Emission Color";
                                    content.tooltip = "Adjust the emission color";

                                    GUILayout.Space(24);

                                    EditorGUILayout.LabelField(content, style, GUILayout.Width(141));
                                    tempMatProps.emissionColor = EditorGUILayout.ColorField(tempMatProps.emissionColor, GUILayout.Width(110), GUILayout.ExpandWidth(true));

                                    EditorGUILayout.LabelField("", GUILayout.Width(0)); // Blank label to stop UI field from bleeding ro getting too close to the edge

                                    GUILayout.EndHorizontal();
#endregion EMISSION COLOR


#region UV TILE OFFSET

                                    GUILayout.BeginHorizontal();

                                    style = GUI.skin.label;
                                    content = new GUIContent();
                                    prevPadding = new RectOffset(style.padding.left, style.padding.right, style.padding.top, style.padding.bottom);

                                    content.text = "UV Tile Offset";
                                    content.tooltip = "Adjust the UV tiling and offset values";

                                    GUILayout.Space(24);

                                    EditorGUILayout.LabelField(content, style, GUILayout.Width(141));

                                    tempMatProps.uvTileOffset = EditorGUILayout.Vector4Field("", tempMatProps.uvTileOffset, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
                                    style = GUI.skin.textField;

                                    EditorGUILayout.LabelField("", GUILayout.Width(0)); // Blank label to stop UI field from bleeding ro getting too close to the edge

                                    GUILayout.EndHorizontal();

#endregion UV TILE OFFSET

#region DETAIL UV TILE OFFSET

                                    GUILayout.BeginHorizontal();

                                    style = GUI.skin.label;
                                    content = new GUIContent();
                                    prevPadding = new RectOffset(style.padding.left, style.padding.right, style.padding.top, style.padding.bottom);

                                    content.text = "Detail UV Tile Offset";
                                    content.tooltip = "Adjust the UV tiling and offset values for detail maps";

                                    GUILayout.Space(24);

                                    EditorGUILayout.LabelField(content, style, GUILayout.Width(141));

                                    tempMatProps.detailUVTileOffset = EditorGUILayout.Vector4Field("", tempMatProps.detailUVTileOffset, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
                                    style = GUI.skin.textField;

                                    EditorGUILayout.LabelField("", GUILayout.Width(0)); // Blank label to stop UI field from bleeding ro getting too close to the edge

                                    GUILayout.EndHorizontal();

#endregion DETAIL UV TILE OFFSET


                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        try
                                        {
                                            tempMatProps.BurnAttrToImg(ref attrImg, matIndex, texArrIndex);

                                            int index = dataContainer.materialsToRestore.IndexOf(originalMatProps);
                                            if (index == -1)
                                            {
                                                dataContainer.materialsToRestore.Add(originalMatProps);
                                            }
                                        }

                                        catch (Exception ex)
                                        {

                                        }
                                    }

                                    matIndex++;
                                }

                            }
                        }

                        if (wasFeasible) { EditorGUILayout.EndVertical(); }
                        
                        dataContainer.reInitializeTempMatProps = false;
                    }
                    
                }

#endregion DRAW MATERIALS PROPERTIES



#region REGENERATE TEXTURE ARRAYS


                Rect drop_area = EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Height(50), GUILayout.ExpandWidth(true));
                drop_area.height = 70;

                UtilityServices.DrawHorizontalLine(Color.black, 1, 2, 7, 4);

                content = new GUIContent();//TEXTURE ARRAYS SETTINGS //2F4F4F //008080 //191970 //006699
                content.text = "<b><color=#006699>Alter Texture Arrays</color></b>";
                content.tooltip = "Change properties of existing texture arrays.";

                style = GUI.skin.label;
                style.richText = true;
                OldAlignment = style.alignment;
                style.alignment = TextAnchor.MiddleCenter;

                EditorGUILayout.LabelField(content, style);

                style.alignment = OldAlignment;

                UtilityServices.DrawHorizontalLine(Color.black, 1, 1, 7, 4);


                EditorGUILayout.HelpBox("Drop here existing texture arrays to change their properties", MessageType.Info);

                GUILayout.Space(4);

                GUILayout.BeginHorizontal();


#if UNITY_2019_1_OR_NEWER

                GUILayout.Space(17);                
                
#else
                GUILayout.Space(16);
#endif


                content = new GUIContent();
                content.text = "Texture Arrays";
                content.tooltip = "Expand this to see which texture arrays have been added.";
                style = new GUIStyle(EditorStyles.foldout);
                float width = style.fixedWidth;
                style.fixedWidth = 60;

                existingTextureArraysFoldout = EditorGUILayout.Foldout(existingTextureArraysFoldout, content, true, style);

                style.fixedWidth = width;
                GUILayout.Space(60);

                content.text = "Size";

                EditorGUILayout.LabelField(content, GUILayout.Width(30));

                content.text = "";
                int oldSize = existingTextureArrays.Count;

                existingTextureArraysSize = Mathf.Abs(EditorGUILayout.DelayedIntField(content, existingTextureArrays.Count, GUILayout.Width(40)));

                int diff = existingTextureArraysSize - oldSize;

                if (existingTextureArraysSize == 0)
                {
                    existingTextureArraysFoldout = false;
                    existingTextureArrays = new List<Texture2DArray>();
                }

                else if (diff < 0)
                {
                    diff *= -1;

                    for (int a = 0; a < diff; a++)
                    {
                        existingTextureArrays.RemoveAt(existingTextureArrays.Count - 1);
                    }
                }

                else if (diff != 0)
                {
                    for (int a = 0; a < diff; a++)
                    {
                        existingTextureArrays.Add(null);
                    }
                }

                if (oldSize != existingTextureArraysSize)
                {
                    existingTextureArraysFoldout = true;
                }


                GUILayout.FlexibleSpace();

                style = GUI.skin.button;
                style.richText = true;
                oldPadding = style.margin;
                style.margin = new RectOffset(0, 0, -1, 0);

                content = new GUIContent();
                content.text = "<b><size=11><color=#D2691E>Regenerate</color></size></b>";
                content.tooltip = "Regenerate the added texture arrays with the new properties.";

                if (GUILayout.Button(content, style, GUILayout.Width(100), GUILayout.Height(20)))
                {

                    try
                    {
                        if (existingTextureArrays != null && existingTextureArrays.Count > 0)
                        {

                            int index = 0;
                            DiffuseColorSpace colorSpace = DiffuseColorSpace.NON_LINEAR;


                            if (dataContainer.colorSpaceChoices[dataContainer.choiceColorSpace].ToLower().Contains("non"))
                            {
                                colorSpace = DiffuseColorSpace.NON_LINEAR;
                            }
                            else
                            {
                                colorSpace = DiffuseColorSpace.LINEAR;
                            }


                            foreach (var existingTexArr in existingTextureArrays)
                            {

                                index++;

                                EditorUtility.DisplayProgressBar("Changing Texture Arrays", $"Regenerating texture arrays with new settings {index}/{existingTextureArrays.Count}", (float)(index) / existingTextureArrays.Count);

                                if (existingTexArr == null) { continue; }

                                string existingTexArrPath = AssetDatabase.GetAssetPath(existingTexArr);
                                Texture2DArray textArr = AssetDatabase.LoadAssetAtPath<Texture2DArray>(existingTexArrPath);
                                string parentDirPath = Directory.GetParent(existingTexArrPath).ToString().Replace('\\', '/');
                                string attrImgPath = parentDirPath + "/_MatAttributes.atim";


                                var attrImg = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(attrImgPath);

                                if (attrImg == null)
                                {
                                    Debug.LogWarning($"The TextureArray \"{existingTexArr.name}\" at path \"{existingTexArrPath}\" has no AttributesImage present at the same path");
                                    continue;
                                }

                                try
                                {
                                    existingTexArr.GetPixels(0);
                                }

                                catch (Exception ex)
                                {
                                    Debug.LogWarning($"The TextureArray \"{existingTexArr.name}\" at path \"{existingTexArrPath}\" is not readible. Please mark it readible from the inspector.");
                                    continue;
                                }

                                Resolution resolution = new Resolution();
                                resolution.width = existingTexArr.width;
                                resolution.height = existingTexArr.height;

                                existingArraysProps.resolution = resolution;

                                var relocatedTexArr = MaterialCombiner.AllocateArray(existingArraysProps, existingTexArr.depth, colorSpace, MaterialCombiner.IsAlphaCompressed(existingTexArr));


                                for (int a = 0; a < existingTexArr.depth; a++)
                                {
                                    Texture2D tempTex = new Texture2D(1, 1);

                                    Resolution reso = new Resolution();
                                    reso.width = existingTexArr.width;
                                    reso.height = existingTexArr.height;
                                    Texture2D texture = MaterialCombiner.GetResizedTexture(tempTex, reso, colorSpace == DiffuseColorSpace.NON_LINEAR ? false : true);
                                    texture.SetPixels(existingTexArr.GetPixels(a));
                                    texture.Apply();


                                    DestroyImmediate(tempTex);

                                    MaterialCombiner.CompressTexture(texture, existingArraysProps, true);

                                    MaterialCombiner.WriteTextureToTextureArray(texture, relocatedTexArr, a, MaterialCombiner.SizeToMipCount(existingArraysProps));

                                    DestroyImmediate(texture);

                                }



                                EditorUtility.CopySerialized(relocatedTexArr, existingTexArr);
                                AssetDatabase.SaveAssets();

                                DestroyImmediate(relocatedTexArr);

                            }

                            EditorUtility.ClearProgressBar();
                        }
                    }

                    catch (Exception ex)
                    {
                        EditorUtility.ClearProgressBar();

                        string error = $"Failed to regenerate texture arrays with the specified settings due to unknown reasons. Please check console for any clues.";
                        EditorUtility.DisplayDialog("Operation Failed", error, "Ok");
                        Debug.LogError(ex);
                    }


                }

                style.margin = oldPadding;

                GUILayout.EndHorizontal();


                Event evt = Event.current;

                switch (evt.type)
                {
                    case EventType.DragUpdated:
                    case EventType.DragPerform:

                        if (drop_area.Contains(evt.mousePosition))
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                            if (evt.type == EventType.DragPerform)
                            {
                                DragAndDrop.AcceptDrag();

                                foreach (UnityEngine.Object dragged_object in DragAndDrop.objectReferences)
                                {
                                    // Do On Drag Stuff here
                                    string path = AssetDatabase.GetAssetPath(dragged_object);

                                    if (!String.IsNullOrWhiteSpace(path))
                                    {
                                        Texture2DArray textArr = AssetDatabase.LoadAssetAtPath<Texture2DArray>(path);

                                        if (textArr != null)
                                        {
                                            string attrImgPath = Directory.GetParent(path).ToString().Replace('\\', '/') + "/_MatAttributes.atim";

                                            var attrImg = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(attrImgPath);

                                            //if(attrImg == null) { Debug.Log("Not loaded attr image check your path   " + attrImgPath); }

                                            if (!existingTextureArrays.Contains(textArr) && attrImg != null)
                                            {
                                                //Debug.Log("Added");
                                                if (existingTextureArrays.Count > 0)
                                                {
                                                    bool alreadyAdded = false;

                                                    for (int a = 0; a < existingTextureArrays.Count; a++)
                                                    {
                                                        var item = existingTextureArrays[a];

                                                        if (item == null)
                                                        {
                                                            existingTextureArrays[a] = textArr;

                                                            alreadyAdded = true;
                                                            break;
                                                        }
                                                    }

                                                    if (!alreadyAdded)
                                                    {
                                                        int oldCount = existingTextureArrays.Count;
                                                        existingTextureArrays.Add(textArr);
                                                        if (oldCount == 0) { existingTextureArraysFoldout = true; }

                                                    }
                                                }

                                                else
                                                {
                                                    int oldCount = existingTextureArrays.Count;
                                                    existingTextureArrays.Add(textArr);
                                                    if (oldCount == 0) { existingTextureArraysFoldout = true; }
                                                }

                                            }

                                        }
                                    }

                                }
                            }

                        }

                        break;
                }


                if (existingTextureArrays != null && existingTextureArrays.Count > 0)
                {
                    if (existingTextureArraysFoldout)
                    {

                        UtilityServices.DrawHorizontalLine(new Color(105 / 255f, 105 / 255f, 105 / 255f), 1, 5, 8, 4);

                        GUILayout.Space(8);


                        for (int a = 0; a < existingTextureArrays.Count; a++)
                        {
                            var existingItem = existingTextureArrays[a];
                            var rect = GUILayoutUtility.GetRect(0.0f, 16.0f, GUILayout.ExpandWidth(true));
                            GUILayout.Space(2);
                            var obj = EditorGUI.ObjectField(rect, existingItem, typeof(UnityEngine.Object), false);

                            if (obj != null && existingItem == null)
                            {
                                string path = AssetDatabase.GetAssetPath(obj);

                                if (!String.IsNullOrWhiteSpace(path))
                                {
                                    Texture2DArray textArr = AssetDatabase.LoadAssetAtPath<Texture2DArray>(path);
                                    string attrImgPath = Directory.GetParent(path).ToString().Replace('\\', '/') + "/_MatAttributes.atim";

                                    var attrImg = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(attrImgPath);


                                    if (textArr != null && !existingTextureArrays.Contains(textArr) && attrImg != null)
                                    {
                                        existingTextureArrays[a] = textArr;
                                    }
                                }
                            }

                            else if (obj == null)
                            {
                                existingTextureArrays[a] = null;
                            }
                        }


                        UtilityServices.DrawHorizontalLine(new Color(105 / 255f, 105 / 255f, 105 / 255f), 1, 5, 8, 4);


                    }


                    GUILayout.Space(6);
                    content = new GUIContent();
                    content.text = "Properties";
                    content.tooltip = "Expand this to change properties for the added texture arrays.";

                    GUILayout.BeginHorizontal();


#if UNITY_2019_1_OR_NEWER
                
                    GUILayout.Space(17);
               
#else
                    GUILayout.Space(16);

#endif

                    textureArraysPropsFoldout = EditorGUILayout.Foldout(textureArraysPropsFoldout, content, true);


                    GUILayout.EndHorizontal();


                    if (existingArraysProps == null)
                    {
                        dataContainer.textureArraysSettings = new MaterialCombiner.CombiningInformation.TextureArrayGroup();

                        var defaultRes = new MaterialCombiner.CombiningInformation.Resolution();
                        defaultRes.width = Int32.Parse(dataContainer.resolutionsChoices[4]);
                        defaultRes.height = Int32.Parse(dataContainer.resolutionsChoices[4]);
                        fMode = dataContainer.filteringModesChoices[0];

                        if (fMode.ToLower().Contains("point")) { filteringMode = FilterMode.Point; }
                        else if (fMode.ToLower().Contains("Bilinear")) { filteringMode = FilterMode.Bilinear; }
                        else { filteringMode = FilterMode.Trilinear; }

                        cT = dataContainer.compressionTypesChoices[0];

                        compType = (MaterialCombiner.CombiningInformation.CompressionType)Enum.Parse(typeof(MaterialCombiner.CombiningInformation.CompressionType), cT.ToUpper());

                        cQ = dataContainer.compressionQualitiesChoices[1];

                        compQuality = (MaterialCombiner.CombiningInformation.CompressionQuality)Enum.Parse(typeof(MaterialCombiner.CombiningInformation.CompressionQuality), cQ.ToUpper());

                        existingArraysProps = new TextureArrayUserSettings
                        (
                            defaultRes,
                            filteringMode,
                            CompressionType.UNCOMPRESSED,
                            compQuality,
                            1
                        );

                    }

                    if (textureArraysPropsFoldout)
                    {
                        GUILayout.Space(4);


#region PROPERTIES


#region FILTERING MODE
                        GUILayout.BeginHorizontal();

                        content.text = "Filtering Mode";
                        content.tooltip = "The filtering mode for textures in the added texture arrays.";


                        GUILayout.Space(16);
                        style = EditorStyles.label;

                        EditorGUILayout.LabelField(content, style, GUILayout.Width(129));

                        existingArraysProps.choiceFilteringMode = EditorGUILayout.Popup("", existingArraysProps.choiceFilteringMode, dataContainer.filteringModesChoices, GUILayout.Width(142), GUILayout.ExpandWidth(true));

                        fMode = dataContainer.filteringModesChoices[existingArraysProps.choiceFilteringMode];

                        if (fMode.ToLower().Contains("point")) { filteringMode = FilterMode.Point; }
                        else if (fMode.ToLower().Contains("Bilinear")) { filteringMode = FilterMode.Bilinear; }
                        else { filteringMode = FilterMode.Trilinear; }

                        existingArraysProps.filteringMode = filteringMode;

                        EditorGUILayout.LabelField("", style, GUILayout.Width(0)); // Blank label to stop UI field from bleeding ro getting too close to the edge

                        GUILayout.EndHorizontal();
#endregion FILTERING MODE


#region ANISOTROPIC FILTERING
                        GUILayout.Space(-2);

                        GUILayout.BeginHorizontal();

                        content = new GUIContent();
                        prevPadding = new RectOffset(style.padding.left, style.padding.right, style.padding.top, style.padding.bottom);

                        content.text = "Anisotropic Level";
                        content.tooltip = "The level of the anisotropic filtering for the textures in the added texture arrays.";


                        GUILayout.Space(16);

                        EditorGUILayout.LabelField(content, style, GUILayout.Width(129));

                        floatLevel = Mathf.Abs(GUILayout.HorizontalSlider(existingArraysProps.anisotropicFilteringLevel, 0, 16, GUILayout.Width(142), GUILayout.ExpandWidth(true)));
                        style = GUI.skin.textField;
                        existingArraysProps.anisotropicFilteringLevel = Mathf.RoundToInt(floatLevel);

                        GUILayout.Space(5);

                        content.text = "";
                        existingArraysProps.anisotropicFilteringLevel = Mathf.Abs(EditorGUILayout.IntField(content, existingArraysProps.anisotropicFilteringLevel, style, GUILayout.Width(40)));
                        existingArraysProps.anisotropicFilteringLevel = Mathf.Clamp(existingArraysProps.anisotropicFilteringLevel, (int)0, (int)16);

                        EditorGUILayout.LabelField("", GUILayout.Width(0)); // Blank label to stop UI field from bleeding ro getting too close to the edge

                        GUILayout.EndHorizontal();
#endregion ANISOTROPIC FILTERING


#region COMPRESSION QUALITY
                        GUILayout.Space(1);

                        GUILayout.BeginHorizontal();

                        content.text = "Compression Quality";
                        content.tooltip = "The compression quality for the textures in the added texture arrays. This option is only valid if the compression type selected is \"ASTC RGB\" ";

                        style = GUI.skin.label;
                        GUILayout.Space(16);


                        EditorGUI.BeginDisabledGroup(dataContainer.compressionTypesChoices[existingArraysProps.choiceCompressionType] != "ASTC_RGB");

                        EditorGUILayout.LabelField(content, style, GUILayout.Width(129));

                        existingArraysProps.choiceCompressionQuality = EditorGUILayout.Popup("", existingArraysProps.choiceCompressionQuality, dataContainer.compressionQualitiesChoices, GUILayout.Width(142), GUILayout.ExpandWidth(true));

                        EditorGUI.EndDisabledGroup();


                        cQ = dataContainer.compressionQualitiesChoices[existingArraysProps.choiceCompressionQuality];
                        compQuality = (MaterialCombiner.CombiningInformation.CompressionQuality)Enum.Parse(typeof(MaterialCombiner.CombiningInformation.CompressionQuality), cQ.ToUpper());

                        existingArraysProps.compressionQuality = compQuality;

                        EditorGUILayout.LabelField("", GUILayout.Width(0)); // Blank label to stop UI field from bleeding ro getting too close to the edge

                        GUILayout.EndHorizontal();
#endregion COMPRESSION QUALITY


#region COMPRESSION TYPE

                        GUILayout.BeginHorizontal();

                        content.text = "Compression Type";
                        content.tooltip = "The compression type for the textures in the added texture arrays.";

                        style = GUI.skin.label;
                        GUILayout.Space(16);

                        EditorGUILayout.LabelField(content, style, GUILayout.Width(129));

                        existingArraysProps.choiceCompressionType = EditorGUILayout.Popup("", existingArraysProps.choiceCompressionType, dataContainer.compressionTypesChoices, GUILayout.Width(142), GUILayout.ExpandWidth(true));

                        cT = dataContainer.compressionTypesChoices[existingArraysProps.choiceCompressionType];
                        compType = (MaterialCombiner.CombiningInformation.CompressionType)Enum.Parse(typeof(MaterialCombiner.CombiningInformation.CompressionType), cT.ToUpper());
                        existingArraysProps.compressionType = compType;

                        EditorGUILayout.LabelField("", GUILayout.Width(0)); // Blank label to stop UI field from bleeding ro getting too close to the edge

                        GUILayout.EndHorizontal();

#endregion COMPRESSION TYPE


#region COLOR SPACE

                        GUILayout.BeginHorizontal();

                        content.text = "Color Space";
                        content.tooltip = "The color space of the textures in the selected texture arrays.[Usually NON-LINEAR]";

                        style = GUI.skin.label;
                        GUILayout.Space(16);


                        EditorGUILayout.LabelField(content, style, GUILayout.Width(129));

                        dataContainer.choiceColorSpace = EditorGUILayout.Popup("", dataContainer.choiceColorSpace, dataContainer.colorSpaceChoices, GUILayout.Width(142), GUILayout.ExpandWidth(true));

                        EditorGUILayout.LabelField("", GUILayout.Width(0)); // Blank label to stop UI field from bleeding ro getting too close to the edge

                        GUILayout.EndHorizontal();

#endregion COLOR SPACE


#endregion PROPERTIES

                    }


                }


                EditorGUILayout.EndVertical();



#endregion REGENERATE TEXTURE ARRAYS


            }

        }


        private void SelectionChanged()
        {
            //Debug.Log("SelectionChanged called on InspectorDrawer");

            if (lastMultiSelectedObjects != null)
            {
                foreach (var lastMultiSelectObject in lastMultiSelectedObjects)
                {
                    if (lastMultiSelectObject != null)
                    {
                        PolyFewHost host = lastMultiSelectObject.GetComponent<PolyFewHost>();
                        if (host != null)
                        {
                            DestroyImmediate(host, true);
                        }
                    }
                }

            }


            if (Selection.activeTransform != null && isFeasibleTargetForPolyFew)
            {
                //Debug.Log("Selection Changed and the target is Feasible  reductionPending?  " + ReductionPending);

                bool sameTarget = PrevSelection && Selection.activeTransform.GetHashCode().Equals(PrevSelection.GetHashCode());

                if (ReductionPending && PrevFeasibleTarget != null && !sameTarget)
                {
                    //Debug.Log("Restoring the original meshes as the reductions weren't applied");
                    UtilityServices.RestoreMeshesFromPairs(dataContainer.objectMeshPairs);
                    dataContainer.objectMeshPairs = UtilityServices.GetObjectMeshPairs(Selection.activeGameObject, true, true);
                    ReductionPending = false;
                }

                if (!sameTarget)
                {

                    dataContainer.objectMeshPairs = UtilityServices.GetObjectMeshPairs(Selection.activeGameObject, true, true);
                    //dataContainer.toleranceSpheres = new List<DataContainer.ToleranceSphere>();

                    //if(dataContainer.toleranceSpheres != null)
                    //{
                    //    foreach(var sphere in dataContainer.toleranceSpheres)
                    //    {
                    //        Vector3 lossyScale = Selection.activeTransform.lossyScale;
                    //        float avg = UtilityServices.Average(lossyScale.x, lossyScale.y, lossyScale.z);
                    //        sphere.diameter = sphere.diameter * avg;

                    //        sphere.worldPosition = new Vector3(Selection.activeTransform.position.x + (lossyScale.x / 2f + sphere.diameter / 2f), Selection.activeTransform.position.y, Selection.activeTransform.position.z);
                    //    }
                    //}

                    // reset all params

                    ReductionPending = false;
                    ReductionStrength = 0;

                    if(dataContainer.currentLodLevelSettings == null || dataContainer.currentLodLevelSettings.Count == 0)
                    {
                        dataContainer.currentLodLevelSettings = new List<DataContainer.LODLevelSettings>();

                        dataContainer.currentLodLevelSettings.Add(new DataContainer.LODLevelSettings(0, 0.6f, false, false, false, true, false, 7, 100, false, false, false, new List<float>()));
                        dataContainer.currentLodLevelSettings.Add(new DataContainer.LODLevelSettings(30, 0.4f, false, false, false, true, false, 7, 100, false, false, false, new List<float>()));
                        dataContainer.currentLodLevelSettings.Add(new DataContainer.LODLevelSettings(60, 0.15f, false, false, false, true, false, 7, 100, false, false, false, new List<float>()));
                    }


                    //if (dataContainer.currentLodLevelSettings != null)
                    //{
                    //    foreach (var lodLevel in dataContainer.currentLodLevelSettings)
                    //    {
                    //        lodLevel.sphereIntensities = new List<float>();
                    //    }
                    //}

                    dataContainer.reInitializeTempMatProps = true;

                    if (PrevSelection && dataContainer.materialsToRestore != null)
                    {
                        ObjectMaterialLinks links = PrevSelection.GetComponent<ObjectMaterialLinks>();

                        if(links != null && links.linkedAttrImg != null)
                        {
                            foreach (var origMaterialProps in dataContainer.materialsToRestore)
                            {
                                origMaterialProps.BurnAttrToImg(ref links.linkedAttrImg, origMaterialProps.matIndex, origMaterialProps.texArrIndex);
                            }
                        }
                    }

                    dataContainer.materialsToRestore = new List<MaterialProperties>();


                    FoldoutAutoLOD = false;

                    TriangleCount = UtilityServices.CountTriangles(ReduceDeep, dataContainer.objectMeshPairs, Selection.activeGameObject);
                    RunOnThreads = CheckOnThreads();
                }

                PrevSelection = Selection.activeTransform;

            }

            if (Selection.gameObjects != null && Selection.gameObjects.Length > 1)
            {
                if(lastMultiSelectedObjects == null)
                {
                    lastMultiSelectedObjects = new List<GameObject>();

                    foreach (var item in Selection.gameObjects)
                    {
                        lastMultiSelectedObjects.Add(item);
                    }
                }
            }
        }


        [DidReloadScripts]
        public static void ScriptReloaded()
        {

            /*
            if (Selection.gameObjects != null && Selection.gameObjects.Length == 1)
            {
                ObjectMaterialLinks objMaterialLinks = Selection.gameObjects[0].GetComponent<ObjectMaterialLinks>();

                if (objMaterialLinks != null && objMaterialLinks.linkedMaterialEntity != null)
                {
                    var comb = objMaterialLinks.linkedMaterialEntity.combinedMats;

                    if (comb != null && comb.Count != 0)
                    {
                        dataContainer.lastObjMaterialLinks = objMaterialLinks;
                        dataContainer.relocateMaterialLinks = true;
                    }
                }
            }
            */
        }


        public class FileModificationWarning : UnityEditor.AssetModificationProcessor
        {
            
            static string[] OnWillSaveAssets(string[] paths)
            {
                //if (Selection.activeGameObject == null || dataContainer == null) { return paths; }

                //dataContainer.objectMeshPairs = UtilityServices.GetObjectMeshPairs(Selection.activeGameObject, true, true);
                //TriangleCount = UtilityServices.CountTriangles(ReduceDeep, dataContainer.objectMeshPairs, Selection.activeGameObject);

                return paths;

                int a = 0;
                //Debug.Log("OnWillSaveAssets");

                foreach (string path in paths)
                {
                    if (a == 0 && LastDrawer != null)
                    {
                        //Debug.Log("Last drawer not null");
                        UtilityServices.RestoreMeshesFromPairs(dataContainer.objectMeshPairs);
                        dataContainer.objectMeshPairs = UtilityServices.GetObjectMeshPairs(Selection.activeGameObject, true, true);
                        ((InspectorDrawer)LastDrawer).ReductionPending = false;
                        ReductionStrength = 0;
                    }
                    a++;
                    //Debug.Log(path);
                }

                return paths;
            }


            public static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options)
            {
                //Debug.Log("Deleted   "+ path + "  " + options);
                return AssetDeleteResult.DidNotDelete;
            }

        }


        private void DestroyPolyFewHost()
        {
            if (thisGameObject != null)
            {
                PolyFewHost host = thisGameObject.GetComponent<PolyFewHost>();
                if (host != null)
                {
                    DestroyImmediate(host, true);
                }
            }
        }


        private MaterialCombiner.CombiningInformation.TextureArrayUserSettings GetTexArrSettingsFromName(string name)
        {
            if (name.ToLower().Equals("albedo")) { return dataContainer.textureArraysSettings.diffuseArraySettings; }
            else if (name.ToLower().Equals("metallic")) { return dataContainer.textureArraysSettings.metallicArraySettings; }
            else if (name.ToLower().Equals("specular")) { return dataContainer.textureArraysSettings.specularArraySettings; }
            else if (name.ToLower().Equals("normal")) { return dataContainer.textureArraysSettings.normalArraySettings; }
            else if (name.ToLower().Equals("height")) { return dataContainer.textureArraysSettings.heightArraySettings; }
            else if (name.ToLower().Equals("occlusion")) { return dataContainer.textureArraysSettings.occlusionArraySettings; }
            else if (name.ToLower().Equals("emission")) {  return dataContainer.textureArraysSettings.emissiveArraySettings; }
            else if (name.ToLower().Equals("detail mask")) { return dataContainer.textureArraysSettings.detailMaskArraySettings; }
            else if (name.ToLower().Equals("detail albedo")) { return dataContainer.textureArraysSettings.detailAlbedoArraySettings; }
            else if (name.ToLower().Equals("detail normal")) { return dataContainer.textureArraysSettings.detailNormalArraySettings; }

            return null;
        }


        private void ResetTextureArrays()
        {
            dataContainer.textureArraysSettings = new MaterialCombiner.CombiningInformation.TextureArrayGroup();

            var defaultRes = new MaterialCombiner.CombiningInformation.Resolution();
            defaultRes.width = Int32.Parse(dataContainer.resolutionsChoices[4]);
            defaultRes.height = Int32.Parse(dataContainer.resolutionsChoices[4]);
            var fMode = dataContainer.filteringModesChoices[0];
            FilterMode filteringMode;

            if (fMode.ToLower().Contains("point")) { filteringMode = FilterMode.Point; }
            else if (fMode.ToLower().Contains("Bilinear")) { filteringMode = FilterMode.Bilinear; }
            else { filteringMode = FilterMode.Trilinear; }

            var cT = dataContainer.compressionTypesChoices[0];

            var compType = (MaterialCombiner.CombiningInformation.CompressionType)Enum.Parse(typeof(MaterialCombiner.CombiningInformation.CompressionType), cT.ToUpper());

            var cQ = dataContainer.compressionQualitiesChoices[1];

            var compQuality = (MaterialCombiner.CombiningInformation.CompressionQuality)Enum.Parse(typeof(MaterialCombiner.CombiningInformation.CompressionQuality), cQ.ToUpper());

            dataContainer.textureArraysSettings.InitializeDefaultArraySettings(defaultRes, filteringMode, compType, compQuality, 1);

            dataContainer.choiceDiffuseColorSpace = 0;
        }



        private void CopyOverPreviewSettings(DataContainer.LODLevelSettings lodLevel)
        {

            lodLevel.reductionStrength = ReductionStrength;
            lodLevel.preserveUVFoldover = PreserveUVFoldover;
            lodLevel.preserveUVSeams = PreserveUVSeams;
            lodLevel.preserveBorders = PreserveBorders;
            lodLevel.useEdgeSort = UseEdgeSort;
            lodLevel.regardCurvature = RegardCurvature;
            lodLevel.recalculateNormals = RecalculateNormals;
            lodLevel.aggressiveness = Aggressiveness;
            lodLevel.maxIterations = MaxIterations;
            lodLevel.regardTolerance = IsPreservationActive;

            if (dataContainer.toleranceSpheres != null && dataContainer.toleranceSpheres.Count > 0)
            {
                for (int b = 0; b < lodLevel.sphereIntensities.Count; b++)
                {
                    lodLevel.sphereIntensities[b] = dataContainer.toleranceSpheres[b].preservationStrength;
                }
            }

        }



        private void ApplyExtraOptions(GameObject lodObject)
        {
            Transform applyTo = lodObject.transform.Find(LOD_PARENT_OBJECT_NAME);

            foreach (Transform child in applyTo.GetComponentsInChildren<Transform>(true))
            {
                
                if(CopyParentLayer)       { child.gameObject.layer = lodObject.layer; }
                if(CopyParentTag)         { child.gameObject.tag = lodObject.tag; }
                if(CopyParentStaticFlags) { GameObjectUtility.SetStaticEditorFlags(child.gameObject, GameObjectUtility.GetStaticEditorFlags(lodObject)); }
            }
        }


    }




}