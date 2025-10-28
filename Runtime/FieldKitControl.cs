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

        // Optional: store last value as string for display/debugging
        [SerializeField, TextArea]
        private string previewValue;

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
            previewValue = ok && value != null ? value.ToString() : string.Empty;
            return value;
        }

        public bool SetValue(object value)
        {
            if (!HasValidSelection()) return false;
            var ok = FieldKitReflection.TrySetValue(targetComponent, memberName, memberKind, value);
            previewValue = ok && value != null ? value.ToString() : string.Empty;
            return ok;
        }
    }
}