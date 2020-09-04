/**
 
The MIT License (MIT)
Copyright (c) 2015, Max McGuire
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit
persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
**/

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

public class ReorderComponents : EditorWindow
{

    private ReorderableList m_list;
    private GameObject m_targetObject;
    private Component[] m_components;
    private GUIContent[] m_contents;

    [MenuItem("Tools/Reorder Components")]
    private static void ReorderComponentsMenu()
    {
        ReorderComponents window = EditorWindow.GetWindow<ReorderComponents>();
    }

    ReorderComponents()
    {
        titleContent = new GUIContent("Components");
        Undo.undoRedoPerformed += OnUndo;
    }

    void OnDestroy()
    {
        Undo.undoRedoPerformed -= OnUndo;
    }

    static string Wordify(string pascalCaseString)
    {
        Regex r = new Regex("(?<=[a-z])(?<x>[A-Z])|(?<=.)(?<x>[A-Z])(?=[a-z])");
        return r.Replace(pascalCaseString, " ${x}");
    }

    GUIContent GetContentForComponent(Component component)
    {
        string text = Wordify(component.GetType().Name);
        if (component as MonoBehaviour != null)
        {
            text += " (Script)";
        }
        return new GUIContent(text, EditorGUIUtility.ObjectContent(null, component.GetType()).image);
    }

    void RebuildList(GameObject targetObject)
    {
        m_list = null;
        m_components = null;
        m_contents = null;
        m_targetObject = targetObject;

        if (targetObject != null)
        {
            m_components = GetMoveableComponents(targetObject);
            m_list = new ReorderableList(m_components, typeof(Component), true, false, false, false);
            m_list.onReorderCallback += OnListReorder;
            m_list.drawHeaderCallback += OnListDrawHeader;
            m_list.drawElementCallback += OnListDrawElement;

            m_contents = new GUIContent[m_components.Length];
            for (int i = 0; i < m_contents.Length; ++i)
            {
                m_contents[i] = GetContentForComponent(m_components[i]);
            }
        }

        Repaint();
    }

    void OnListDrawHeader(Rect rect)
    {
        EditorGUI.LabelField(rect, m_targetObject.name);
    }

    void OnListDrawElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        EditorGUI.LabelField(rect, m_contents[index]);
    }

    void OnListReorder(ReorderableList list)
    {
        ApplyOrderChange();
    }

    void OnSelectionChange()
    {
        RebuildList(Selection.activeGameObject);
    }

    void OnUndo()
    {
        RebuildList(Selection.activeGameObject);
    }

    void MoveComponent(Component component, int oldIndex, int newIndex)
    {
        while (oldIndex < newIndex)
        {
            UnityEditorInternal.ComponentUtility.MoveComponentDown(component);
            ++oldIndex;
        }
        while (oldIndex > newIndex)
        {
            UnityEditorInternal.ComponentUtility.MoveComponentUp(component);
            --oldIndex;
        }
    }

    static Component[] GetMoveableComponents(GameObject targetObject)
    {
        List<Component> components = new List<Component>(targetObject.GetComponents<Component>());

        // Remove non-movable components.
        for (int componentIndex = 0; componentIndex < components.Count; ++componentIndex)
        {
            Component component = components[componentIndex];
            if (component.GetType() == typeof(Transform) ||
                component.GetType() == typeof(RectTransform))
            {
                components.RemoveAt(componentIndex);
                --componentIndex;
            }
        }

        return components.ToArray();
    }

    void ApplyOrderChange()
    {

        Component[] newComponents = m_components;

        Undo.RegisterCompleteObjectUndo(m_targetObject, "Reorder components");

        bool done = false;
        while (!done)
        {
            done = true;

            Component[] oldComponents = GetMoveableComponents(m_targetObject);

            for (int oldIndex = 0; oldIndex < oldComponents.Length; ++oldIndex)
            {
                Component component = oldComponents[oldIndex];
                int newIndex = Array.IndexOf(newComponents, component);
                if (oldIndex != newIndex)
                {
                    MoveComponent(component, oldIndex, newIndex);
                    done = false;
                    break;
                }
            }

        }

        RebuildList(m_targetObject);

    }

    private void OnGUI()
    {
        GameObject targetObject = Selection.activeGameObject;
        if (m_targetObject != targetObject)
        {
            RebuildList(targetObject);
        }

        if (m_list != null)
        {
            m_list.DoLayoutList();
        }
        else
        {
            GUILayout.Box("Select an object to reorder its components", GUILayout.ExpandWidth(true));
        }
    }

}