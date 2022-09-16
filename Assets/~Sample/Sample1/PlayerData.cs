using System;
using System.Collections.Generic;
using MessagePack;
using Pancake;
using UnityEngine;

[Serializable, MessagePackObject()]
public class PlayerData
{
    [Key(0), SerializeField] private string id;
    [Key(1), SerializeField] private string name;
    [Key(2), SerializeField] private string description;
    [Key(3), SerializeField] private int str;
    [Key(4), SerializeField] private List<ItemSO> items;

    [Key(5), SerializeField, MessagePackFormatter(typeof(AssetFormatter<Sprite>))]
    private Sprite icon;

    public override string ToString()
    {
        Debug.Log("id=" + id);
        Debug.Log("name=" + name);
        Debug.Log("description=" + description);
        Debug.Log("str=" + str);
        Debug.Log("item count=" + items.Count);

        for (int i = 0; i < items.Count; i++)
        {
            Debug.Log("item[" + i + "] displayName="+items[i].DisplayName);
            Debug.Log("item[" + i + "] power="+items[i].Power);
        }
        
        Debug.Log("icon="+icon.name);
        return "";
    }
}