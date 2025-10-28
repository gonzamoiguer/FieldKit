using System;
using System.Globalization;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace FieldKit
{
    public class FieldKitInt : FieldKitControl
    {
        [Header("UI References")]
        public TMP_Text labelText;
        public TMP_InputField inputField;
        public TMP_Text valueText; // optional

        [Header("Options")]
        public bool readOnly;
        public string labelOverride;
        public float pollInterval = 0.1f;
        public int incrementStep = 1;

        private float _timer;

        private void OnEnable()
        {
            ValidateTypeOrDisable(typeof(int));
            if (labelText) labelText.text = GetAutoLabel();

            if (inputField)
            {
                inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
                inputField.onEndEdit.RemoveAllListeners();
                inputField.onEndEdit.AddListener(OnEndEdit);
                inputField.interactable = !readOnly;
            }

            _timer = 0f;
            RefreshUI(force:true);
        }

        private void OnDisable()
        {
            if (inputField) inputField.onEndEdit.RemoveAllListeners();
        }

        private void Update()
        {
            _timer += Time.unscaledDeltaTime;
            if (_timer >= pollInterval)
            {
                _timer = 0f;
                if (inputField && inputField.isFocused) return;
                RefreshUI();
            }
        }

        private void OnEndEdit(string text)
        {
            if (readOnly) return;
            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
            {
                SetValue(v);
            }
        }

        private void RefreshUI(bool force = false)
        {
            var valObj = GetValue();
            int v = valObj is int i ? i : 0;
            if (inputField && !inputField.isFocused)
            {
                var s = v.ToString(CultureInfo.InvariantCulture);
                if (inputField.text != s) inputField.text = s;
            }
            if (valueText) valueText.text = v.ToString(CultureInfo.InvariantCulture);
        }

        private void ValidateTypeOrDisable(Type required)
        {
            var t = GetMemberType();
            if (t != required)
            {
                Debug.LogWarning($"{nameof(FieldKitInt)} on {name}: Selected member type '{t}' doesn't match '{required}'. Disabling.");
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

        public void Increment()
        {
            if (readOnly) return;
            var valObj = GetValue();
            int v = valObj is int i ? i : 0;
            v += incrementStep;
            SetValue(v);
            RefreshUI(force: true);
        }
        public void Decrement()
        {
            if (readOnly) return;
            var valObj = GetValue();
            int v = valObj is int i ? i : 0;
            v -= incrementStep;
            SetValue(v);
            RefreshUI(force: true);
        }
    }
}