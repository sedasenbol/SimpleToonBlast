using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;
using Enumerable = System.Linq.Enumerable;
using Random = UnityEngine.Random;

public class LevelManager : MonoBehaviour
{
    public static event Action<int[]> OnNewItemsAreNeeded;
    
    [SerializeField] private BoardParametersScriptableObject boardParameters;
    
    [SerializeField] private int conditionA;
    [SerializeField] private int conditionB;
    [SerializeField] private int conditionC;
    
    private const float RAY_MAX_DISTANCE = 15f;
    
    private readonly Vector3[] allDirections = {Vector3.left, Vector3.right, Vector3.down, Vector3.up};

    private Camera cam;
    private int itemLayerMask;

    private List<Item>[] boardItems;
    private RaycastHit2D[] raycastHits;
    private HashSet<Item> itemBlockHashSet;
    private HashSet<Item> checkedItemHashSet;
    private HashSet<Item> itemsWithSetConditionsHashSet;
    private int[] newItemCountArray;

    private YieldInstruction waitForEndOfFrame;
    private YieldInstruction waitForTwoSeconds;

    private void OnEnable()
    {
        TouchController.OnPlayerTapped += OnPlayerTapped;
        BoardManager.OnBoardIsCreated += OnBoardIsCreated;
        BoardManager.OnNewItemIsCreated += OnNewItemIsCreated;

        raycastHits = new RaycastHit2D[1];
        cam = Camera.main;
        itemLayerMask = LayerMask.GetMask("Item");
        
        itemBlockHashSet = new HashSet<Item>();
        checkedItemHashSet = new HashSet<Item>();
        itemsWithSetConditionsHashSet = new HashSet<Item>();
        newItemCountArray = new int[boardParameters.ColumnCount];
        
        waitForEndOfFrame = new WaitForEndOfFrame();
        waitForTwoSeconds = new WaitForSeconds(2);
    }

    private void OnNewItemIsCreated(Item newItem)
    {
        newItem.IndexJ = boardItems[newItem.IndexI].Count;
        boardItems[newItem.IndexI].Add(newItem);
    }

    private void OnBoardIsCreated(List<Item>[] initialBoardItems)
    {
        boardItems = initialBoardItems;

        FindAndUpdateItemBlockConditions();
    }

    private void OnPlayerTapped(PointerEventData eventData)
    {
        var origin = cam.ScreenToWorldPoint(eventData.pressPosition);

        if (Physics2D.RaycastNonAlloc(origin, Vector3.forward, raycastHits, RAY_MAX_DISTANCE, itemLayerMask) == 0) {return;}

        var item = raycastHits[0].transform.GetComponent<Item>();
        
        HandleHitItem(item.IndexI, item.IndexJ);
    }

    private void HandleHitItem(int i, int j)
    {
        itemsWithSetConditionsHashSet.Clear();
        
        UpdateItemBlockHashSet(i, j, true);
        
        CheckItemSidesForAllDirections(i, j, true);

        if (itemBlockHashSet.Count == 1) {return;}
        
        HandleHitItemBlock();

        StartCoroutine(UpdateItemConditionsNextFrame());
    }

    private IEnumerator UpdateItemConditionsNextFrame()
    {
        yield return waitForEndOfFrame;

        itemsWithSetConditionsHashSet.Clear();
        FindAndUpdateItemBlockConditions();
    }

    private void FindAndUpdateItemBlockConditions()
    {
        var deadlock = true;
        
        for (var i = 0; i < boardParameters.ColumnCount; i++)
        {
            foreach (var item in boardItems[i])
            {
                UpdateItemBlockHashSet(item.IndexI,item.IndexJ, false);

                CheckItemSidesForAllDirections(item.IndexI, item.IndexJ, false);

                if (deadlock && itemBlockHashSet.Count > 1) { deadlock = false;}
                
                itemsWithSetConditionsHashSet.UnionWith(itemBlockHashSet);
            
                UpdateItemBlockConditions();
            }
        }

        if (deadlock) { StartCoroutine(ShuffleItemsWithDelay(waitForTwoSeconds)); }
    }

    private void UpdateItemBlockConditions()
    {
        if (itemBlockHashSet.Count <= conditionA) { UpdateItemConditions(0); return; }

        if (itemBlockHashSet.Count <= conditionB) { UpdateItemConditions(1); return; }

        if (itemBlockHashSet.Count <= conditionC) { UpdateItemConditions(2); return; }

        UpdateItemConditions(3);
    }

    private IEnumerator ShuffleItemsWithDelay(YieldInstruction waitForSeconds)
    {
        yield return waitForSeconds;

        ShuffleItems();
    }

