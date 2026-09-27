using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Ready, Playing, GameOver }

/// <summary>
/// Oyunun durumunu (Hazır / Oynanıyor / Bitti), puanı, UI panellerini ve
/// Time.timeScale'i yönetir. Sahnede tek bir tane bulunmalıdır.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text bestScoreText;

    public GameState State { get; private set; }

    private int score;
    private const string BestScoreKey = "BestScore";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Application.targetFrameRate = 60; // Mobilde akıcı görüntü
    }

    private void Start()
    {
        State = GameState.Ready;
        score = 0;
        UpdateScoreUI();

        startPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        scoreText.gameObject.SetActive(false);

        Time.timeScale = 0f; // İlk dokunuşa kadar oyun donuk kalır
    }

    public void StartGame()
    {
        if (State != GameState.Ready) return;

        State = GameState.Playing;
        startPanel.SetActive(false);
        scoreText.gameObject.SetActive(true);
        Time.timeScale = 1f;
    }

    public void AddScore(int amount = 1)
    {
        if (State != GameState.Playing) return;
        score += amount;
        UpdateScoreUI();
    }

    public void EndGame()
    {
        if (State != GameState.Playing) return;

        State = GameState.GameOver;
        Time.timeScale = 0f;

        int best = PlayerPrefs.GetInt(BestScoreKey, 0);
        if (score > best)
        {
            best = score;
            PlayerPrefs.SetInt(BestScoreKey, best);
            PlayerPrefs.Save();
        }

        scoreText.gameObject.SetActive(false);
        finalScoreText.text = "Skor: " + score;
        bestScoreText.text = "En İyi: " + best;
        gameOverPanel.SetActive(true);
    }

    // "Tekrar Oyna" butonunun OnClick olayına bağlanır
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void UpdateScoreUI()
    {
        scoreText.text = score.ToString();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
