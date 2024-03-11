using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;

namespace tk
{
    // Wrap a tk.TcpClient and dispatcher to handle network events over a tcp connection.
    // Use Json for message contents. Assumes no newline characters in the json string contents,
    // and end each packet with a newline for separation.
    [RequireComponent(typeof(tk.TcpClient))]
    public class JsonTcpClient : MonoBehaviour {

        // Our reference to the required 'base' component
        private tk.TcpClient client;

        // This allows other objects to register for incoming json messages
        public tk.Dispatcher dispatcher;

        // Some messages need to be handled in the main thread. Unity object creation, etc..
        public bool dispatchInMainThread = false;
    
        // A list of raw json strings received from network and waiting to dispatched locally.
        private List<string> recv_packets;

        // Make sure to protect our recv_packets from race conditions
        readonly object _locker = new object();

        //required for stream parsing where client may recv multiple messages at once.
        const string packetTerminationChar = "\n";

        private EndPoint _remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        void Awake()
        {
            recv_packets = new List<string>();
            dispatcher = new tk.Dispatcher();
            dispatcher.Init();
            client = GetComponent<tk.TcpClient>();
    
            Initcallbacks();

        }

        // Interact with our base TcpClient to handle incoming data
        void Initcallbacks()
        {
            client.onDataRecvCB += OnDataRecv ;
            recv_packets.Add("{\"msg_type\" : \"connected\"}");
        }

        // Send a json packet over our TCP socket asynchronously.
        public void SendMsg(JSONObject msg)
        {
            string packet = msg.ToString() + packetTerminationChar;

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(packet);
            Debug.Log("[JsonTcpClient] Sending packet to [TcpClient] : " + packet);
            client.SendData( bytes );
        }


        // Our callback from the TCPClient to get data.
        void OnDataRecv(byte[] bytes)
        {
            string str = System.Text.Encoding.UTF8.GetString(bytes);
            
            lock(_locker)
            {
                recv_packets.Add(str);
            }

            if(!dispatchInMainThread)
            {
                Dispatch();
            }
        }

        // Over simplified algorithm to extract json payload from TCP stream. It assumes that there are no nested JSON objects
        List<string> ExtractJsonFromStream()
        {
            List<string> result = new List<string>();
            string jsonBuffer = "";

            // Ignore request if TCP buffer list is empty
            if (recv_packets.Count == 0) {
                return result;
            }

            // Concat all received TCP buffers to have a chance to extract as much as possible JSON objects
            foreach (string str in recv_packets)
            {
                jsonBuffer = String.Concat(jsonBuffer, str);
            }

            recv_packets.Clear();

            // Initialize a counter for the number of opening and closing braces
            int openBraces = 0;
            int closeBraces = 0;
            int startIndex = 0;

            // Iterate through the buffer to find complete JSON objects
            for (int i = 0; i < jsonBuffer.Length; i++)
            {
                if (jsonBuffer[i] == '{')
                {
                    openBraces++;
                }
                else if (jsonBuffer[i] == '}')
                {
                    closeBraces++;
                }

                // If the number of opening and closing braces match, we have a complete JSON object
                if (openBraces == closeBraces && openBraces > 0)
                {
                    // Extract the JSON object and add it to the result list
                    string jsonObject = jsonBuffer.Substring(startIndex, i - startIndex + 1);
                    result.Add(jsonObject);

                    // Reset the counters and start index for the next JSON object
                    openBraces = 0;
                    closeBraces = 0;
                    startIndex = i + 1;
                }
            }

            // If there is any remaining data in the buffer, add it back to recv_packets
            if (startIndex < jsonBuffer.Length)
            {
                recv_packets.Add(jsonBuffer.Substring(startIndex));
            }

            return result;
        }

        // Send each queued json packet to the recipient which registered
        // with our dispatcher.
        void Dispatch()
        {
            lock(_locker)
            {
                List<string> msgs = ExtractJsonFromStream();
                foreach (string msg in msgs)
                {
                    try
                    {
                        //Only extract and propagate the last one to avoid to overload simulator in case of burst
                        Debug.Log("[JsonTcpClient] Message extracted from json stream: " + msg);
                        JSONObject j = new JSONObject(msg);

                        string msg_type = j["msg_type"].str;

                        // Debug.Log("Got: " + msg_type);
                        Debug.Log("[JsonTcpClient] Dispatching message of type: " + msg_type);
                        dispatcher.Dipatch(msg_type, j);

                    }
                    catch(Exception e)
                    {
                        Debug.LogError(e.ToString());
                    }

                }
            }
        }


        // Optionally poll our dispatch queue in the main thread context
        void Update()
        {
            if (dispatchInMainThread)
            {
                Dispatch();
            }
        }
    }
}