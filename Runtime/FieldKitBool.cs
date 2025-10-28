using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace FieldKit
{
    public class FieldKitBool : FieldKitControl
    {
        [Header("UI References")]
        public TMP_Text labelText;
        public Toggle toggle;
        public TMP_Text valueText; // optional for read-only display

        [Header("Options")]
        public bool readOnly;
        public string labelOverride;
        public float pollInterval = 0.1f;

        private float _timer;

        private void OnEnable()
        {
            ValidateTypeOrDisable(typeof(bool));
            if (toggle)
            {
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener(v => { if (!readOnly) SetValue(v); });
                toggle.interactable = !readOnly;
            }
            if (labelText)
                labelText.text = GetAutoLabel();
            _timer = 0f;
            RefreshUI(force:true);
        }

        private void OnDisable()
        {
            if (toggle) toggle.onValueChanged.RemoveAllListeners();
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
            var valObj = GetValue();
            bool v = valObj is bool b && b;
            if (toggle && toggle.isOn != v) toggle.isOn = v;
            if (valueText) valueText.text = v ? "True" : "False";
        }

        private void ValidateTypeOrDisable(Type required)
        {
            var t = GetMemberType();
            if (t != required)
            {
                Debug.LogWarning($"{nameof(FieldKitBool)} on {name}: Selected member type '{t}' doesn't match '{required}'. Disabling.");
                enabled = false;
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