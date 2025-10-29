using System;
using System.Globalization;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace FieldKit
{
    public class FieldKitVector3 : FieldKitControl
    {
        [Header("UI References")]
        public TMP_Text labelText;
        public TMP_InputField inputFieldX;
        public TMP_InputField inputFieldY;
        public TMP_InputField inputFieldZ;
        public TMP_Text valueText; // optional

        [Header("Options")]
        public bool readOnly;
        public string labelOverride;
        public float pollInterval = 0.1f;

        private float _timer;

        private void OnEnable()
        {
            ValidateTypeOrDisable(typeof(Vector3));
            if (labelText) labelText.text = GetAutoLabel();

            if (inputFieldX)
            {
                inputFieldX.contentType = TMP_InputField.ContentType.DecimalNumber;
                inputFieldX.onEndEdit.RemoveAllListeners();
                inputFieldX.onEndEdit.AddListener(s => OnEndEdit());
                inputFieldX.interactable = !readOnly;
            }

            if (inputFieldY)
            {
                inputFieldY.contentType = TMP_InputField.ContentType.DecimalNumber;
                inputFieldY.onEndEdit.RemoveAllListeners();
                inputFieldY.onEndEdit.AddListener(s => OnEndEdit());
                inputFieldY.interactable = !readOnly;
            }

            if (inputFieldZ)
            {
                inputFieldZ.contentType = TMP_InputField.ContentType.DecimalNumber;
                inputFieldZ.onEndEdit.RemoveAllListeners();
                inputFieldZ.onEndEdit.AddListener(s => OnEndEdit());
                inputFieldZ.interactable = !readOnly;
            }

            _timer = 0f;
            RefreshUI(force: true);
        }

        private void OnDisable()
        {
            if (inputFieldX) inputFieldX.onEndEdit.RemoveAllListeners();
            if (inputFieldY) inputFieldY.onEndEdit.RemoveAllListeners();
            if (inputFieldZ) inputFieldZ.onEndEdit.RemoveAllListeners();
        }

        private void Update()
        {
            _timer += Time.unscaledDeltaTime;
            if (_timer >= pollInterval)
            {
                _timer = 0f;
                if ((inputFieldX && inputFieldX.isFocused) || (inputFieldY && inputFieldY.isFocused) || (inputFieldZ && inputFieldZ.isFocused)) return;
                RefreshUI();
            }
        }

        private void OnEndEdit()
        {
            if (readOnly) return;
            if (float.TryParse(inputFieldX?.text ?? "0", NumberStyles.Float, CultureInfo.InvariantCulture, out var x) &&
                float.TryParse(inputFieldY?.text ?? "0", NumberStyles.Float, CultureInfo.InvariantCulture, out var y) &&
                float.TryParse(inputFieldZ?.text ?? "0", NumberStyles.Float, CultureInfo.InvariantCulture, out var z))
            {
                SetValue(new Vector3(x, y, z));
            }
        }

        private void RefreshUI(bool force = false)
        {
            var valObj = GetValue();
            Vector3 v = valObj is Vector3 vec ? vec : Vector3.zero;
            if (inputFieldX && !inputFieldX.isFocused)
            {
                var s = v.x.ToString(CultureInfo.InvariantCulture);
                if (inputFieldX.text != s) inputFieldX.text = s;
            }
            if (inputFieldY && !inputFieldY.isFocused)
            {
                var s = v.y.ToString(CultureInfo.InvariantCulture);
                if (inputFieldY.text != s) inputFieldY.text = s;
            }
            if (inputFieldZ && !inputFieldZ.isFocused)
            {
                var s = v.z.ToString(CultureInfo.InvariantCulture);
                if (inputFieldZ.text != s) inputFieldZ.text = s;
            }
            if (valueText) valueText.text = $"({v.x}, {v.y}, {v.z})";
        }

        private void ValidateTypeOrDisable(Type required)
        {
            var t = GetMemberType();
            if (t != required)
            {
                Debug.LogWarning($"{nameof(FieldKitVector3)} on {name}: Selected member type '{t}' doesn't match '{required}'. Disabling.");
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