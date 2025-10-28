using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace FieldKit
{
    public static class FieldKitReflection
    {
        public struct MemberDescriptor
        {
            public string Name;
            public Type Type;
            public FieldKitControl.MemberKind Kind;
            public bool CanRead;
            public bool CanWrite;

            public override string ToString() => $"{Name} : {Type?.Name} ({Kind})";
        }

        public static readonly BindingFlags InstanceBindings =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static Type TryGetMemberType(object target, string memberName, FieldKitControl.MemberKind kind)
        {
            if (target == null || string.IsNullOrEmpty(memberName)) return null;
            var t = target.GetType();
            switch (kind)
            {
                case FieldKitControl.MemberKind.Field:
                    var f = t.GetField(memberName, InstanceBindings);
                    return f?.FieldType;
                case FieldKitControl.MemberKind.Property:
                    var p = t.GetProperty(memberName, InstanceBindings);
                    return p?.PropertyType;
                default:
                    return null;
            }
        }

        public static bool TryGetValue(object target, string memberName, FieldKitControl.MemberKind kind, out object value)
        {
            value = null;
            if (target == null || string.IsNullOrEmpty(memberName)) return false;
            var t = target.GetType();
            try
            {
                switch (kind)
                {
                    case FieldKitControl.MemberKind.Field:
                        var f = t.GetField(memberName, InstanceBindings);
                        if (f == null) return false;
                        value = f.GetValue(target);
                        return true;
                    case FieldKitControl.MemberKind.Property:
                        var p = t.GetProperty(memberName, InstanceBindings);
                        if (p == null || !p.CanRead) return false;
                        value = p.GetValue(target, null);
                        return true;
                    default:
                        return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"FieldKit: Failed to get value {memberName} on {t?.Name}: {e.Message}");
                return false;
            }
        }

        public static bool TrySetValue(object target, string memberName, FieldKitControl.MemberKind kind, object value)
        {
            if (target == null || string.IsNullOrEmpty(memberName)) return false;
            var t = target.GetType();
            try
            {
                switch (kind)
                {
                    case FieldKitControl.MemberKind.Field:
                        var f = t.GetField(memberName, InstanceBindings);
                        if (f == null) return false;
                        var fv = ConvertValue(value, f.FieldType);
                        f.SetValue(target, fv);
                        return true;
                    case FieldKitControl.MemberKind.Property:
                        var p = t.GetProperty(memberName, InstanceBindings);
                        if (p == null || !p.CanWrite) return false;
                        var pv = ConvertValue(value, p.PropertyType);
                        p.SetValue(target, pv, null);
                        return true;
                    default:
                        return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"FieldKit: Failed to set value {memberName} on {t?.Name}: {e.Message}");
                return false;
            }
        }

        public static List<MemberDescriptor> GetEligibleMembers(Type type, bool supportedTypesOnly = true)
        {
            var list = new List<MemberDescriptor>();
            if (type == null) return list;

            // Fields
            foreach (var f in type.GetFields(InstanceBindings))
            {
                if (f.IsStatic) continue;
                var md = new MemberDescriptor
                {
                    Name = f.Name,
                    Type = f.FieldType,
                    Kind = FieldKitControl.MemberKind.Field,
                    CanRead = true,
                    CanWrite = !f.IsInitOnly
                };
                if (!supportedTypesOnly || IsSupportedType(md.Type))
                    list.Add(md);
            }

            // Properties
            foreach (var p in type.GetProperties(InstanceBindings))
            {
                var getter = p.GetGetMethod(true);
                var setter = p.GetSetMethod(true);
                if ((getter == null && setter == null) || (getter != null && getter.IsStatic) || (setter != null && setter.IsStatic))
                    continue;

                var md = new MemberDescriptor
                {
                    Name = p.Name,
                    Type = p.PropertyType,
                    Kind = FieldKitControl.MemberKind.Property,
                    CanRead = getter != null,
                    CanWrite = setter != null
                };
                if (!supportedTypesOnly || IsSupportedType(md.Type))
                    list.Add(md);
            }

            // Sort alphabetically
            list = list.OrderBy(m => m.Name).ToList();
            return list;
        }

        public static bool IsSupportedType(Type t)
        {
            if (t == null) return false;
            if (t.IsEnum) return true;
            return t == typeof(bool) ||
                   t == typeof(int) ||
                   t == typeof(float) ||
                   t == typeof(string) ||
                   t == typeof(Vector2) ||
                   t == typeof(Vector3) ||
                   t == typeof(Vector4) ||
                   t == typeof(Color);
        }

        private static object ConvertValue(object value, Type targetType)
        {
            if (value == null) return null;
            var vType = value.GetType();
            if (targetType.IsAssignableFrom(vType)) return value;
            try
            {
                if (targetType.IsEnum)
                {
                    if (value is string s) return Enum.Parse(targetType, s);
                    return Enum.ToObject(targetType, value);
                }
                // Basic numeric conversions
                if (targetType == typeof(int)) return System.Convert.ToInt32(value);
                if (targetType == typeof(float)) return System.Convert.ToSingle(value);
                if (targetType == typeof(string)) return value.ToString();
            }
            catch { /* fall through */ }
            return value;
        }
    }
}