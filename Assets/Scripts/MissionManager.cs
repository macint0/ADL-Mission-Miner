using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    [Header("Timer")]
    public float missionTime = 300f;
    public TextMeshProUGUI timerText;

    [Header("Score")]
    public TextMeshProUGUI scoreText;
    public int   baseScore            = 50;
    public int   precisionBonus       = 25;
    public int   wrongItemPenalty     = 25;
    public float wrongItemTimePenalty = 5f;

    [Header("Mission UI")]
    public TextMeshProUGUI missionTitleText;
    public TextMeshProUGUI missionSubtitleText;
    public TextMeshProUGUI missionTagText;

    [Header("Refs")]
    public ItemSpawner itemSpawner;
    public GameObject  winScreen;
    public GameObject  loseScreen;

    float timeLeft;
    int   score;
    bool  running;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start() => StartStage();

    void StartStage()
    {
        timeLeft = missionTime;
        score    = 0;
        running  = true;

        UpdateTimerUI();
        UpdateScoreUI();

        if (missionTitleText)    missionTitleText.text    = "Daily Tasks";
        if (missionSubtitleText) missionSubtitleText.text = "Grab the glowing items!";
        if (missionTagText)      missionTagText.text      = "ADL MISSION";

        itemSpawner?.StartContinuousSpawning();
    }

    // Called by HookController when the hook returns with an item.
    public void EvaluateItem(CollectibleItem item, bool precisionHit)
    {
        if (!running) return;

        if (item.isTarget)
        {
            score += baseScore + (precisionHit ? precisionBonus : 0);
            UpdateScoreUI();
        }
        else
        {
            score    = Mathf.Max(0, score - wrongItemPenalty);
            timeLeft = Mathf.Max(0f, timeLeft - wrongItemTimePenalty);
            UpdateScoreUI();
        }
    }

    void Update()
    {
        if (!running) return;
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            running  = false;
            StartCoroutine(EndStage());
        }
        UpdateTimerUI();
    }

    IEnumerator EndStage()
    {
        running = false;
        itemSpawner?.StopSpawning();
        yield return new WaitForSeconds(0.5f);
        PlayerPrefs.SetInt("LastScore", score);
        if (loseScreen) loseScreen.SetActive(true);
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;
        int mins = Mathf.FloorToInt(timeLeft / 60f);
        int secs = Mathf.FloorToInt(timeLeft % 60f);
        timerText.text = $"{mins}:{secs:00}";
    }

    void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = score.ToString();
    }

    public void PlayAgain()    => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    public void GoToMainMenu() => SceneManager.LoadScene("MainMenu");
}
