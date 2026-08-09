using UnityEngine;
using UnityEngine.UI;

public class StageSelectButton : MonoBehaviour
{
    [SerializeField] private StageData stageData;
    [SerializeField] private bool isUnlocked = true;
    [SerializeField] private Text iconStatusText;
    [SerializeField] private Text perfectText;

    public void Configure(StageData data)
    {
        Configure(data, true);
    }

    public void Configure(StageData data, bool unlocked)
    {
        Configure(data, unlocked, iconStatusText, perfectText);
    }

    public void Configure(StageData data, bool unlocked, Text iconText)
    {
        Configure(data, unlocked, iconText, perfectText);
    }

    public void Configure(StageData data, bool unlocked, Text iconText, Text perfectLabel)
    {
        stageData = data;
        isUnlocked = unlocked;
        iconStatusText = iconText != null ? iconText : iconStatusText;
        perfectText = perfectLabel != null ? perfectLabel : perfectText;

        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.interactable = isUnlocked;
        }

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
        }

        Text perfectLabel = GetPerfectText();
        if (perfectLabel != null)
        {
            perfectLabel.text = showPerfect ? "PERFECT" : string.Empty;
        }
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

    private static string BuildIconStatus(string label, bool acquired)
    {
        return acquired ? $"[{GetSafeIconLabel(label)}]" : "[-]";
    }

    private static string GetSafeIconLabel(string label)
    {
        return string.IsNullOrWhiteSpace(label) ? "-" : label.Trim();
    }
}
