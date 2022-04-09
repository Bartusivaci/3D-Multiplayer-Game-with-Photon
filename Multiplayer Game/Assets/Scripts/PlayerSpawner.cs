using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{

    public static PlayerSpawner instance;
    void Awake()
    {
        instance = this;
    }


    public GameObject playerPrefab;
    public GameObject deathFX;

    GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
    }

    public void SpawnPlayer()
    {
        Transform spawnPoint = SpawnManager.instance.GetSpawnPoint();

        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
    }

    public void Die()
    {
        PhotonNetwork.Instantiate(deathFX.name, player.transform.position, Quaternion.identity);
        
        PhotonNetwork.Destroy(player);

        SpawnPlayer();
    }
}
