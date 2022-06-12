#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

using UnityEngine;

[System.Serializable]

public struct SpawnCommand
{
    [SerializeField]
    internal string name;
    [SerializeField]
    internal GameObject spawnObject;

    public void Validate()
    {
        if (System.String.IsNullOrEmpty(name))
        {
            throw new System.ArgumentNullException("name");
        }
        if(spawnObject == null)
        {
            throw new System.ArgumentNullException("spawnObject");
        }
    }

}
