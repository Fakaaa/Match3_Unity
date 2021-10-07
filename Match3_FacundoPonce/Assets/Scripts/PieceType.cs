using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceType : MonoBehaviour
{
    [SerializeField]
    public enum TypePiece { Fire,Water,Earth,Light,Dark,Leaf,Air,Ice}
    public TypePiece pieceType;
    [HideInInspector] public bool spawnAviable;
}
