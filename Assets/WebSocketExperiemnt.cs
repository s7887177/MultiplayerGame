using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;

class WebSocketExperiemnt : MonoBehaviour
{
    [SerializeField] string host = "localhost";
    [SerializeField] int port = 8080;
    readonly Dictionary<string, PlayerMovement> players = new Dictionary<string, PlayerMovement>();
    [SerializeField] string[] ids;
    [SerializeField] PlayerMovement[] i_players;
    WebSocket ws;
    [SerializeField]
    string id;
    [SerializeField]
    PlayerMovement player;
    [SerializeField]
    PlayerMovement playerPrefab;
    event System.Action onceAction;
    private void OnEnable()
    {
        var url = $"ws://{host}:{port}";
        ws = new WebSocket(url);
        ws.OnOpen += OnOpen;
        ws.OnMessage += OnMessage;
        ws.OnClose += OnClose;
        ws.OnError += OnError;
        ws.Connect();
        onceAction = () => { };
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        Debug.LogException(e.Exception);
    }
    private void OnOpen(object sender, System.EventArgs e)
    {
        Debug.Log(nameof(OnOpen));
        LogEventInfos(sender, e);

        var sender_ws = (WebSocket)sender;
        //var args = (StartEventArgs)e;
        //Debug.Log(e);
    }
    private void OnMessage(object sender, MessageEventArgs e)
    {
        
        Debug.Log(nameof(OnMessage));
        if (e.IsText)
        {
            Debug.Log(e.Data);
            var jMessage = JObject.Parse(e.Data);
            var type = jMessage["type"];
            if (type == null) return;
            switch (type.ToString())
            {
                case "InitPlayer":
                    {
                        var args = JsonUtility.FromJson<InitPlayerEventArgs>(e.Data);
                        this.id = args.data.id;
                        players[id] = this.player;
                        lock (onceAction)
                        {
                            onceAction += () =>
                            {

                                var toSendArgs = new SpawnPlayerEventArgs
                                {
                                    data = new SpawnPlayerEventData
                                    {
                                        id = this.id,
                                        position = this.player.transform.position
                                    }
                                };
                                string json = JsonUtility.ToJson(toSendArgs);
                                ws.Send(json);
                                foreach (var otherPlayer in args.data.others)
                                {
                                    SpawnPlayer(otherPlayer.data.id, otherPlayer.data.position);
                                }
                            };

                        }

                    }
                    break;
                case "SpawnPlayer":
                    {
                        var args = JsonUtility.FromJson<SpawnPlayerEventArgs>(e.Data);
                        lock (onceAction)
                        {
                            onceAction += () =>
                            {
                                SpawnPlayer(args.data.id, args.data.position);
                            };
                        }
                    }
                    break;
                case "MovePlayer":
                    {
                        var args = JsonUtility.FromJson<MovePlayerEventArgs>(e.Data);
                        lock (onceAction)
                        {
                            onceAction += () =>
                            {
                                if (players.ContainsKey(args.data.id))
                                {
                                    var player = players[args.data.id];
                                    player.transform.position = args.data.position;
                                }
                            };
                        }
                    }
                    break;
                case "PlayerExit":
                    {
                        var args = JsonUtility.FromJson<PlayerExitEventArgs>(e.Data);
                        lock (onceAction)
                        {
                            onceAction += () =>
                            {
                                if (this.players.ContainsKey(args.data.id))
                                {
                                    var player = players[args.data.id];
                                    Object.Destroy(player.gameObject);
                                    players.Remove(args.data.id);
                                }
                            };
                        }
                    }
                    break;
                case "newFunction":
                    {
                        JObject jsonObject = JObject.Parse(e.Data);
                        Debug.Log("newFunction:");
                        var dogName = jsonObject["data"]["animals"][0]["name"].Value<string>();
                        Debug.Log($"{nameof(dogName)}: {dogName}");
                    }
                    break;
                default:
                    break;
            }


        }
        LogEventInfos(sender, e);
    }
    private void OnClose(object sender, CloseEventArgs e)
    {
        Debug.Log(nameof(OnClose));
        LogEventInfos(sender, e);
    }

    private void OnDisable()
    {
        ws.Close();
    }

    private void Update()
    {
        ids = players.Select(kvp => kvp.Key).ToArray();
        i_players = players.Select(kvp => kvp.Value).ToArray();
        var movePlayerArgs = new MovePlayerEventArgs()
        {
            data = new MovePlayerEventData()
            {
                id = this.id,
                position = this.player.transform.position,
            }
        };
        ws.Send(JsonUtility.ToJson(movePlayerArgs));
        lock (onceAction)
        {
            onceAction();
            onceAction = () => { };
        }
        
    }
    //Utils
    private void LogEventInfos(object sender, System.EventArgs e)
    {
        //Debug.Log($"{nameof(sender)}: {sender}");
        //Debug.Log($"{nameof(e)}: {e}");
    }
    [DisableInEditorMode]
    [Button(nameof(ShutdownServer))]
    private void ShutdownServer()
    {
        var json = JsonUtility.ToJson(new ShutdownServerEventArgs());
        ws.Send(json);
    }
    void SpawnPlayer(string id, Vector3 position)
    {
        if (string.IsNullOrEmpty(id)) throw new System.Exception("You have not inited yet");
        if (id == this.id) return;
        if (players.ContainsKey(id)) return;
        var player = GameObject.Instantiate(playerPrefab);
        player.id = id;
        player.transform.position = position;
        player.isMine = false;
        players[id] = player;
    }
}
class StartEventArgs : System.EventArgs
{
    public string id;
}
[System.Serializable]
class ShutdownServerEventArgs : System.EventArgs
{
    [SerializeField]
    string type = "ShutdownServer";
}
[System.Serializable]
class SpawnPlayerEventArgs : System.EventArgs
{
    [SerializeField]
    string type = "SpawnPlayer";
    [SerializeField]
    internal SpawnPlayerEventData data;
}
[System.Serializable]
class SpawnPlayerEventData
{
    [SerializeField] internal string id;
    [SerializeField]
    internal Vector3 position;
}
[System.Serializable]
class MovePlayerEventArgs : System.EventArgs
{
    [SerializeField] string type = "MovePlayer";
    [SerializeField] internal MovePlayerEventData data;
}
[System.Serializable]
class MovePlayerEventData
{
    [SerializeField] internal string id;
    [SerializeField] internal Vector3 position;
}
[System.Serializable]
class InitPlayerEventArgs : System.EventArgs
{
    [SerializeField] string type = "InitPlayer";
    [SerializeField] internal InitPlayerEventData data;
}
[System.Serializable]
class InitPlayerEventData
{
    [SerializeField] internal string id;
    [SerializeField] internal MovePlayerEventArgs[] others;
}

[System.Serializable]
class PlayerExitEventArgs : System.EventArgs
{
    [SerializeField] string type = "PlayerExit";
    [SerializeField] internal PlayerExitEventData data;
}
[System.Serializable]
class PlayerExitEventData
{
    [SerializeField] internal string id;
}
class MessageSender
{

}