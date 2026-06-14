using UnityEngine;
using UnityEngine.UI;

public class StageSelectButton : MonoBehaviour
{
    [SerializeField] private StageData stageData;
    [SerializeField] private bool isUnlocked = true;

    public void Configure(StageData data)
    {
        Configure(data, true);
    }

    public void Configure(StageData data, bool unlocked)
    {
        stageData = data;
        isUnlocked = unlocked;

        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.interactable = isUnlocked;
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
            Debug.Log($"Selected stage: {stageData.StageName}");
        }

        SceneLoader.LoadScene(SceneLoader.GameSceneName);
    }
}
