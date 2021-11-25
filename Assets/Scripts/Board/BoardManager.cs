using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour
{
    public static event Action<List<Item>[]> OnBoardIsCreated;
    public static event Action<Item> OnNewItemIsCreated;
    
    [SerializeField] private BoardParametersScriptableObject boardParameters;

    [SerializeField] private Transform[] itemTransforms;
    [SerializeField] private Transform bottomCollider;
    
    private const int MIN_BOARD_ROW_COUNT = 2;
    private const int MAX_BOARD_ROW_COUNT = 10;
    
    private const int MIN_BOARD_COLUMN_COUNT = 2;
    private const int MAX_BOARD_COLUMN_COUNT = 10;

    private const int MIN_COLOR_COUNT = 1;
    private const int MAX_COLOR_COUNT = 6;

    private const float BOARD_MARGIN_VERTICAL = 0.5f;
    private const float BOARD_MARGIN_HORIZONTAL = 0.5f;

    private float itemLength;
    private Vector3 itemScale;
    private Vector3 topRightPos;
    private Vector3 screenCenter;
    private Camera cam;
    private List<Item>[] initialBoardItems;

    private void OnEnable()
    {
        if (!AreBoardInputsValid()) {return;}
        
        LevelManager.OnNewItemsAreNeeded += OnNewItemsAreNeeded;
        GameManager.OnGameSceneLoaded += OnGameSceneLoaded;
        
        cam = Camera.main;
    }

    private void OnGameSceneLoaded()
    {
        StartCoroutine(CreateBoardNextFrame());
    }

    private void OnNewItemsAreNeeded(int[] newItemCountArray)
    {
        for (var i = 0; i < boardParameters.ColumnCount; i++)
        {
            if (newItemCountArray[i] == 0) {continue;}
            
            CreateNewItems(i,newItemCountArray[i]);
        }
    }

    private void CreateNewItems(int indexI, int newItemCount)
    {
        for (var j = 0; j < newItemCount; j++)
        {
            var pos = new Vector3()
            {
                x = GetNthItemsPlaceOnTheAxis(indexI, boardParameters.ColumnCount, Vector3.right),
                y = GetNewItemSpawnHeight(j),
                z = 0
            };

            var item = CreateBoardItem(pos).GetComponent<Item>();
            item.IndexI = indexI;
            OnNewItemIsCreated?.Invoke(item);
        }
    }

    private float GetNewItemSpawnHeight(int j)
    {
        return topRightPos.y + (j + 1) * itemLength;
    }

    private IEnumerator CreateBoardNextFrame()
    {
        yield return new WaitForEndOfFrame();
        
        SetItemLength();
        SetItemScale();
        CreateBoardItems();
        
        OnBoardIsCreated?.Invoke(initialBoardItems);
    }

    private bool AreBoardInputsValid()
    {
        if (!IsBoardRowCountValid())
        {
            Debug.LogError($"Board row count must be between {MIN_BOARD_ROW_COUNT} and {MAX_BOARD_ROW_COUNT}.");
            return false;
        }

        if (!IsBoardColumnCountValid())
        {
            Debug.LogError( $"Board column count must be between {MIN_BOARD_COLUMN_COUNT} and {MAX_BOARD_COLUMN_COUNT}.");
            return false;
        }

        if (!IsColorCountValid())
        {
            Debug.LogError($"Color count must be between {MIN_COLOR_COUNT} and {MAX_COLOR_COUNT}.");
            return false;
        }

        if (IsUnsolvableDeadlockSituationHighlyLikely())
        {
            Debug.LogError("Unsolvable deadlock situation is highly likely for these parameters. Try increasing board size or decreasing color count.");
            return false;
        }
        
        return true;
    }

    private bool IsUnsolvableDeadlockSituationHighlyLikely()
    {
        return boardParameters.ColumnCount * boardParameters.RowCount <= boardParameters.ColorCount;
    }

    private bool IsBoardRowCountValid()
    {
        return boardParameters.RowCount >= MIN_BOARD_ROW_COUNT && boardParameters.RowCount <= MAX_BOARD_ROW_COUNT;
    } 
    
    private bool IsBoardColumnCountValid()
    {
        return boardParameters.ColumnCount >= MIN_BOARD_COLUMN_COUNT &&  boardParameters.ColumnCount <= MAX_BOARD_COLUMN_COUNT;
    }

    private bool IsColorCountValid()
    {
        return boardParameters.ColorCount >= MIN_COLOR_COUNT && boardParameters.ColorCount <= MAX_COLOR_COUNT;
    }

    private void SetItemLength()
    {
        var bottomLeftPos = cam.ViewportToWorldPoint(new Vector3(0f, 0f, cam.nearClipPlane));
        topRightPos = cam.ViewportToWorldPoint(new Vector3(1f, 1f, cam.nearClipPlane));
        screenCenter = (bottomLeftPos + topRightPos) / 2;
        
        var horizontalLength = topRightPos.x - bottomLeftPos.x - 2 * BOARD_MARGIN_HORIZONTAL;
        var verticalLength = topRightPos.y - bottomLeftPos.y - 2 * BOARD_MARGIN_VERTICAL;
        
        itemLength = Mathf.Min(horizontalLength / boardParameters.ColumnCount, verticalLength / boardParameters.RowCount);
    }

    private void SetItemScale()
    {
        var currentScale = itemTransforms[0].localScale;
        
        //Assuming all items are squares.
        var currentItemLength = itemTransforms[0].GetComponent<Renderer>().bounds.size.x;

        itemScale = currentScale * itemLength / currentItemLength;
    }
    
    private void CreateBoardItems()
    {
        initialBoardItems = new List<Item>[boardParameters.ColumnCount];

        for (var i = 0; i < boardParameters.ColumnCount; i++)
        {
            initialBoardItems[i] = new List<Item>();

            for (var j = 0; j < boardParameters.RowCount; j++)
            {
                var item = CreateBoardItem(GetGridPosition(i, j)).GetComponent<Item>();
                initialBoardItems[i].Add(item);
                item.IndexI = i;
                item.IndexJ = j;
            }
        }

        CreateBottomCollider();
    }

    private void CreateBottomCollider()
    {
        var pos = GetGridPosition(boardParameters.ColumnCount / 2, 0) - new Vector3(0f, itemLength, 0f) / 2;
        Instantiate(bottomCollider, pos, Quaternion.identity);
    }

    private Transform CreateBoardItem(Vector3 pos)
    {
        var randomColor = (ItemColor) Random.Range(0, boardParameters.ColorCount);
        var itemTransform = ItemPooler.Instance.SpawnFromPool(randomColor, pos, Quaternion.identity);

        itemTransform.localScale = itemScale;

        return itemTransform;
    }

    private Vector3 GetGridPosition(int i, int j)
    {
        return new Vector3()
        {
            x = GetNthItemsPlaceOnTheAxis(i, boardParameters.ColumnCount, Vector3.right),
            y = GetNthItemsPlaceOnTheAxis(j, boardParameters.RowCount, Vector3.up),
            z = 0
        };
    }

    private float GetNthItemsPlaceOnTheAxis(int i, int axisItemCount, Vector3 axis)
    {
        if (axisItemCount % 2 == 0)
        {
            var leftMiddleItem = axisItemCount / 2 - 1;

            return Vector3.Dot(screenCenter, axis) + (i - leftMiddleItem - 0.5f) * itemLength;
        }
        
        var middleItem = axisItemCount / 2;

        return Vector3.Dot(screenCenter, axis) + (i - middleItem) * itemLength;
    }

    private void OnDisable()
    {
        LevelManager.OnNewItemsAreNeeded -= OnNewItemsAreNeeded;
        GameManager.OnGameSceneLoaded -= OnGameSceneLoaded;
        
        initialBoardItems = null;
        cam = null;
    }
}
