using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayState : MonoBehaviour
{
    [SerializeField] CanvasGroup allGrid;
    [SerializeField] Animator gridAnimator;

    void Start()
    {
        if(GameManager.Instance != null)
        {
            GameManager.Instance.isMatchEnded += BlockAndBlendGrid;
            GameManager.Instance.resetGrid += RestoreGrid;
            GameManager.Instance.resetGrid += ResetGrid;
        }
    }

    public void BlockAndBlendGrid()
    {
        allGrid.alpha = 0.6f;
        allGrid.blocksRaycasts = false;
    }

    public void RestoreGrid()
    {
        allGrid.alpha = 1f;
        allGrid.blocksRaycasts = true;
    }

    public void ResetGrid()
    {
        gridAnimator.SetBool("ResetGrid", true);
    }
}
