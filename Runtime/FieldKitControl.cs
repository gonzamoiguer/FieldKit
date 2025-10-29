using System;
using UnityEngine;

namespace FieldKit
{
    public class FieldKitControl : MonoBehaviour
    {
        [Header("Target selection")]
        [Tooltip("Component (MonoBehaviour) on a scene object whose variable you want to control.")]
        public MonoBehaviour targetComponent;

        public enum MemberKind { None, Field, Property }

        [Tooltip("Name of the selected field/property on the target component.")]
        public string memberName;

        [Tooltip("Whether the selected member is a Field or Property.")]
        public MemberKind memberKind = MemberKind.None;

        [Tooltip("Assembly-qualified type name of the selected member. Used for validation.")]
        public string memberTypeAQN;

        [Header("Persistence")]
        [Tooltip("If true, the value will be saved to PlayerPrefs and loaded on play.")]
        public bool saveToPlayerPrefs;


        private bool _isLoading;

        private void Awake()
        {
            if (saveToPlayerPrefs && HasValidSelection())
            {
                _isLoading = true;
                LoadFromPlayerPrefs();
                _isLoading = false;
            }
        }

        public bool HasValidSelection()
        {
            return targetComponent != null && !string.IsNullOrEmpty(memberName) && memberKind != MemberKind.None;
        }

        public Type GetMemberType()
        {
            if (!HasValidSelection()) return null;
            if (!string.IsNullOrEmpty(memberTypeAQN))
            {
                var t = Type.GetType(memberTypeAQN);
                if (t != null) return t;
            }
            return FieldKitReflection.TryGetMemberType(targetComponent, memberName, memberKind);
        }

        public object GetValue()
        {
            if (!HasValidSelection()) return null;
            var ok = FieldKitReflection.TryGetValue(targetComponent, memberName, memberKind, out var value);
            return value;
        }

        public bool SetValue(object value)
        {
            if (!HasValidSelection()) return false;
            var ok = FieldKitReflection.TrySetValue(targetComponent, memberName, memberKind, value);
            if (ok && saveToPlayerPrefs && !_isLoading)
            {
                SaveToPlayerPrefs(value);
            }
            return ok;
        }

        public string GetPlayerPrefsKey()
        {
            if (!HasValidSelection()) return null;
            return $"FieldKit_{targetComponent.gameObject.name}_{targetComponent.GetType().Name}_{memberName}";
        }

        private void LoadFromPlayerPrefs()
        {
            var key = GetPlayerPrefsKey();
            var type = GetMemberType();
            if (type == typeof(bool) && PlayerPrefs.HasKey(key))
            {
                SetValue(PlayerPrefs.GetInt(key) == 1);
            }
            else if (type == typeof(int) && PlayerPrefs.HasKey(key))
            {
                SetValue(PlayerPrefs.GetInt(key));
            }
            else if (type == typeof(float) && PlayerPrefs.HasKey(key))
            {
                SetValue(PlayerPrefs.GetFloat(key));
            }
            else if (type == typeof(string) && PlayerPrefs.HasKey(key))
            {
                SetValue(PlayerPrefs.GetString(key));
            }
            else if (type.IsEnum && PlayerPrefs.HasKey(key))
            {
                var valStr = PlayerPrefs.GetString(key);
                try
                {
                    var val = Enum.Parse(type, valStr);
                    SetValue(val);
                }
                catch { }
            }
            else if (type == typeof(Vector2) && PlayerPrefs.HasKey(key))
            {
                var str = PlayerPrefs.GetString(key);
                var parts = str.Split(',');
                if (parts.Length == 2 && float.TryParse(parts[0], out var x) && float.TryParse(parts[1], out var y))
                {
                    SetValue(new Vector2(x, y));
                }
            }
            else if (type == typeof(Vector3) && PlayerPrefs.HasKey(key))
            {
                var str = PlayerPrefs.GetString(key);
                var parts = str.Split(',');
                if (parts.Length == 3 && float.TryParse(parts[0], out var x) && float.TryParse(parts[1], out var y) && float.TryParse(parts[2], out var z))
                {
                    SetValue(new Vector3(x, y, z));
                }
            }
        }

        private void SaveToPlayerPrefs(object value)
        {
            var key = GetPlayerPrefsKey();
            var type = GetMemberType();
            if (type == typeof(bool))
                PlayerPrefs.SetInt(key, (bool)value ? 1 : 0);
            else if (type == typeof(int))
                PlayerPrefs.SetInt(key, (int)value);
            else if (type == typeof(float))
                PlayerPrefs.SetFloat(key, (float)value);
            else if (type == typeof(string))
                PlayerPrefs.SetString(key, (string)value);
            else if (type.IsEnum)
                PlayerPrefs.SetString(key, value.ToString());
            else if (type == typeof(Vector2))
            {
                var v = (Vector2)value;
                PlayerPrefs.SetString(key, $"{v.x},{v.y}");
            }
            else if (type == typeof(Vector3))
            {
                var v = (Vector3)value;
                PlayerPrefs.SetString(key, $"{v.x},{v.y},{v.z}");
            }
            PlayerPrefs.Save();
        }
    }
}