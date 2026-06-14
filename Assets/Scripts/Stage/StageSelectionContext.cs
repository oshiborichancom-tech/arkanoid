using UnityEngine;

public static class StageSelectionContext
{
    public static StageData SelectedStageData { get; private set; }

    public static void SelectStage(StageData stageData)
    {
        SelectedStageData = stageData;
    }

    public static void Clear()
    {
        SelectedStageData = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnSubsystemRegistration()
    {
        Clear();
    }
}
