#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif
using UnityEngine;
using WebSocketSharp;

public class Projectile : MonoBehaviour
{
    public string ownerId;
    public float atk = 10f;
    public float destoryInterval = 3f;
    public WebSocket ws => FindObjectOfType<WebSocketExperiemnt>().ws;
    private void OnTriggerEnter(Collider other)
    {
        Invoke(nameof(DestorySelf), destoryInterval);
        var player = other.GetComponent<PlayerMovement>();
        if (player)
        {
            if(player.id != ownerId)
            {

                //player.TakeDamage(atk);
                var arg = new BulletHitEventArgs
                {
                    data = new BulletEventData
                    {
                        id = this.ownerId,
                        hitPlayerId = player.id,
                        atk = this.atk
                    }
                };
                ws.Send(JsonUtility.ToJson(arg));
                Destroy(this.gameObject);
            }
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    private void DestorySelf()
    {
        Destroy(this.gameObject);
    }

}
