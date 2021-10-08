using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PiecesManager : MonoBehaviour
{
    #region Singleton

    static PiecesManager instance;
    public static PiecesManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<PiecesManager>();

            return instance;
        }
    }

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
        }
        instance = this;
    }

    #endregion

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

    private Stack<PieceType> matchingPieces;
    public bool chainBegin;

    private void Start()
    {
        chainBegin = false;

        grid = gameObject.GetComponent<GridManager>();
        matchingPieces = new Stack<PieceType>();
        indicesPiecesToSpawm = new List<int>();

        indicesPrematches = new List<int>();
        preMatchesPieces = new List<PieceType>();

        piecesX = grid.amountPiecesX;
        piecesY = grid.amountPiecesY;

        CheckTypePiecesToSpawn();

        if(indicesPiecesToSpawm.Count > 2)
        {
            GeneratePiecesGrid();
            for(int i=0; i< 5; i++) //Cinco veces para asegurarnos
                FilterPreMatchesOnGrid();
        }
        else
        {
            GenerateGrid2Colors();
        }
    }

    #region Clear and Filter Pre-Mathces before starts gameplay

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

    public void FilterPreMatchesOnGrid()
    {
        //HORIZONTAL FILTER
        for (int i = 0; i < piecesOnGrid.Count; i++)
        {
            CheckHorizontalMatchRecursive(i);

            if(CheckIfThereIsPreMatch())
            {
                NewPiecesHorizontal();
            }

            ClearPreMatches();
        }

        //VERTICAL FILTER
        for (int i = 0; i < piecesOnGrid.Count; i++)
        {
            CheckVerticalMatchRecursive(i);

            if (CheckIfThereIsPreMatch())
            {
                NewPiecesVertical();
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

    public void CreatePieceFromCondition(bool condition, int iteration)
    {
        if (condition)
        {
            PieceType newPiece = Instantiate(CreateDifferentPiece(preMatchesPieces[iteration].pieceType), grid.transform);
            piecesOnGrid.Insert(indicesPrematches[iteration], newPiece);
            piecesOnGrid.Remove(piecesOnGrid.Find(pieceToFind => pieceToFind == preMatchesPieces[iteration]));
            Destroy(preMatchesPieces[iteration].gameObject);
        }
    }
    
    public void NewPiecesHorizontal()
    {
        for (int i = 0; i < preMatchesPieces.Count; i++)
        {
            CreatePieceFromCondition(((i % 2) == 0), i); //Rompe el match con una separacion par
        }
    }
    
    public void NewPiecesVertical()
    {
        for (int i = 0; i < preMatchesPieces.Count; i++)
        {
            CreatePieceFromCondition(((i % 2) != 0), i); //Rompe el match con una separacion inpar
        }
    }

    public bool CheckIfThereIsPreMatch()
    {
        if (preMatchesPieces.Count >= minMatchAmount)
            return true;
        else 
            return false;
    }

    public void CheckHorizontalMatchRecursive(int initialIteration)
    {
        if (piecesOnGrid[initialIteration] != null)
        {
            preMatchesPieces.Add(piecesOnGrid[initialIteration]);
            indicesPrematches.Add(initialIteration);

            if(initialIteration + 1 < piecesOnGrid.Count)//Horizontal
            {
                if (piecesOnGrid[initialIteration + 1] != null)
                {
                    if (piecesOnGrid[initialIteration + 1].pieceType == piecesOnGrid[initialIteration].pieceType)
                        CheckHorizontalMatchRecursive(initialIteration + 1);
                }
            }
        }
    }

    public void CheckVerticalMatchRecursive(int initialIteration)
    {
        if(piecesOnGrid[initialIteration] != null)
        {
            preMatchesPieces.Add(piecesOnGrid[initialIteration]);
            indicesPrematches.Add(initialIteration);

            if (initialIteration + piecesX < piecesOnGrid.Count)//Vertical
            {
                if (piecesOnGrid[initialIteration + piecesX] != null)
                {
                    if (piecesOnGrid[initialIteration + piecesX].pieceType == piecesOnGrid[initialIteration].pieceType)
                        CheckVerticalMatchRecursive(initialIteration + piecesX);
                }
            }
        }
    }

    public void ClearPreMatches()
    {
        preMatchesPieces.Clear();
    }
    
    #endregion


    public bool IsChainStarting()
    {
        if (chainBegin && matchingPieces.Count >= 1)
            return true;
        else 
            return false;
    }

    public void StartChain(PieceType piece)
    {
        if(!chainBegin)
        {
            chainBegin = true;
            matchingPieces.Push(piece);
        }
    }

    public bool AddPieceToChain(PieceType piece)
    {
        if (!matchingPieces.Contains(piece))
        {
            if (CheckPieceDirection(piece))
            {
                matchingPieces.Push(piece);
                return true;
            }
            else
            {
                if (matchingPieces.Count <= 1)
                {
                    Debug.Log("You are hovering a lonly piece.");
                    EndChain();
                    return false;
                }
                else
                {
                    matchingPieces.Push(piece);
                    return true;
                }
            }
        }
        else
        {
            matchingPieces.Pop();
            return false;
        }
    }

    public void RemovePieceFromChain(PieceType piece)
    {

    }

    public bool EndChain()
    {
        if(matchingPieces.Count >= minMatchAmount)
        {
            //Match!
            GameManager.Instance.DecreaseTurns();
            GameManager.Instance.IncreaceScoreMultipler(matchingPieces.Count);
            chainBegin = false;
            matchingPieces.Clear();

            return true;
        }
        else
        {
            //No Match!
            GameManager.Instance.DecreaseTurns();
            chainBegin = false;
            matchingPieces.Clear();
            
            return false;
        }
    }

    public bool CheckPieceDirection(PieceType piece)
    {
        PieceType pieceOnList = piecesOnGrid.Find(pieceFinded => pieceFinded == piece);

        if (pieceOnList == null)
            return false;
        else
        {
            int indexPieceOnList = piecesOnGrid.IndexOf(pieceOnList);

            if (MatchDirectionUp(indexPieceOnList) || MatchDirectionDown(indexPieceOnList) ||
                MatchDirectionLeft(indexPieceOnList) || MatchDirectionRight(indexPieceOnList) ||
                MatchDirectionUpRight(indexPieceOnList) || MatchDirectionUpLeft(indexPieceOnList) ||
                MatchDirectionDownRight(indexPieceOnList) || MatchDirectionDownLeft(indexPieceOnList))
                return true;
            else
                return false;
        }
    }
    public bool MatchDirectionUp(int index)
    {
        if (piecesOnGrid[index] != null)
        {
            if (index - piecesX > 0 && piecesOnGrid[index - piecesX] != null)
            {
                if (matchingPieces.Contains(piecesOnGrid[index - piecesX]) &&
                    piecesOnGrid[index].pieceType == piecesOnGrid[index - piecesX].pieceType)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
        else
            return false;
    }

    public bool MatchDirectionDown(int index)
    {
        if (piecesOnGrid[index] != null)
        {
            if (index + piecesX < piecesOnGrid.Count-1 && piecesOnGrid[index + piecesX] != null)
            {
                if (matchingPieces.Contains(piecesOnGrid[index + piecesX]) &&
                    piecesOnGrid[index].pieceType == piecesOnGrid[index + piecesX].pieceType)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
        else
            return false;
    }

    public bool MatchDirectionLeft(int index)
    {
        if (piecesOnGrid[index] != null)
        {
            if (index - 1 > 0 && piecesOnGrid[index - 1] != null)
            {
                if (matchingPieces.Contains(piecesOnGrid[index - 1]) &&
                    piecesOnGrid[index].pieceType == piecesOnGrid[index - 1].pieceType)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
        else
            return false;
    }
    
    public bool MatchDirectionRight(int index)
    {
        if (piecesOnGrid[index] != null)
        {
            if (index + 1 < piecesOnGrid.Count-1 && piecesOnGrid[index + 1] != null)
            {
                if (matchingPieces.Contains(piecesOnGrid[index + 1]) &&
                    piecesOnGrid[index].pieceType == piecesOnGrid[index + 1].pieceType)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
        else
            return false;
    }

    public bool MatchDirectionUpRight(int index)
    {
        if (piecesOnGrid[index] != null)
        {
            if (index - (piecesX + 1) > 0 && piecesOnGrid[index - (piecesX + 1)] != null)
            {
                if (matchingPieces.Contains(piecesOnGrid[index - (piecesX + 1)]) &&
                    piecesOnGrid[index].pieceType == piecesOnGrid[index - (piecesX + 1)].pieceType)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
        else
            return false;
    }

    public bool MatchDirectionUpLeft(int index)
    {
        if (piecesOnGrid[index] != null)
        {
            if (index - (piecesX - 1) > 0 && piecesOnGrid[index - (piecesX - 1)] != null)
            {
                if (matchingPieces.Contains(piecesOnGrid[index - (piecesX - 1)]) &&
                    piecesOnGrid[index].pieceType == piecesOnGrid[index - (piecesX - 1)].pieceType)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
        else
            return false;
    }

    public bool MatchDirectionDownRight(int index)
    {
        if (piecesOnGrid[index] != null)
        {
            if (index + (piecesX + 1) < piecesOnGrid.Count-1 && piecesOnGrid[index + (piecesX + 1)] != null)
            {
                if (matchingPieces.Contains(piecesOnGrid[index + (piecesX + 1)]) &&
                    piecesOnGrid[index].pieceType == piecesOnGrid[index + (piecesX + 1)].pieceType)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
        else
            return false;
    }
    
    public bool MatchDirectionDownLeft(int index)
    {
        if (piecesOnGrid[index] != null)
        {
            if (index + (piecesX - 1) < piecesOnGrid.Count-1 && piecesOnGrid[index + (piecesX - 1)] != null)
            {
                if (matchingPieces.Contains(piecesOnGrid[index + (piecesX - 1)]) &&
                    piecesOnGrid[index].pieceType == piecesOnGrid[index + (piecesX - 1)].pieceType)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
        else
            return false;
    }
}
