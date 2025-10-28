using System;
using UnityEditor;
using FieldKit;

[CustomEditor(typeof(FieldKitBool))]
public class FieldKitBoolEditor : FieldKitEditorBase<FieldKitBool>
{
    protected override bool AcceptType(Type t) => t == typeof(bool);
    protected override string HeaderTitle => "Bool Control";
}