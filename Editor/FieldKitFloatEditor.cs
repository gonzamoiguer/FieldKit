using System;
using UnityEditor;
using FieldKit;

[CustomEditor(typeof(FieldKitFloat))]
public class FieldKitFloatEditor : FieldKitEditorBase<FieldKitFloat>
{
    protected override bool AcceptType(Type t) => t == typeof(float);
    protected override string HeaderTitle => "Float Control";
}