using UnityEngine;
using TMPro;

public class UI_Gameplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI turnsRemaining;
    [SerializeField] TextMeshProUGUI amountPoints;

    void Start()
    {
        if(GameManager.Instance != null)
        {
            GameManager.Instance.updateTurnsAmount += UpdateTurns;
            GameManager.Instance.updateScoreAmount += UpdatePoints;
        }
    }

    private void OnDisable()
    {
        if(GameManager.Instance != null)
        {
            GameManager.Instance.updateTurnsAmount -= UpdateTurns;
            GameManager.Instance.updateScoreAmount -= UpdatePoints;
        }
    }

    public void UpdateTurns(int amount)
    {
        turnsRemaining.text = "Turns:" + amount.ToString();
    }

    public void UpdatePoints(int amount)
    {
        amountPoints.text = "Score:" + amount.ToString();
    }
}
