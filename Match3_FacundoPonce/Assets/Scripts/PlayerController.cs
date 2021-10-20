using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    public bool draging;

    private void Start()
    {
        Cursor.visible = false;
    }
    void Update()
    {
        UpdatePosition();

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            if(PiecesManager.Instance != null)
            {
                draging = false;
                PiecesManager.Instance.EndChain();
            }
        }
    }

    public void UpdatePosition()
    {
        transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 5));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TestController piece;
        if (collision.TryGetComponent<TestController>(out piece))
        {
            piece.HoverAndChainPieces(draging);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        TestController piece;
        if (collision.TryGetComponent<TestController>(out piece))
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                draging = true;
                piece.StartChainAndPressPiece();
            }
            if (!draging && PiecesManager.Instance.matchingPieces.Count <= 1)
                piece.RemoveFromChain();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        TestController piece;
        if (collision.TryGetComponent<TestController>(out piece))
        {
            piece.ExitPieceHover();
        }
    }
}
