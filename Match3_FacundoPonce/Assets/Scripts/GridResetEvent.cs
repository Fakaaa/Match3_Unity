using UnityEngine;

public class GridResetEvent : MonoBehaviour
{
    [SerializeField] PiecesManager managerPieces;

    Animator gridAnim;

    private void Start()
    {
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
