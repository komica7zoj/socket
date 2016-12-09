using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace socket
{



    public partial class Form1 : Form
    {
        //    TcpClient tcpClient = new TcpClient("10.100.51.117", 2000);

        Thread thread;
        Thread thread2;

        public Form1( Thread thread, Thread thread2)
        {
            InitializeComponent();
            this.thread = thread;
            this.thread2 = thread2;

        }
        public void closed(object sender, System.EventArgs e)
        {

            if (thread.IsAlive && thread != null) thread.Abort();

            if (thread2.IsAlive && thread2 != null) thread2.Abort();
            try
            {
                this.Close();
            }
            catch (Exception)
            {

            }
        }

        delegate byte[] sendoutfile();

        public byte[] sendfile()
        {
            OpenFileDialog open = new OpenFileDialog();

            if (open.ShowDialog() == DialogResult.OK)
            {
                return System.IO.File.ReadAllBytes(open.FileName);
            }
            return new byte[] { };
        }

        public byte[] sendout()
        {
            sendoutfile sf = new sendoutfile(sendfile);
            if (this.InvokeRequired)
            {
                this.BeginInvoke(sf);
            }
            return sf();
        }

        delegate void testmessages(string s);
        public void message(string s)
        {
            MessageBox.Show(s);
        }
        public void showmessage(string s)
        {
            testmessages t = new testmessages(message);

            if (this.InvokeRequired)
                this.BeginInvoke(t, new object[] { s });
        }
    }
}
