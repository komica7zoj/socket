// Asynchronous Client Socket Example
// http://msdn.microsoft.com/en-us/library/bew39x2a.aspx

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace socketclient
{
    // State object for receiving data from remote device.
    public class Client
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 256;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

    public static class AsynchronousClient
    {
        // The port number for the remote device.
        private const int port = 11000;

        // ManualResetEvent instances signal completion.
        private static ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        // The response from the remote device.
        private static String response = String.Empty;


        /* public  AsynchronousClient()
            { 
               //... 
        }*/
        [STAThread]
        public static void StartClient()
        {

            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                // The name of the 
                // remote device is "host.contoso.com".
                //        IPHostEntry ipHostInfo = Dns.Resolve("host.contoso.com");
                //       IPAddress ipAddress = ipHostInfo.AddressList[0];



                IPAddress ipAddress = IPAddress.Parse("10.100.51.117");
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);


                Socket client = null;

                // Connect to the remote endpoint.
                // Send test data to the remote device.
                // Create a TCP/IP socket.
                while (true)
                {
                    try
                    {

                        client = new Socket(AddressFamily.InterNetwork,
                        SocketType.Stream, ProtocolType.Tcp);

                        client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
                        connectDone.WaitOne();

                        //     if (!client.Connected) client.Connect(remoteEP);
                        OpenFileDialog open = new OpenFileDialog();
                        if (open.ShowDialog() == DialogResult.OK)
                            
                        {
                            using (Stream source = File.OpenRead(open.FileName))
                            {
                                Send(client, Encoding.ASCII.GetBytes("<SOF>"+ Path.GetExtension(open.FileName)));
                                sendDone.WaitOne();
                                byte[] buffer = new byte[1024];
                                int bytesRead;

                                while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                                {

                                    Send(client, buffer);
                                    sendDone.WaitOne();


                                }



                            }


                            Send(client, Encoding.ASCII.GetBytes("<EOF>"));
                            sendDone.WaitOne();
                            // Send(client, Encoding.ASCII.GetBytes("<EOF>"));
                            Thread.Sleep(10000);
                            //Thread.Sleep(10);
                            // Receive the response from the remote device.
                            Receive(client);
                            receiveDone.WaitOne();

                            // Write the response to the console.
                            socket.hello.f.showmessage("Response received : {0}" + response + "Client by StartClient() ");
                            // Release the socket.

                            //Thread.Sleep(1000);
                        }
                    
                        else
                        {
                            Thread.Sleep(100000);
                        }


                    }
                    catch (Exception e)
                    {
                        socket.hello.f.showmessage(e.Message + " Client by StartClient() error");

                        if (!client.Connected)
                        {
                            client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
                            connectDone.WaitOne();
                        }

                    }

                }

            }
            catch (Exception e)
            {
                socket.hello.f.showmessage(e.Message + "Client StartClient error");

            }


        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                //      socket.hello.f.showmessage("Client Socket connected to {0}" +
                //           client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception e)
            {
                socket.hello.f.showmessage(e.Message + "Client ConnectCallback error ");
            }
        }

        private static void Receive(Socket client)
        {
            try
            {
                // Create the state object.
                Client state = new Client();
                state.workSocket = client;

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, Client.BufferSize, 0,
                new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                socket.hello.f.showmessage(e.Message + "Client Receive error");
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                Client state = (Client)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Get the rest of the data.
                    client.BeginReceive(state.buffer, 0, Client.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.
                    if (state.sb.Length > 1)
                    {
                        response = state.sb.ToString();
                    }
                    // Signal that all bytes have been received.
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
               socket.hello.f.showmessage(e.Message + "Client ReceiveCallback error");
            }
        }

        private static void Send(Socket client, byte[] data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = data;
            // Begin sending the data to the remote device.
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

               // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
           //     socket.hello.f.showmessage("Sent {0} bytes to server." + bytesSent + "Client SendCallback ");

                // Signal that all bytes have been sent.
                sendDone.Set();

            }
            catch (Exception e)
            {
                socket.hello.f.showmessage(e.Message + "Client SendCallback error");
            }

        }

    }

}