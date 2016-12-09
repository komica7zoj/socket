// Asynchronous Server Socket Example
// http://msdn.microsoft.com/en-us/library/fx6588te.aspx

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace socketserver
{
    // State object for reading client data asynchronously
    public class Server
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

    public class AsynchronousSocketListener
    {
        // Thread signal.
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        static System.IO.FileStream fs;
        static string filetype = "";
        static bool iscannel = false;
        static void savefile()

        {
            SaveFileDialog savedialog = new SaveFileDialog();
            savedialog.Filter = filetype + " file (*" + filetype + ")|" + filetype + "|All files(*.*)|*.*";
            if (savedialog.ShowDialog() == DialogResult.OK)
            {
                if (savedialog.FileName != "")
                {
                    fs = (System.IO.FileStream)savedialog.OpenFile();
                    fs.Seek(0, SeekOrigin.Begin);
                }

            }
            else
            {
                iscannel = true;
            }
        }
        /*      public AsynchronousSocketListener()
              {
              }*/
        [STAThread]
        public static void StartListening()
        {

            // Data buffer for incoming data.
            byte[] bytes = new byte[1024];

            // Establish the local endpoint for the socket.
            // The DNS name of the computer
            // running the listener is "host.contoso.com".
            // IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            //IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPAddress ipAddress = IPAddress.Parse("10.100.51.117");
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);
                while (true)
                {
                    try
                    {
                        // Set the event to nonsignaled state.
                        allDone.Reset();

                        // Start an asynchronous socket to listen for connections.
                        //    socket.hello.f.showmessage(" Server Waiting for a connection...");
                        listener.BeginAccept(
                            new AsyncCallback(AcceptCallback),
                            listener);

                        // Wait until a connection is made before continuing.
                        allDone.WaitOne();
                    }
                    catch (Exception e)
                    {

                    }
                }

            }
            catch (Exception e)
            {
                socket.hello.f.showmessage(e.Message);
            }

            //   socket.hello.f.showmessage("\nPress ENTER to continue...");


        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);


            // Create the state object.
            Server state = new Server();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, Server.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }
        [STAThread]
        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            Server state = (Server)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                if (Encoding.ASCII.GetString(state.buffer, 0, bytesRead).IndexOf("<SOF>") > -1)
                {

                    filetype = Encoding.ASCII.GetString(state.buffer, 5, bytesRead);
                    Thread save = new Thread(new ThreadStart(savefile));
                    save.SetApartmentState(ApartmentState.STA);
                    save.Start();
                    save.Join();
                    handler.BeginReceive(state.buffer, 0, Server.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
                else
                {
                    // There  might be more data, so store the data received so far.
                    //   state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Check for end-of-file tag. If it is not there, read 
                    // more data.
                    //      content = state.sb.ToString();
                    if (Encoding.ASCII.GetString(
                        state.buffer, 0, bytesRead).IndexOf("<EOF>") > -1)
                    {
                        if(iscannel)
                        {
                            Send(handler,"stop");
                        }
                        else
                        {
                            socket.hello.f.showmessage(bytesRead.ToString() + " and " + state.buffer.Length);

                            //      fs.Seek(0, SeekOrigin.Current);
                            //   fs.Write(state.buffer, 0, bytesRead - 5);
                            /*        fs.BeginWrite(
                    state.buffer, 0, bytesRead,
                    new AsyncCallback(EndWriteCallback),
                   fs);
                                    fs.Flush();*/
                            // All the data has been read from the 
                            // client. Display it on the console.
                            socket.hello.f.showmessage("Read {0} bytes from socket. \n Data : {1}" +
                                 content.Length + content + "Server ReadCallback");
                            // Echo the data back to the client.
                            Send(handler, content);
                            fs.Close();
                            fs = null;
                        }
                       



                    }
                    else
                    {
                        if (iscannel)
                        {
                            handler.BeginReceive(state.buffer, 0, Server.BufferSize, 0, new AsyncCallback(ReadCallback), state);

                        }
                        else
                        {
                            while (fs == null) ;
                            fs.Seek(0, SeekOrigin.Current);
                            fs.Write(state.buffer, 0, bytesRead);
                            /*                    fs.BeginWrite(
                                state.buffer, 0, bytesRead,
                                new AsyncCallback(EndWriteCallback),
                               fs);
                                                fs.Flush();*/
                            // Not all data received. Get more.
                            handler.BeginReceive(state.buffer, 0, Server.BufferSize, 0,
                            new AsyncCallback(ReadCallback), state);
                        }


                    }
                }
            }
        }
        private static void EndWriteCallback(IAsyncResult ar)
        {

            FileStream stream = (FileStream)ar.AsyncState;
            stream.Seek(0, SeekOrigin.Current);
            stream.EndWrite(ar);

        }
        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                socket.hello.f.showmessage("Sent {0} bytes to client." + bytesSent + " Server SendCallback");

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                socket.hello.f.showmessage(e.Message + "Server SendCallback error");
            }
        }



    }
}