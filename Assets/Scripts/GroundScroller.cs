using UnityEngine;

/// <summary>
/// Zemin görselini sola kaydırıp belli bir mesafede başa sarar (sonsuz zemin hissi).
/// </summary>
public class GroundScroller : MonoBehaviour
{
    [SerializeField] private float speed = 2.5f;       // PipePair ile aynı hız
    [SerializeField] private float resetDistance = 1f; // Zemin deseninin tekrar genişliği

    private float startX;

    private void Start() => startX = transform.position.x;

    private void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;
        if (transform.position.x <= startX - resetDistance)
            transform.position += Vector3.right * resetDistance;
    }
}
