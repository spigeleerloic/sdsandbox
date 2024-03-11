using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Net;
using System.Net.Sockets;
using System;

namespace tk
{

    public class TcpMenuHandler : MonoBehaviour
    {

        public List<string> scene_names = new List<string>();
        public SceneLoader loader;
        public GameObject ButtonGridLayout;
        public GameObject ButtonPrefab;
        private tk.JsonTcpClient client;
        private string[] bundleAssetScenePaths;

        public void Init(tk.JsonTcpClient _client)
        {
            if (_client == null){
                Debug.Log("client is null");
            }
            Debug.Log("start init menu handler");
            _client.dispatchInMainThread = true;

            Debug.Log("assignment of client to _client");
            client = _client;
            Debug.Log("load scene register");
            client.dispatcher.Register("load_scene", new tk.Delegates.OnMsgRecv(OnLoadScene));
            Debug.Log("get protocol version register");
            client.dispatcher.Register("get_protocol_version", new tk.Delegates.OnMsgRecv(OnProtocolVersion));
            Debug.Log("get scene names register");
            client.dispatcher.Register("get_scene_names", new tk.Delegates.OnMsgRecv(OnGetSceneNames));
            Debug.Log("quit app register");
            client.dispatcher.Register("quit_app", new tk.Delegates.OnMsgRecv(OnQuitApp));
            Debug.Log("connected register");
            client.dispatcher.Register("connected", new tk.Delegates.OnMsgRecv(OnConnected));
        }

        public void Start()
        {
            if (GlobalState.sceneNames == null)
            {
                try
                {   
                    Debug.Log("load scene paths from file");
                    bundleAssetScenePaths = loader.LoadScenePathsFromFile(GlobalState.additionnalContentPath);
                    if (bundleAssetScenePaths != null) // Add those paths to the scene names
                    {
                        Debug.Log("add bundle asset scene paths to scene names");
                        scene_names.AddRange(bundleAssetScenePaths);
                    }

                }
                catch (Exception e) { Debug.LogError(e); 
                }

                GlobalState.sceneNames = scene_names.ToArray();

            }
            Debug.Log("before for each scene name button add");
            foreach (string scene_name in GlobalState.sceneNames)
            {
                Debug.Log("add button to menu" + scene_name);
                AddButtonToMenu(scene_name);
            }
        }

        public void OnDestroy()
        {
            if (client)
                client.dispatcher.Reset();
        }

        void OnProtocolVersion(JSONObject msg)
        {
            Debug.Log("OnProtocolVersion called in TcpMenuHandler.cs");
            JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
            json.AddField("msg_type", "protocol_version");
            json.AddField("version", "2");

            client.SendMsg(json);
        }

        void OnConnected(JSONObject msg)
        {
            Debug.Log("OnConnected called in TcpMenuHandler.cs");
            SendFELoaded();
        }

        private void SendFELoaded()
        {
            Debug.Log("SendFELoaded called in TcpMenuHandler.cs");

            JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
            json.AddField("msg_type", "scene_selection_ready");
            json.AddField("loaded", "1");

            client.SendMsg(json);
        }

        void OnGetSceneNames(JSONObject jsonObject)
        {
            Debug.Log("OnGetSceneNames called in TcpMenuHandler.cs");

            SendSceneNames();
        }

        private void SendSceneNames()
        {
            Debug.Log("SendSceneNames called in TcpMenuHandler.cs");
            JSONObject scenes = new JSONObject(JSONObject.Type.ARRAY);

            foreach (string scene_name in scene_names)
            {
                scenes.Add(scene_name);
            }

            JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
            json.AddField("scene_names", scenes);
            json.AddField("msg_type", "scene_names");

            client.SendMsg(json);
        }

        void OnLoadScene(JSONObject jsonObject)
        {
            Debug.Log("OnLoadScene" + jsonObject.ToString());
            GlobalState.bAutoHideSceneMenu = true;

            // since we know this is called only from a network client,
            // we can also infer that we don't want to auto create 
            Debug.Log("OnLoadScene bAutoHideSceneMenu = true");
            GlobalState.bCreateCarWithoutNetworkClient = false;
            Debug.Log("OnLoadScene bCreateCarWithoutNetworkClient = false");
            string scene_name = jsonObject.GetField("scene_name").str;
            Debug.Log("OnLoadScene scene_name = " + scene_name);
            LoadScene(scene_name);
            Debug.Log("OnLoadScene LoadScene(scene_name)");
        }

        public void LoadScene(string scene_name)
        {
            Debug.Log("LoadScene called in TcpMenuHandler.cs");
            // check wether the scene_name is in the scene_names list, if so, load it
            if (Array.Exists(GlobalState.sceneNames, element => element == scene_name))
            {
                loader.LoadScene(scene_name);
                Debug.Log("loaded scene");
            }
        }

        void OnQuitApp(JSONObject json)
        {
            Debug.Log("OnQuitApp called in TcpMenuHandler.cs");
            Application.Quit();
        }

        void AddButtonToMenu(string scene_path)
        {
            Debug.Log("AddButtonToMenu called in TcpMenuHandler.cs");
            string[] split_scene_name = scene_path.Split('/');
            split_scene_name = split_scene_name[split_scene_name.Length - 1].Split('.');
            string scene_name = split_scene_name[0]; // get the scene name (last part of the path and remove the .unity extension)

            // create a new button and add it to the grid layout
            GameObject go = Instantiate(ButtonPrefab);

            go.name = scene_name;
            go.transform.SetParent(ButtonGridLayout.transform);
            go.transform.localScale = Vector3.one;

            // add a function to be called when the button is clicked
            Button button = go.GetComponent<Button>();
            button.onClick.AddListener(delegate { LoadScene(scene_path); });

            // modify the text to match the scene_name
            GameObject text_go = go.transform.GetChild(0).gameObject;
            Text text = text_go.GetComponent<Text>();
            text.text = scene_name;

            // try to load the preview image located in the Resources folder
            Texture2D texture = Resources.Load<Texture2D>("UI/" + scene_name);
            if (texture != null)
            {
                GameObject image_go = go.transform.GetChild(1).gameObject;
                RawImage raw_image = image_go.GetComponent<RawImage>();
                raw_image.texture = texture;

            }
        }
    }
}
