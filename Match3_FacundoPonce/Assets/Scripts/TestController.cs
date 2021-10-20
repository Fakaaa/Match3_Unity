using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestController : MonoBehaviour
{
    [SerializeField] Animator pieceAnimator;

    private PieceType piece;

    void Start()
    {
        piece = GetComponent<PieceType>();
    }

    public void ExitPieceHover()
    {
        if (pieceAnimator == null)
            return;

        pieceAnimator.SetBool("Hover", false);
    }

    public void EndChainAndRealeasePiece()
    {
        if (PiecesManager.Instance.IsChainStarting(piece))
        {
            if (!PiecesManager.Instance.EndChain())
                pieceAnimator.SetBool("Drag", false);
        }
    }

    public void StartChainAndPressPiece()
    {
        if (pieceAnimator == null || PiecesManager.Instance.chainBegin)
            return;

        pieceAnimator.SetBool("Hover", false);
        pieceAnimator.SetBool("Drag", true);

        PiecesManager.Instance.StartChain(piece);
    }

    public void HoverAndChainPieces(bool playerDraging)
    {
        if (pieceAnimator == null)
            return;

        if (!PiecesManager.Instance.IsChainStarting(piece) && !playerDraging)
        {
            pieceAnimator.SetBool("Hover", true);
        }
        else
        {
            if (PiecesManager.Instance.AddPieceToChain(piece))
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.Play("Add");

                AddToChain();
            }
            else
            {
                if (PiecesManager.Instance.RemovePieceFromChain(piece))
                {
                    if (AudioManager.Instance != null)
                        AudioManager.Instance.Play("Remove");
                }
            }
        }
    }
    public void RemoveFromChain()
    {
        if (pieceAnimator == null)
            return;

        pieceAnimator.SetBool("Hover", false);
        pieceAnimator.SetBool("Drag", false);
    }
    public void AddToChain()
    {
        if (pieceAnimator == null)
            return;

        pieceAnimator.SetBool("Hover", false);
        pieceAnimator.SetBool("Drag", true);
    }
}
