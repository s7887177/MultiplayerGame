#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class PlayerMovement : MonoBehaviour
{
    public WebSocket ws => FindObjectOfType<WebSocketExperiemnt>()?.ws;
    public float maxHp = 100;
    public float hp = 100;
    public float hpRatio => hp / maxHp;
    public Image hpBar;
    [Sirenix.OdinInspector.Button]
    public void TakeDamage(float atk)
    {
        hp -= atk;
        hp = Mathf.Clamp(hp, 0, maxHp);
        UpdateHpBar();
        if(hp == 0)
        {
            Die();
        }
    }
    public void InitializeImage()
    {
        hpBar.type = Image.Type.Filled;
        hpBar.fillMethod = Image.FillMethod.Horizontal;
        TakeDamage(0);
    }
    public void Die()
    {
        Debug.Log(nameof(Die));
    }
    public void Fire()
    {
        var bullet = GameObject.Instantiate(bulletPrefab, gun.position, gun.rotation);
        bullet.GetComponent<Rigidbody>().AddForce(bulletForce * bullet.transform.forward, ForceMode.Impulse);
    }
    public void UpdateHpBar()
    {
        
        hpBar.fillAmount = hpRatio;
        if(hpRatio > .5f)
        {
            hpBar.color = Color.Lerp(Color.yellow, Color.green, (hpRatio-.5f)*2);
        }
        else
        {
            hpBar.color = Color.Lerp(Color.red, Color.yellow, hpRatio*2);
        }
    }

    public Projectile bulletPrefab;
    public GroundChecker groundChecker;
    public string id;
    public float speed;
    public float rotSpeedX = 150;
    public float rotSpeedY = 70;
    public bool isMine = true;

    public int? nullable = 0;
    public int anotherInt = 0;
    public Transform gun;
    [SerializeField] Vector3 jumpForce;
    public float bulletForce;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        FindObjectOfType<CameraFollow>().gunPlaceHoder.position = gun.position;
        FindObjectOfType<CameraFollow>().gunPlaceHoder.rotation= gun.rotation;
        InitializeImage();
    }

    private void Update()
    {

        if (!Application.isPlaying)
            return;

        MoveUpdate();
        FaceUpdate();
        HandleInput();
        GunUpdate();
    }
    
    private void MoveUpdate()
    {
        if (!isMine) return;
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
        input = this.transform.rotation * input;
        Vector3 moveVector = input * speed * Time.deltaTime;
        transform.position += moveVector;
    }
    private void FaceUpdate()
    {
        if (!isMine) return;
        Vector3 input = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0).normalized;
        var angle = transform.rotation.eulerAngles;
        angle.y += input.x * rotSpeedX * Time.deltaTime;
        transform.rotation = Quaternion.Euler(angle);
        var cam = Camera.main;
        angle = cam.transform.eulerAngles;
        angle.x += -input.y * rotSpeedY * Time.deltaTime;
        var cameraFollow = GameObject.FindObjectOfType<CameraFollow>();
        angle.x = angle.x > 180f ? angle.x - 360f : angle.x;
        angle.x = Mathf.Clamp(angle.x, -89.99f, 89.99f);
        cam.transform.rotation = Quaternion.Euler(angle);

        cameraFollow.transform.position = this.transform.position;
        cameraFollow.transform.rotation = this.transform.rotation;
    }
    private void GunUpdate()
    {
        if (!isMine) return;
        var gunPlaceHolder = GameObject.FindObjectOfType<CameraFollow>().gunPlaceHoder;
        gun.position = gunPlaceHolder.position;
        gun.rotation = gunPlaceHolder.rotation;
    }
    private void HandleInput()
    {
        if (!isMine) return;
        if (Input.GetKeyDown(KeyCode.Space)&& groundChecker.isGround){
            Jump();
        }
        if (Input.GetMouseButtonDown(0))
        {
            var args = new FireEventArgs
            {
                data = new FireEventData
                {
                    id = this.id,
                }
            };
            ws.Send(JsonUtility.ToJson(args));
            Fire();
        }
    }
    private void Jump()
    {
        GetComponent<Rigidbody>().AddForce(jumpForce, ForceMode.Impulse);
    }
}
