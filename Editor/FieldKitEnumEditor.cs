using System;
using UnityEditor;
using FieldKit;

[CustomEditor(typeof(FieldKitEnum))]
public class FieldKitEnumEditor : FieldKitEditorBase<FieldKitEnum>
{
    protected override bool AcceptType(Type t) => t != null && t.IsEnum;
    protected override string HeaderTitle => "Enum Control";
}