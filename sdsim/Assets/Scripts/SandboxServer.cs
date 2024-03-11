using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tk;
using System.Net;
using System.Net.Sockets;
using System;

[RequireComponent(typeof(tk.TcpServer))]
public class SandboxServer : MonoBehaviour
{
    public string host;
    public int port;

    tk.TcpServer _server = null;

    public GameObject clientTemplateObj = null;
    public Transform spawn_pt;
    public bool spawnCarswClients = true;
    public bool privateAPI = false;
    bool argHost = false;
    bool argPort = false;

    tk.JsonTcpClient jsonTcpClient;
    tk.TcpClient _client;

    public void CheckCommandLineConnectArgs()
    {
        string[] args = System.Environment.GetCommandLineArgs();

        if (!privateAPI)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--host")
                {
                    host = args[i + 1];
                    argHost = true;
                }
                else if (args[i] == "--port")
                {
                    port = int.Parse(args[i + 1]);
                    argPort = true;
                }
            }
        }

        if (argHost == false) { host = GlobalState.host; }
        if (argPort == false) { port = GlobalState.port; }
        
        if (privateAPI)
        {
            port = GlobalState.portPrivateAPI;
        }

    }

    private void Awake()
    {
        _server = GetComponent<tk.TcpServer>();
        
    }

    // Start is called before the first frame update
    void Start()
    {
        CheckCommandLineConnectArgs();

        Debug.Log("[SandboxServer] SDSandbox Server starts running on " + GlobalState.host + ":" + port);
        _server.Run(GlobalState.host, port);
        //Debug.Log("[SandboxServer] starting client");
        //tk.TcpClient client = OnClientConnected();
    }

    // It's our responsibility to create a GameObject with a TcpClient
    // and return it to the server.
    public tk.TcpClient OnClientConnected()
    {
        if (clientTemplateObj == null)
        {
            Debug.LogError("[SandboxServer] client template object was null.");
            return null;
        }

        if (_server.debug)
            Debug.Log("[SandboxServer] creating client obj");

        GameObject go = GameObject.Instantiate(clientTemplateObj) as GameObject;

        go.transform.parent = this.transform;

        if (spawn_pt != null)
            go.transform.position = spawn_pt.position + UnityEngine.Random.insideUnitSphere * 2;

        tk.TcpClient client = go.GetComponent<tk.TcpClient>();

        Debug.Log("[SandboxServer] connecting client to server.");
        //client.connect(GlobalState.host, port);
        InitClient(client);

        return client;
    }

    private void InitClient(tk.TcpClient client)
    {
        
        if (privateAPI) // private API client server
        {
            Debug.Log("[SandboxServer] init client private API.");
            PrivateAPI privateAPIHandler = GameObject.FindObjectOfType<PrivateAPI>();
            if (privateAPIHandler != null)
            {
                if (_server.debug)
                    Debug.Log("init private API handler.");

                Debug.Log("before init private API handler.");
                if (jsonTcpClient == null)
                {
                    jsonTcpClient = this.gameObject.AddComponent<tk.JsonTcpClient>();
                    Debug.Log(client == null);
                }
                Debug.Log("[SandboxServer] jsonTcpclient created");
                privateAPIHandler.Init(jsonTcpClient);
                Debug.Log("[SandboxServer] after init private API handler.");

            }
        }

        else // normal client server
        {
            Debug.Log("[SandboxServer] init client normal server.");
            if (spawnCarswClients) // we are on in a track scene
            {
                CarSpawner spawner = GameObject.FindObjectOfType<CarSpawner>();
                if (spawner != null)
                {
                    Debug.Log("[SandboxServer] spawning car.");
                    if (jsonTcpClient == null)
                    {
                        jsonTcpClient = this.gameObject.AddComponent<tk.JsonTcpClient>();
                        Debug.Log(client == null);
                    }
                    spawner.Spawn(jsonTcpClient, false);
                    Debug.Log("[SandboxServer] car spawned.");
                }
            }
            else //we are in the menu
            {
                Debug.Log("[SandboxServer] we are in the menu.");
                tk.TcpMenuHandler handler = GameObject.FindObjectOfType<TcpMenuHandler>();
                if (handler != null)
                {
                    if (_server.debug)
                        Debug.Log("[SandboxServer] init menu handler.");
                    Debug.Log("[SandboxServer] init menu handler.");
                    tk.JsonTcpClient jsonTcpClient = GetComponent<tk.JsonTcpClient>();
                    if (jsonTcpClient == null)
                    {
                        Debug.Log("[SandboxServer] creating new json tcp object ");
                        jsonTcpClient = this.gameObject.AddComponent<tk.JsonTcpClient>();
                        Debug.Log(client == null);
                    }
                    Debug.Log("[SandboxServer] jsonTcpclient created");
                    handler.Init(jsonTcpClient);
                    Debug.Log("[SandboxServer] menu handler initialized.");
                }
            }
        }
    }


    public void OnSceneLoaded(bool bFrontEnd)
    {
        Debug.Log("[SandboxServer] on scene loaded function");
        spawnCarswClients = !bFrontEnd;
        Debug.Log("[SandboxServer] bfrontend value : " +  bFrontEnd);

        if (_server.debug)
            Debug.Log("[SandboxServer] init network client.");
        Debug.Log("[SandboxServer] initclient call");
        InitClient(_client);
        Debug.Log("[SandboxServer] initclient called.");

        Debug.Log("[SandboxServer] pace car : " + GlobalState.paceCar);
        if (GlobalState.paceCar && !bFrontEnd) // && clients.Count == 0
        {
            CarSpawner spawner = GameObject.FindObjectOfType<CarSpawner>();
            Debug.Log("[SandboxServer] spawner : " + spawner == null);
            if (spawner)
            {
                Debug.Log("[SandboxServer] ensure one car.");
                spawner.EnsureOneCar();
            }
        }
        Debug.Log("[SandboxServer] on scene loaded function end");
    }

    public void OnClientDisconnected(tk.TcpClient client)
    {
        if (privateAPI)
        {
        }
        else
        {
            CarSpawner spawner = GameObject.FindObjectOfType<CarSpawner>();
            if (spawner)
            {
                spawner.RemoveCar(client.gameObject.GetComponent<tk.JsonTcpClient>());
            }
        }
        GameObject.Destroy(client.gameObject);
    }
}
