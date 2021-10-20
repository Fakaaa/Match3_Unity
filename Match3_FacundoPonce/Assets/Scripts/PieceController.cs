using UnityEngine;
using UnityEngine.EventSystems;

public class PieceController : MonoBehaviour//, IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] Animator pieceAnimator;

    private PieceType piece;

    void Start()
    {
        piece = GetComponent<PieceType>();
    }

    //public void OnPointerDown(PointerEventData eventData)
    //{
    //    if (pieceAnimator == null)
    //        return;
        
    //    pieceAnimator.SetBool("Hover", false);
    //    pieceAnimator.SetBool("Drag", true);

    //    PiecesManager.Instance.StartChain(piece);
    //}

    //public void OnPointerEnter(PointerEventData eventData)
    //{
    //    if (pieceAnimator == null)
    //        return;

    //    if(!PiecesManager.Instance.IsChainStarting(piece))
    //    {            
    //        pieceAnimator.SetBool("Hover", true);
    //    }
    //    else
    //    {
    //        if(PiecesManager.Instance.AddPieceToChain(piece))
    //        {
    //            if (AudioManager.Instance != null)
    //                AudioManager.Instance.Play("Add");

    //            AddToChain();
    //        }
    //        else
    //        {
    //            if(PiecesManager.Instance.RemovePieceFromChain(piece))
    //            {
    //                if (AudioManager.Instance != null)
    //                    AudioManager.Instance.Play("Remove");
    //            }
    //        }
    //    }
    //}

    //public void OnPointerExit(PointerEventData eventData)
    //{
    //    if (pieceAnimator == null)
    //        return;

    //    pieceAnimator.SetBool("Hover", false);
    //}
    //public void OnPointerUp(PointerEventData eventData)
    //{
    //    if (PiecesManager.Instance.IsChainStarting(piece))
    //    {
    //        if (!PiecesManager.Instance.EndChain())
    //            pieceAnimator.SetBool("Drag", false);
    //    }
    //}
    //public void RemoveFromChain()
    //{
    //    if (pieceAnimator == null)
    //        return;

    //    pieceAnimator.SetBool("Hover", false);
    //    pieceAnimator.SetBool("Drag", false);
    //}

    //public void AddToChain()
    //{
    //    if (pieceAnimator == null)
    //        return;

    //    pieceAnimator.SetBool("Hover", false);
    //    pieceAnimator.SetBool("Drag", true);
    //}
}
