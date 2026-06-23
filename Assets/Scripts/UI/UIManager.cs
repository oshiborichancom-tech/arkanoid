using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public const string ClearMessage = "CLEAR!\nStage Select: Back to Stage Select\nR: Restart";
    public const string GameOverMessage = "GAME OVER\nR: Restart\nStage Select: Back to Stage Select";

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
        SetActive(clearText, false);
        SetActive(gameOverText, false);
    }

    public void ShowClear()
    {
        SetText(clearText, ClearMessage);
        SetActive(clearText, true);
        SetActive(gameOverText, false);
    }

    public void ShowGameOver()
    {
        SetActive(clearText, false);
        SetText(gameOverText, GameOverMessage);
        SetActive(gameOverText, true);
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
