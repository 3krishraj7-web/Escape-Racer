using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameUI : MonoBehaviour
{
    [Header("Player Reference")]
    public PlayerCarController player;
    [Header("Resource Bars")]
    public Slider fuelSlider;
    public Slider armorSlider;
    [Header("Text Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI enemyTauntText;  
    public TextMeshProUGUI shieldTimerText; 
    public TextMeshProUGUI waveText;
    [Header("Score Settings")]
    public float scorePerSecond = 10f;
    private float score = 0;
    private bool gameOverShown = false;
    private float tauntTimer = 0f;
    private float tauntDuration = 3f;
    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.GetComponent<PlayerCarController>();
        }

        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);

        if (enemyTauntText != null)
            enemyTauntText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;
        UpdateBars();
        UpdateShieldTimer();
        UpdateTauntTimer();
        if (player.isAlive && !gameOverShown)
        {
            score += scorePerSecond * Time.deltaTime;
            UpdateScoreText();
        }
        else if (!player.isAlive && !gameOverShown)
        {
            ShowGameOver();
        }
        if (gameOverShown)
        {
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null &&
                UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame)
                RestartGame();
#else
            if (Input.GetKeyDown(KeyCode.R))
                RestartGame();
#endif
        }
    }

    void UpdateBars()
    {
        if (fuelSlider != null)
        {
            fuelSlider.value = player.fuel / player.maxFuel;
            Image fillImg = fuelSlider.fillRect.GetComponent<Image>();
            if (fillImg != null)
            {
                fillImg.color = player.fuel < 30f
                    ? Color.Lerp(Color.yellow, Color.red,
                        Mathf.PingPong(Time.time * 2, 1))
                    : Color.yellow;
            }
        }

        if (armorSlider != null)
        {
            armorSlider.value = player.armor / player.maxArmor;
            Image fillImg = armorSlider.fillRect.GetComponent<Image>();
            if (fillImg != null)
            {
                fillImg.color = player.armor < 30f
                    ? Color.red : Color.green;
            }
        }
    }

    void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    void UpdateShieldTimer()
    {
        if (shieldTimerText == null) return;
        if (player.isShielded)
        {
            shieldTimerText.gameObject.SetActive(true);
            shieldTimerText.text = "🛡 SHIELD: " +
                player.shieldTimeRemaining.ToString("F1") + "s";
            shieldTimerText.color = Color.cyan;
        }
        else
        {
            shieldTimerText.gameObject.SetActive(false);
        }
    }

    void UpdateTauntTimer()
    {
        if (tauntTimer > 0)
        {
            tauntTimer -= Time.deltaTime;
            if (tauntTimer <= 0 && enemyTauntText != null)
            {
                enemyTauntText.gameObject.SetActive(false);
            }
        }
    }

    public void ShowEnemyTaunt(string taunt)
    {
        if (enemyTauntText == null) return;
        enemyTauntText.gameObject.SetActive(true);
        enemyTauntText.text = "💀 " + taunt;
        tauntTimer = tauntDuration;
    }
    public void ShowWave(int waveNumber)
    {
        if (waveText != null)
            waveText.text = "WAVE " + waveNumber;
        StartCoroutine(HideWaveText());
    }

    System.Collections.IEnumerator HideWaveText()
    {
        yield return new WaitForSeconds(3f);
        if (waveText != null)
            waveText.text = "";
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = "SCORE: " + Mathf.RoundToInt(score);
    }

    public void ShowGameOver()
    {
        if (gameOverShown) return;
        gameOverShown = true;
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = "GAME OVER!\n" +
                               "Score: " + Mathf.RoundToInt(score) + "\n" +
                               "Press R to Restart";
        }

        UpdateScoreText();
    }

    public void AddScore(float points)
    {
        if (player != null && player.isAlive)
        {
            score += points;
            UpdateScoreText();
        }
    }

}