using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WGPM.R.Logger;

namespace WGPM.R.TcpComm
{
    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 21;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }
    /// <summary>
    /// 201709251317 SocketHelper 不允许修改
    /// </summary>
    public class SocketHelper
    {
        // Thread signal.
        public ManualResetEvent allDone = new ManualResetEvent(false);
        public SocketHelper()
        {
        }
        public static int Addr1 { get; set; }
        public static int Addr2 { get; set; }
        public void StartListening()
        {
            // Data buffer for incoming data.
            //byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // The DNS name of the computer
            // running the listener is "host.contoso.com".
            //IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = GetLocalIP();
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 10001);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(2);
                Thread trd = new Thread(() =>
                {
                    while (true)
                    {
                        // Set the event to nonsignaled state.
                        allDone.Reset();
                        listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                        // Wait until a connection is made before continuing.
                        allDone.WaitOne();
                    }
                })
                { IsBackground = true };
                trd.Start();
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
            }
        }
        public void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();
            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }
        public void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            try
            {
                // Read data from the client socket. 
                int bytesRead = handler.EndReceive(ar);
                if (bytesRead > 0 && state.buffer[5] == 15 && state.buffer[6] == 255)
                {
                    int addr = BitConverter.ToInt32(new byte[] { state.buffer[14], state.buffer[13], state.buffer[16], state.buffer[15] }, 0);
                    bool validData = (addr >= 35000 && addr <= 895000) || (addr >= 985000 && addr <= 1845000);
                    if (handler.RemoteEndPoint.ToString().Split(':')[0] == "192.168.0.154")
                    {//3# 熄焦车
                        Addr1 = validData ? addr : Addr1;
                    }
                    else if (handler.RemoteEndPoint.ToString().Split(':')[0] == "192.168.0.164")
                    {
                        Addr2 = validData ? addr : Addr2;
                    }
                }
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            }
            catch (Exception err)
            {
                Log.LogErr.Info(err.Message + "\n" + err.ToString());
                handler.Close();
                //StartListening();
            }
        }
        private void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
        }
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;
                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                //Console.WriteLine("Sent {0} bytes to client.", bytesSent);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
            }
        }
        private IPAddress GetLocalIP()
        {
            IPAddress ip = null;
            IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());
            for (int i = 0; i < ips.Length; i++)
            {
                if (ips[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    ip = ips[i];
                    break;
                }
            }
            return ip;
        }
    }
}
