#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SocketSpawner : MonoBehaviour
{
    [SerializeField]
    List<SpawnCommand> commands = new List<SpawnCommand>();

    public void HandleMessage(SpawnMessage message)
    {
        var command = commands.Find(command => command.name == message.spawnObjectName);
        try
        {
            command.Validate();
        } catch (System.Exception e)
        {
            Debug.LogException(e);
        }

        GameObject.Instantiate(command.spawnObject);
    }
}
