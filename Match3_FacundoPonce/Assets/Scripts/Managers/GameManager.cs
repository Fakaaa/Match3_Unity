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

    int initialTurns;

    public delegate void TurnsUpdate(int amount);
    public TurnsUpdate updateTurnsAmount;

    public delegate void ScoreUpdate(int amount);
    public ScoreUpdate updateScoreAmount;

    public delegate void MatchEnded();
    public MatchEnded isMatchEnded;

    public delegate void ResetGrid();
    public ResetGrid resetGrid;

    private void Start()
    {
        initialTurns = amountTurns;

        updateTurnsAmount?.Invoke(amountTurns);
        updateScoreAmount?.Invoke(scorePlayer);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void ResetAllTheGrid()
    {
        resetGrid?.Invoke();
        amountTurns = initialTurns;
        scorePlayer = 0;
        updateTurnsAmount?.Invoke(amountTurns);
        updateScoreAmount?.Invoke(scorePlayer);
    }

    public void DecreaseTurns()
    {
        amountTurns--;
        updateTurnsAmount?.Invoke(amountTurns);

        if(amountTurns <= 0)
        {
            amountTurns = 0;
            isMatchEnded?.Invoke();
        }
    }

    public void IncreaceScore()
    {
        scorePlayer += scoreEarnByMatch;
        updateScoreAmount?.Invoke(scorePlayer);
    }

    public void IncreaceScoreMultipler(int multiplerPieces, int minimumMatch)
    {
        if (multiplerPieces > minimumMatch)
            scorePlayer += scoreEarnByMatch * (multiplerPieces-minimumMatch);
        else
            scorePlayer += scoreEarnByMatch;

        updateScoreAmount?.Invoke(scorePlayer);
    }

}
