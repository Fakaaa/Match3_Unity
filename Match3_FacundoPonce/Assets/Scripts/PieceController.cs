using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PieceController : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    [SerializeField] Animator pieceAnimator;

    public void OnPointerClick(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (pieceAnimator == null)
            return;

        pieceAnimator.SetBool("Hover", true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (pieceAnimator == null)
            return;

        pieceAnimator.SetBool("Hover", false);
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
