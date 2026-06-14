using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageDatabase", menuName = "Arkanoid/Stage Database")]
public class StageDatabase : ScriptableObject
{
    private static readonly StageData[] EmptyStages = new StageData[0];

    [SerializeField] private List<StageData> stages = new List<StageData>();

    public IReadOnlyList<StageData> Stages => stages != null ? stages : EmptyStages;
}
