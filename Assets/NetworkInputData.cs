using Fusion;
using UnityEngine;

/// <summary>
/// Our custom definition of an INetworkStruct. Keep in mind that
/// * bool does not work (C# does not define a consistent size on different platforms)
/// * Must be a top-level struct (cannot be a nested class) (JB: WRONG?)
/// * Stick to primitive types and structs
/// * Size is not an issue since only modified data is serialized, but things that change often should be compact (e.g. button states)
/// </summary>
public struct NetworkInputData : INetworkInput
{
    public Vector2 move;
    public float rotation;
    public bool jump;
}