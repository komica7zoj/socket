using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace socket
{
    public class hello
    {

        public static Form1 f;
    }
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        static Thread thread2;
        static Thread thread;
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            thread2 = new Thread(new ThreadStart(SecondFoo2));
            thread2.SetApartmentState(ApartmentState.STA);
            thread2.IsBackground = true;
            thread2.Start();
            thread = new Thread( new ThreadStart( SecondFoo));
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();

            Form1 f = new Form1(thread, thread2);
            hello.f = f;

            Application.Run(f);

        }
        [STAThread]
        static void SecondFoo()
        {
            //    DefaultNamespace.SocketServer s = new DefaultNamespace.SocketServer();
            //   s.ShowDialog();


            socketclient.AsynchronousClient.StartClient();
            //   Application.EnableVisualStyles();
            //    Application.SetCompatibleTextRenderingDefault(false);
            //  Application.Run(new DefaultNamespace.SocketServer());

        }
        [STAThread]
        static void SecondFoo2()
        {
            //  DefaultNamespace.SocketClient s = new DefaultNamespace.SocketClient();
            //   s.ShowDialog();
            socketserver. AsynchronousSocketListener.StartListening();
            // Application.EnableVisualStyles();
            // Application.SetCompatibleTextRenderingDefault(false);
            // Application.Run(new DefaultNamespace.SocketClient());
        }
    }
}
