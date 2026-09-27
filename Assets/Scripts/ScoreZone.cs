using UnityEngine;

/// <summary>
/// İki borunun arasındaki görünmez tetikleyici. Kuş içinden geçince 1 puan verir.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ScoreZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            GameManager.Instance.AddScore(1);
    }
}
