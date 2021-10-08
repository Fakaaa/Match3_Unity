using UnityEngine;
using TMPro;

public class UI_Gameplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI turnsRemaining;

    void Start()
    {
        if(GameManager.Instance != null)
            GameManager.Instance.updateTurnsAmount += UpdateTurns;
    }

    private void OnDisable()
    {
        if(GameManager.Instance != null)
            GameManager.Instance.updateTurnsAmount -= UpdateTurns;
    }

    public void UpdateTurns(int amount)
    {
        turnsRemaining.text = "Turns:" + amount.ToString();
    }
}
