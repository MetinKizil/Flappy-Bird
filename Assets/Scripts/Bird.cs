using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Kuşun zıplaması, yerçekimiyle düşmesi, dönüşü ve çarpışma kontrolü.
/// Hem yeni Input System hem de eski Input Manager ile çalışır.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Bird : MonoBehaviour
{
    [Header("Hareket")]
    [SerializeField] private float jumpVelocity = 6f;
    [SerializeField] private float topLimitY = 4.8f;      // Ekranın üstünden kaçmasın

    [Header("Dönüş")]
    [SerializeField] private float maxUpAngle = 30f;
    [SerializeField] private float maxDownAngle = -90f;
    [SerializeField] private float rotationLerp = 10f;

    [Header("Başlangıç süzülmesi")]
    [SerializeField] private float idleBobAmplitude = 0.15f;
    [SerializeField] private float idleBobSpeed = 4f;

    private Rigidbody2D rb;
    private bool isDead;
    private Vector3 startPos;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
    }

    private void Update()
    {
        if (isDead) return;

        GameManager gm = GameManager.Instance;

        if (TapDetected())
        {
            if (gm.State == GameState.Ready) gm.StartGame();
            if (gm.State == GameState.Playing) Flap();
        }

        if (gm.State == GameState.Ready)
        {
            // timeScale = 0 olduğu için unscaledTime kullanılır
            float y = startPos.y + Mathf.Sin(Time.unscaledTime * idleBobSpeed) * idleBobAmplitude;
            transform.position = new Vector3(startPos.x, y, startPos.z);
            return;
        }

        // Ekran üst sınırı
        if (transform.position.y > topLimitY)
        {
            transform.position = new Vector3(transform.position.x, topLimitY, transform.position.z);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Min(0f, rb.linearVelocity.y));
        }

        // Hıza göre burun yukarı/aşağı
        float t = Mathf.InverseLerp(-jumpVelocity * 1.5f, jumpVelocity, rb.linearVelocity.y);
        float targetAngle = Mathf.Lerp(maxDownAngle, maxUpAngle, t);
        Quaternion targetRot = Quaternion.Euler(0f, 0f, targetAngle);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotationLerp * Time.deltaTime);
    }

    private void Flap()
    {
        rb.linearVelocity = new Vector2(0f, jumpVelocity);
    }

    private bool TapDetected()
    {
#if ENABLE_INPUT_SYSTEM
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) return true;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) return true;
        return false;
#else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) return true;
        return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space);
#endif
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Borular ve zemin normal (trigger olmayan) collider'dır
        if (isDead) return;
        isDead = true;
        GameManager.Instance.EndGame();
    }
}
