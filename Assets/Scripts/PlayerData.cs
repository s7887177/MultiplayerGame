#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif
using UnityEngine;

[System.Serializable]
public struct PlayerData
{
    public string id;
    public Vector3 position;
    public double timestamp;
}
