using UnityEngine;

/// <summary>
/// Boru çiftini sola kaydırır; kameranın solundan çıkınca havuza geri gönderir.
/// PipePair prefab'ının kök objesine eklenir.
/// </summary>
public class PipePair : MonoBehaviour
{
    [SerializeField] private float speed = 2.5f;
    [SerializeField] private float offscreenMargin = 1.5f;

    private PipeSpawner spawner;
    private float despawnX;

    public void Init(PipeSpawner owner)
    {
        spawner = owner;
        // Kameranın sol kenarının biraz dışı
        despawnX = Camera.main.ViewportToWorldPoint(new Vector3(0f, 0.5f, 0f)).x - offscreenMargin;
    }

    private void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;

        if (transform.position.x < despawnX)
            spawner.ReturnToPool(this);
    }
}
