/// ***
/// InspectorPlus v0.1, programmed by https://tzdevil.github.io
/// If you'd like to contact me: champtzcsgo@gmail.com
/// ***

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace tzdevil.InspectorPlus
{
    public class InspectorPlusOverrider : EditorWindow
    {
        #region Currently selected game object
        private static GameObject _selectedObject;
        private string _selectedBaseType;
        private static string _selectedObjectName;
        #endregion

        #region Component's Data
        private static List<ComponentData> _componentData;
        #endregion

        #region Is there anything to show in Inspector+?
        private bool _showComponents;
        #endregion

        #region Current scroll position
        private Vector2 _scrollPos = Vector2.zero;
        #endregion

        #region Tab types
        private string[] _uniqueTabTypes;
        private int _uniqueTabTypesCount;
        #endregion

        #region Create the Inspector+ window
        [MenuItem("Window/General/Inspector+", priority = 0)]
        public static void CreateInspectorOverriderWindow()
        {
            GetWindow(typeof(InspectorPlusOverrider), false, "Inspector+")
                            .position = new Rect(400, 120, 360, 240);
        }
        #endregion

        #region Updating Inspector
        private void OnInspectorUpdate()
        {
            if (Selection.activeGameObject != _selectedObject)
            {
                _selectedBaseType = "All";
                _selectedObject = Selection.activeGameObject;
                _selectedObjectName = _selectedObject != null ? _selectedObject.name : "";
                Repaint();
            }
        }
        #endregion

        #region Everything GUI drawing related
        private void OnGUI()
        {
            CheckDraggingScripts();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, false, false);

            if (_selectedObject != null)
            {
                _componentData = new();
                foreach (var component in _selectedObject.GetComponents(typeof(Component)))
                    _componentData.Add(new(component));

                _uniqueTabTypes = ReturnUniqueBaseTypes();
                _uniqueTabTypesCount = _uniqueTabTypes.Length;

                GUISetObjectName();

                _showComponents = true;
            }
            else
                _showComponents = false;

            if (_showComponents)
            {
                ShowButtons();
                ShowCurrentlySelectedObjectsComponents(_selectedBaseType);
            }

            EditorGUILayout.EndScrollView();
        }

        private void CheckDraggingScripts()
        {
            Event currentEvent = Event.current;

            if (currentEvent.type == EventType.DragUpdated || currentEvent.type == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (currentEvent.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is MonoScript script)
                        {
                            _selectedObject.AddComponent(script.GetClass());
                        }
                    }
                }

                Event.current.Use();
            }
        }

        private static void GUISetObjectName()
        {
            GUI.SetNextControlName("SelectedObjectName");
            _selectedObjectName = EditorGUILayout.TextField("Name", _selectedObjectName);
            if (GUI.GetNameOfFocusedControl() != "SelectedObjectName")
            {
                _selectedObject.name = _selectedObjectName;
            }
            else if (Event.current.keyCode == KeyCode.Return || Event.current.isMouse)
                EditorGUI.FocusTextInControl(null);
        }

        private string[] ReturnUniqueBaseTypes()
        {
            HashSet<string> baseTypes = _componentData.Select(c => c.Types[0]).ToHashSet();

            return baseTypes.ToArray();
        }

        private static void ShowCurrentlySelectedObjectsComponents(string currentlySelectedBaseType)
        {
            GUIStyle style = new(EditorStyles.foldoutHeader)
            {
                fixedHeight = 24,
                fixedWidth = EditorGUIUtility.currentViewWidth,
            };

            var componentData = currentlySelectedBaseType == "All" ? _componentData : _componentData.Where(c => c.Types[0] == currentlySelectedBaseType);
            foreach (var component in componentData)
            {
                Color defaultColor = GUI.color;
                EditorGUILayout.BeginHorizontal();

                if (component.Types[0] != "Transform")
                {
                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("X", GUILayout.Width(24)))
                    {
                        DestroyImmediate(component.Component);
                        EditorGUILayout.EndHorizontal();
                        return;
                    }
                }

                GUI.backgroundColor = new Color(.9f, .9f, .9f, 1f);
                component.Foldout = EditorGUILayout.Foldout(component.Foldout, component.Component.GetType().Name, false, style);
                GUI.backgroundColor = defaultColor;

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(4);

                if (component.Foldout)
                    Editor.CreateEditor(component.Component).OnInspectorGUI();

                GUILayout.Space(8);
            }
        }

        private void ShowButtons()
        {
            EditorGUILayout.BeginHorizontal();

            var defaultColor = GUI.backgroundColor;

            if (_selectedBaseType == "All")
                GUI.backgroundColor = Color.cyan;

            if (GUILayout.Button("All"))
            {
                ChangeCurrentlySelectedBaseType("All");
            }

            GUI.backgroundColor = defaultColor;

            for (int i = 0; i < _uniqueTabTypesCount; i++)
            {
                var typeName = _uniqueTabTypes[i];

                if (_selectedBaseType == typeName)
                    GUI.backgroundColor = Color.cyan;

                if (GUILayout.Button(typeName))
                    ChangeCurrentlySelectedBaseType(typeName);

                GUI.backgroundColor = defaultColor;
            }

            EditorGUILayout.EndHorizontal();

            void ChangeCurrentlySelectedBaseType(string typeName)
            {
                _selectedBaseType = typeName;
                Repaint();
            }
        }
        #endregion

        #region GUI Focus Related
        private void OnLostFocus()
        {
            if (_selectedObject != null)
                _selectedObject.name = _selectedObjectName;

            _showComponents = false;
        }
        #endregion
    }
}