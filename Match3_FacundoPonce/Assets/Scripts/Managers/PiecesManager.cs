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
    //[SerializeField] public List<PieceType> piecesOnGrid;

    public bool piecesGenerated;
    public bool chainBegin;

    public bool autoSortAndMatch;

    public int piecesCreated;

    public Transform piecesParent;

    //OLD
    //List<PieceType> preMatchesVertical;
    //List<PieceType> preMatchesHorizontal;
    //NEW
    List<NodeGrid> preMatchesVertical;
    List<NodeGrid> preMatchesHorizontal;


    List<Stack<PieceType>> automaticMatches;

    GridBehaviour grid;
    GridState stateGrid;
    int piecesX;
    int piecesY;

    int indexLastPieceCreated = 0;

    public Stack<PieceType> matchingPieces;

    //OLD
    int[] indexDirectionsStackPeek;
    //NEW
    Vector2[] indexDirectionsPieceStack;

    int[] indexDirectionsAutomaticMatch;
    int directions = 8;
    int directionsAutoMatch = 4;

    public delegate void CreateNewPoint(int actualIndex);
    public CreateNewPoint newPointLine;

    public delegate void RemovePoint();
    public RemovePoint removePointStack;

    public delegate void ClearLineRenderer();
    public ClearLineRenderer clearLineMatches;

    #region AUTO_SORT&MATCH

    NodeGrid nodeEmpty = null;
    int emptyNodesAmount;

    #endregion


    private void Start()
    {
        piecesCreated = 0;
        emptyNodesAmount = 0;
        chainBegin = false;
        piecesGenerated = false;

        grid = gameObject.GetComponent<GridBehaviour>();
        stateGrid = gameObject.GetComponent<GridState>();
        matchingPieces = new Stack<PieceType>();
        indicesPiecesToSpawm = new List<int>();

        automaticMatches = new List<Stack<PieceType>>();

        preMatchesVertical = new List<NodeGrid>();
        preMatchesHorizontal = new List<NodeGrid>();

        //OLD
        indexDirectionsStackPeek = new int[directions];
        indexDirectionsAutomaticMatch = new int[directionsAutoMatch];
        //NEW
        indexDirectionsPieceStack = new Vector2[directions];
        //nodeEmpty = new List<NodeGrid>();

        piecesX = grid.amountPiecesX;
        piecesY = grid.amountPiecesY;

        CheckTypePiecesToSpawn();
    }

    private void Update()
    {
        if (grid.amountNodesGenerated < piecesX * piecesY)
            return;

        if(!piecesGenerated)
        {
            if (indicesPiecesToSpawm.Count > 2)
            {
                StartCoroutine(GeneratePiecesCorutine());

                //FilterPreMatchesOnGrid();
            }
            else
            {
                GenerateGrid2Colors();
            }
            piecesGenerated = true;
        }

        //if (autoSortAndMatch)
        //{
        //    FindEmptyNodes();
        //}

        //if(automaticMatchingState)
        //{
        //    for (int i = 0; i < piecesOnGrid.Count; i++)
        //    {
        //        Stack<PieceType> detectNewMatchHor = new Stack<PieceType>();
        //        CheckAutoMatchHorizontal(i, detectNewMatchHor);
        //        Stack<PieceType> detectNewMatchVert = new Stack<PieceType>();
        //        CheckAutoMatchVertical(i, detectNewMatchVert);
        //    }
        //
        //    StartCoroutine(MakeAutomaticMatch());
        //    automaticMatchingState = false;
        //}
    }

    //public void CheckAutoMatchHorizontal(int index, Stack<PieceType> matchDetected)
    //{
    //    if(piecesOnGrid[index] != null)
    //    {
    //        CalcHorValuesIndexDirectionStack(index);

    //        for (int i = 0; i < 2; i++)
    //        {
    //            if(index +1 < piecesOnGrid.Count-1)
    //            {
    //                if (index+1 == indexDirectionsAutomaticMatch[i] &&
    //                    piecesOnGrid[index + 1].pieceType == piecesOnGrid[index].pieceType)
    //                {
    //                    if(!matchDetected.Contains(piecesOnGrid[index]))
    //                        matchDetected.Push(piecesOnGrid[index]);
    //                    if (!matchDetected.Contains(piecesOnGrid[index+1]))
    //                        matchDetected.Push(piecesOnGrid[index+1]);

    //                    CheckAutoMatchHorizontal(index+1, matchDetected);
    //                }
    //                else
    //                {
    //                    if(matchDetected.Count < minMatchAmount)
    //                        matchDetected.Clear();
    //                    else if(matchDetected.Count >= minMatchAmount)
    //                    {
    //                        if(!automaticMatches.Contains(matchDetected))
    //                            automaticMatches.Add(matchDetected);
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}

    //public void CheckAutoMatchVertical(int index, Stack<PieceType> matchDetected)
    //{
    //    if (piecesOnGrid[index] != null)
    //    {
    //        CalcVertValuesIndexDirectionStack(index);

    //        for (int i = 2; i < 4; i++)
    //        {
    //            if (index + piecesX < piecesOnGrid.Count - 1)
    //            {
    //                if (index + piecesX == indexDirectionsAutomaticMatch[i] &&
    //                    piecesOnGrid[index + piecesX].pieceType == piecesOnGrid[index].pieceType)
    //                {
    //                    if (!matchDetected.Contains(piecesOnGrid[index]))
    //                        matchDetected.Push(piecesOnGrid[index]);
    //                    if (!matchDetected.Contains(piecesOnGrid[index + piecesX]))
    //                        matchDetected.Push(piecesOnGrid[index + piecesX]);

    //                    CheckAutoMatchVertical(index + piecesX, matchDetected);
    //                }
    //                else
    //                {
    //                    if (matchDetected.Count < minMatchAmount)
    //                        matchDetected.Clear();
    //                    else if (matchDetected.Count >= minMatchAmount)
    //                    {
    //                        if (!automaticMatches.Contains(matchDetected))
    //                            automaticMatches.Add(matchDetected);
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}

    //public IEnumerator MakeAutomaticMatch()
    //{
    //    yield return new WaitForSeconds(1f);

    //    if (GameManager.Instance.amountTurns <= 0)
    //    {
    //        automaticMatches.Clear();
    //        //stateGrid.UnblockGrid();
    //        yield return null;
    //    }        

    //    if(automaticMatches.Count > 0)
    //    {
    //        //stateGrid.BlockGrid();
    //    }

    //    for (int i = 0; i < automaticMatches.Count; i++)
    //    {
    //        GameManager.Instance.IncreaceScoreMultipler(automaticMatches[i].Count, minMatchAmount);
    //        AudioManager.Instance.Play("Match");

    //        while (automaticMatches[i].Count > 0)
    //        {
    //            if(piecesOnGrid.Count <= piecesX * piecesY)
    //                piecesOnGrid.Add(Instantiate(CreateDifferentPiece(automaticMatches[i].Peek().pieceType), grid.transform));

    //            piecesOnGrid.Remove(automaticMatches[i].Peek());

    //            if(automaticMatches[i].Peek() != null)
    //            {
    //                Animator peekPieceAnim = automaticMatches[i].Peek().GetComponent<Animator>();
    //                if (peekPieceAnim != null)
    //                    peekPieceAnim.SetBool("Destroy", true);
    //            }
    //            automaticMatches[i].Pop();

    //            yield return new WaitForEndOfFrame();
    //        }
    //    }

    //    automaticMatches.Clear();
    //    yield return new WaitForSeconds(1f);
    //    //stateGrid.UnblockGrid();
    //    yield return null;
    //}

    IEnumerator GeneratePiecesCorutine()
    {
        for (int i = 0; i < piecesY; i++)
        {
            for (int j = 0; j < piecesX; j++)
            {
                int pieceToSpawn = Random.Range(0, indicesPiecesToSpawm.Count);
                if(grid.gridNodes[j, i] != null)
                {
                    PieceType pieceCreated = Instantiate(prefabsPieces[indicesPiecesToSpawm[pieceToSpawn]], grid.gridNodes[j, i].transform.position, Quaternion.identity, piecesParent);
                    grid.gridNodes[j, i].SetPieceOnNode(pieceCreated, j, i);
                    pieceCreated.myNode = grid.gridNodes[j, i];
                    //piecesOnGrid.Add(pieceCreated);
                }

                piecesCreated++;
                yield return new WaitForEndOfFrame();
            }
        }
        stateGrid.ShowGrid();
        yield return null;
    }

    public void ResetTheGridPieces()
    {
        piecesCreated = 0;
        chainBegin = false;
        autoSortAndMatch = false;

        StopAllCoroutines();

        for (int i = 0; i < piecesY; i++)
        {
            for (int j = 0; j < piecesX; j++)
            {
                if(grid.gridNodes[j,i] != null)
                {
                    if(grid.gridNodes[j, i].GetPiece() != null)
                        Destroy(grid.gridNodes[j, i].GetPiece().gameObject);
                }
            }
        }

        //piecesOnGrid.Clear();
        matchingPieces.Clear();
        automaticMatches.Clear();
        piecesGenerated = false;
    }

    #region Clear and Filter Pre-Mathces before starts gameplay

    void CheckTypePiecesToSpawn()
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

    void CheckPieceType(bool typeCheck, int iteration)
    {
        if (typeCheck)
        {
            indicesPiecesToSpawm.Add(iteration);
            prefabsPieces[iteration].spawnAviable = true;
        }
        else
            prefabsPieces[iteration].spawnAviable = false;
    }

    //public void GeneratePiecesGrid()
    //{
    //    for (int i = 0; i < piecesX * piecesY; i++)
    //    {
    //        int pieceToSpawn = Random.Range(0, indicesPiecesToSpawm.Count);
    //        piecesOnGrid.Add(Instantiate(prefabsPieces[indicesPiecesToSpawm[pieceToSpawn]], grid.transform));
    //    }
    //}

    void FilterPreMatchesOnGrid()
    {
        for (int i = 0; i < piecesY; i++)
        {
            for (int j = 0; j < piecesX; j++)
            {
                CheckVerticalMatchRecursive(i,j);
                CheckHorizontalMatchRecursive(j,i);
                
                if(CheckIfThereIsHorizontalPreMatch())
                    NewPiecesHorizontal();
            }

            if(CheckIfThereIsVerticalPreMatch())
                NewPiecesVertical();

            ClearPreMatches();
        }


        ////HORIZONTAL FILTER
        //for (int i = 0; i < piecesOnGrid.Count; i++)
        //{
        //    CheckHorizontalMatchRecursive(i);

        //    if (CheckIfThereIsPreMatch())
        //    {
        //        NewPiecesHorizontal();
        //    }

        //    ClearPreMatches();
        //}

        ////VERTICAL FILTER
        //for (int i = 0; i < piecesOnGrid.Count; i++)
        //{
        //    CheckVerticalMatchRecursive(i);

        //    if (CheckIfThereIsPreMatch())
        //    {
        //        NewPiecesVertical();
        //    }

        //    ClearPreMatches();
        //}
    }

    void GenerateGrid2Colors()
    {
        for (int i = 0; i < piecesY; i++)
        {
            for (int j = 0; j < piecesX; j++)
            {
                if (grid.gridNodes[j, i] == null)
                    continue;

                if (i % 2 == 0)
                {
                    if (j % 2 == 0)
                    {
                        PieceType piece = Instantiate(prefabsPieces[indicesPiecesToSpawm[0]], grid.gridNodes[j,i].transform.position, Quaternion.identity, piecesParent);
                        grid.gridNodes[j, i].SetPieceOnNode(piece, j, i);
                        piece.myNode = grid.gridNodes[j, i];
                        //piecesOnGrid.Add(piece);
                    }
                    else
                    {
                        PieceType piece = Instantiate(prefabsPieces[indicesPiecesToSpawm[1]], grid.gridNodes[j, i].transform.position, Quaternion.identity, piecesParent);
                        grid.gridNodes[j, i].SetPieceOnNode(piece, j, i);
                        piece.myNode = grid.gridNodes[j, i];
                        //piecesOnGrid.Add(piece);
                    }
                }
                else
                {
                    if (j % 2 != 0)
                    {
                        PieceType piece = Instantiate(prefabsPieces[indicesPiecesToSpawm[0]], grid.gridNodes[j, i].transform.position, Quaternion.identity, piecesParent);
                        grid.gridNodes[j, i].SetPieceOnNode(piece, j, i);
                        piece.myNode = grid.gridNodes[j, i];
                        //piecesOnGrid.Add(piece);
                    }
                    else
                    {
                        PieceType piece = Instantiate(prefabsPieces[indicesPiecesToSpawm[1]], grid.gridNodes[j, i].transform.position, Quaternion.identity, piecesParent);
                        grid.gridNodes[j, i].SetPieceOnNode(piece, j, i);
                        piece.myNode = grid.gridNodes[j, i];
                        //piecesOnGrid.Add(piece);
                    }
                }
            }
        }
        stateGrid.ShowGrid();
    }

    PieceType CreateDifferentPiece(PieceType.TypePiece piece)
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

    void CreatePieceFromCondition(bool condition, int iteration, List<NodeGrid> prematches)
    {
        if (condition)
        {
            PieceType newPiece = Instantiate(CreateDifferentPiece(prematches[iteration].GetPiece().pieceType), prematches[iteration].transform.position, Quaternion.identity, piecesParent);
            Destroy(prematches[iteration].GetPiece().gameObject);
            prematches[iteration].SetPieceOnNode(newPiece);
            newPiece.myNode = prematches[iteration];
            //piecesOnGrid.Add(newPiece);
            //piecesOnGrid.Remove(piecesOnGrid.Find(pieceToFind => pieceToFind == prematches[iteration]));
        }
    }

    void NewPiecesHorizontal()
    {
        for (int i = 0; i < preMatchesHorizontal.Count; i++)
        {
            CreatePieceFromCondition(((i % 2) == 0), i, preMatchesHorizontal); //Rompe el match con una separacion par
        }
    }

    void NewPiecesVertical()
    {
        for (int i = 0; i < preMatchesVertical.Count; i++)
        {
            CreatePieceFromCondition(((i % 2) != 0), i, preMatchesVertical); //Rompe el match con una separacion inpar
        }
    }

    bool CheckIfThereIsVerticalPreMatch()
    {
        if (preMatchesVertical.Count >= minMatchAmount)
            return true;
        else
            return false;
    }

    bool CheckIfThereIsHorizontalPreMatch()
    {
        if (preMatchesHorizontal.Count >= minMatchAmount)
            return true;
        else
            return false;
    }

    void CheckHorizontalMatchRecursive(int columnsIter, int rowsIter)
    {
        if(grid.gridNodes[columnsIter,rowsIter] != null)
        {
            preMatchesHorizontal.Add(grid.gridNodes[columnsIter, rowsIter]);

            if(columnsIter + 1 < piecesX && grid.gridNodes[columnsIter+1, rowsIter] != null)
            {
                if (grid.gridNodes[columnsIter + 1, rowsIter].GetPiece().pieceType ==
                    grid.gridNodes[columnsIter, rowsIter].GetPiece().pieceType)
                    CheckHorizontalMatchRecursive(columnsIter+1, rowsIter);
            }
        }
        //if (piecesOnGrid[columnsIter] != null)
        //{
        //    preMatchesPieces.Add(piecesOnGrid[columnsIter]);
        //    indicesPrematches.Add(columnsIter);

        //    if (columnsIter + 1 < piecesOnGrid.Count)//Horizontal
        //    {
        //        if (piecesOnGrid[columnsIter + 1] != null)
        //        {
        //            if (piecesOnGrid[columnsIter + 1].pieceType == piecesOnGrid[columnsIter].pieceType)
        //                CheckHorizontalMatchRecursive(columnsIter + 1);
        //        }
        //    }
        //}
    }

    void CheckVerticalMatchRecursive(int rowsIter, int columnsIter)
    {
        if(grid.gridNodes[columnsIter,rowsIter] != null)
        {
            preMatchesVertical.Add(grid.gridNodes[columnsIter, rowsIter]);

            if(rowsIter + 1 < piecesY && grid.gridNodes[columnsIter, rowsIter + 1] != null)
            {
                if (grid.gridNodes[columnsIter, rowsIter + 1].GetPiece().pieceType ==
                    grid.gridNodes[columnsIter, rowsIter].GetPiece().pieceType)
                    CheckVerticalMatchRecursive(rowsIter+1, columnsIter);
            }
        }
        //if (piecesOnGrid[rowsIter] != null)
        //{
        //    preMatchesPieces.Add(piecesOnGrid[rowsIter]);
        //    //indicesPrematches.Add(rowsIter);

        //    if (rowsIter + piecesX < piecesOnGrid.Count)//Vertical
        //    {
        //        if (piecesOnGrid[rowsIter + piecesX] != null)
        //        {
        //            if (piecesOnGrid[rowsIter + piecesX].pieceType == piecesOnGrid[rowsIter].pieceType)
        //                CheckVerticalMatchRecursive(rowsIter + piecesX);
        //        }
        //    }
        //}
    }

    void ClearPreMatches()
    {
        preMatchesVertical.Clear();
        preMatchesHorizontal.Clear();
    }

    #endregion


    public bool IsChainStarting(PieceType piece)
    {
        if (chainBegin && matchingPieces.Count >= 1)
        {
            if (CheckMatchWithStack(piece))
                return true;
            else
                return false;
        }
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

            SaveDirectionsPiece(piece.myNode);
            //int indexPieceOnList = piecesOnGrid.IndexOf(piece);
            //CalcValuesIndexDirectionPreChain(indexPieceOnList);
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

        TestController peekPieceOnStack = matchingPieces.Peek().GetComponent<TestController>();
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

            RemoveMatchedPieces();
            autoSortAndMatch = true;

            clearLineMatches?.Invoke();

            AudioManager.Instance.Play("Match");

            return true;
        }
        else
        {
            if (matchingPieces.Count >= 1)
            {
                //No Match!
                TestController peekPieceOnStack = matchingPieces.Peek().GetComponent<TestController>();
                int actualMatchCount = matchingPieces.Count;
                for (int i = 0; i < actualMatchCount; i++)
                {
                    if(peekPieceOnStack != null)
                    {
                        peekPieceOnStack = matchingPieces.Peek().GetComponent<TestController>();
                        peekPieceOnStack.RemoveFromChain();
                        matchingPieces.Pop();
                        removePointStack?.Invoke();
                    }
                }
                clearLineMatches?.Invoke();
            }
            return false;
        }
    }

    void RemoveMatchedPieces()
    {
        int amountMatchPieces = matchingPieces.Count;
        for (int i = 0; i < amountMatchPieces; i++)
        {
            Animator peekPieceAnim = matchingPieces.Peek().GetComponent<Animator>();
            if (peekPieceAnim != null)
                peekPieceAnim.SetBool("Destroy", true);

            Vector3 newPositionPiece = new Vector3(matchingPieces.Peek().myNode.transform.position.x, matchingPieces.Peek().myNode.transform.position.y + piecesY);
            PieceType piece = Instantiate(CreateDifferentPiece(matchingPieces.Peek().pieceType), newPositionPiece, Quaternion.identity, piecesParent);
            matchingPieces.Peek().myNode.SetPieceOnNode(null);
            matchingPieces.Pop();
        }
    }

    void FindEmptyNodes()
    {
       
    }

    void SortGrid()
    {
        
    }

    IEnumerator PlacePieceOnPosition(PieceType piece, Vector3 initialPosition, Vector3 targetPosition)
    {
        float timeToEnd = 0;

        while (piece.transform.position != targetPosition)
        {
            timeToEnd += Time.deltaTime;

            piece.transform.position = Vector3.Lerp(initialPosition, targetPosition, timeToEnd);

            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    bool CheckMatchWithStack(PieceType piece)
    {
        if (piece.myNode == null || piece == null)
            return false;
        else
        {
            if (CompareDirectionsPieceStaked(piece.myNode) && piece.pieceType == matchingPieces.Peek().pieceType)
                return true;
            else
                return false;
        }
        //PieceType pieceOutStack = piecesOnGrid.Find(pieceFinded => pieceFinded == piece);
        //PieceType lastPieceStack = piecesOnGrid.Find(pieceStacked => pieceStacked == matchingPieces.Peek());

        //if (pieceOutStack == null)
        //    return false;
        //else
        //{
        //    int indexPieceOutStack = piecesOnGrid.IndexOf(pieceOutStack);
        //    int indexLastPieceStack = piecesOnGrid.IndexOf(lastPieceStack);

        //    if(CalcIndicesLastPieceStaked(indexLastPieceStack, indexPieceOutStack))
        //        return true;
        //    else
        //        return false;
        //}
    }

    void CalcHorValuesIndexDirectionStack(int indexLastPieceStaked)
    {
        //Derecha
        SetValueDirection(0, indexLastPieceStaked + 1);
        //Izquierda
        SetValueDirection(1, indexLastPieceStaked - 1);
    }

    void CalcVertValuesIndexDirectionStack(int indexLastPieceStaked)
    {
        //Abajo
        SetValueDirection(2, indexLastPieceStaked + piecesX);
        //Arriba
        SetValueDirection(3, indexLastPieceStaked - piecesX);
    }

    void SaveDirectionsPiece(NodeGrid pieceNode)
    {
        //Arriba
        SetDirectionsNode(0, new Vector2(pieceNode.GetGridPos().x, pieceNode.GetGridPos().y - 1));
        //Abajo
        SetDirectionsNode(1, new Vector2(pieceNode.GetGridPos().x, pieceNode.GetGridPos().y + 1));
        //Derecha
        SetDirectionsNode(2, new Vector2(pieceNode.GetGridPos().x + 1, pieceNode.GetGridPos().y));
        //Izquierda
        SetDirectionsNode(3, new Vector2(pieceNode.GetGridPos().x - 1, pieceNode.GetGridPos().y));
        //Arriba-Der
        SetDirectionsNode(4, new Vector2(pieceNode.GetGridPos().x + 1, pieceNode.GetGridPos().y - 1));
        //Arriba-Izq
        SetDirectionsNode(5, new Vector2(pieceNode.GetGridPos().x - 1, pieceNode.GetGridPos().y - 1));
        //Abajo-Der
        SetDirectionsNode(6, new Vector2(pieceNode.GetGridPos().x + 1, pieceNode.GetGridPos().y + 1));
        //Abajo-Izq
        SetDirectionsNode(7, new Vector2(pieceNode.GetGridPos().x - 1, pieceNode.GetGridPos().y + 1));
    }

    bool CompareDirectionsPieceStaked(NodeGrid nodeOutStack)
    {
        if (nodeOutStack == null || matchingPieces.Peek().myNode == null)
            return false;

        SaveDirectionsPiece(matchingPieces.Peek().myNode);

        for (int i = 0; i < directions; i++)
        {
            if ((nodeOutStack.GetGridPos().x >= 0 || nodeOutStack.GetGridPos().x < piecesX) &&
               (nodeOutStack.GetGridPos().y >= 0 || nodeOutStack.GetGridPos().y < piecesY))
            {
                if (nodeOutStack.GetGridPos() == indexDirectionsPieceStack[i])
                    return true;
            }
            else
                return false;
        }
        return false;
    }

    void SetDirectionsNode(int directionIndex,Vector2 directionGrid)
    {
        if ((directionGrid.x >= 0 || directionGrid.x < piecesX) && (directionGrid.y >= 0 || directionGrid.y < piecesY))
            indexDirectionsPieceStack[directionIndex] = directionGrid;
    }

    //public void CalcValuesIndexDirectionPreChain(int indexLastPieceStaked)
    //{
    //    //Derecha
    //    SetValueIndexDirection(0, indexLastPieceStaked + 1);
    //    //Izquierda
    //    SetValueIndexDirection(1, indexLastPieceStaked - 1);
    //    //Arriba
    //    SetValueIndexDirection(2, indexLastPieceStaked - piecesX);
    //    //Abajo
    //    SetValueIndexDirection(3, indexLastPieceStaked + piecesX);
    //    //Arriba-Der
    //    SetValueIndexDirection(4, indexLastPieceStaked - (piecesX + 1));
    //    //Arriba-Izq
    //    SetValueIndexDirection(5, indexLastPieceStaked - (piecesX - 1));
    //    //Abajo-Der
    //    SetValueIndexDirection(6, indexLastPieceStaked + (piecesX + 1));
    //    //Abajo-Izq
    //    SetValueIndexDirection(7, indexLastPieceStaked + (piecesX - 1));
    //}

    //public bool CalcIndicesLastPieceStaked(int indexLastPieceStaked, int indexPieceOutStack)
    //{
    //    //Derecha
    //    SetValueIndexDirection( 0 ,indexLastPieceStaked + 1);
    //    //Izquierda
    //    SetValueIndexDirection( 1 ,indexLastPieceStaked - 1);
    //    //Arriba
    //    SetValueIndexDirection( 2 ,indexLastPieceStaked - piecesX);
    //    //Abajo
    //    SetValueIndexDirection( 3 ,indexLastPieceStaked + piecesX);
    //    //Arriba-Der
    //    SetValueIndexDirection( 4 ,indexLastPieceStaked - (piecesX+1));
    //    //Arriba-Izq
    //    SetValueIndexDirection( 5 ,indexLastPieceStaked - (piecesX-1));
    //    //Abajo-Der
    //    SetValueIndexDirection( 6 ,indexLastPieceStaked + (piecesX+1));
    //    //Abajo-Izq
    //    SetValueIndexDirection( 7 ,indexLastPieceStaked + (piecesX-1));


    //    for (int i = 0; i < directions; i++)
    //    {
    //        if (indexPieceOutStack >= 0 && indexPieceOutStack < piecesOnGrid.Count - 1)
    //        {
    //            if (indexPieceOutStack == indexDirectionsStackPeek[i] &&
    //                piecesOnGrid[indexPieceOutStack].pieceType == piecesOnGrid[indexLastPieceStaked].pieceType)
    //                return true;
    //        }
    //        else
    //            return false;
    //    }
    //    return false;
    //}

    //public void SetValueIndexDirection(int indexArray, int value)
    //{
    //    if(value >= 0 && value < piecesOnGrid.Count -1)
    //        indexDirectionsStackPeek[indexArray] = value;
    //}

    void SetValueDirection(int indexArray, int value)
    {
        //if (value >= 0 && value < piecesOnGrid.Count -1)
        //    indexDirectionsAutomaticMatch[indexArray] = value;
    }
}
