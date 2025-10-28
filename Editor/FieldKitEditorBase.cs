using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FieldKit
{
    public abstract class FieldKitEditorBase<T> : Editor where T : FieldKitControl
    {
        protected T tool;

        private List<MonoBehaviour> _sceneComponents = new List<MonoBehaviour>();
        private string[] _componentOptions = Array.Empty<string>();
        private int _selectedComponentIndex = -1;

        private List<FieldKitReflection.MemberDescriptor> _memberOptions = new List<FieldKitReflection.MemberDescriptor>();
        private string[] _memberDisplayOptions = Array.Empty<string>();
        private int _selectedMemberIndex = -1;

        protected abstract bool AcceptType(Type t);
        protected virtual string HeaderTitle => typeof(T).Name;

        protected virtual void OnEnable()
        {
            tool = (T)target;
            RefreshComponents();
            SyncComponentIndex();
            RefreshMembers();
            SyncMemberIndex();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField(HeaderTitle, EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Pick a target script and then choose a variable of the correct type.", MessageType.Info);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Target Component", EditorStyles.miniBoldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                var newObj = (MonoBehaviour)EditorGUILayout.ObjectField(tool.targetComponent, typeof(MonoBehaviour), true);
                if (newObj != tool.targetComponent)
                {
                    Undo.RecordObject(tool, "Set Target Component");
                    tool.targetComponent = newObj;
                    SyncComponentIndex();
                    RefreshMembers();
                    SyncMemberIndex();
                }

                if (GUILayout.Button("Refresh", GUILayout.Width(70)))
                {
                    RefreshComponents();
                    SyncComponentIndex();
                    RefreshMembers();
                    SyncMemberIndex();
                }
            }

            int newIdx = EditorGUILayout.Popup("From Scene", _selectedComponentIndex, _componentOptions);
            if (newIdx != _selectedComponentIndex)
            {
                _selectedComponentIndex = newIdx;
                var picked = (_selectedComponentIndex >= 0 && _selectedComponentIndex < _sceneComponents.Count)
                    ? _sceneComponents[_selectedComponentIndex]
                    : null;
                if (picked != tool.targetComponent)
                {
                    Undo.RecordObject(tool, "Pick Target Component");
                    tool.targetComponent = picked;
                    RefreshMembers();
                    SyncMemberIndex();
                }
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Variable (filtered)", EditorStyles.miniBoldLabel);
            using (new EditorGUI.DisabledScope(tool.targetComponent == null))
            {
                int newMemberIdx = EditorGUILayout.Popup("Field / Property", _selectedMemberIndex, _memberDisplayOptions);
                if (newMemberIdx != _selectedMemberIndex)
                {
                    _selectedMemberIndex = newMemberIdx;
                    var md = (_selectedMemberIndex >= 0 && _selectedMemberIndex < _memberOptions.Count)
                        ? _memberOptions[_selectedMemberIndex]
                        : default;
                    Undo.RecordObject(tool, "Pick Member");
                    tool.memberName = md.Name;
                    tool.memberKind = md.Kind;
                    tool.memberTypeAQN = md.Type != null ? md.Type.AssemblyQualifiedName : null;
                    EditorUtility.SetDirty(tool);
                    UpdateLabelAndValueInEditor();
                }
            }

            // Show current value preview in editor
            if (tool.HasValidSelection())
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Preview", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField("Auto Label", GetPreviewLabel());
                EditorGUILayout.LabelField("Current Value", GetPreviewValue());
            }

            DrawDerivedExtras();
            EditorGUILayout.Space(8);
            DrawRemainingProperties();
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawDerivedExtras() { }

        private void UpdateLabelAndValueInEditor()
        {
            var labelTextProp = serializedObject.FindProperty("labelText");
            var valueTextProp = serializedObject.FindProperty("valueText");
            if (labelTextProp != null && labelTextProp.objectReferenceValue != null)
            {
                var labelText = labelTextProp.objectReferenceValue as TMPro.TMP_Text;
                if (labelText != null)
                {
                    Undo.RecordObject(labelText, "Update Label");
                    labelText.text = GetPreviewLabel();
                    EditorUtility.SetDirty(labelText);
                }
            }
            if (valueTextProp != null && valueTextProp.objectReferenceValue != null)
            {
                var valueText = valueTextProp.objectReferenceValue as TMPro.TMP_Text;
                if (valueText != null)
                {
                    Undo.RecordObject(valueText, "Update Value");
                    valueText.text = GetPreviewValue();
                    EditorUtility.SetDirty(valueText);
                }
            }
            if (tool.targetComponent != null)
            {
                EditorUtility.SetDirty(tool);
            }
        }

        private string GetPreviewLabel()
        {
            if (!tool.HasValidSelection()) return "(none)";
            return SplitCamelCase(tool.memberName);
        }

        private string GetPreviewValue()
        {
            if (!tool.HasValidSelection()) return "(none)";
            try
            {
                var val = tool.GetValue();
                return val != null ? val.ToString() : "(null)";
            }
            catch (Exception ex)
            {
                return $"(error: {ex.Message})";
            }
        }

        private static string SplitCamelCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var result = new System.Text.StringBuilder();
            result.Append(char.ToUpper(input[0]));
            for (int i = 1; i < input.Length; i++)
            {
                if (char.IsUpper(input[i]) && i > 0)
                    result.Append(' ');
                result.Append(input[i]);
            }
            return result.ToString();
        }

        protected virtual void DrawRemainingProperties()
        {
            string[] exclude = new[]
            {
                "m_Script",
                nameof(FieldKitControl.targetComponent),
                nameof(FieldKitControl.memberName),
                nameof(FieldKitControl.memberKind),
                nameof(FieldKitControl.memberTypeAQN)
            };
            DrawPropertiesExcluding(serializedObject, exclude);
        }

        private void RefreshComponents()
        {
            _sceneComponents = FindObjectsOfType<MonoBehaviour>(true)
                .Where(mb => mb != null && mb.gameObject.scene.IsValid())
                .OrderBy(mb => mb.gameObject.name)
                .ThenBy(mb => mb.GetType().Name)
                .ToList();
            _componentOptions = _sceneComponents.Select(FormatComponentOption).ToArray();
        }

        private void RefreshMembers()
        {
            _memberOptions.Clear();
            if (tool.targetComponent != null)
            {
                var type = tool.targetComponent.GetType();
                _memberOptions = FieldKitReflection.GetEligibleMembers(type, supportedTypesOnly: false)
                    .Where(m => (m.CanRead || m.CanWrite) && AcceptType(m.Type))
                    .OrderBy(m => m.Name)
                    .ToList();
            }
            _memberDisplayOptions = _memberOptions
                .Select(m => $"{m.Name} : {m.Type?.Name} [{m.Kind}]")
                .ToArray();
        }

        private void SyncComponentIndex()
        {
            _selectedComponentIndex = tool.targetComponent ? _sceneComponents.IndexOf(tool.targetComponent) : -1;
        }

        private void SyncMemberIndex()
        {
            if (tool.targetComponent == null || string.IsNullOrEmpty(tool.memberName))
            {
                _selectedMemberIndex = -1;
                return;
            }
            _selectedMemberIndex = _memberOptions.FindIndex(m => m.Name == tool.memberName && m.Kind == tool.memberKind);
        }

        private static string FormatComponentOption(MonoBehaviour mb)
        {
            if (mb == null) return "<null>";
            var go = mb.gameObject;
            string path = go.name;
            Transform t = go.transform;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return $"{path} ({mb.GetType().Name})";
        }
    }
}
