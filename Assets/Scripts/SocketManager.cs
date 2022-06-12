#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json.Linq;

[System.Serializable]
public struct PlayerDatas
{
    [SerializeField]
    internal string type;
    [SerializeField]
    internal PlayerData[] datas;
}

public class SocketManager : MonoBehaviour
{
    WebSocket socket;
    [SerializeField]
    GameObject player;
    [SerializeField]
    PlayerData playerData;
    [SerializeField]
    string ip;
    [SerializeField]
    int port;
    void OnEnable()
    {
        socket = new WebSocket($"ws://{ip}:{port}");
        socket.OnMessage += (sender, e) =>
        {
            if(e.IsText)
            {
                //Debug.Log(JObject.Parse(e.Data));

                JObject jsonObject = JObject.Parse(e.Data);
                if (jsonObject["type"]?.ToString() == "playerDatas")
                {
                    var tempData = JsonUtility.FromJson<PlayerDatas>(e.Data);
                    //Debug.Log(tempData.type);
                    foreach (var data in tempData.datas)
                    {
                        //Debug.Log(data);
                    }
                    return;
                }
                if (jsonObject["type"]?.ToString() == "newFunction")
                {
                    Debug.Log("newFunction:");
                    var dogName = jsonObject["data"]["animals"][0]["name"].Value<string>();
                    Debug.Log($"{nameof(dogName)}: {dogName}");
                }
                if(jsonObject["id"] != null)
                {
                    var tempData = JsonUtility.FromJson<PlayerData>(e.Data);
                    playerData = tempData;
                    //Debug.Log("Player ID is " + playerData.id);
                    return;
                }
            }
        };
        socket.Connect();

        socket.OnClose += (sender, e) =>
        {
            Debug.Log(e.Code);
            Debug.Log(e.Reason);
            Debug.Log("Connection Closed.");
        };
    }

    void OnMessage(object sender, MessageEventArgs args)
    {

    }
    void Update()
    {
        if (socket == null)
        {
            return;
        }
        
        if(player != null && !string.IsNullOrEmpty(playerData.id))
        {
            playerData.position = player.transform.position;
            var epochStart = new System.DateTime(1970, 1, 1, 8, 0, 0, System.DateTimeKind.Utc);
            var timestamp = (System.DateTime.UtcNow - epochStart).TotalSeconds;
            playerData.timestamp = timestamp;
            var playerDataJSON = JsonUtility.ToJson(playerData);
            socket.Send(playerDataJSON);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            var messageJSON = "{\"message\": \"Some Message From Client\"}";
            socket.Send(messageJSON);
        }
    }
    void OnDisable()
    {
        socket.Close();
    }

}
