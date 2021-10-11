using System.Collections;
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
        if (instance != null)
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

    public bool piecesGenerated;
    public bool chainBegin;

    public bool automaticMatchingState;

    public int piecesCreated;

    List<PieceType> preMatchesPieces;
    List<Stack<PieceType>> automaticMatches;

    List<int> indicesPrematches;

    GridManager grid;
    GameplayState stateGrid;
    int piecesX;
    int piecesY;

    int indexLastPieceCreated = 0;

    public Stack<PieceType> matchingPieces;
    int[] indexDirectionsStackPeek;
    int[] indexDirectionsAutomaticMatch;
    int directions = 8;
    int directionsAutoMatch = 4;

    public delegate void CreateNewPoint(int actualIndex);
    public CreateNewPoint newPointLine;

    public delegate void RemovePoint();
    public RemovePoint removePointStack;

    public delegate void ClearLineRenderer();
    public ClearLineRenderer clearLineMatches;

    private void Start()
    {
        piecesCreated = 0;
        chainBegin = false;
        piecesGenerated = false;

        grid = gameObject.GetComponent<GridManager>();
        stateGrid = gameObject.GetComponentInParent<GameplayState>();
        matchingPieces = new Stack<PieceType>();
        indicesPiecesToSpawm = new List<int>();

        indicesPrematches = new List<int>();

        automaticMatches = new List<Stack<PieceType>>();

        preMatchesPieces = new List<PieceType>();

        indexDirectionsStackPeek = new int[directions];
        indexDirectionsAutomaticMatch = new int[directionsAutoMatch];

        piecesX = grid.amountPiecesX;
        piecesY = grid.amountPiecesY;

        CheckTypePiecesToSpawn();
    }

    private void Update()
    {
        if(!piecesGenerated)
        {
            if (indicesPiecesToSpawm.Count > 2)
            {
                StartCoroutine(GeneratePiecesCorutine());

                FilterPreMatchesOnGrid();
            }
            else
            {
                GenerateGrid2Colors();
            }
            piecesGenerated = true;
        }

        if(automaticMatchingState)
        {
            stateGrid.BlockGrid();

            for (int i = 0; i < piecesOnGrid.Count; i++)
            {
                Stack<PieceType> detectNewMatchHor = new Stack<PieceType>();
                CheckAutoMatchHorizontal(i, detectNewMatchHor);
                Stack<PieceType> detectNewMatchVert = new Stack<PieceType>();
                CheckAutoMatchVertical(i, detectNewMatchVert);
            }

            StartCoroutine(MakeAutomaticMatch());
            automaticMatchingState = false;
        }
    }

    public void CheckAutoMatchHorizontal(int index, Stack<PieceType> matchDetected)
    {
        if(piecesOnGrid[index] != null)
        {
            CalcHorValuesIndexDirectionStack(index);

            for (int i = 0; i < 2; i++)
            {
                if(index +1 < piecesOnGrid.Count-1)
                {
                    if (index+1 == indexDirectionsAutomaticMatch[i] &&
                        piecesOnGrid[index + 1].pieceType == piecesOnGrid[index].pieceType)
                    {
                        if(!matchDetected.Contains(piecesOnGrid[index]))
                            matchDetected.Push(piecesOnGrid[index]);
                        if (!matchDetected.Contains(piecesOnGrid[index+1]))
                            matchDetected.Push(piecesOnGrid[index+1]);

                        CheckAutoMatchHorizontal(index+1, matchDetected);
                    }
                    else
                    {
                        if(matchDetected.Count < minMatchAmount)
                            matchDetected.Clear();
                        else if(matchDetected.Count >= minMatchAmount)
                        {
                            if(!automaticMatches.Contains(matchDetected))
                                automaticMatches.Add(matchDetected);
                        }
                    }
                }
            }
        }
    }

    public void CheckAutoMatchVertical(int index, Stack<PieceType> matchDetected)
    {
        if (piecesOnGrid[index] != null)
        {
            CalcVertValuesIndexDirectionStack(index);

            for (int i = 2; i < 4; i++)
            {
                if (index + piecesX < piecesOnGrid.Count - 1)
                {
                    if (index + piecesX == indexDirectionsAutomaticMatch[i] &&
                        piecesOnGrid[index + piecesX].pieceType == piecesOnGrid[index].pieceType)
                    {
                        if (!matchDetected.Contains(piecesOnGrid[index]))
                            matchDetected.Push(piecesOnGrid[index]);
                        if (!matchDetected.Contains(piecesOnGrid[index + piecesX]))
                            matchDetected.Push(piecesOnGrid[index + piecesX]);

                        CheckAutoMatchVertical(index + piecesX, matchDetected);
                    }
                    else
                    {
                        if (matchDetected.Count < minMatchAmount)
                            matchDetected.Clear();
                        else if (matchDetected.Count >= minMatchAmount)
                        {
                            if (!automaticMatches.Contains(matchDetected))
                                automaticMatches.Add(matchDetected);
                        }
                    }
                }
            }
        }
    }

    public IEnumerator MakeAutomaticMatch()
    {
        yield return new WaitForSeconds(1f);

        if (GameManager.Instance.amountTurns <= 0)
        {
            automaticMatches.Clear();
            stateGrid.UnblockGrid();
            yield return null;
        }

        for (int i = 0; i < automaticMatches.Count; i++)
        {
            GameManager.Instance.IncreaceScoreMultipler(automaticMatches[i].Count, minMatchAmount);

            while (automaticMatches[i].Count > 0)
            {
                if(piecesOnGrid.Count <= piecesX * piecesY)
                    piecesOnGrid.Add(Instantiate(CreateDifferentPiece(automaticMatches[i].Peek().pieceType), grid.transform));
                
                piecesOnGrid.Remove(automaticMatches[i].Peek());

                if(automaticMatches[i].Peek() != null)
                {
                    Animator peekPieceAnim = automaticMatches[i].Peek().GetComponent<Animator>();
                    if (peekPieceAnim != null)
                        peekPieceAnim.SetBool("Destroy", true);
                }
                automaticMatches[i].Pop();

                yield return new WaitForEndOfFrame();
            }
        }

        automaticMatches.Clear();
        stateGrid.UnblockGrid();
        yield return null;
    }

    public IEnumerator GeneratePiecesCorutine()
    {
        while (piecesCreated < piecesX * piecesY)
        {
            int pieceToSpawn = Random.Range(0, indicesPiecesToSpawm.Count);
            piecesOnGrid.Add(Instantiate(prefabsPieces[indicesPiecesToSpawm[pieceToSpawn]], grid.transform));
            piecesCreated++;

            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    public void ResetTheGridPieces()
    {
        piecesCreated = 0;
        chainBegin = false;
        automaticMatchingState = false;

        StopAllCoroutines();

        for (int i = 0; i < piecesX * piecesY; i++)
        {
            Destroy(piecesOnGrid[i].gameObject);
        }

        piecesOnGrid.Clear();
        matchingPieces.Clear();
        automaticMatches.Clear();
        piecesGenerated = false;
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

            if (CheckIfThereIsPreMatch())
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
                    if (j % 2 == 0)
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
        if(indicesPiecesToSpawm.Count > 2)
        {
            do
            {
                randIndex = Random.Range(0, indicesPiecesToSpawm.Count);

            } while (indicesPiecesToSpawm[randIndex] == indexToEvade || indicesPiecesToSpawm[randIndex] == indexLastPieceCreated);
        }
        else
        {
            randIndex = Random.Range(0, indicesPiecesToSpawm.Count);
        }

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
            piecesOnGrid.Add(newPiece);
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

            if (initialIteration + 1 < piecesOnGrid.Count)//Horizontal
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
        if (piecesOnGrid[initialIteration] != null)
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
        if (!chainBegin)
        {
            chainBegin = true;
            matchingPieces.Push(piece);
            newPointLine?.Invoke(matchingPieces.Count);

            int indexPieceOnList = piecesOnGrid.IndexOf(piece);
            CalcValuesIndexDirectionPreChain(indexPieceOnList);
        }
    }

    public bool AddPieceToChain(PieceType piece)
    {
        if (!matchingPieces.Contains(piece))
        {
            if (CheckMatchWithStack(piece))
            {
                matchingPieces.Push(piece);
                newPointLine?.Invoke(matchingPieces.Count);
                return true;
            }
            else
            {
                return false;
            }
        }

        return false;
    }

    public bool RemovePieceFromChain(PieceType piece)
    {
        if (matchingPieces.Count <= 0)
            return false;

        PieceController peekPieceOnStack = matchingPieces.Peek().GetComponent<PieceController>();
        if (peekPieceOnStack == null || piece.gameObject.GetInstanceID() == peekPieceOnStack.gameObject.GetInstanceID())
            return false;

        if (matchingPieces.Contains(piece)) //Verifico si la pieza actual esta dentro del stack, si lo esta quito el ultimo push
        {
            peekPieceOnStack.RemoveFromChain();
            matchingPieces.Pop();
            removePointStack?.Invoke();
            return true;
        }
        return false;
    }

    public bool EndChain()
    {
        chainBegin = false;
        
        if (matchingPieces.Count >= minMatchAmount)
        {
            //Match!
            GameManager.Instance.DecreaseTurns();
            GameManager.Instance.IncreaceScoreMultipler(matchingPieces.Count, minMatchAmount);

            HandlePiecesAfterMatch();

            clearLineMatches?.Invoke();

            automaticMatchingState = true;

            return true;
        }
        else
        {
            //No Match!
            PieceController peekPieceOnStack = matchingPieces.Peek().GetComponent<PieceController>();
            int actualMatchCount = matchingPieces.Count;
            for (int i = 0; i < actualMatchCount; i++)
            {
                if(peekPieceOnStack != null)
                {
                    peekPieceOnStack = matchingPieces.Peek().GetComponent<PieceController>();
                    peekPieceOnStack.RemoveFromChain();
                    matchingPieces.Pop();
                    removePointStack?.Invoke();
                }
            }
            clearLineMatches?.Invoke();

            return false;
        }
    }

    void HandlePiecesAfterMatch()
    {
        int amountMatchPieces = matchingPieces.Count;
        for (int i = 0; i < amountMatchPieces; i++)
        {
            if(piecesOnGrid.Count <= piecesX * piecesY)
                piecesOnGrid.Add(Instantiate(CreateDifferentPiece(matchingPieces.Peek().pieceType), grid.transform));
            piecesOnGrid.Remove(matchingPieces.Peek());

            Animator peekPieceAnim = matchingPieces.Peek().GetComponent<Animator>();
            if (peekPieceAnim != null)
                peekPieceAnim.SetBool("Destroy", true);
            matchingPieces.Pop();
        }
    }

    public bool CheckMatchWithStack(PieceType piece)
    {
        PieceType pieceOutStack = piecesOnGrid.Find(pieceFinded => pieceFinded == piece);
        PieceType lastPieceStack = piecesOnGrid.Find(pieceStacked => pieceStacked == matchingPieces.Peek());

        if (pieceOutStack == null)
            return false;
        else
        {
            int indexPieceOutStack = piecesOnGrid.IndexOf(pieceOutStack);
            int indexLastPieceStack = piecesOnGrid.IndexOf(lastPieceStack);

            if(CalcIndicesLastPieceStaked(indexLastPieceStack, indexPieceOutStack))
                return true;
            else
                return false;
        }
    }

    public void CalcHorValuesIndexDirectionStack(int indexLastPieceStaked)
    {
        //Derecha
        SetValueDirection(0, indexLastPieceStaked + 1);
        //Izquierda
        SetValueDirection(1, indexLastPieceStaked - 1);
    }

    public void CalcVertValuesIndexDirectionStack(int indexLastPieceStaked)
    {
        //Abajo
        SetValueDirection(2, indexLastPieceStaked + piecesX);
        //Arriba
        SetValueDirection(3, indexLastPieceStaked - piecesX);
    }

    public void CalcValuesIndexDirectionPreChain(int indexLastPieceStaked)
    {
        //Derecha
        SetValueIndexDirection(0, indexLastPieceStaked + 1);
        //Izquierda
        SetValueIndexDirection(1, indexLastPieceStaked - 1);
        //Arriba
        SetValueIndexDirection(2, indexLastPieceStaked - piecesX);
        //Abajo
        SetValueIndexDirection(3, indexLastPieceStaked + piecesX);
        //Arriba-Der
        SetValueIndexDirection(4, indexLastPieceStaked - (piecesX + 1));
        //Arriba-Izq
        SetValueIndexDirection(5, indexLastPieceStaked - (piecesX - 1));
        //Abajo-Der
        SetValueIndexDirection(6, indexLastPieceStaked + (piecesX + 1));
        //Abajo-Izq
        SetValueIndexDirection(7, indexLastPieceStaked + (piecesX - 1));
    }

    public bool CalcIndicesLastPieceStaked(int indexLastPieceStaked, int indexPieceOutStack)
    {
        //Derecha
        SetValueIndexDirection( 0 ,indexLastPieceStaked + 1);
        //Izquierda
        SetValueIndexDirection( 1 ,indexLastPieceStaked - 1);
        //Arriba
        SetValueIndexDirection( 2 ,indexLastPieceStaked - piecesX);
        //Abajo
        SetValueIndexDirection( 3 ,indexLastPieceStaked + piecesX);
        //Arriba-Der
        SetValueIndexDirection( 4 ,indexLastPieceStaked - (piecesX+1));
        //Arriba-Izq
        SetValueIndexDirection( 5 ,indexLastPieceStaked - (piecesX-1));
        //Abajo-Der
        SetValueIndexDirection( 6 ,indexLastPieceStaked + (piecesX+1));
        //Abajo-Izq
        SetValueIndexDirection( 7 ,indexLastPieceStaked + (piecesX-1));


        for (int i = 0; i < directions; i++)
        {
            if (indexPieceOutStack >= 0 && indexPieceOutStack < piecesOnGrid.Count - 1)
            {
                if (indexPieceOutStack == indexDirectionsStackPeek[i] &&
                    piecesOnGrid[indexPieceOutStack].pieceType == piecesOnGrid[indexLastPieceStaked].pieceType)
                    return true;
            }
            else
                return false;
        }
        return false;
    }

    public void SetValueIndexDirection(int indexArray, int value)
    {
        if(value >= 0 && value < piecesOnGrid.Count -1)
            indexDirectionsStackPeek[indexArray] = value;
    }

    public void SetValueDirection(int indexArray, int value)
    {
        if (value >= 0 && value < piecesOnGrid.Count -1)
            indexDirectionsAutomaticMatch[indexArray] = value;
    }
}
