using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections.Generic;

namespace tk
{   
    public class TcpServer : MonoBehaviour
    {

        UdpClient udpServer;

        // Accept thread
        Thread thread = null;

        // Thread signal.  
        public ManualResetEvent allDone = new ManualResetEvent(false);

        // Lock object to protect access to new_clients
        readonly object _locker = new object();

        private int _port;
        private IPEndPoint _endPoint;

        // Verbose messages
        public bool debug = false;


        // Call the Run method to start the server. The ip address is typically 127.0.0.1 to accept only local connections.
        // Or 0.0.0.0 to bind to all incoming connections for this NIC.
        public void Run(string ip, int port)
        {

            //Bind(ip, port);
            _port = port;
            if (_endPoint == null)
            {
                _endPoint = new IPEndPoint(IPAddress.Parse(ip), _port);
            }
        }

        // Stop the server. Will disconnect all clients and shutdown networking.
        public void Stop()
        {
            if (udpServer != null)
            {
                udpServer.Close();
                udpServer = null;
            }
        }

        // When GameObject is deleted..
        void OnDestroy()
        {
            Stop();
        }

        // SendData will broadcast send to all peers
        public void SendData(byte[] data)
        {
            if (udpServer != null)
            {
                udpServer.Send(data, data.Length, _endPoint);
            }
        }

        // Start listening for connections
        private void Bind(string ip, int port)
        {
            if (ip == "0.0.0.0" || ip == "::0")
            {
                Debug.LogError("[connect] Invalid IP address. Cannot bind to unspecified address.");
                _endPoint = new IPEndPoint(IPAddress.Parse(GlobalState.host), GlobalState.portPrivateAPI);
            }
            else{
                IPAddress ipAddress = IPAddress.Parse(ip);
                _endPoint = new IPEndPoint(ipAddress, GlobalState.portPrivateAPI);
            }

            Debug.Log("Binding to: " + _endPoint.ToString());
            udpServer = new UdpClient(port);
            Debug.Log("Server Bound to port : " + port.ToString());

            //udpServer.BeginReceive(ListenLoop, udpServer);

            //Debug.Log("Server Listening on: " + ip + ":" + port.ToString());
        }

        private void ListenLoop(IAsyncResult ar)
        {

            byte [] data = udpServer.Receive(ref _endPoint);
            Debug.Log("[ListenLoop] Received data from: " + _endPoint.ToString());

            string message = Encoding.ASCII.GetString(data);
            Debug.Log("[ListenLoop] Received data: " + message);

            this.SendData(data);
            Debug.Log("[ListenLoop] Sent data");


            udpServer.BeginReceive(ListenLoop, udpServer);
            Debug.Log("[ListenLoop] Begin receive again");
        }
    }

}