using System;
using UnityEditor;
using FieldKit;

[CustomEditor(typeof(FieldKitInt))]
public class FieldKitIntEditor : FieldKitEditorBase<FieldKitInt>
{
    protected override bool AcceptType(Type t) => t == typeof(int);
    protected override string HeaderTitle => "Int Control";
}