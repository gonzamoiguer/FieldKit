using System;
using UnityEditor;
using FieldKit;

[CustomEditor(typeof(FieldKitString))]
public class FieldKitStringEditor : FieldKitEditorBase<FieldKitString>
{
    protected override bool AcceptType(Type t) => t == typeof(string);
    protected override string HeaderTitle => "String Control";
}