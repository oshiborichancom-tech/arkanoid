using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public const string ClearMessage = "CLEAR!\nNext stage unlocked.\nPress R to retry\nor select another stage.";
    public const string FinalClearMessage = "CLEAR!\nAll stages cleared.\nPress R to retry\nor return to Stage Select.";
    public const string GameOverMessage = "GAME OVER\nPress R to retry\nor return to Stage Select.";

    [SerializeField] private Text livesText;
    [SerializeField] private Text stageNameText;
    [SerializeField] private Text clearText;
    [SerializeField] private Text gameOverText;
    [SerializeField] private string livesFormat = "Lives: {0}";

    public void Configure(Text lives, Text stageName, Text clear, Text gameOver)
    {
        livesText = lives;
        stageNameText = stageName;
        clearText = clear;
        gameOverText = gameOver;
    }

    public void SetLives(int lives)
    {
        if (livesText != null)
        {
            livesText.text = string.Format(livesFormat, lives);
        }
    }

    public void SetStageName(string stageName)
    {
        if (stageNameText != null)
        {
            stageNameText.text = stageName;
        }
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
        SetText(clearText, ClearMessage);
        if (isFinalStage)
        {
            SetText(clearText, FinalClearMessage);
        }
        else if (!unlockedNextStage)
        {
            SetText(clearText, "CLEAR!\nPress R to retry\nor return to Stage Select.");
        }

        SetActive(clearText, true);
        SetActive(gameOverText, false);
    }

    public void ShowGameOver()
    {
        SetActive(clearText, false);
        SetText(gameOverText, GameOverMessage);
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
}
