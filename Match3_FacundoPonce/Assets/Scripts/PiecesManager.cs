using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    private int indexLastPieceCreated =0;

    private void Start()
    {
        grid = gameObject.GetComponent<GridManager>();
        indicesPiecesToSpawm = new List<int>();

        indicesPrematches = new List<int>();
        preMatchesPieces = new List<PieceType>();

        piecesX = grid.amountPiecesX;
        piecesY = grid.amountPiecesY;

        CheckTypePiecesToSpawn();

        if(indicesPiecesToSpawm.Count > 2)
        {
            GeneratePiecesGrid();
            FilterHorizontalMatches();
        }
        else
        {
            GenerateGrid2Colors();
        }
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
        for (int i = 0; i < piecesX * piecesY; i++)
        {
            int pieceToSpawn = Random.Range(0, indicesPiecesToSpawm.Count);
            piecesOnGrid.Add(Instantiate(prefabsPieces[indicesPiecesToSpawm[pieceToSpawn]], grid.transform));
        }
    }

    public void FilterHorizontalMatches()
    {
        for (int i = 0; i < piecesOnGrid.Count; i++)
        {
            CheckRightRecur(i);

            if(CheckIfThereIsMatch())
            {
                Debug.Log("Match At " + piecesOnGrid[i].name + " INDEX="+ i);
                RemplaceWithNewPieces();
            }

            ClearPreMatches();
        }
    }

    public void GenerateGrid2Colors()
    {
        for (int i = 0; i < piecesY; i++)
        {
            for (int j = 0; j < piecesX; j++)
            {
                if (i % 2 == 0)
                {
                    if(j % 2 == 0)
                        piecesOnGrid.Add(Instantiate(prefabsPieces[indicesPiecesToSpawm[0]], grid.transform));
                    else
                        piecesOnGrid.Add(Instantiate(prefabsPieces[indicesPiecesToSpawm[1]], grid.transform));
                }
                else
                {
                    if (j % 2 != 0)
                        piecesOnGrid.Add(Instantiate(prefabsPieces[indicesPiecesToSpawm[0]], grid.transform));
                    else
                        piecesOnGrid.Add(Instantiate(prefabsPieces[indicesPiecesToSpawm[1]], grid.transform));
                }

            }
        }
    }

    public PieceType CreateDifferentPiece(PieceType.TypePiece piece)
    {
        PieceType pieceToCreate = null;
        int indexToEvade = 0;

        for (int i = 0; i < prefabsPieces.Count; i++)
        {
            if (prefabsPieces[i].spawnAviable)
            {
                if (prefabsPieces[i].pieceType == piece)
                {
                    indexToEvade = i;
                }
            }
        }
        int randIndex = 0;
        int randomPiece = 0;
        do
        {
            randIndex = Random.Range(0, indicesPiecesToSpawm.Count);

        } while (indicesPiecesToSpawm[randIndex] == indexToEvade || indicesPiecesToSpawm[randIndex] == indexLastPieceCreated);

        randomPiece = indicesPiecesToSpawm[randIndex];
        pieceToCreate = prefabsPieces[randomPiece];

        indexLastPieceCreated = randomPiece;

        return pieceToCreate;
    }

    public void RemplaceWithNewPieces()
    {
        for (int i = 0; i < preMatchesPieces.Count; i++)
        {
            if((i % 2) == 0)    //Rompe el match con una separacion par
            {
                PieceType newPiece = Instantiate(CreateDifferentPiece(preMatchesPieces[i].pieceType), grid.transform);

                piecesOnGrid.Insert(indicesPrematches[i], newPiece);
                piecesOnGrid.Remove(piecesOnGrid.Find(x => x == preMatchesPieces[i]));

                Destroy(preMatchesPieces[i].gameObject);
                //preMatchesPieces[i].gameObject.SetActive(false);
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
