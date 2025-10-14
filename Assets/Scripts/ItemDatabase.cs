using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance = null;

    [SerializeField]
    private Item _baseItemPrefab = null;

    [SerializeField]
    private List<ItemDataSO> _itemDatas = null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Item CreateInstance(ItemDataSO itemData)
    {
        var item = Instantiate(_baseItemPrefab);
        item.Init(itemData);

        return item;
    }

    public Item CreateInstance(int itemId)
    {
        var item = Instantiate(_baseItemPrefab);
        item.Init(GetItemByID(itemId));

        return item;
    }

    public ItemDataSO GetItemByID(int id)
    {
        ItemDataSO itemData = _itemDatas.Where(x => x.ItemID == id).First();
        return itemData;
    }

    public ItemDataSO GetItemByName(string name)
    {
        ItemDataSO itemData = _itemDatas.Where(x => x.ItemName == name).First();
        return itemData;
    }
}
