using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager instance;

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Text promptText;

    private void Awake()
    {
        if (instance != null) Destroy(this.gameObject);
        else instance = this;
    }

    private void Start()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            SpawnAllPlayers();
        }
    }

    private void SpawnAllPlayers()
    {
        var clients = NetworkManager.Singleton.ConnectedClientsIds.ToList();
        for (int i = 0; i < clients.Count; i++)
        {
            ulong clientId = clients[i];
            Transform spawn = spawnPoints[i % spawnPoints.Length];

            GameObject playerObj = Instantiate(playerPrefab, spawn.position, spawn.rotation);

            // Назначаем Dome после создания игрока
            var movement = playerObj.GetComponent<PlayerMovement>();
            var textInter = playerObj.GetComponent<PlayerInteraction>();
            if (movement != null)
            {
                movement.dome = GameObject.FindWithTag("Dome");
            }
            if (movement != null)
            {
                textInter.promptText = promptText;
            }

            playerObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }
    }
}
