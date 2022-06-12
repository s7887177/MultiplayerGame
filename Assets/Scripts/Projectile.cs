#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public string ownerId;
    public float atk = 10f;
    public float destoryInterval = 3f;
    private void OnTriggerEnter(Collider other)
    {
        Invoke(nameof(DestorySelf), destoryInterval);
        var player = other.GetComponent<PlayerMovement>();
        if (player)
        {
            if(player.id != ownerId)
            {
                player.TakeDamage(atk);
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
