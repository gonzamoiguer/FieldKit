using System;
using System.Globalization;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace FieldKit
{
    public class FieldKitFloat : FieldKitControl
    {
        [Header("UI References")]
        public TMP_Text labelText;
        public TMP_InputField inputField; // optional
        public Slider slider;         // optional
        public TMP_Text valueText;        // optional

        [Header("Options")]
        public bool readOnly;
        public string labelOverride;
        public float pollInterval = 0.1f;
        public float min = 0f;
        public float max = 1f;

        private float _timer;

        private void OnEnable()
        {
            ValidateTypeOrDisable(typeof(float));
            if (labelText) labelText.text = GetAutoLabel();

            if (inputField)
            {
                inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
                inputField.onEndEdit.RemoveAllListeners();
                inputField.onEndEdit.AddListener(OnEndEdit);
                inputField.interactable = !readOnly;
            }

            if (slider)
            {
                slider.minValue = min;
                slider.maxValue = max;
                slider.onValueChanged.RemoveAllListeners();
                slider.onValueChanged.AddListener(v => { if (!readOnly) SetValue(v); SyncText(v); });
                slider.interactable = !readOnly;
            }

            _timer = 0f;
            RefreshUI(force:true);
        }

        private void OnDisable()
        {
            if (inputField) inputField.onEndEdit.RemoveAllListeners();
            if (slider) slider.onValueChanged.RemoveAllListeners();
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
            if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
            {
                SetValue(v);
                if (slider) slider.value = Mathf.Clamp(v, min, max);
                SyncText(v);
            }
        }

        private void RefreshUI(bool force = false)
        {
            var valObj = GetValue();
            float v = valObj is float f ? f : 0f;
            if (slider && !Mathf.Approximately(slider.value, v)) slider.value = Mathf.Clamp(v, min, max);
            if (inputField && !inputField.isFocused)
            {
                var s = v.ToString(CultureInfo.InvariantCulture);
                if (inputField.text != s) inputField.text = s;
            }
            SyncText(v);
        }

        private void SyncText(float v)
        {
            if (valueText) valueText.text = v.ToString(CultureInfo.InvariantCulture);
        }

        private void ValidateTypeOrDisable(Type required)
        {
            var t = GetMemberType();
            if (t != required)
            {
                Debug.LogWarning($"{nameof(FieldKitFloat)} on {name}: Selected member type '{t}' doesn't match '{required}'. Disabling.");
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