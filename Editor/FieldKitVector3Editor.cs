using System;
using UnityEditor;
using UnityEngine;
using FieldKit;

[CustomEditor(typeof(FieldKitVector3))]
public class FieldKitVector3Editor : FieldKitEditorBase<FieldKitVector3>
{
    protected override bool AcceptType(Type t) => t == typeof(Vector3);
    protected override string HeaderTitle => "Vector3 Control";
}