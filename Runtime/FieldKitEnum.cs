using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace FieldKit
{
    public class FieldKitEnum : FieldKitControl
    {
        [Header("UI References")]
        public TMP_Text labelText;
        public TMP_Dropdown dropdown; // editable
        public TMP_Text valueText;    // optional read-only display

        [Header("Options")]
        public bool readOnly;
        public string labelOverride;
        public float pollInterval = 0.1f;

        private float _timer;
        private string[] _names = Array.Empty<string>();

        private void OnEnable()
        {
            var mt = GetMemberType();
            if (mt == null || !mt.IsEnum)
            {
                Debug.LogWarning($"{nameof(FieldKitEnum)} on {name}: Selected member type '{mt}' is not an enum. Disabling.");
                enabled = false;
                return;
            }

            if (labelText) labelText.text = GetAutoLabel();

            if (dropdown)
            {
                dropdown.onValueChanged.RemoveAllListeners();
                _names = Enum.GetNames(mt);
                dropdown.ClearOptions();
                dropdown.AddOptions(new List<string>(_names));
                dropdown.onValueChanged.AddListener(idx =>
                {
                    if (!readOnly && idx >= 0 && idx < _names.Length)
                    {
                        var ev = Enum.Parse(mt, _names[idx]);
                        SetValue(ev);
                        if (valueText) valueText.text = _names[idx];
                    }
                });
                dropdown.interactable = !readOnly;
            }

            _timer = 0f;
            RefreshUI(force:true);
        }

        private void OnDisable()
        {
            if (dropdown) dropdown.onValueChanged.RemoveAllListeners();
        }

        private void Update()
        {
            _timer += Time.unscaledDeltaTime;
            if (_timer >= pollInterval)
            {
                _timer = 0f;
                RefreshUI();
            }
        }

        private void RefreshUI(bool force = false)
        {
            var mt = GetMemberType();
            var valObj = GetValue();
            if (mt == null) return;

            if (dropdown && _names != null && _names.Length > 0)
            {
                int idx = 0;
                if (valObj != null)
                {
                    var name = Enum.GetName(mt, valObj) ?? valObj.ToString();
                    for (int i = 0; i < _names.Length; i++)
                    {
                        if (_names[i] == name) { idx = i; break; }
                    }
                }
#if UNITY_2019_1_OR_NEWER
                if (dropdown.value != idx) dropdown.SetValueWithoutNotify(idx);
#else
                if (dropdown.value != idx) dropdown.value = idx;
#endif
                if (valueText) valueText.text = _names[idx];
            }
            else if (valueText)
            {
                valueText.text = valObj != null ? valObj.ToString() : "(null)";
            }
        }

        private string GetAutoLabel()
        {
            if (!string.IsNullOrWhiteSpace(labelOverride)) return labelOverride;
            return SplitCamelCase(memberName);
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
    }
}