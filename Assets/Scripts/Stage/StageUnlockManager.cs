using UnityEngine;

public static class StageUnlockManager
{
    public const string StageUnlockKeyPrefix = "Arkanoid.StageUnlocked.";
    private const int FirstStageId = 1;
    private const int ResetStageIdLimit = 100;

    public static bool IsStageUnlocked(int stageId)
    {
        int safeStageId = Mathf.Max(FirstStageId, stageId);
        if (safeStageId == FirstStageId)
        {
            return true;
        }

        return PlayerPrefs.GetInt(GetStageUnlockKey(safeStageId), 0) == 1;
    }

    public static void UnlockStage(int stageId)
    {
        int safeStageId = Mathf.Max(FirstStageId, stageId);
        if (safeStageId == FirstStageId || IsStageUnlocked(safeStageId))
        {
            return;
        }

        PlayerPrefs.SetInt(GetStageUnlockKey(safeStageId), 1);
        PlayerPrefs.Save();
        Debug.Log($"Stage unlocked: {safeStageId}");
    }

    public static void UnlockNextStage(int clearedStageId)
    {
        UnlockStage(Mathf.Max(FirstStageId, clearedStageId) + 1);
    }

    public static void ResetProgress()
    {
        for (int stageId = FirstStageId + 1; stageId <= ResetStageIdLimit; stageId++)
        {
            PlayerPrefs.DeleteKey(GetStageUnlockKey(stageId));
        }

        PlayerPrefs.Save();
        Debug.Log("Stage progress reset. Stage 1 remains unlocked.");
    }

    private static string GetStageUnlockKey(int stageId)
    {
        return $"{StageUnlockKeyPrefix}{stageId}";
    }
}
