using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPooler : MonoBehaviour
{
    [System.Serializable]
    public class ItemPool
    {
        public ItemColor color;
        public Transform itemTransform;
    }

    #region Singleton

    private static ItemPooler instance;

    public static ItemPooler Instance => instance;


    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(this.gameObject); return; } 
        
        instance = this;
    }

    #endregion

    [SerializeField] private BoardParametersScriptableObject boardParameters;
    [SerializeField] public Transform itemContainerTransform;
    [SerializeField] private List<ItemPool> itemPools;
    
    private Dictionary<ItemColor, Queue<GameObject>> itemPoolDict;

    public Transform SpawnFromPool(ItemColor color, Vector3 position, Quaternion rotation)
    {
        if (itemPoolDict[color].Count == 0) { Debug.LogError($"Pool size of {color} item is insufficient."); return null; }
        
        var objectSpawned = itemPoolDict[color].Dequeue();
        objectSpawned.SetActive(true);
        
        var objectSpawnedTransform = objectSpawned.transform;
        objectSpawnedTransform.position = position;
        objectSpawnedTransform.rotation = rotation;

        return objectSpawnedTransform;
    }

    public void AddItemBackToThePool(GameObject itemGameObject, ItemColor color)
    {
        itemGameObject.SetActive(false);
        itemPoolDict[color].Enqueue(itemGameObject);
    }
    
    private void OnEnable()
    {
        GameManager.OnGameSceneLoaded += OnGameSceneLoaded;
    }

    private void OnGameSceneLoaded()
    {
        InitializeItemPoolDict();
    }

    private void InitializeItemPoolDict()
    {
        itemPoolDict = new Dictionary<ItemColor, Queue<GameObject>>();

        var poolSize = GetPoolSize();

        // Use for loop instead of foreach to improve performance.
        for (var i = 0; i < itemPools.Count; i++)
        {
            var newItemPool = new Queue<GameObject>(poolSize);

            InitializeItemPool(poolSize, i, newItemPool);

            itemPoolDict[itemPools[i].color] = newItemPool;
        }
    }

    private void InitializeItemPool(int poolSize, int i, Queue<GameObject> newItemPool)
    {
        for (var j = 0; j < poolSize; j++)
        {
            var obj = Instantiate(itemPools[i].itemTransform, itemContainerTransform).gameObject;
            obj.SetActive(false);
            newItemPool.Enqueue(obj);
        }
    }

    private int GetPoolSize()
    {
        return boardParameters.ColumnCount * boardParameters.RowCount * 5;
    }
    
    private void OnDisable()
    {
        GameManager.OnGameSceneLoaded -= OnGameSceneLoaded;

        itemPoolDict = null;
        instance = null;
    }
}
