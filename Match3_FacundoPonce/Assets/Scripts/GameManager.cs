using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton

    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<GameManager>();

            return instance;
        }
    }
    public void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #endregion

    [SerializeField] public int amountTurns;
    [SerializeField] public int pointsPlayer;

    public delegate void TurnsUpdate(int amount);
    public TurnsUpdate updateTurnsAmount;

    public void DecreaseTurns()
    {
        amountTurns--;
        updateTurnsAmount?.Invoke(amountTurns);
    }

    private void Start()
    {
        updateTurnsAmount?.Invoke(amountTurns);
    }
}
