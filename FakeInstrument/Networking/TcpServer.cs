using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FakeInstrument.Networking
{
    public class TcpServer
    {
        public event Action<byte[], TcpClient> DataReceived;

        private TcpListener tcpListener;

        private ushort port;

        private int receiveBufferSize;

        private bool started;

        public TcpServer(ushort port, int bufferSize = StateObject.DefaultBufferSize)
        {
            this.port = port;

            this.receiveBufferSize = bufferSize;

            this.started = false;
        }

        public void Start()
        {
            if (!started)
            {
                this.tcpListener = new TcpListener(IPAddress.Any, this.port);
                this.tcpListener.Start();

                this.tcpListener.BeginAcceptTcpClient(this.AcceptCallback, null);

                this.started = true;
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            if (started)
            {
                try
                {
                    var tcpClient = this.tcpListener.EndAcceptTcpClient(ar);

                    var stateObject = new StateObject(tcpClient);

                    tcpClient.Client.BeginReceive(stateObject.Buffer, 0, stateObject.BufferSize, SocketFlags.None, this.ReceiveCallBack, stateObject);

                    this.tcpListener.BeginAcceptTcpClient(this.AcceptCallback, null);
                }
                catch (Exception exception)
                {

                }
            }
        }

        private void ReceiveCallBack(IAsyncResult ar)
        {
            if (started)
            {
                try
                {
                    StateObject so = (StateObject)ar.AsyncState;

                    int bytesRead = so.Client.Client.EndReceive(ar);

                    var receivedBytes = new byte[bytesRead];

                    Array.Copy(so.Buffer, receivedBytes, bytesRead);

                    System.Diagnostics.Debug.WriteLine("Message received: " + Encoding.ASCII.GetString(receivedBytes));

                    var stateObject = new StateObject(so.Client, this.receiveBufferSize);

                    so.Client.Client.BeginReceive(stateObject.Buffer, 0, stateObject.BufferSize, SocketFlags.None, this.ReceiveCallBack, stateObject);

                    this.DataReceived?.Invoke(receivedBytes, so.Client);
                }
                //catch (ObjectDisposedException)
                //{
                //}
                catch (Exception exception)
                {
                    this.started = false;
                }
            }
        }

        public void Stop()
        {
            if (started)
            {
                this.tcpListener.Server.Dispose();

                this.started = false;
            }
        }
    }
}
