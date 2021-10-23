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
    [SerializeField] float speedVerticalFall;
    //[SerializeField] public List<PieceType> piecesOnGrid;

    public bool piecesGenerated;
    public int piecesCreated;
    public bool chainBegin;

    public Transform piecesParent;

    List<NodeGrid> preMatchesVertical;
    List<NodeGrid> preMatchesHorizontal;


    List<Stack<NodeGrid>> automaticMatches;

    GridBehaviour grid;
    GridState stateGrid;
    int piecesX;
    int piecesY;

    int indexLastPieceCreated = 0;

    public Stack<PieceType> matchingPieces;

    Vector2[] indexDirectionsPieceStack;

    Vector2[] indexDirectionsAutomaticMatch;
    public bool canMakeAutoMatch;
    public bool onMatchingProcess;
    public bool autoSortGrid;
    public int maxCallsToMakeAutoMatch;
    public int callsOfMakeAutoMatch;
    public int flagScoreIncrement;
    int flagCheckForAutoMatches;

    int directions = 8;
    int directionsAutoMatch = 4;

    public delegate void CreateNewPoint(int actualIndex);
    public CreateNewPoint newPointLine;

    public delegate void RemovePoint();
    public RemovePoint removePointStack;

    public delegate void ClearLineRenderer();
    public ClearLineRenderer clearLineMatches;

    #region AUTO_SORT

    List<NodeGrid> nodesEmpty;
    public bool createPiecesAfterSort;

    #endregion

    private void Start()
    {
        piecesCreated = 0;
        maxCallsToMakeAutoMatch = 1;
        callsOfMakeAutoMatch = 0;
        flagScoreIncrement = 0;
        flagCheckForAutoMatches = 0;
        chainBegin = false;
        piecesGenerated = false;
        canMakeAutoMatch = false;
        onMatchingProcess = false;

        grid = gameObject.GetComponent<GridBehaviour>();
        stateGrid = gameObject.GetComponent<GridState>();
        matchingPieces = new Stack<PieceType>();
        indicesPiecesToSpawm = new List<int>();

        automaticMatches = new List<Stack<NodeGrid>>();

        preMatchesVertical = new List<NodeGrid>();
        preMatchesHorizontal = new List<NodeGrid>();

        nodesEmpty = new List<NodeGrid>();

        //OLD
        indexDirectionsAutomaticMatch = new Vector2[directionsAutoMatch];
        //NEW
        indexDirectionsPieceStack = new Vector2[directions];

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
                if(indicesPiecesToSpawm.Count == 2)
                    GenerateGrid2Colors();
                else if(indicesPiecesToSpawm.Count != 0)
                {
                    int randomPrefPiece=0;
                    do
                    {
                        randomPrefPiece = Random.Range(0, prefabsPieces.Count);
                    } while (randomPrefPiece == indicesPiecesToSpawm[0]);

                    indicesPiecesToSpawm.Add(randomPrefPiece);
                    prefabsPieces[randomPrefPiece].spawnAviable = true;

                    GenerateGrid2Colors();
                }
            }
            piecesGenerated = true;
        }

        if (canMakeAutoMatch)
        {
            MakeAutomaticMatch();

            if (!onMatchingProcess)
            {
                StartCoroutine(DelayToSortGrid());
                canMakeAutoMatch = false;
            }
        }
        else
        {
            if(flagCheckForAutoMatches == 0)
            {
                CheckForAutoMatchesOnGrid();
                flagCheckForAutoMatches = 1;
            }
        }

        if (autoSortGrid)
        {
            FindEmptyNodes();

            FillFirstRowWithPieces();
        }
    }

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
        StopAllCoroutines();

        piecesCreated = 0;
        callsOfMakeAutoMatch = 0;

        chainBegin = false;
        autoSortGrid = false;
        onMatchingProcess = false;
        canMakeAutoMatch = false;

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

        RemoveTrashPiecesAutoMatched();

        matchingPieces.Clear();
        automaticMatches.Clear();
        piecesGenerated = false;
    }

    void RemoveTrashPiecesAutoMatched()
    {
        for (int i = 0; i < automaticMatches.Count; i++)
        {
            while (automaticMatches[i].Count > 0)
            {
                if (automaticMatches[i] != null)
                {
                    if (automaticMatches[i].Peek().GetPiece() != null)
                    {
                        Destroy(automaticMatches[i].Peek().GetPiece().gameObject);
                        automaticMatches[i].Pop();
                    }
                    else
                    {
                        automaticMatches[i].Pop();
                    }
                }
            }
        }
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

    void FilterPreMatchesOnGrid()
    {
        //for (int i = 0; i < piecesY; i++)
        //{
        //    for (int j = 0; j < piecesX; j++)
        //    {
        //        CheckVerticalMatchRecursive(i,j);

        //        if(CheckIfThereIsVerticalPreMatch())
        //        {
        //            NewPiecesVertical();
        //        }

        //        ClearPreMatches();
        //    }
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
                    }
                    else
                    {
                        PieceType piece = Instantiate(prefabsPieces[indicesPiecesToSpawm[1]], grid.gridNodes[j, i].transform.position, Quaternion.identity, piecesParent);
                        grid.gridNodes[j, i].SetPieceOnNode(piece, j, i);
                        piece.myNode = grid.gridNodes[j, i];
                    }
                }
                else
                {
                    if (j % 2 != 0)
                    {
                        PieceType piece = Instantiate(prefabsPieces[indicesPiecesToSpawm[0]], grid.gridNodes[j, i].transform.position, Quaternion.identity, piecesParent);
                        grid.gridNodes[j, i].SetPieceOnNode(piece, j, i);
                        piece.myNode = grid.gridNodes[j, i];
                    }
                    else
                    {
                        PieceType piece = Instantiate(prefabsPieces[indicesPiecesToSpawm[1]], grid.gridNodes[j, i].transform.position, Quaternion.identity, piecesParent);
                        grid.gridNodes[j, i].SetPieceOnNode(piece, j, i);
                        piece.myNode = grid.gridNodes[j, i];
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
        
    }

    void CheckVerticalMatchRecursive(int rowsIter, int columnsIter)
    {
        
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
            StopAllCoroutines();
            
            chainBegin = true;
            matchingPieces.Push(piece);
            newPointLine?.Invoke(matchingPieces.Count);

            SaveDirectionsPiece(piece.myNode);
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

            StartCoroutine(DelayToSortGrid());

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

    IEnumerator DelayToSortGrid()
    {
        yield return new WaitForSeconds(0.6f);

        //StopCoroutine(MakeAutoMatch());
        GameManager.Instance.BlockPlayerInteractions();
        autoSortGrid = true;

        yield return null;
    }

    void RemoveMatchedPieces()
    {
        int amountMatchPieces = matchingPieces.Count;
        for (int i = 0; i < amountMatchPieces; i++)
        {
            Animator peekPieceAnim = matchingPieces.Peek().GetComponent<Animator>();
            if (peekPieceAnim != null)
                peekPieceAnim.SetBool("Destroy", true);

            nodesEmpty.Add(matchingPieces.Peek().myNode);
            matchingPieces.Peek().myNode.SetPieceOnNode(null);
            matchingPieces.Pop();
        }
    }

    void CheckForAutoMatchesOnGrid()
    {
        if (autoSortGrid || onMatchingProcess)
            return;

        for (int i = 0; i < piecesY; i++)
        {
            for (int j = 0; j < piecesX; j++)
            {
                if (!onMatchingProcess)
                {
                    Stack<NodeGrid> mathcesHorizontal = new Stack<NodeGrid>();
                    CheckAutoMatchHorizontal(grid.gridNodes[j, i], mathcesHorizontal);
                }
                else
                    return;
            }
        }
    }

    void MakeAutomaticMatch()
    {
        if (automaticMatches.Count <= 0)
            return;

        if(callsOfMakeAutoMatch < maxCallsToMakeAutoMatch)
            StartCoroutine(MakeAutoMatch());
    }

    void CheckAutoMatchHorizontal(NodeGrid nodeToCheck, Stack<NodeGrid> matchDetected)
    {
        if(nodeToCheck != null)
        {
            CalcHorzValuesIndexDirectionStack(nodeToCheck);

            for (int i = 2; i < directionsAutoMatch; i++)
            {
                if(nodeToCheck.GetGridPos().x +1 < piecesX )
                {
                    NodeGrid rightNode = grid.gridNodes[(int)nodeToCheck.GetGridPos().x + 1, (int)nodeToCheck.GetGridPos().y];

                    if(rightNode != null && indexDirectionsAutomaticMatch[i] != null && nodeToCheck != null)
                    {
                        if(rightNode.GetPiece() != null && nodeToCheck.GetPiece() != null)
                        {
                            if (rightNode.GetGridPos().x == indexDirectionsAutomaticMatch[i].x && 
                                rightNode.GetPiece().pieceType == nodeToCheck.GetPiece().pieceType)
                            {
                                if (!matchDetected.Contains(nodeToCheck))
                                {
                                    matchDetected.Push(nodeToCheck);
                                }

                                if (!matchDetected.Contains(rightNode))
                                {
                                    matchDetected.Push(rightNode);
                                }

                                CheckAutoMatchHorizontal(rightNode, matchDetected);
                            }
                            else
                            {
                                if (matchDetected.Count < minMatchAmount)
                                {
                                    matchDetected.Clear();
                                }
                                else if(matchDetected.Count >= minMatchAmount)
                                {
                                    if (!automaticMatches.Contains(matchDetected))
                                    {
                                        automaticMatches.Add(matchDetected);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    IEnumerator MakeAutoMatch()
    {
        onMatchingProcess = true;
        callsOfMakeAutoMatch++;
        
        if (automaticMatches.Count > 0)
        {
            GameManager.Instance.BlockPlayerInteractions();
        }

        yield return new WaitForSeconds(0.5f);

        int finalScoreToAdd = 0;
        int maxAmountAutoMatch=0;

        for (int i = 0; i < automaticMatches.Count; i++)
        {
            AudioManager.Instance.Play("Match");

            if (maxAmountAutoMatch < automaticMatches[i].Count)
                maxAmountAutoMatch = automaticMatches[i].Count;

            while (automaticMatches[i].Count > 0)
            {
                if(automaticMatches[i].Peek().GetPiece() != null)
                {
                    PieceType pieceDestroyed = automaticMatches[i].Peek().GetPiece();
                    automaticMatches[i].Peek().SetPieceOnNode(null);
                    nodesEmpty.Add(automaticMatches[i].Peek());

                    Animator peekPieceAnimator = pieceDestroyed.GetComponent<Animator>();
                    if (peekPieceAnimator != null)
                    {
                        peekPieceAnimator.SetBool("Destroy", true);
                    }

                    automaticMatches[i].Pop();
                    yield return new WaitForEndOfFrame();
                }
                else
                {
                    break;
                }
            }
        }

        finalScoreToAdd = automaticMatches.Count * (maxAmountAutoMatch);
        CalcAmountSocreOnAutoMatch(finalScoreToAdd);

        onMatchingProcess = false;
        automaticMatches.Clear();

        yield return null;
    }

    void CalcAmountSocreOnAutoMatch(int score)
    {
        if(flagScoreIncrement == 0)
        {
            GameManager.Instance.IncreaceScoreMultipler(score, minMatchAmount);
            flagScoreIncrement = 1;
        }
    }

    void FindEmptyNodes()
    {
        if (nodesEmpty.Count <= 0)
            return;

        for (int i = 0; i < nodesEmpty.Count; i++)
        {
            if(nodesEmpty[i]!= null)
            {
                SortNodesOnColumn(nodesEmpty[i]);
            }
        }
    }

    void SortNodesOnColumn(NodeGrid fromNode)
    {
        if(fromNode.GetGridPos().y - 1 >= 0)
        {
            NodeGrid upNode = grid.gridNodes[(int)fromNode.GetGridPos().x, (int)fromNode.GetGridPos().y - 1];

            if (upNode.GetPiece() == null)
            {
                SortNodesOnColumn(upNode);
            }

            if (upNode.GetPiece() != null)
            {
                PieceType pieceToSort = upNode.GetPiece();

                if(fromNode.GetPiece() == null)
                {
                    fromNode.SetPieceOnNode(pieceToSort);
                    pieceToSort.myNode = fromNode;

                    upNode.SetPieceOnNode(null);
                    nodesEmpty.Add(upNode);

                    StartCoroutine(PlacePieceOnPosition(pieceToSort, pieceToSort.transform.position, fromNode.transform.position));

                    nodesEmpty.Remove(fromNode);
                }
            }
        }
    }

    void FillFirstRowWithPieces()
    {
        if (onMatchingProcess)
            return;

        for (int j = 0; j < piecesX; j++)
        {
            if(grid.gridNodes[j,0] != null)
            {
                if(grid.gridNodes[j, 0].GetPiece() == null)
                {
                    int randPiece = Random.Range(0, indicesPiecesToSpawm.Count);

                    PieceType pieceToCreate = Instantiate(prefabsPieces[indicesPiecesToSpawm[randPiece]], grid.gridNodes[j, 0].transform.position, Quaternion.identity, piecesParent);

                    Debug.Log("Tiene pieza: " + grid.gridNodes[j, 0].GetPiece());
                    grid.gridNodes[j, 0].SetPieceOnNode(pieceToCreate);
                    pieceToCreate.myNode = grid.gridNodes[j, 0];

                    Animator pieceCreatedAnimator = pieceToCreate.GetComponent<Animator>();
                    if(pieceCreatedAnimator != null)
                        pieceCreatedAnimator.SetBool("HidePiece", true);

                    StartCoroutine(DelayToShowPieceFirstRow(pieceCreatedAnimator));
                }
            }
        }
        StartCoroutine(CheckIfStillEmptyNodes());
    }

    IEnumerator DelayToShowPieceFirstRow(Animator pieceCreatedAnimator)
    {
        yield return new WaitForSeconds(0.1f);

        if (pieceCreatedAnimator != null)
        {
            pieceCreatedAnimator.SetBool("HidePiece", false);
        }

        yield return null;
    }

    IEnumerator CheckIfStillEmptyNodes()
    {
        yield return new WaitForSeconds(.5f);

        flagCheckForAutoMatches = 0;
        int piecesChecked = 0;

        for (int i = 0; i < piecesY; i++)
        {
            for (int j = 0; j < piecesX; j++)
            {
                if(grid.gridNodes[j,i] != null)
                {
                    if(grid.gridNodes[j, i].GetPiece() != null)
                    {
                        piecesChecked++;
                    }
                }
            }
        }

        if (piecesChecked == piecesX * piecesY)
        {
            autoSortGrid = false;
            if(!onMatchingProcess && automaticMatches.Count > 0)
            {
                callsOfMakeAutoMatch = 0;
                flagScoreIncrement = 0;
                canMakeAutoMatch = true;
            }
            else if(!onMatchingProcess && automaticMatches.Count <= 0)
            {
                GameManager.Instance.UnblockPlayerInteractions();
            }
            nodesEmpty.Clear();
        }

        yield return null;
    }

    IEnumerator PlacePieceOnPosition(PieceType piece, Vector3 initialPosition, Vector3 targetPosition)
    {
        float timeToEnd = 0;

        if (piece != null)
        {
            while (piece != null && piece.transform.position != targetPosition)
            {
                timeToEnd += Time.deltaTime * speedVerticalFall;

                if (piece != null)
                {
                    piece.transform.position = Vector3.Lerp(initialPosition, targetPosition, timeToEnd);
                }

                yield return new WaitForEndOfFrame();
            }
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
    }

    void CalcVertValuesIndexDirectionStack(NodeGrid pieceNode)
    {
        //Arriba
        SetDirectionsNodeAutoMatch(0, new Vector2(pieceNode.GetGridPos().x, pieceNode.GetGridPos().y - 1));
        //Abajo
        SetDirectionsNodeAutoMatch(1, new Vector2(pieceNode.GetGridPos().x, pieceNode.GetGridPos().y + 1));
    }

    void CalcHorzValuesIndexDirectionStack(NodeGrid pieceNode)
    {
        //Derecha
        SetDirectionsNodeAutoMatch(2, new Vector2(pieceNode.GetGridPos().x + 1, pieceNode.GetGridPos().y));
        //Izquierda
        SetDirectionsNodeAutoMatch(3, new Vector2(pieceNode.GetGridPos().x - 1, pieceNode.GetGridPos().y));
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
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    void SetDirectionsNode(int directionIndex,Vector2 directionGrid)
    {
        if ((directionGrid.x >= 0 || directionGrid.x < piecesX) 
            && (directionGrid.y >= 0 || directionGrid.y < piecesY))
        {
            indexDirectionsPieceStack[directionIndex] = directionGrid;
        }
    }

    void SetDirectionsNodeAutoMatch(int directionIndex, Vector2 directionGrid)
    {
        if ((directionGrid.x >= 0 || directionGrid.x < piecesX) 
            && (directionGrid.y >= 0 || directionGrid.y < piecesY))
        {
            indexDirectionsAutomaticMatch[directionIndex] = directionGrid;
        }
    }
}