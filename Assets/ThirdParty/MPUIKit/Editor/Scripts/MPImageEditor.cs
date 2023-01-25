﻿using System;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MPUIKIT.Editor {
    [CustomEditor(typeof(MPImage), true)]
    [CanEditMultipleObjects]
    public class MPImageEditor : ImageEditor {
        private SerializedProperty spSprite;
        private SerializedProperty spCircle, spTriangle, spRectangle, spPentagon, spHexagon, spNStarPolygon;
        private SerializedProperty spPreserveAspect;
        private SerializedProperty spFillMethod, spFillOrigin, spFillAmount, spFillClockwise;
        private SerializedProperty spShape;
        private SerializedProperty spStrokeWidth, spOutlineWidth, spOutlineColor, spFalloffDistance;
        private SerializedProperty spConstrainRotation, spShapeRotation, spFlipHorizontal, spFlipVertical;
        private SerializedProperty spMaterialSettings, spMaterial, spImageType;

        private SerializedProperty spGradient;

        private bool gsInitialized, shaderChannelsNeedUpdate;

        protected override void OnEnable() {
            foreach (Object obj in serializedObject.targetObjects) {
                ((MPImage) obj).UpdateSerializedValuesFromSharedMaterial();
            }

            base.OnEnable();

            spSprite = serializedObject.FindProperty("m_Sprite");

            spShape = serializedObject.FindProperty("m_DrawShape");

            spStrokeWidth = serializedObject.FindProperty("m_StrokeWidth");
            spOutlineWidth = serializedObject.FindProperty("m_OutlineWidth");
            spOutlineColor = serializedObject.FindProperty("m_OutlineColor");
            spFalloffDistance = serializedObject.FindProperty("m_FalloffDistance");

            spMaterialSettings = serializedObject.FindProperty("m_MaterialMode");
            spMaterial = serializedObject.FindProperty("m_Material");
            spImageType = serializedObject.FindProperty("m_ImageType");

            spFillMethod = serializedObject.FindProperty("m_FillMethod");
            spFillOrigin = serializedObject.FindProperty("m_FillOrigin");
            spFillAmount = serializedObject.FindProperty("m_FillAmount");
            spFillClockwise = serializedObject.FindProperty("m_FillClockwise");

            spConstrainRotation = serializedObject.FindProperty("m_ConstrainRotation");
            spShapeRotation = serializedObject.FindProperty("m_ShapeRotation");
            spFlipHorizontal = serializedObject.FindProperty("m_FlipHorizontal");
            spFlipVertical = serializedObject.FindProperty("m_FlipVertical");


            spCircle = serializedObject.FindProperty("m_Circle");
            spRectangle = serializedObject.FindProperty("m_Rectangle");
            spTriangle = serializedObject.FindProperty("m_Triangle");
            spPentagon = serializedObject.FindProperty("m_Pentagon");
            spHexagon = serializedObject.FindProperty("m_Hexagon");
            spNStarPolygon = serializedObject.FindProperty("m_NStarPolygon");

            spPreserveAspect = serializedObject.FindProperty("m_PreserveAspect");

            spGradient = serializedObject.FindProperty("m_GradientEffect");
        }



        public override void OnInspectorGUI() {
            serializedObject.Update();

            FixShaderChannelGUI();

            RaycastControlsGUI();
            EditorGUILayout.PropertyField(m_Color);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(spShape);

            if (spShape.enumValueIndex != (int) DrawShape.None) {
                EditorGUILayout.BeginVertical("Box");
                if (!spShape.hasMultipleDifferentValues) {
                    switch ((DrawShape) spShape.enumValueIndex) {
                        case DrawShape.Circle:
                            EditorGUILayout.PropertyField(spCircle);
                            break;
                        case DrawShape.Rectangle:
                            EditorGUILayout.PropertyField(spRectangle);
                            break;
                        case DrawShape.Pentagon:
                            EditorGUILayout.PropertyField(spPentagon);
                            break;
                        case DrawShape.Triangle:
                            EditorGUILayout.PropertyField(spTriangle);
                            break;
                        case DrawShape.Hexagon:
                            EditorGUILayout.PropertyField(spHexagon);
                            break;
                        case DrawShape.NStarPolygon:
                            EditorGUILayout.PropertyField(spNStarPolygon);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                EditorGUILayout.Space();

                if (spShape.enumValueIndex != (int) DrawShape.None) {
                    AdditionalShapeDataGUI();
                }
                EditorGUILayout.EndVertical();
            }
            
            

            EditorGUILayout.Space();
            ImageTypeGUI();

            SpriteGUI();

            if (!spSprite.hasMultipleDifferentValues && spSprite.objectReferenceValue != null) {
                EditorGUILayout.PropertyField(spPreserveAspect);
            }
            
            SetShowNativeSize(spSprite.objectReferenceValue != null, true);
            NativeSizeButtonGUI();
            
            EditorGUILayout.Space();
            SharedMaterialGUI();

            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("Box");
            {
                EditorGUILayout.PropertyField(spGradient);
            }
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            Repaint();
        }
        
        private void AdditionalShapeDataGUI() {
            EditorGUILayout.Space();

            float strokeWidth = spStrokeWidth.floatValue;
            float outlineWidth = spOutlineWidth.floatValue;
            float falloff = spFalloffDistance.floatValue;
            Color outlineColor = spOutlineColor.colorValue;
            
            Rect r = EditorGUILayout.GetControlRect(true,
                EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing);
            Rect line = r;
            line.height = EditorGUIUtility.singleLineHeight;
            float x = (line.width - 10f) / 2;

            float fieldWidth = x / 2 - 10f;
            float labelWidth = x - fieldWidth;

            line.width = labelWidth;
            EditorGUI.LabelField(line, "Stroke");
            Rect dragZone = line;
            line.x += labelWidth;
            line.width = fieldWidth;
            EditorGUI.BeginChangeCheck();
            {
                EditorGUI.showMixedValue = spStrokeWidth.hasMultipleDifferentValues;
                strokeWidth =
                    EditorGUILayoutExtended.FloatFieldExtended(line, spStrokeWidth.floatValue, dragZone);
                EditorGUI.showMixedValue = false;
            }
            if (EditorGUI.EndChangeCheck())
            {
                spStrokeWidth.floatValue = strokeWidth;
            }
            line.x += fieldWidth + 10;
            line.width = labelWidth;
            EditorGUI.LabelField(line, "Falloff");
            dragZone = line;
            line.x += labelWidth;
            line.width = fieldWidth;

            EditorGUI.BeginChangeCheck();
            {
                EditorGUI.showMixedValue = spFalloffDistance.hasMultipleDifferentValues;
                falloff =
                    EditorGUILayoutExtended.FloatFieldExtended(line, spFalloffDistance.floatValue, dragZone);
                EditorGUI.showMixedValue = false;
            }
            if (EditorGUI.EndChangeCheck())
            {
                spFalloffDistance.floatValue = falloff;
            }
            line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            line.x = r.x;
            line.width = labelWidth;
            EditorGUI.LabelField(line, "Outline Width");
            dragZone = line;
            line.x += labelWidth;
            line.width = fieldWidth;
            EditorGUI.BeginChangeCheck();
            {
                EditorGUI.showMixedValue = spOutlineWidth.hasMultipleDifferentValues;
                outlineWidth =
                    EditorGUILayoutExtended.FloatFieldExtended(line, spOutlineWidth.floatValue, dragZone);
                EditorGUI.showMixedValue = false;
            }
            if (EditorGUI.EndChangeCheck())
            {
                spOutlineWidth.floatValue = outlineWidth;
            }
            line.x += fieldWidth + 10;
            line.width = labelWidth;
            EditorGUI.LabelField(line, "Outline Color");
            dragZone = line;
            line.width = fieldWidth;
            line.x += labelWidth;
            EditorGUI.BeginChangeCheck();
            {
                EditorGUI.showMixedValue = spOutlineColor.hasMultipleDifferentValues;
                outlineColor = EditorGUI.ColorField(line, spOutlineColor.colorValue);
                EditorGUI.showMixedValue = false;
            }
            if (EditorGUI.EndChangeCheck())
            {
                spOutlineColor.colorValue = outlineColor;
            }
                
            EditorGUILayout.Space();
            
            RotationGUI();
        }

        private void RotationGUI() {
            Rect r =EditorGUILayout.GetControlRect(true,
                EditorGUIUtility.singleLineHeight + 24 + EditorGUIUtility.standardVerticalSpacing);
            Rect line = r;
            line.height = EditorGUIUtility.singleLineHeight;
            float x = (line.width - 10f) / 2;

            float fieldWidth = x / 2 - 10f;
            float labelWidth = x - fieldWidth;

            line.width = labelWidth;
            EditorGUI.LabelField(line, "Rotation");
            line.x += labelWidth;
            line.width = r.width - labelWidth - 78;
            
            string[] options = spConstrainRotation.hasMultipleDifferentValues? new []{ "---", "---" } : new []{"Free", "Constrained"};
            bool boolVal = spConstrainRotation.boolValue;
            EditorGUI.BeginChangeCheck();
            {
                boolVal = GUI.Toolbar(line, boolVal ? 1 : 0, options) == 1;
            }
            if (EditorGUI.EndChangeCheck()) {
                spConstrainRotation.boolValue = boolVal;
                GUI.FocusControl(null);
            }
            
            line.x += line.width + 14;
            line.width = 64;
            EditorGUI.LabelField(line, "Flip");

            line.y += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
            line.x = r.x + 10;
            line.height = EditorGUIUtility.singleLineHeight;
            line.width = labelWidth - 10;
            EditorGUI.BeginDisabledGroup(spConstrainRotation.boolValue);
            {
                Rect dragZone = line;
                EditorGUI.LabelField(line, "Angle");
                line.x = r.x + labelWidth;
                line.width = r.width - labelWidth - 148;

                float rotationValue = spShapeRotation.floatValue;
                EditorGUI.BeginChangeCheck();
                {
                    EditorGUI.showMixedValue = spShapeRotation.hasMultipleDifferentValues;
                    rotationValue = EditorGUILayoutExtended.FloatFieldExtended(line, spShapeRotation.floatValue, dragZone);
                    EditorGUI.showMixedValue = false;
                }
                if (EditorGUI.EndChangeCheck()) {
                    spShapeRotation.floatValue = rotationValue;
                }
            }
            EditorGUI.EndDisabledGroup();
            
            EditorGUI.BeginDisabledGroup(!spConstrainRotation.boolValue);
            {
                line.x += line.width + 4;
                line.width = 30;
                line.height = 24;
                if (GUI.Button(line, MPEditorContents.RotateLeftNormal)) {
                    float rotation = spShapeRotation.floatValue;
                    float remainder = rotation % 90;
                    if (Mathf.Abs(remainder) <= 0) {
                        rotation += 90;
                    }
                    else {
                        rotation = rotation - remainder + 90;
                    }
                    if (Math.Abs(rotation) >= 360) rotation = 0;
                    spShapeRotation.floatValue = rotation;
                }

                line.x += 34;
                if (GUI.Button(line, MPEditorContents.RotateRightNormal)) {
                    float rotation = spShapeRotation.floatValue;
                    float remainder = rotation % 90;
                    if (Mathf.Abs(remainder) <= 0) {
                        rotation -= 90;
                    }
                    else {
                        rotation -= remainder;
                    }
                    
                    if (Math.Abs(rotation) >= 360) rotation = 0;
                    spShapeRotation.floatValue = rotation;
                }
            }
            EditorGUI.EndDisabledGroup();

            line.x += 46;
            bool flipH = spFlipHorizontal.boolValue;
            bool flipV = spFlipVertical.boolValue;
            EditorGUI.BeginChangeCheck();
            {
                EditorGUI.BeginDisabledGroup(spFlipHorizontal.hasMultipleDifferentValues || spFlipVertical.hasMultipleDifferentValues);
                flipH = GUI.Toggle(line, spFlipHorizontal.boolValue, spFlipHorizontal.boolValue ? MPEditorContents.FlipHorizontalActive:MPEditorContents.FlipHorizontalNormal, "button");
                line.x += 34;
                flipV = GUI.Toggle(line, spFlipVertical.boolValue, spFlipVertical.boolValue?MPEditorContents.FlipVerticalActive:MPEditorContents.FlipVerticalNormal, "button");
                EditorGUI.EndDisabledGroup();
            }
            if (EditorGUI.EndChangeCheck()) {
                spFlipHorizontal.boolValue = flipH;
                spFlipVertical.boolValue = flipV;
            }
            
        }

        private void FixShaderChannelGUI() {
            if (!shaderChannelsNeedUpdate) return;
            EditorGUILayout.HelpBox(
                "Parent Canvas needs to have these additional shader channels : Texcoord1, Texcoord2",
                MessageType.Error);
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Fix", GUILayout.Width(100))) {
                    Canvas canvas = (target as MPImage)?.GetComponentInParent<Canvas>();
                    if (canvas != null) {
                        MPEditorUtility.AddAdditionalShaderChannelsToCanvas(canvas);
                        shaderChannelsNeedUpdate = false;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private new void SpriteGUI() {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(spSprite, new GUIContent("Sprite"));
            if (EditorGUI.EndChangeCheck()) {
                Sprite newSprite = spSprite.objectReferenceValue as Sprite;
                if (newSprite) {
                    Image.Type oldType = (Image.Type) spImageType.enumValueIndex;
                    if (newSprite.border.SqrMagnitude() > 0) {
                        spImageType.enumValueIndex = (int) Image.Type.Sliced;
                    }
                    else if (oldType == Image.Type.Sliced) {
                        spImageType.enumValueIndex = (int) Image.Type.Simple;
                    }
                }

                (serializedObject.targetObject as Image)?.DisableSpriteOptimizations();
            }
        }

        private void ImageTypeGUI() {
            int selectedIndex = spImageType.enumValueIndex == (int) Image.Type.Simple ? 0 : 1;
            Rect imageTypeRect = EditorGUILayout.GetControlRect();
            EditorGUI.BeginChangeCheck();
            {
                EditorGUI.LabelField(
                    new Rect(imageTypeRect.x, imageTypeRect.y, EditorGUIUtility.labelWidth, imageTypeRect.height),
                    "Type");
                imageTypeRect.x += EditorGUIUtility.labelWidth + 2;
                imageTypeRect.width -= EditorGUIUtility.labelWidth + 2;
                selectedIndex = EditorGUI.Popup(imageTypeRect, selectedIndex, new[] {"Simple", "Filled"});
            }
            if (EditorGUI.EndChangeCheck()) {
                spImageType.enumValueIndex = (int) (selectedIndex == 0 ? Image.Type.Simple : Image.Type.Filled);
            }

            if (!spImageType.hasMultipleDifferentValues && spImageType.enumValueIndex == (int) Image.Type.Filled) {
                ++EditorGUI.indentLevel;
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(spFillMethod);
                if (EditorGUI.EndChangeCheck()) {
                    spFillOrigin.intValue = 0;
                }

                switch ((Image.FillMethod) spFillMethod.enumValueIndex) {
                    case Image.FillMethod.Horizontal:
                        spFillOrigin.intValue = (int) (Image.OriginHorizontal) EditorGUILayout.EnumPopup("Fill Origin",
                            (Image.OriginHorizontal) spFillOrigin.intValue);
                        break;
                    case Image.FillMethod.Vertical:
                        spFillOrigin.intValue = (int) (Image.OriginVertical) EditorGUILayout.EnumPopup("Fill Origin",
                            (Image.OriginVertical) spFillOrigin.intValue);
                        break;
                    case Image.FillMethod.Radial90:
                        spFillOrigin.intValue =
                            (int) (Image.Origin90) EditorGUILayout.EnumPopup("Fill Origin",
                                (Image.Origin90) spFillOrigin.intValue);
                        break;
                    case Image.FillMethod.Radial180:
                        spFillOrigin.intValue =
                            (int) (Image.Origin180) EditorGUILayout.EnumPopup("Fill Origin",
                                (Image.Origin180) spFillOrigin.intValue);
                        break;
                    case Image.FillMethod.Radial360:
                        spFillOrigin.intValue =
                            (int) (Image.Origin360) EditorGUILayout.EnumPopup("Fill Origin",
                                (Image.Origin360) spFillOrigin.intValue);
                        break;
                }

                EditorGUILayout.PropertyField(spFillAmount);
                if ((Image.FillMethod) spFillMethod.enumValueIndex > Image.FillMethod.Vertical) {
                    EditorGUILayout.PropertyField(spFillClockwise, new GUIContent("Clockwise"));
                }

                --EditorGUI.indentLevel;
            }
        }

        private void SharedMaterialGUI() {
            Rect rect = EditorGUILayout.GetControlRect(true,
                EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            int matSett = spMaterialSettings.enumValueIndex;
            Rect labelRect = rect;
            labelRect.width = EditorGUIUtility.labelWidth;
            EditorGUI.LabelField(labelRect, "Material Mode");
            rect.x += labelRect.width;
            rect.width -= labelRect.width;

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = spMaterialSettings.hasMultipleDifferentValues;
            string[] options = new[] {"Dynamic", "Shared"};
            if (EditorGUI.showMixedValue) options = new[] {"---", "---"};
            matSett = GUI.Toolbar(rect, matSett, options);

            if (EditorGUI.EndChangeCheck()) {
                spMaterialSettings.enumValueIndex = matSett;
                foreach (Object obj in targets) {
                    ((MPImage) obj).MaterialMode = (MaterialMode) matSett;
                    EditorUtility.SetDirty(obj);
                }
            }

            EditorGUI.showMixedValue = false;


            EditorGUI.BeginDisabledGroup(spMaterialSettings.enumValueIndex == (int) MaterialMode.Dynamic);
            {
                rect = EditorGUILayout.GetControlRect(true,
                    EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

                Object matObj = spMaterial.objectReferenceValue;

                EditorGUI.BeginChangeCheck();
                {
                    EditorGUI.showMixedValue = spMaterialSettings.hasMultipleDifferentValues;
                    matObj = (Material) EditorGUI.ObjectField(
                        new Rect(rect.x, rect.y, rect.width - 60, EditorGUIUtility.singleLineHeight),
                        matObj, typeof(Material), false);
                    EditorGUI.showMixedValue = false;
                }
                if (EditorGUI.EndChangeCheck()) {
                    spMaterial.objectReferenceValue = matObj;
                    foreach (Object obj in targets) {
                        ((MPImage) obj).material = (Material) matObj;
                        EditorUtility.SetDirty(obj);
                    }
                }

                EditorGUI.BeginDisabledGroup(spMaterial.objectReferenceValue != null);
                {
                    if (GUI.Button(new Rect(rect.x + rect.width - 55, rect.y, 55, EditorGUIUtility.singleLineHeight),
                        "Create")) {
                        Material mat = ((MPImage) target).CreateMaterialAssetFromComponentSettings();
                        spMaterial.objectReferenceValue = mat;
                        foreach (Object obj in targets) {
                            ((MPImage) obj).material = mat;
                            EditorUtility.SetDirty(obj);
                        }
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
