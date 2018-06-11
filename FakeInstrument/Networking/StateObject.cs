using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace FakeInstrument.Networking
{
    internal class StateObject
    {
        public const int DefaultBufferSize = 4096;

        public TcpClient Client { get; }

        public byte[] Buffer { get; }

        public int BufferSize => this.Buffer.Length;

        public StateObject(TcpClient tcpClient, int bufferSize = DefaultBufferSize)
        {
            this.Client = tcpClient;
            this.Buffer = new byte[bufferSize];
        }
    }
}
