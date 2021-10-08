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
    [SerializeField] public int scorePlayer;

    [SerializeField] public int scoreEarnByMatch;

    public delegate void TurnsUpdate(int amount);
    public TurnsUpdate updateTurnsAmount;

    public delegate void ScoreUpdate(int amount);
    public ScoreUpdate updateScoreAmount;

    public void DecreaseTurns()
    {
        amountTurns--;
        updateTurnsAmount?.Invoke(amountTurns);
    }

    public void IncreaceScore()
    {
        scorePlayer += scoreEarnByMatch;
        updateScoreAmount?.Invoke(scorePlayer);
    }

    private void Start()
    {
        updateTurnsAmount?.Invoke(amountTurns);
        updateScoreAmount?.Invoke(scorePlayer);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            IncreaceScore();
        }
    }
}
