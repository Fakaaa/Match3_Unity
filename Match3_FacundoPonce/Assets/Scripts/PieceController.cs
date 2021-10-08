using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PieceController : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] Animator pieceAnimator;

    private PieceType piece;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (pieceAnimator == null)
            return;
        
        pieceAnimator.SetBool("Hover", false);
        pieceAnimator.SetBool("Drag", true);

        PiecesManager.Instance.StartChain(piece);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (pieceAnimator == null)
            return;

        if(!PiecesManager.Instance.IsChainStarting())
            pieceAnimator.SetBool("Hover", true);
        else
        {
            if(PiecesManager.Instance.AddPieceToChain(piece))
            {
                AddToChain();
            }
            else
            {
                PiecesManager.Instance.RemovePieceFromChain(piece);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (pieceAnimator == null)
            return;

        pieceAnimator.SetBool("Hover", false);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (PiecesManager.Instance.IsChainStarting())
        {
            if (!PiecesManager.Instance.EndChain())
                pieceAnimator.SetBool("Drag", false);
        }
    }

    void Start()
    {
        piece = GetComponent<PieceType>();
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
