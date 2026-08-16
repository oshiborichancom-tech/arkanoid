using UnityEngine;
using UnityEngine.UI;

public class StageSelectButton : MonoBehaviour
{
    private static readonly Color CardUnlockedColor = new Color32(0x24, 0x10, 0x2F, 0xF2);
    private static readonly Color CardLockedColor = new Color32(0x30, 0x28, 0x38, 0xD8);
    private static readonly Color BorderUnlockedColor = new Color32(0xFF, 0x4F, 0xD8, 0xE6);
    private static readonly Color BorderLockedColor = new Color32(0x55, 0x50, 0x5A, 0xD8);
    private static readonly Color StageUnlockedTextColor = new Color32(0xFF, 0xD6, 0xF5, 0xFF);
    private static readonly Color StageLockedTextColor = new Color32(0x9A, 0x90, 0xA0, 0xFF);
    private static readonly Color IconUnlockedTextColor = Color.white;
    private static readonly Color IconLockedTextColor = new Color32(0x9A, 0x90, 0xA0, 0xFF);
    private static readonly Color PerfectTextColor = new Color32(0xFF, 0xD8, 0x66, 0xFF);

    [SerializeField] private StageData stageData;
    [SerializeField] private bool isUnlocked = true;
    [SerializeField] private Image cardBackgroundImage;
    [SerializeField] private Image cardBorderImage;
    [SerializeField] private Text stageNameText;
    [SerializeField] private Text lockedText;
    [SerializeField] private Text iconStatusText;
    [SerializeField] private Text perfectText;

    public void Configure(StageData data)
    {
        Configure(data, true);
    }

    public void Configure(StageData data, bool unlocked)
    {
        Configure(data, unlocked, cardBackgroundImage, cardBorderImage, stageNameText, lockedText, iconStatusText, perfectText);
    }

    public void Configure(StageData data, bool unlocked, Text iconText)
    {
        Configure(data, unlocked, cardBackgroundImage, cardBorderImage, stageNameText, lockedText, iconText, perfectText);
    }

    public void Configure(StageData data, bool unlocked, Text iconText, Text perfectLabel)
    {
        Configure(data, unlocked, cardBackgroundImage, cardBorderImage, stageNameText, lockedText, iconText, perfectLabel);
    }

    public void Configure(
        StageData data,
        bool unlocked,
        Image backgroundImage,
        Image borderImage,
        Text stageText,
        Text lockedLabel,
        Text iconText,
        Text perfectLabel)
    {
        stageData = data;
        isUnlocked = unlocked;
        cardBackgroundImage = backgroundImage != null ? backgroundImage : cardBackgroundImage;
        cardBorderImage = borderImage != null ? borderImage : cardBorderImage;
        stageNameText = stageText != null ? stageText : stageNameText;
        lockedText = lockedLabel != null ? lockedLabel : lockedText;
        iconStatusText = iconText != null ? iconText : iconStatusText;
        perfectText = perfectLabel != null ? perfectLabel : perfectText;

        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.interactable = isUnlocked;
        }

