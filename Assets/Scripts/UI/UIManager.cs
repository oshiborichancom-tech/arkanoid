using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public const string ClearMessage = "CLEAR!\nNext stage unlocked.\nPress R to retry\nor select another stage.";
    public const string FinalClearMessage = "CLEAR!\nAll stages cleared.\nPress R to retry\nor return to Stage Select.";
    public const string GameOverMessage = "GAME OVER\nPress R to retry\nor return to Stage Select.";

    [SerializeField] private Text livesText;
    [SerializeField] private Text stageNameText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text clearText;
    [SerializeField] private Text gameOverText;
    [SerializeField] private string livesFormat = "LIFE: {0}";
    [SerializeField] private string stageNameFormat = "STAGE: {0}";
    [SerializeField] private string scoreFormat = "SCORE: {0}";

    public void Configure(Text lives, Text stageName, Text clear, Text gameOver)
    {
        Configure(lives, stageName, null, clear, gameOver);
    }

    public void Configure(Text lives, Text stageName, Text score, Text clear, Text gameOver)
    {
        livesText = lives;
        stageNameText = stageName;
        scoreText = score;
        clearText = clear;
        gameOverText = gameOver;
    }

    public void SetLives(int lives)
    {
        if (livesText != null)
        {
            livesText.text = string.Format(livesFormat, lives);
        }
    }

    public void SetStageName(string stageName)
    {
        if (stageNameText != null)
        {
            stageNameText.text = string.Format(stageNameFormat, stageName);
        }
    }

    public void SetScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = string.Format(scoreFormat, Mathf.Max(0, score));
        }
    }

    public void ShowPlaying()
    {
        HideResult();
    }

    public void ShowClear()
    {
        ShowClear(true, false);
    }

    public void ShowClear(bool unlockedNextStage, bool isFinalStage)
    {
        SetText(clearText, BuildClearMessage(unlockedNextStage, isFinalStage));
        SetActive(clearText, true);
        SetActive(gameOverText, false);
    }

    public void ShowClear(
        bool unlockedNextStage,
        bool isFinalStage,
        int destroyedBlocks,
        int totalBlocks,
        int lives,
        string rank)
    {
        SetText(clearText, BuildClearMessage(
            unlockedNextStage,
            isFinalStage,
            destroyedBlocks,
            totalBlocks,
            lives,
            rank));
        SetActive(clearText, true);
        SetActive(gameOverText, false);
    }

    public void ShowGameOver()
    {
        SetActive(clearText, false);
        SetText(gameOverText, GameOverMessage);
        SetActive(gameOverText, true);
    }

    public void ShowGameOver(int destroyedBlocks, int totalBlocks, int lives, string rank)
    {
        SetActive(clearText, false);
        SetText(gameOverText, BuildGameOverMessage(destroyedBlocks, totalBlocks, lives, rank));
        SetActive(gameOverText, true);
    }

    public void HideResult()
    {
        SetActive(clearText, false);
        SetActive(gameOverText, false);
    }

    private static void SetText(Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }

    private static void SetActive(Text text, bool isActive)
    {
        if (text != null)
        {
            text.gameObject.SetActive(isActive);
        }
    }

    private static string BuildClearMessage(bool unlockedNextStage, bool isFinalStage)
    {
        if (isFinalStage)
        {
            return FinalClearMessage;
        }

        if (!unlockedNextStage)
        {
            return "CLEAR!\nPress R to retry\nor return to Stage Select.";
        }

        return ClearMessage;
    }

    private static string BuildClearMessage(
        bool unlockedNextStage,
        bool isFinalStage,
        int destroyedBlocks,
        int totalBlocks,
        int lives,
        string rank)
    {
        return $"CLEAR!\n{BuildResultLines(destroyedBlocks, totalBlocks, lives, rank)}\n\n{BuildClearActionMessage(unlockedNextStage, isFinalStage)}";
    }

    private static string BuildGameOverMessage(int destroyedBlocks, int totalBlocks, int lives, string rank)
    {
        return $"GAME OVER\n{BuildResultLines(destroyedBlocks, totalBlocks, lives, rank)}\n\nPress R to retry\nor return to Stage Select.";
    }

    private static string BuildResultLines(int destroyedBlocks, int totalBlocks, int lives, string rank)
    {
        int safeTotalBlocks = Mathf.Max(0, totalBlocks);
        int safeDestroyedBlocks = Mathf.Clamp(destroyedBlocks, 0, Mathf.Max(safeTotalBlocks, destroyedBlocks));
        int safeLives = Mathf.Max(0, lives);
        string safeRank = string.IsNullOrWhiteSpace(rank) ? "-" : rank;

        return $"Blocks: {safeDestroyedBlocks} / {safeTotalBlocks}\nLives: {safeLives}\nRank: {safeRank}";
    }

    private static string BuildClearActionMessage(bool unlockedNextStage, bool isFinalStage)
    {
        if (isFinalStage)
        {
            return "All stages cleared.\nPress R to retry\nor return to Stage Select.";
        }

        if (unlockedNextStage)
        {
            return "Next stage unlocked.\nPress R to retry\nor select another stage.";
        }

        return "Press R to retry\nor return to Stage Select.";
    }
}
