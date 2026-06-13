using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Block : MonoBehaviour
{
    [SerializeField] private bool countAsTarget = true;
    [SerializeField] private GameManager gameManager;

    private bool isBroken;

    private void Start()
    {
        if (gameManager == null)
        {
            gameManager = GameManager.Instance != null ? GameManager.Instance : FindObjectOfType<GameManager>();
        }
    }

    public void Initialize(GameManager manager)
    {
        gameManager = manager;
    }

    public void Break()
    {
        if (isBroken)
        {
            return;
        }

        isBroken = true;

        if (countAsTarget && gameManager != null)
        {
            gameManager.NotifyBlockDestroyed();
        }

        Destroy(gameObject);
    }
}
