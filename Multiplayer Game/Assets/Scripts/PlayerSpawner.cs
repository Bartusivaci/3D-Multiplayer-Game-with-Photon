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
    public float respawnTime = 5f;

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

    public void Die(string damager)
    {
        

        UIController.instance.deathText.text = "You Got Clapped By " + damager;

        // PhotonNetwork.Destroy(player);

        // SpawnPlayer();

        if (player != null)
        {
            StartCoroutine(DieCo());
        }
    }


    public IEnumerator DieCo()
    {
        PhotonNetwork.Instantiate(deathFX.name, player.transform.position, Quaternion.identity);

        PhotonNetwork.Destroy(player);

        UIController.instance.deathScreen.SetActive(true);

        yield return new WaitForSeconds(respawnTime);

        UIController.instance.deathScreen.SetActive(false);

        SpawnPlayer();
    }
}