    private void ShuffleItems()
    {
        for (var i = 0; i < boardParameters.ColumnCount; i++)
        {
            for (var j = 0; j < boardParameters.RowCount; j++)
            {
                var randomI = Random.Range(0, boardParameters.ColumnCount);
                var randomJ = Random.Range(0, boardParameters.RowCount);

                SwapTwoItems(randomI, randomJ, i, j);
            }
        }
        
        itemsWithSetConditionsHashSet.Clear();
        FindAndUpdateItemBlockConditions();
    }

    private void SwapTwoItems(int randomI, int randomJ, int i, int j)
    {
        var firstItem = boardItems[i][j];
        var randomItem = boardItems[randomI][randomJ];
        
        (boardItems[randomI][randomJ], boardItems[i][j]) = (boardItems[i][j], boardItems[randomI][randomJ]);
        (randomItem.MyTransform.position, firstItem.MyTransform.position) = (firstItem.MyTransform.position, randomItem.MyTransform.position);
        (firstItem.IndexI, randomItem.IndexI) = (randomItem.IndexI, firstItem.IndexI);
        (firstItem.IndexJ, randomItem.IndexJ) = (randomItem.IndexJ, firstItem.IndexJ);
    }

    private void UpdateItemConditions(int condition)
    {
        foreach (var item in itemBlockHashSet)
        {
            item.ChangeMyCondition((ItemCondition) condition);
        }
    }

    private void UpdateItemBlockHashSet(int i, int j, bool hitCheck)
    {
        if (itemsWithSetConditionsHashSet.Contains(boardItems[i][j])) {return;}
        
        itemBlockHashSet.Clear();
        itemBlockHashSet.Add(boardItems[i][j]);
        
        checkedItemHashSet.Clear();
        checkedItemHashSet.Add(boardItems[i][j]);
    }
    
    private void CheckItemSidesForAllDirections(int i, int j, bool hitCheck)
    {
        for (var directionIndex = 0; directionIndex < 4; directionIndex++)
        {
            CheckItemSides(allDirections[directionIndex], i, j, hitCheck);    
        }
    }
    
    private void CheckItemSides(Vector3 direction, int i, int j, bool hitCheck )
    {
        if (direction == Vector3.right && i == boardParameters.ColumnCount - 1) {return;}
        if (direction == Vector3.left && i == 0) {return;}
        if (direction == Vector3.up && j == boardParameters.RowCount - 1) {return;}
        if (direction == Vector3.down && j == 0) {return;}

        var newItem = boardItems[i + (int)direction.x][j + (int)direction.y];

        if (HandleNewItem(i, j, hitCheck, newItem)) {return;}
        
        CheckItemSidesForAllDirections(i + (int)direction.x, j + (int)direction.y, hitCheck);
    }

    private bool HandleNewItem(int i, int j, bool hitCheck, Item newItem)
    {
        if (checkedItemHashSet.Contains(newItem)) { return true;}
        if (!hitCheck && itemsWithSetConditionsHashSet.Contains(newItem)) { return true; }

        checkedItemHashSet.Add(newItem);

        if (newItem.Color != boardItems[i][j].Color) { return true; }

        itemBlockHashSet.Add(newItem);
        
        return false;
    }

    private void HandleHitItemBlock()
    {
        DestroyHitItemsInBlock();
        CreateNewItems();
    }

    private void DestroyHitItemsInBlock()
    {
        foreach (var item in itemBlockHashSet)
        {
            ItemPooler.Instance.AddItemBackToThePool(item.gameObject, item.Color);
            boardItems[item.IndexI].Remove(item);
            UpdateItemIndexesAbove(item);
        }
    }

    private void UpdateItemIndexesAbove(Item item)
    {
        var indexI = item.IndexI;
        var indexJ = item.IndexJ;

        for (var j = indexJ; j < boardParameters.RowCount - 1; j++)
        {
            if (boardItems[indexI].Count < j + 1) {break;}
            
            boardItems[indexI][j].IndexJ = j;
        }
    }

    private void CreateNewItems()
    {
        Array.Clear(newItemCountArray, 0, boardParameters.ColumnCount);

        foreach (var item in itemBlockHashSet)
        {
            newItemCountArray[item.IndexI]++;   
        }

        OnNewItemsAreNeeded?.Invoke(newItemCountArray);
    }
    
    private void OnDisable()
    {
        TouchController.OnPlayerTapped -= OnPlayerTapped;
        BoardManager.OnBoardIsCreated -= OnBoardIsCreated;
        BoardManager.OnNewItemIsCreated -= OnNewItemIsCreated;

        boardItems = null;
        cam = null;
        itemBlockHashSet = null;
        checkedItemHashSet = null;
        itemsWithSetConditionsHashSet = null;
    }
}
