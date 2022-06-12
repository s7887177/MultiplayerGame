#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

using UnityEngine;

[System.Serializable]
public struct SpawnMessage
{
    [SerializeField]
    internal string spawnObjectName;
}
