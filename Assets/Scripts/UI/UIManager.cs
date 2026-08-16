using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public const string ClearMessage = "CLEAR!\nNext stage unlocked.\nPress R to retry\nor select another stage.";
    public const string FinalClearMessage = "CLEAR!\nAll stages cleared.\nPress R to retry\nor return to Stage Select.";
    public const string GameOverMessage = "GAME OVER\nPress R to retry\nor return to Stage Select.";
    private const string PerfectClearMessage = "All target icons achieved!";

    [SerializeField] private Text livesText;
    [SerializeField] private Text stageNameText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text clearText;
    [SerializeField] private Text gameOverText;
    [SerializeField] private Text clearResultTitleText;
    [SerializeField] private Text clearResultSubText;
    [SerializeField] private Text clearResultBodyText;
    [SerializeField] private Text gameOverResultTitleText;
    [SerializeField] private Text gameOverResultSubText;
    [SerializeField] private Text gameOverResultBodyText;
    [SerializeField] private Image clearResultBackground;
    [SerializeField] private Image gameOverResultBackground;
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
    [SerializeField] private Color acquiredIconColor = Color.white;
    [SerializeField] private Color lockedIconColor = new Color(1f, 0.84f, 0.96f, 0.78f);
    [SerializeField] private Color lockedIconFillColor = new Color(0.33f, 0.31f, 0.35f, 0.52f);
    [SerializeField] private Color clearIconFillColor = new Color(1f, 0.31f, 0.85f, 0.92f);
    [SerializeField] private Color noMissIconFillColor = new Color(0.39f, 0.96f, 1f, 0.92f);
    [SerializeField] private Color scoreIconFillColor = new Color(1f, 0.85f, 0.40f, 0.94f);
    [SerializeField] private Color acquiredIconBorderColor = new Color(1f, 0.84f, 0.96f, 0.98f);
    [SerializeField] private Color lockedIconBorderColor = new Color(0.33f, 0.31f, 0.35f, 0.82f);
    [SerializeField] private Color clearResultTitleColor = new Color(0.39f, 0.96f, 1f, 1f);
    [SerializeField] private Color perfectClearResultTitleColor = new Color(1f, 0.85f, 0.40f, 1f);
    [SerializeField] private Color gameOverResultTitleColor = new Color(1f, 0.36f, 0.66f, 1f);
    [SerializeField] private Color resultSubTextColor = new Color(1f, 0.31f, 0.85f, 1f);
    [SerializeField] private Color resultBodyTextColor = new Color(1f, 0.84f, 0.96f, 1f);

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

    public void ConfigureResultBackgrounds(Image clearBackground, Image gameOverBackground)
    {
        clearResultBackground = clearBackground;
        gameOverResultBackground = gameOverBackground;
    }

    public void ConfigureResultTexts(
        Text clearTitle,
        Text clearSub,
        Text clearBody,
        Text gameOverTitle,
        Text gameOverSub,
        Text gameOverBody)
    {
        clearResultTitleText = clearTitle;
        clearResultSubText = clearSub;
        clearResultBodyText = clearBody;
        gameOverResultTitleText = gameOverTitle;
        gameOverResultSubText = gameOverSub;
        gameOverResultBodyText = gameOverBody;
    }

    public void ConfigureLifeHearts(string heartSymbol, string separator = null, string emptySymbol = null)
    {
        if (!string.IsNullOrEmpty(heartSymbol))
        {
            lifeHeartSymbol = heartSymbol;
        }

        if (separator != null)
        {
            lifeHeartSeparator = separator;
        }

        if (!string.IsNullOrEmpty(emptySymbol))
        {
            emptyLivesSymbol = emptySymbol;
        }
    }

    public void ConfigureHudTextFormats(string lives, string stageName, string score)
    {
        if (lives != null)
        {
            livesFormat = lives;
        }

        if (stageName != null)
        {
            stageNameFormat = stageName;
        }

        if (score != null)
        {
            scoreFormat = score;
        }
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
        string body = BuildClearActionMessage(unlockedNextStage, isFinalStage);
        ShowClearResult(
            false,
            string.Empty,
            body,
            BuildClearMessage(unlockedNextStage, isFinalStage));
        SetGameOverResultActive(false);
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
        string body = BuildClearBody(
            unlockedNextStage,
            isFinalStage,
            score,
            destroyedBlocks,
            totalBlocks,
            lives,
            rank);
        ShowClearResult(
            false,
            string.Empty,
            body,
            BuildClearMessage(
                unlockedNextStage,
                isFinalStage,
                score,
                destroyedBlocks,
                totalBlocks,
                lives,
                rank));
        SetGameOverResultActive(false);
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
        bool isPerfectClear = achievedClearIcon && achievedNoMissIcon && achievedScoreIcon;
        ShowClear(
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
            achievedScoreIcon,
            isPerfectClear);
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
        bool achievedScoreIcon,
        bool isPerfectClear)
    {
        string body = BuildClearBody(
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
            achievedScoreIcon);
        ShowClearResult(
            isPerfectClear,
            isPerfectClear ? PerfectClearMessage : string.Empty,
            body,
            BuildClearMessage(
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
                achievedScoreIcon,
                isPerfectClear));
        SetGameOverResultActive(false);
    }

    public void ShowGameOver()
    {
        SetClearResultActive(false);
        ShowGameOverResult("Press R to retry\nor return to Stage Select.", GameOverMessage);
    }

    public void ShowGameOver(int destroyedBlocks, int totalBlocks, int lives, string rank)
    {
        ShowGameOver(0, destroyedBlocks, totalBlocks, lives, rank);
    }

    public void ShowGameOver(int score, int destroyedBlocks, int totalBlocks, int lives, string rank)
    {
        SetClearResultActive(false);
        ShowGameOverResult(
            BuildGameOverBody(score, destroyedBlocks, totalBlocks, lives, rank),
            BuildGameOverMessage(score, destroyedBlocks, totalBlocks, lives, rank));
    }

    public void HideResult()
    {
        SetClearResultActive(false);
        SetGameOverResultActive(false);
    }

    private static void SetText(Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }

    private static void SetText(Text text, string value, Color color)
    {
        if (text != null)
        {
            text.text = value;
            text.color = color;
        }
    }

    private static void SetActive(Text text, bool isActive)
    {
        if (text != null)
        {
            text.gameObject.SetActive(isActive);
        }
    }

    private static void SetActive(Image image, bool isActive)
    {
        if (image != null)
        {
            image.gameObject.SetActive(isActive);
        }
    }

    private void SetClearResultActive(bool isActive)
    {
        SetActive(clearResultBackground, isActive);
        SetActive(clearText, isActive);
        SetActive(clearResultTitleText, isActive);
        SetActive(clearResultSubText, isActive);
        SetActive(clearResultBodyText, isActive);
    }

    private void SetGameOverResultActive(bool isActive)
    {
        SetActive(gameOverResultBackground, isActive);
        SetActive(gameOverText, isActive);
        SetActive(gameOverResultTitleText, isActive);
        SetActive(gameOverResultSubText, isActive);
        SetActive(gameOverResultBodyText, isActive);
    }

    private void ShowClearResult(bool isPerfectClear, string subText, string bodyText, string legacyText)
    {
        Color titleColor = isPerfectClear ? perfectClearResultTitleColor : clearResultTitleColor;
        ShowResult(
            clearResultBackground,
            clearResultTitleText,
            clearResultSubText,
            clearResultBodyText,
            clearText,
            BuildClearHeading(isPerfectClear),
            subText,
            bodyText,
            legacyText,
            titleColor);
    }

    private void ShowGameOverResult(string bodyText, string legacyText)
    {
        ShowResult(
            gameOverResultBackground,
            gameOverResultTitleText,
            gameOverResultSubText,
            gameOverResultBodyText,
            gameOverText,
            "GAME OVER",
            string.Empty,
            bodyText,
            legacyText,
            gameOverResultTitleColor);
    }

    private void ShowResult(
        Image background,
        Text titleText,
        Text subText,
        Text bodyText,
        Text legacyText,
        string title,
        string subtitle,
        string body,
        string legacyBody,
        Color titleColor)
    {
        SetActive(background, true);

        bool hasSplitText = titleText != null && bodyText != null;
        if (hasSplitText)
        {
            SetActive(legacyText, false);
            SetText(titleText, title, titleColor);
            SetActive(titleText, true);

            bool hasSubtitle = !string.IsNullOrWhiteSpace(subtitle);
            SetText(subText, subtitle, resultSubTextColor);
            SetActive(subText, hasSubtitle);

            SetText(bodyText, body, resultBodyTextColor);
            SetActive(bodyText, true);
            return;
        }

        SetText(legacyText, legacyBody);
        SetActive(legacyText, true);
        SetActive(titleText, false);
        SetActive(subText, false);
        SetActive(bodyText, false);
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

        string symbol = string.IsNullOrEmpty(lifeHeartSymbol) ? "\u2665" : lifeHeartSymbol;
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
        return $"{BuildClearHeading(false)}\n{BuildClearBody(score, destroyedBlocks, totalBlocks, lives, rank, unlockedNextStage, isFinalStage)}";
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
        bool achievedScoreIcon,
        bool isPerfectClear)
    {
        string perfectLine = isPerfectClear ? $"\n{PerfectClearMessage}" : string.Empty;
        return $"{BuildClearHeading(isPerfectClear)}{perfectLine}\n{BuildClearBody(unlockedNextStage, isFinalStage, score, destroyedBlocks, totalBlocks, lives, rank, clearIconLabel, achievedClearIcon, noMissIconLabel, achievedNoMissIcon, scoreIconLabel, achievedScoreIcon)}";
    }

    private static string BuildClearHeading(bool isPerfectClear)
    {
        return isPerfectClear ? "PERFECT CLEAR!" : "CLEAR!";
    }

    private static string BuildGameOverMessage(int destroyedBlocks, int totalBlocks, int lives, string rank)
    {
        return BuildGameOverMessage(0, destroyedBlocks, totalBlocks, lives, rank);
    }

    private static string BuildGameOverMessage(int score, int destroyedBlocks, int totalBlocks, int lives, string rank)
    {
        return $"GAME OVER\n{BuildGameOverBody(score, destroyedBlocks, totalBlocks, lives, rank)}";
    }

    private static string BuildClearBody(
        bool unlockedNextStage,
        bool isFinalStage,
        int score,
        int destroyedBlocks,
        int totalBlocks,
        int lives,
        string rank)
    {
        return BuildClearBody(score, destroyedBlocks, totalBlocks, lives, rank, unlockedNextStage, isFinalStage);
    }

    private static string BuildClearBody(
        int score,
        int destroyedBlocks,
        int totalBlocks,
        int lives,
        string rank,
        bool unlockedNextStage,
        bool isFinalStage)
    {
        return $"{BuildResultLines(score, destroyedBlocks, totalBlocks, lives, rank)}\n\n{BuildClearActionMessage(unlockedNextStage, isFinalStage)}";
    }

    private static string BuildClearBody(
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

        return $"{BuildResultLines(score, destroyedBlocks, totalBlocks, lives, rank)}\n{achievedIconsLine}\n\n{BuildClearActionMessage(unlockedNextStage, isFinalStage)}";
    }

    private static string BuildGameOverBody(int score, int destroyedBlocks, int totalBlocks, int lives, string rank)
    {
        return $"{BuildResultLines(score, destroyedBlocks, totalBlocks, lives, rank)}\n\nPress R to retry\nor return to Stage Select.";
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
