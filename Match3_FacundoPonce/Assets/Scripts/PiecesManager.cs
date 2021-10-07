using System.Collections.Generic;
using UnityEngine;

public class PiecesManager : MonoBehaviour
{
    [Header("PIECES GENERATION")]
    [SerializeField] public List<PieceType> prefabsPieces;
    [SerializeField] public bool generateFirePieces;
    [SerializeField] public bool generateWaterPieces;
    [SerializeField] public bool generateAirPieces;
    [SerializeField] public bool generateEarthPieces;
    [SerializeField] public bool generateDarkPieces;
    [SerializeField] public bool generateLightPieces;
    [SerializeField] public bool generateIcePieces;
    [SerializeField] public bool generateLeafPieces;
    private List<int> indicesPiecesToSpawm;
    [Space(20)]
    [Header("PIECES CONFIG AND REFERENCES")]
    [SerializeField] public int minMatchAmount;
    [SerializeField] public List<PieceType> piecesOnGrid;

    private List<PieceType> preMatchesPieces;
    private List<int> indicesPrematches;

    private GridManager grid;
    private int piecesX;
    private int piecesY;

    private void Start()
    {
        grid = gameObject.GetComponent<GridManager>();
        indicesPiecesToSpawm = new List<int>();

        indicesPrematches = new List<int>();
        preMatchesPieces = new List<PieceType>();

        piecesX = grid.amountPiecesX;
        piecesY = grid.amountPiecesY;

        CheckTypePiecesToSpawn();
        GeneratePiecesGrid();
        FilterHorizontalMatches();
    }

    public void CheckTypePiecesToSpawn()
    {
        for (int i = 0; i < prefabsPieces.Count; i++)
        {
            switch (prefabsPieces[i].pieceType)
            {
                case PieceType.TypePiece.Fire:
                    CheckPieceType(generateFirePieces, i);
                    break;
                case PieceType.TypePiece.Water:
                    CheckPieceType(generateWaterPieces, i);
                    break;
                case PieceType.TypePiece.Earth:
                    CheckPieceType(generateEarthPieces, i);
                    break;
                case PieceType.TypePiece.Light:
                    CheckPieceType(generateLightPieces, i);
                    break;
                case PieceType.TypePiece.Dark:
                    CheckPieceType(generateDarkPieces, i);
                    break;
                case PieceType.TypePiece.Leaf:
                    CheckPieceType(generateLeafPieces, i);
                    break;
                case PieceType.TypePiece.Air:
                    CheckPieceType(generateAirPieces, i);
                    break;
                case PieceType.TypePiece.Ice:
                    CheckPieceType(generateIcePieces, i);
                    break;
            }
        }
    }

    public void CheckPieceType(bool typeCheck, int iteration)
    {
        if (typeCheck)
        {
            indicesPiecesToSpawm.Add(iteration);
            prefabsPieces[iteration].spawnAviable = true;
        }
        else
            prefabsPieces[iteration].spawnAviable = false;
    }

    public void GeneratePiecesGrid()
    {
        for (int i = 0; i < piecesX; i++)
        {
            for (int j = 0; j < piecesY; j++)
            {
                int pieceToSpawn = Random.Range(0, indicesPiecesToSpawm.Count);
                piecesOnGrid.Add(Instantiate(prefabsPieces[indicesPiecesToSpawm[pieceToSpawn]], grid.transform));
            }
        }
    }

    public void FilterHorizontalMatches()
    {
        for (int i = 0; i < piecesOnGrid.Count; i++)
        {
            CheckRightRecur(i);

            if(CheckIfThereIsMatch())
            {
                RemplaceWithNewPieces();
            }

            ClearPreMatches();
        }
    }

    public PieceType CreateDifferentPiece(PieceType.TypePiece piece)
    {
        PieceType pieceToCreate = null;
        for (int i = 0; i < prefabsPieces.Count; i++)
        {
            if(prefabsPieces[i].spawnAviable)
            {
                if(prefabsPieces[i].pieceType != piece)
                {
                    pieceToCreate = prefabsPieces[i];
                    break;
                }
            }
        }

        return pieceToCreate;
    }

    public void RemplaceWithNewPieces()
    {
        for (int i = 0; i < preMatchesPieces.Count; i++)
        {
            if((i % 2) == 0)    //Rompe el match con una separacion par
            {
                piecesOnGrid.RemoveAt(indicesPrematches[i]);
                piecesOnGrid.Insert(indicesPrematches[i], Instantiate(CreateDifferentPiece(preMatchesPieces[i].pieceType), preMatchesPieces[i].transform.position, Quaternion.identity));
                Destroy(preMatchesPieces[i]);
            }
        }
    }

    public bool CheckIfThereIsMatch()
    {
        if (preMatchesPieces.Count >= minMatchAmount)
            return true;
        else 
            return false;
    }

    //Derecha
    public void CheckRightRecur(int initialIteration)
    {
        if (piecesOnGrid[initialIteration] != null)
        {
            preMatchesPieces.Add(piecesOnGrid[initialIteration]);
            indicesPrematches.Add(initialIteration);

            if(initialIteration + 1 < piecesOnGrid.Count)
            {
                if (piecesOnGrid[initialIteration + 1] != null)
                {
                    if (piecesOnGrid[initialIteration + 1].pieceType == piecesOnGrid[initialIteration].pieceType)
                        CheckRightRecur(initialIteration + 1);
                    else
                        return;
                }
            }
        }
    }

    public void ClearPreMatches()
    {
        preMatchesPieces.Clear();
    }
}
