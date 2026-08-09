using UnityEngine;

public static class StageIconProgressManager
{
    public const string StageIconKeyPrefix = "Arkanoid_StageIcon_";
    public const int IconCount = 3;
    public const int ClearIconIndex = 1;
    public const int NoMissIconIndex = 2;
    public const int ScoreIconIndex = 3;

    private const int FirstStageId = 1;
    private const int ResetStageIdLimit = 100;

    public static bool IsIconAcquired(int stageId, int iconIndex)
    {
        return PlayerPrefs.GetInt(GetIconKey(stageId, iconIndex), 0) == 1;
    }

    public static bool SetIconAcquiredIfNeeded(int stageId, int iconIndex)
    {
        if (IsIconAcquired(stageId, iconIndex))
        {
            return false;
        }

        PlayerPrefs.SetInt(GetIconKey(stageId, iconIndex), 1);
        return true;
    }

    public static void SetIconAcquired(int stageId, int iconIndex, bool saveImmediately = true)
    {
        PlayerPrefs.SetInt(GetIconKey(stageId, iconIndex), 1);
        if (saveImmediately)
        {
            PlayerPrefs.Save();
        }
    }

    public static void ResetStageIcons(int stageId, bool saveImmediately = true)
    {
        int safeStageId = GetSafeStageId(stageId);
        for (int iconIndex = 1; iconIndex <= IconCount; iconIndex++)
        {
            PlayerPrefs.DeleteKey(GetIconKey(safeStageId, iconIndex));
        }

        if (saveImmediately)
        {
            PlayerPrefs.Save();
        }
    }

    public static void ResetAllKnownStageIcons(bool saveImmediately = true)
    {
        for (int stageId = FirstStageId; stageId <= ResetStageIdLimit; stageId++)
        {
            ResetStageIcons(stageId, false);
        }

        if (saveImmediately)
        {
            PlayerPrefs.Save();
        }
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }

    public static string GetIconKey(int stageId, int iconIndex)
    {
        return $"{StageIconKeyPrefix}{GetSafeStageId(stageId)}_{GetSafeIconIndex(iconIndex)}";
    }

    private static int GetSafeStageId(int stageId)
    {
        return Mathf.Max(FirstStageId, stageId);
    }

    private static int GetSafeIconIndex(int iconIndex)
    {
        return Mathf.Clamp(iconIndex, 1, IconCount);
    }
}
