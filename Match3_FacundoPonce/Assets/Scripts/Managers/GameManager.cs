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

    [SerializeField] public float porcentTurnsDecreasePitch;

    int initialTurns;

    public delegate void TurnsUpdate(int amount);
    public TurnsUpdate updateTurnsAmount;

    public delegate void ScoreUpdate(int amount);
    public ScoreUpdate updateScoreAmount;

    public delegate void MatchEnded();
    public MatchEnded isMatchEnded;

    public delegate void ResetGrid();
    public ResetGrid resetGrid;

    bool pitchChanged;
    float targetPicth;
    float originalPitch;
    float actualPitch;

    PlayerController playerInput;


    private void Start()
    {
        pitchChanged = false;
        initialTurns = amountTurns;

        originalPitch = AudioManager.Instance.GetSoundPitch("TrackGameplay");
        targetPicth = originalPitch + 0.5f;
        actualPitch = originalPitch;

        updateTurnsAmount?.Invoke(amountTurns);
        updateScoreAmount?.Invoke(scorePlayer);

        AudioManager.Instance.Play("TrackGameplay");

        playerInput = FindObjectOfType<PlayerController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        IncreasePitchMusic();

        DecreasePitchMusic();
    }

    public void IncreasePitchMusic()
    {
        if (!pitchChanged)
        {
            if (amountTurns <= ((porcentTurnsDecreasePitch * initialTurns) / 100))
            {
                if (actualPitch < targetPicth)
                {
                    actualPitch += Time.deltaTime * 0.5f;
                    AudioManager.Instance.SetPitchSound(actualPitch, "TrackGameplay");
                }
                else
                {
                    actualPitch = targetPicth;
                    AudioManager.Instance.SetPitchSound(actualPitch, "TrackGameplay");
                    pitchChanged = true;
                }
            }
        }
    }

    public void DecreasePitchMusic()
    {
        if (amountTurns <= 0 && pitchChanged)
        {
            if (actualPitch > originalPitch)
            {
                actualPitch -= Time.deltaTime * 0.5f;
                AudioManager.Instance.SetPitchSound(actualPitch, "TrackGameplay");
            }
            else
            {
                actualPitch = originalPitch;
                AudioManager.Instance.SetPitchSound(originalPitch, "TrackGameplay");
            }
        }
    }

    public void BlockPlayerInteractions()
    {
        playerInput.BlockPlayerInteraction();
    }

    public void UnblockPlayerInteractions()
    {
        playerInput.UnblockPlayerInteraction();
    }

    public void ResetAllTheGrid()
    {
        resetGrid?.Invoke();
        playerInput.UnblockPlayerInteraction();
        PiecesManager.Instance.StopAllCoroutines();

        pitchChanged = false;
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
            playerInput.BlockPlayerInteraction();
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
