using System;
using System.Text;
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
    [SerializeField] private Text icon1Text;
    [SerializeField] private Text icon2Text;
    [SerializeField] private Text icon3Text;
    [SerializeField] private string livesFormat = "LIFE\n{0}";
    [SerializeField] private string lifeHeartSymbol = "♥";
    [SerializeField] private string lifeHeartSeparator = " ";
    [SerializeField] private string emptyLivesSymbol = "-";
    [SerializeField] private string stageNameFormat = "STAGE: {0}";
    [SerializeField] private string scoreFormat = "SCORE: {0}";
    [SerializeField] private Color acquiredIconColor = new Color(0.98f, 0.92f, 0.30f, 1f);
    [SerializeField] private Color lockedIconColor = new Color(0.86f, 0.93f, 1f, 0.58f);

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

    public void Configure(
        Text lives,
        Text stageName,
        Text score,
        Text clear,
        Text gameOver,
        Text icon1,
        Text icon2,
        Text icon3)
    {
        Configure(lives, stageName, score, clear, gameOver);
        ConfigureStageIconTexts(icon1, icon2, icon3);
    }

    public void ConfigureStageIconTexts(Text icon1, Text icon2, Text icon3)
    {
        icon1Text = icon1;
        icon2Text = icon2;
        icon3Text = icon3;
    }

    public void SetLives(int lives)
    {
        if (livesText != null)
        {
            livesText.text = BuildLivesText(lives);
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

    public void SetStageIcons(string icon1, bool got1, string icon2, bool got2, string icon3, bool got3)
    {
        SetStageIcon(icon1Text, icon1, got1);
        SetStageIcon(icon2Text, icon2, got2);
        SetStageIcon(icon3Text, icon3, got3);
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
        ShowClear(unlockedNextStage, isFinalStage, 0, destroyedBlocks, totalBlocks, lives, rank);
    }

    public void ShowClear(
        bool unlockedNextStage,
        bool isFinalStage,
        int score,
        int destroyedBlocks,
        int totalBlocks,
        int lives,
        string rank)
    {
        SetText(clearText, BuildClearMessage(
            unlockedNextStage,
            isFinalStage,
            score,
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
        ShowGameOver(0, destroyedBlocks, totalBlocks, lives, rank);
    }

    public void ShowGameOver(int score, int destroyedBlocks, int totalBlocks, int lives, string rank)
    {
        SetActive(clearText, false);
        SetText(gameOverText, BuildGameOverMessage(score, destroyedBlocks, totalBlocks, lives, rank));
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

    private void SetStageIcon(Text text, string label, bool acquired)
    {
        if (text == null)
        {
            return;
        }

        text.text = acquired ? $"[{GetSafeIconLabel(label)}]" : "[-]";
        text.color = acquired ? acquiredIconColor : lockedIconColor;
    }

    private string BuildLivesText(int lives)
    {
        string hearts = BuildLifeHearts(lives);
        if (string.IsNullOrEmpty(livesFormat))
        {
            return hearts;
        }

        try
        {
            return string.Format(livesFormat, hearts);
        }
        catch (FormatException)
        {
            return $"LIFE\n{hearts}";
        }
    }

    private string BuildLifeHearts(int lives)
    {
        int safeLives = Mathf.Max(0, lives);
        if (safeLives <= 0)
        {
            return string.IsNullOrEmpty(emptyLivesSymbol) ? "-" : emptyLivesSymbol;
        }

        string symbol = string.IsNullOrEmpty(lifeHeartSymbol) ? "*" : lifeHeartSymbol;
        string separator = lifeHeartSeparator ?? string.Empty;
        StringBuilder builder = new StringBuilder(symbol.Length * safeLives + separator.Length * Mathf.Max(0, safeLives - 1));

        for (int i = 0; i < safeLives; i++)
        {
            if (i > 0)
            {
                builder.Append(separator);
            }

            builder.Append(symbol);
        }

        return builder.ToString();
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
        return BuildClearMessage(unlockedNextStage, isFinalStage, 0, destroyedBlocks, totalBlocks, lives, rank);
    }

    private static string BuildClearMessage(
        bool unlockedNextStage,
        bool isFinalStage,
        int score,
        int destroyedBlocks,
        int totalBlocks,
        int lives,
        string rank)
    {
        return $"CLEAR!\n{BuildResultLines(score, destroyedBlocks, totalBlocks, lives, rank)}\n\n{BuildClearActionMessage(unlockedNextStage, isFinalStage)}";
    }

    private static string BuildGameOverMessage(int destroyedBlocks, int totalBlocks, int lives, string rank)
    {
        return BuildGameOverMessage(0, destroyedBlocks, totalBlocks, lives, rank);
    }

    private static string BuildGameOverMessage(int score, int destroyedBlocks, int totalBlocks, int lives, string rank)
    {
        return $"GAME OVER\n{BuildResultLines(score, destroyedBlocks, totalBlocks, lives, rank)}\n\nPress R to retry\nor return to Stage Select.";
    }

    private static string BuildResultLines(int destroyedBlocks, int totalBlocks, int lives, string rank)
    {
        return BuildResultLines(0, destroyedBlocks, totalBlocks, lives, rank);
    }

    private static string BuildResultLines(int score, int destroyedBlocks, int totalBlocks, int lives, string rank)
    {
        int safeScore = Mathf.Max(0, score);
        int safeTotalBlocks = Mathf.Max(0, totalBlocks);
        int safeDestroyedBlocks = Mathf.Clamp(destroyedBlocks, 0, Mathf.Max(safeTotalBlocks, destroyedBlocks));
        int safeLives = Mathf.Max(0, lives);
        string safeRank = string.IsNullOrWhiteSpace(rank) ? "-" : rank;

        return $"Score: {safeScore}\nBlocks: {safeDestroyedBlocks} / {safeTotalBlocks}\nLives: {safeLives}\nRank: {safeRank}";
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

    private static string GetSafeIconLabel(string label)
    {
        return string.IsNullOrWhiteSpace(label) ? "-" : label.Trim();
    }
}
