using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;

namespace tk
{
    public class TcpClient : MonoBehaviour
    {
        public bool debug = false;

        private byte[] _receiveBuffer = new byte[8142];
        IPEndPoint _remoteEndPointReceive;
        IPEndPoint _remoteEndPointSend;

        public delegate void OnDataRecv(byte[] data);

        public OnDataRecv onDataRecvCB;

        UdpClient udpClient;

        public void Start(){
            this.connect(GlobalState.host, GlobalState.port, GlobalState.portPrivateAPI);
        }

        public void connect(string ip, int receivePort, int sendPort)
        {        
            if (ip == "0.0.0.0" ||  ip == "::0"){
                Debug.Log("[TcpClient] network parameter set to any address on port " + ": " + receivePort);
                _remoteEndPointReceive = new IPEndPoint(IPAddress.Any, receivePort);
                _remoteEndPointSend = new IPEndPoint(IPAddress.Any, sendPort);
            }
            else{
                Debug.Log("[TcpClient] network parameter set to " + ip + ":" + receivePort);
                _remoteEndPointReceive = new IPEndPoint(IPAddress.Parse(ip), receivePort);
                _remoteEndPointSend = new IPEndPoint(IPAddress.Parse(ip), sendPort);
            }
            udpClient = new UdpClient(receivePort);

            //udpClientSend.Client.Bind(_remoteEndPointSend);

            Debug.Log("[TcpClient] Connecting to " + _remoteEndPointSend.ToString());
            udpClient.Connect(_remoteEndPointSend);
            Debug.Log("[TcpClient] Receiver Connected to " + _remoteEndPointSend.ToString());
            udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
            Debug.Log("[TcpClient] Begin receive");
        }

        void OnDestroy()
        {
            Debug.Log("[TcpClient] OnDestroy called");
            if (udpClient != null)
            {
                udpClient.Close();
            }

        }

        public bool SendData(byte[] data)
        {
            try
            {
                // If connected, send data to the connected endpoint
                Debug.Log("[TcpClient] Sending data to connected endpoint :" + _remoteEndPointSend.ToString()) ;
                udpClient.Send(data, data.Length);
                
                return true;
            }
            catch (SocketException e)
            {
                Debug.LogWarning("[TcpClient] Error sending data: " + e.Message);
                return false;
            }
        }   

        public bool SendDataToPeers(byte[] data)
        {
            Debug.Log("[TcpClient] sending data to the server");
            this.SendData(data);
            return true;
        }

        public void SetDebug(bool d)
        {
            debug = d;
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            Debug.Log("[TcpClient] Receive callback called.");
            try
            {
                Debug.Log("[TcpClient] Receive callback called.");
                // End the receive operation
                byte [] data  = udpClient.EndReceive(ar, ref _remoteEndPointReceive);
                Debug.Log("[TcpClient] End receive ");
                // Invoke the callback with the received data
                // call to send to peers ! 
                Debug.Log("[TcpClient] Invoking callback to handle messages");
                onDataRecvCB?.Invoke(data);
                Debug.Log("[TcpClient] Callback invoked");

                udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
                Debug.Log("[TcpClient] Begin receive after sending data ");

            }
            catch (Exception e)
            {
                if (debug)
                {
                    Debug.LogError("[TcpClient] Error receiving data: " + e);
                }
            }
        }

        
    }
}