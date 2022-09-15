using System;
using Pancake;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;

    private void Awake()
    {
        Archive.LoadFile("player-storage");
        playerData = Archive.Load<PlayerData>("PlayerDataKey");

        playerData.ToString();
    }

    private void OnApplicationQuit()
    {
        Archive.Save("PlayerDataKey", playerData);
        Archive.SaveFile("player-storage");
    }
}