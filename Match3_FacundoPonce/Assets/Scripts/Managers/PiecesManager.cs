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
    public bool chainBegin;

    public bool autoSortAndMatch;

    public int piecesCreated;

    public Transform piecesParent;

    List<NodeGrid> preMatchesVertical;
    List<NodeGrid> preMatchesHorizontal;


    List<Stack<PieceType>> automaticMatches;

    GridBehaviour grid;
    GridState stateGrid;
    int piecesX;
    int piecesY;

    int indexLastPieceCreated = 0;

    public Stack<PieceType> matchingPieces;

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

    List<NodeGrid> nodesEmpty;
    public bool createPiecesAfterSort;

    #endregion


    private void Start()
    {
        piecesCreated = 0;
        chainBegin = false;
        piecesGenerated = false;

        grid = gameObject.GetComponent<GridBehaviour>();
        stateGrid = gameObject.GetComponent<GridState>();
        matchingPieces = new Stack<PieceType>();
        indicesPiecesToSpawm = new List<int>();

        automaticMatches = new List<Stack<PieceType>>();

        preMatchesVertical = new List<NodeGrid>();
        preMatchesHorizontal = new List<NodeGrid>();

        nodesEmpty = new List<NodeGrid>();

        //OLD
        indexDirectionsAutomaticMatch = new int[directionsAutoMatch];
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

        if (autoSortAndMatch)
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

        autoSortAndMatch = true;

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

                fromNode.SetPieceOnNode(pieceToSort);
                pieceToSort.myNode = fromNode;

                upNode.SetPieceOnNode(null);
                nodesEmpty.Add(upNode);

                StartCoroutine(PlacePieceOnPosition(pieceToSort, pieceToSort.transform.position, fromNode.transform.position));

                nodesEmpty.Remove(fromNode);
            }
        }
    }

    void FillFirstRowWithPieces()
    {
        for (int i = 0; i < piecesX; i++)
        {
            if(grid.gridNodes[i,0] != null)
            {
                if(grid.gridNodes[i, 0].GetPiece() == null)
                {
                    int randPiece = Random.Range(0, indicesPiecesToSpawm.Count);
                    PieceType pieceToCreate = Instantiate(prefabsPieces[indicesPiecesToSpawm[randPiece]], grid.gridNodes[i, 0].transform.position, Quaternion.identity, piecesParent);
                    grid.gridNodes[i, 0].SetPieceOnNode(pieceToCreate);
                    pieceToCreate.myNode = grid.gridNodes[i, 0];

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
            pieceCreatedAnimator.SetBool("HidePiece", false);

        yield return null;
    }

    IEnumerator CheckIfStillEmptyNodes()
    {
        yield return new WaitForSeconds(.5f);

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
            autoSortAndMatch = false;

        yield return null;
    }

    void CreatePiecesAndRestoreTop()
    {
        
    }

    IEnumerator PlacePieceOnPosition(PieceType piece, Vector3 initialPosition, Vector3 targetPosition)
    {
        float timeToEnd = 0;

        if (piece != null)
        {
            while (piece.transform.position != targetPosition)
            {
                timeToEnd += Time.deltaTime * speedVerticalFall;

                if (piece != null)
                    piece.transform.position = Vector3.Lerp(initialPosition, targetPosition, timeToEnd);

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
        //Abajo
        SetDirectionsNode(2, new Vector2(pieceNode.GetGridPos().x + 1, pieceNode.GetGridPos().y));
        //Arriba
        SetDirectionsNode(3, new Vector2(pieceNode.GetGridPos().x - 1, pieceNode.GetGridPos().y));
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

    void SetValueDirection(int indexArray, int value)
    {
        
    }
}