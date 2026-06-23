using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageDatabase", menuName = "Arkanoid/Stage Database")]
public class StageDatabase : ScriptableObject
{
    private static readonly StageData[] EmptyStages = new StageData[0];

    [SerializeField] private List<StageData> stages = new List<StageData>();

    public IReadOnlyList<StageData> Stages => stages != null ? stages : EmptyStages;

    public bool HasStageAfter(int stageId)
    {
        IReadOnlyList<StageData> currentStages = Stages;
        for (int i = 0; i < currentStages.Count; i++)
        {
            StageData stageData = currentStages[i];
            if (stageData != null && stageData.StageId > stageId)
            {
                return true;
            }
        }

        return false;
    }
}
