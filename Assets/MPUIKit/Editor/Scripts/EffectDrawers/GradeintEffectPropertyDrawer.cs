using UnityEditor;
using UnityEngine;

namespace MPUIKIT.Editor {
    [CustomPropertyDrawer(typeof(GradientEffect))]
    public class GradeintEffectPropertyDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);
            {
                SerializedProperty Enabled = property.FindPropertyRelative("m_Enabled");
                bool enabled = Enabled.boolValue;
                SerializedProperty gradientType = property.FindPropertyRelative("m_GradientType");
                GradientType gradType = (GradientType) gradientType.enumValueIndex;
                SerializedProperty gradient = property.FindPropertyRelative("m_Gradient");
                SerializedProperty rotation = property.FindPropertyRelative("m_Rotation");
                SerializedProperty cornerColors = property.FindPropertyRelative("m_CornerGradientColors");

                Rect line = position;
                line.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.BeginChangeCheck();
                {
                    EditorGUI.showMixedValue = Enabled.hasMultipleDifferentValues;
                    enabled = EditorGUI.Toggle(line, "Gradient", enabled);
                    EditorGUI.showMixedValue = false;

                    if (enabled) {
                        line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        
                        EditorGUI.showMixedValue = gradientType.hasMultipleDifferentValues;
                        gradType = (GradientType) EditorGUI.EnumPopup(line, "Type", gradType);
                        EditorGUI.showMixedValue = false;
                    }
                }
                if (EditorGUI.EndChangeCheck()) {
                    Enabled.boolValue = enabled;
                    gradientType.enumValueIndex = (int) gradType;
                }

                if (enabled) {
                    if (gradType == GradientType.Corner) {

                        if (cornerColors.arraySize != 4)
                            cornerColors.arraySize = 4;
                        
                        line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        float colFieldWidth = line.width / 2f - 5f;
                        line.width = colFieldWidth;
                        EditorGUI.PropertyField(line, cornerColors.GetArrayElementAtIndex(0),  GUIContent.none);
                        line.x += colFieldWidth + 10;
                        EditorGUI.PropertyField(line, cornerColors.GetArrayElementAtIndex(1), GUIContent.none);
                        line.x -= colFieldWidth + 10;
                        line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        EditorGUI.PropertyField(line, cornerColors.GetArrayElementAtIndex(2),  GUIContent.none);
                        line.x += colFieldWidth + 10;
                        EditorGUI.PropertyField(line, cornerColors.GetArrayElementAtIndex(3),  GUIContent.none);
                        line.x -= colFieldWidth + 10;
                        line.width = colFieldWidth * 2 + 10;
                    }
                    else {
                        line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        EditorGUI.showMixedValue = gradient.hasMultipleDifferentValues;
                        EditorGUI.PropertyField(line, gradient, false);

                        if (gradType == GradientType.Linear) {
                            line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                            EditorGUI.showMixedValue = rotation.hasMultipleDifferentValues;
                            EditorGUI.PropertyField(line, rotation, new GUIContent("Rotation"));
                        }

                        EditorGUI.showMixedValue = false;
                    }
                }
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            SerializedProperty enabled = property.FindPropertyRelative("m_Enabled");
            if (enabled.boolValue) {
                SerializedProperty gradientMode = property.FindPropertyRelative("m_GradientType");
                if (gradientMode.enumValueIndex == (int) GradientType.Radial) {
                    return EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing * 2;
                }
                return EditorGUIUtility.singleLineHeight * 4 + EditorGUIUtility.standardVerticalSpacing * 3;
            }
            return EditorGUIUtility.singleLineHeight;
        }
    }
}