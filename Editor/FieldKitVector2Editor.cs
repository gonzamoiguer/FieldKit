using System;
using UnityEditor;
using UnityEngine;
using FieldKit;

[CustomEditor(typeof(FieldKitVector2))]
public class FieldKitVector2Editor : FieldKitEditorBase<FieldKitVector2>
{
    protected override bool AcceptType(Type t) => t == typeof(Vector2);
    protected override string HeaderTitle => "Vector2 Control";
}