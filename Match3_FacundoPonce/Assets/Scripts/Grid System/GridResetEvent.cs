using UnityEngine;

public class GridResetEvent : MonoBehaviour
{
    PiecesManager managerPieces;
    Animator gridAnim;

    private void Start()
    {
        managerPieces = GetComponent<PiecesManager>();
        gridAnim = GetComponent<Animator>();
    }

    public void MakeGridReset()
    {
        if (managerPieces == null)
            return;

        managerPieces.ResetTheGridPieces();
        gridAnim.SetBool("ResetGrid", false);
    }
}
