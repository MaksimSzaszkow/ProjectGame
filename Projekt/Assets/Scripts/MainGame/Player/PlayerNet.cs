﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerNet : NetworkBehaviour
{
#if UNITY_EDITOR
    [String("Camera reference:")]
#endif
    [SerializeField]
    GameObject PlayerCam;
    GameObject MainCam;
#if UNITY_EDITOR
    [String("Player components references:")]
#endif
    [SerializeField]
    GameObject playerModel;
    [SerializeField]
    Rigidbody playerRigidbody;
    [SerializeField]
    Collider playerCollider;
#if UNITY_EDITOR
    [String("Player strting values:")]
#endif
    [SerializeField]
    float respawnTime;
    [SerializeField]
    float maxHP;
#if UNITY_EDITOR
    [String("Prefab references:")]
#endif
    [SerializeField]
    GameObject RemoteText;
    [SerializeField]
    Text UIText;
    [SerializeField]
    GameObject TextMesh;
#if UNITY_EDITOR
    [String("Canvas reference:")]
#endif
    [SerializeField]
    GameObject LocalCanvas;
    GameObject[] SpawnPoints;
    GameObject SpawnPoint;

    void Awake()
    {
        MainCam = Camera.main.gameObject;
        SpawnPoints = GameObject.FindGameObjectsWithTag("Spawn");
    }
    void Start()
    {
        FirstEnablePlayer();
    }    

    //!!!POCZĄTEK PIERWSZEGO URUCHOMIENIA!!!
    void FirstEnablePlayer()
    {
        Shared(true);
        if (isLocalPlayer) Local(true);
        else Remote(true);
    }
    //aktywowanie lokalnych rzeczy
    void Local(bool truefalse)
    {
        gameObject.tag = "Player";
        MainCam.SetActive(!truefalse);
        PlayerCam.SetActive(truefalse);
        LocalCanvas.SetActive(truefalse);
        TextMesh.GetComponent<MeshRenderer>().enabled = false;
        UIText.text = maxHP + "/" + maxHP;
        gameObject.GetComponent<PlayerController>().enabled = truefalse;
        ActivateSpells();
    }
    //aktywowanie wspólnych rzeczy
    void Shared(bool truefalse)
    {
        TextMesh = Instantiate(RemoteText, gameObject.transform.position, gameObject.transform.rotation);
        TextMesh.GetComponent<RemoteText>().Player = gameObject;
        TextMesh.GetComponent<TextMesh>().text = maxHP + "/" + maxHP;
        gameObject.GetComponent<PlayerHP>().TextMesh = TextMesh;
        gameObject.GetComponent<PlayerHP>().UIText = UIText;
        gameObject.GetComponent<PlayerHP>().maxHP = maxHP;
        playerCollider.enabled = truefalse;
        playerModel.SetActive(truefalse);
        playerRigidbody.useGravity = truefalse;
        gameObject.GetComponent<PlayerHP>().ResetHP();
    }
    void Remote(bool truefalse)
    {

    }
    void ActivateSpells()
    {
        if(PlayerPrefs.GetString("OffElement") == "Fire" || PlayerPrefs.GetString("DeffElement") == "Fire")
        {
            gameObject.GetComponent<FireSpells>().enabled = true;
        }
        if (PlayerPrefs.GetString("OffElement") == "Thunder" || PlayerPrefs.GetString("DeffElement") == "Thunder")
        {
            gameObject.GetComponent<ThunderSpells>().enabled = true;
        }
        if (PlayerPrefs.GetString("OffElement") == "Earth" || PlayerPrefs.GetString("DeffElement") == "Earth")
        {
            gameObject.GetComponent<EarthSpells>().enabled = true;
        }
    }
    //!!!KONIEC PIERWSZEGO URUCHOMIENIA!!!

    //!!!POCZĄTEK FUNKCJI SERVEROWYCH!!!

    //Włączanie i wyłączanie obiektów aby gracz nie był na chwilę nie uczestniczył w grze
    [ServerCallback]
    void EnablePlayer()
    {
        TargetLocal(connectionToClient, true);
        RpcShared(true);
    }
    [ServerCallback]
    void DisablePlayer()
    {
        TargetLocal(connectionToClient, false);
        RpcShared(false);
    }
    //3 następne funkcje są używane w powyższych 2
    [TargetRpc]
    void TargetLocal(NetworkConnection player, bool truefalse)
    {
        MainCam.SetActive(!truefalse);
        PlayerCam.SetActive(truefalse);
        LocalCanvas.SetActive(truefalse);
        gameObject.GetComponent<FireSpells>().enabled = truefalse;
        gameObject.GetComponent<EarthSpells>().enabled = truefalse;
        gameObject.GetComponent<ThunderSpells>().enabled = truefalse;
        gameObject.GetComponent<PlayerController>().enabled = truefalse;
    }
    [TargetRpc]
    void TargetSpawnPosition(NetworkConnection player)
    {
        Transform spawn = NetworkManager.singleton.GetStartPosition();
        transform.position = spawn.position;
        transform.rotation = spawn.rotation;
        PlayerCam.GetComponent<CamController>().currentX = 0;
        
    }
    [ClientRpc]
    void RpcShared(bool truefalse)
    {
        if (!isLocalPlayer) TextMesh.GetComponent<MeshRenderer>().enabled = truefalse;
        playerCollider.enabled = truefalse;
        playerModel.SetActive(truefalse);
        playerRigidbody.useGravity = truefalse;
        gameObject.GetComponent<PlayerHP>().ResetHP();
    }
    //Funkcja odpowiedzialna za śmierć
    public void Die()
    {
        DisablePlayer();
        TargetSpawnPosition(connectionToClient);
        Invoke("EnablePlayer", 4);
    }
    
    public void DisableCC()
    {
        gameObject.GetComponent<FireSpells>().enabled = false;
        gameObject.GetComponent<EarthSpells>().enabled = false;
        gameObject.GetComponent<PlayerController>().enabled = false;
        gameObject.GetComponent<ThunderSpells>().enabled = false;
        Invoke("EnableCC", 2);
    }
    void EnableCC()
    {
        if(isLocalPlayer)
        {
            gameObject.GetComponent<FireSpells>().enabled = true;
            gameObject.GetComponent<EarthSpells>().enabled = true;
            gameObject.GetComponent<PlayerController>().enabled = true;
            gameObject.GetComponent<ThunderSpells>().enabled = true;
        }
    }
}