        ApplyCardStyle(false);
        RefreshIconStatus();
    }

    public void RefreshIconStatus()
    {
        if (stageData == null)
        {
            SetIconStatus("C", false, "N", false, "S", false);
            return;
        }

        int stageId = stageData.StageId;
        bool gotClear = StageIconProgressManager.IsIconAcquired(stageId, StageIconProgressManager.ClearIconIndex);
        bool gotNoMiss = StageIconProgressManager.IsIconAcquired(stageId, StageIconProgressManager.NoMissIconIndex);
        bool gotScore = StageIconProgressManager.IsIconAcquired(stageId, StageIconProgressManager.ScoreIconIndex);

        SetIconStatus(
            stageData.Icon1Label,
            gotClear,
            stageData.Icon2Label,
            gotNoMiss,
            stageData.Icon3Label,
            gotScore,
            isUnlocked && gotClear && gotNoMiss && gotScore);
    }

    public void SetIconStatus(string icon1, bool got1, string icon2, bool got2, string icon3, bool got3)
    {
        SetIconStatus(icon1, got1, icon2, got2, icon3, got3, isUnlocked && got1 && got2 && got3);
    }

    public void SetIconStatus(string icon1, bool got1, string icon2, bool got2, string icon3, bool got3, bool showPerfect)
    {
        Text statusText = GetIconStatusText();
        if (statusText != null)
        {
            statusText.text = $"{BuildIconStatus(icon1, got1)} {BuildIconStatus(icon2, got2)} {BuildIconStatus(icon3, got3)}";
            statusText.color = isUnlocked ? IconUnlockedTextColor : IconLockedTextColor;
        }

        Text lockedLabel = GetLockedText();
        if (lockedLabel != null)
        {
            lockedLabel.text = isUnlocked ? string.Empty : "LOCKED";
            lockedLabel.color = StageLockedTextColor;
        }

        Text perfectLabel = GetPerfectText();
        if (perfectLabel != null)
        {
            perfectLabel.text = showPerfect ? "PERFECT" : string.Empty;
            perfectLabel.color = PerfectTextColor;
        }

        ApplyCardStyle(showPerfect);
    }

    public void SelectStageAndLoadGame()
    {
        if (!isUnlocked)
        {
            string lockedStageName = stageData != null ? stageData.StageName : "Unknown";
            Debug.LogWarning($"Stage is locked: {lockedStageName}");
            return;
        }

        if (stageData == null)
        {
            StageSelectionContext.Clear();
            Debug.LogWarning("StageSelectButton has no StageData. Loading GameScene with the GameScene Bootstrap fallback.");
        }
        else
        {
            StageSelectionContext.SelectStage(stageData);
        }

        SceneLoader.LoadScene(SceneLoader.GameSceneName);
    }

    private Image GetCardBackgroundImage()
    {
        if (cardBackgroundImage != null)
        {
            return cardBackgroundImage;
        }

        cardBackgroundImage = GetComponent<Image>();
        return cardBackgroundImage;
    }

    private Image GetCardBorderImage()
    {
        if (cardBorderImage != null)
        {
            return cardBorderImage;
        }

        Transform found = transform.Find("CardBorder");
        if (found != null)
        {
            cardBorderImage = found.GetComponent<Image>();
        }

        return cardBorderImage;
    }

    private Text GetStageNameText()
    {
        if (stageNameText != null)
        {
            return stageNameText;
        }

        Transform found = transform.Find("Text");
        if (found != null)
        {
            stageNameText = found.GetComponent<Text>();
        }

        return stageNameText;
    }

    private Text GetLockedText()
    {
        if (lockedText != null)
        {
            return lockedText;
        }

        Transform found = transform.Find("LockedText");
        if (found != null)
        {
            lockedText = found.GetComponent<Text>();
        }

        return lockedText;
    }

    private Text GetIconStatusText()
    {
        if (iconStatusText != null)
        {
            return iconStatusText;
        }

        Transform found = transform.Find("IconStatusText");
        if (found != null)
        {
            iconStatusText = found.GetComponent<Text>();
        }

        return iconStatusText;
    }

    private Text GetPerfectText()
    {
        if (perfectText != null)
        {
            return perfectText;
        }

        Transform found = transform.Find("PerfectText");
        if (found != null)
        {
            perfectText = found.GetComponent<Text>();
        }

        return perfectText;
    }

    private void ApplyCardStyle(bool showPerfect)
    {
        Image background = GetCardBackgroundImage();
        if (background != null)
        {
            background.color = isUnlocked ? CardUnlockedColor : CardLockedColor;
        }

        Image border = GetCardBorderImage();
        if (border != null)
        {
            border.color = showPerfect ? PerfectTextColor : isUnlocked ? BorderUnlockedColor : BorderLockedColor;
        }

        Text stageText = GetStageNameText();
        if (stageText != null)
        {
            stageText.color = isUnlocked ? StageUnlockedTextColor : StageLockedTextColor;
        }
    }

    private static string BuildIconStatus(string label, bool acquired)
    {
        return acquired ? $"[{GetSafeIconLabel(label)}]" : "[-]";
    }

    private static string GetSafeIconLabel(string label)
    {
        return string.IsNullOrWhiteSpace(label) ? "-" : label.Trim();
    }
}
