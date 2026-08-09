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
    [SerializeField] private Image icon1FillImage;
    [SerializeField] private Image icon2FillImage;
    [SerializeField] private Image icon3FillImage;
    [SerializeField] private Image icon1BorderImage;
    [SerializeField] private Image icon2BorderImage;
    [SerializeField] private Image icon3BorderImage;
    [SerializeField] private Text iconConditionText;
    [SerializeField] private string livesFormat = "LIFE\n{0}";
    [SerializeField] private string lifeHeartSymbol = "♥";
    [SerializeField] private string lifeHeartSeparator = " ";
    [SerializeField] private string emptyLivesSymbol = "-";
    [SerializeField] private string stageNameFormat = "STAGE: {0}";
    [SerializeField] private string scoreFormat = "SCORE: {0}";
    [SerializeField] private Color acquiredIconColor = new Color(0.04f, 0.08f, 0.13f, 1f);
    [SerializeField] private Color lockedIconColor = new Color(0.78f, 0.84f, 0.90f, 0.85f);
    [SerializeField] private Color lockedIconFillColor = new Color(0.38f, 0.42f, 0.48f, 0.42f);
    [SerializeField] private Color clearIconFillColor = new Color(0.63f, 0.82f, 1f, 0.92f);
    [SerializeField] private Color noMissIconFillColor = new Color(0.70f, 0.88f, 0.62f, 0.92f);
    [SerializeField] private Color scoreIconFillColor = new Color(1f, 0.88f, 0.42f, 0.92f);
    [SerializeField] private Color acquiredIconBorderColor = new Color(0.95f, 0.98f, 1f, 0.95f);
    [SerializeField] private Color lockedIconBorderColor = new Color(0.70f, 0.76f, 0.82f, 0.55f);

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

    public void ConfigureStageIconImages(
        Image icon1Fill,
        Image icon2Fill,
        Image icon3Fill,
        Image icon1Border,
        Image icon2Border,
        Image icon3Border,
        Text conditionText)
    {
        icon1FillImage = icon1Fill;
        icon2FillImage = icon2Fill;
        icon3FillImage = icon3Fill;
        icon1BorderImage = icon1Border;
        icon2BorderImage = icon2Border;
        icon3BorderImage = icon3Border;
        iconConditionText = conditionText;
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
        SetStageIcons(icon1, got1, icon2, got2, icon3, got3, 0);
    }

    public void SetStageIcons(string icon1, bool got1, string icon2, bool got2, string icon3, bool got3, int scoreTarget)
    {
        SetStageIcons(icon1, got1, "Clear", icon2, got2, "No Miss", icon3, got3, "Score", scoreTarget);
    }

    public void SetStageIcons(
        string icon1,
        bool got1,
        string icon1Description,
        string icon2,
        bool got2,
        string icon2Description,
        string icon3,
        bool got3,
        string icon3Description,
        int scoreTarget)
    {
        SetStageIcon(icon1Text, icon1FillImage, icon1BorderImage, icon1, got1, clearIconFillColor);
        SetStageIcon(icon2Text, icon2FillImage, icon2BorderImage, icon2, got2, noMissIconFillColor);
        SetStageIcon(icon3Text, icon3FillImage, icon3BorderImage, icon3, got3, scoreIconFillColor);
        SetIconConditionText(icon1, icon1Description, icon2, icon2Description, icon3, icon3Description, scoreTarget);
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

    public void ShowClear(
        bool unlockedNextStage,
        bool isFinalStage,
        int score,
        int destroyedBlocks,
        int totalBlocks,
        int lives,
        string rank,
        string clearIconLabel,
        bool achievedClearIcon,
        string noMissIconLabel,
        bool achievedNoMissIcon,
        string scoreIconLabel,
        bool achievedScoreIcon)
    {
        SetText(clearText, BuildClearMessage(
            unlockedNextStage,
            isFinalStage,
            score,
            destroyedBlocks,
            totalBlocks,
            lives,
            rank,
            clearIconLabel,
            achievedClearIcon,
            noMissIconLabel,
            achievedNoMissIcon,
            scoreIconLabel,
            achievedScoreIcon));
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

    private void SetStageIcon(Text text, Image fillImage, Image borderImage, string label, bool acquired, Color acquiredFillColor)
    {
        if (text != null)
        {
            text.text = acquired ? GetSafeIconLabel(label) : "-";
            text.color = acquired ? acquiredIconColor : lockedIconColor;
        }

        if (fillImage != null)
        {
            fillImage.color = acquired ? acquiredFillColor : lockedIconFillColor;
        }

        if (borderImage != null)
        {
            borderImage.color = acquired ? acquiredIconBorderColor : lockedIconBorderColor;
        }
    }

    private void SetIconConditionText(
        string icon1,
        string icon1Description,
        string icon2,
        string icon2Description,
        string icon3,
        string icon3Description,
        int scoreTarget)
    {
        if (iconConditionText == null)
        {
            return;
        }

        string clearLabel = GetSafeIconLabel(icon1);
        string clearDescription = GetSafeIconDescription(icon1Description, "Clear");
        string noMissLabel = GetSafeIconLabel(icon2);
        string noMissDescription = GetSafeIconDescription(icon2Description, "No Miss");
        string scoreLabel = GetSafeIconLabel(icon3);
        string scoreDescription = GetSafeIconDescription(icon3Description, "Score");
        int safeScoreTarget = Mathf.Max(0, scoreTarget);

        iconConditionText.text = $"{clearLabel}: {clearDescription}\n{noMissLabel}: {noMissDescription}\n{scoreLabel}: {scoreDescription} {safeScoreTarget}+";
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

    private static string BuildClearMessage(
        bool unlockedNextStage,
        bool isFinalStage,
        int score,
        int destroyedBlocks,
        int totalBlocks,
        int lives,
        string rank,
        string clearIconLabel,
        bool achievedClearIcon,
        string noMissIconLabel,
        bool achievedNoMissIcon,
        string scoreIconLabel,
        bool achievedScoreIcon)
    {
        string achievedIconsLine = BuildAchievedIconsLine(
            clearIconLabel,
            achievedClearIcon,
            noMissIconLabel,
            achievedNoMissIcon,
            scoreIconLabel,
            achievedScoreIcon);

        return $"CLEAR!\n{BuildResultLines(score, destroyedBlocks, totalBlocks, lives, rank)}\n{achievedIconsLine}\n\n{BuildClearActionMessage(unlockedNextStage, isFinalStage)}";
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

    private static string BuildAchievedIconsLine(
        string clearIconLabel,
        bool achievedClearIcon,
        string noMissIconLabel,
        bool achievedNoMissIcon,
        string scoreIconLabel,
        bool achievedScoreIcon)
    {
        return $"Icons: {BuildAchievedIconText(clearIconLabel, achievedClearIcon)} {BuildAchievedIconText(noMissIconLabel, achievedNoMissIcon)} {BuildAchievedIconText(scoreIconLabel, achievedScoreIcon)}";
    }

    private static string BuildAchievedIconText(string label, bool achieved)
    {
        return achieved ? $"[{GetSafeIconLabel(label)}]" : "[-]";
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

    private static string GetSafeIconDescription(string description, string fallback)
    {
        return string.IsNullOrWhiteSpace(description) ? fallback : description.Trim();
    }
}
