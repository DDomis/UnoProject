using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        Socket server;
        Thread atender;
        Form2 activeForm2 = null; // Store reference to the game form

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void AtenderServidor()
        {
            while (true)
            {
                // Receive message from server
                byte[] msg2 = new byte[2048]; // Increased buffer size for lists
                int bytesRec = server.Receive(msg2);
                string receivedData = Encoding.ASCII.GetString(msg2, 0, bytesRec);
                
                string[] trozos = receivedData.Split('/');
                int codigo = Convert.ToInt32(trozos[0]);
                string mensaje;

                if (activeForm2 != null)
                {
                    switch (codigo)
                    {
                        case 1:  // Register response: 1/YES or 1/NO
                            mensaje = trozos[1].Split('\0')[0];
                            activeForm2.OnRegisterResponse(mensaje);
                            break;

                        case 2:  // Login response: 2/YES or 2/NO
                            mensaje = trozos[1].Split('\0')[0];
                            activeForm2.OnLoginResponse(mensaje);
                            break;

                        case 3: // Match History: 3/Line1/Line2...
                            // Pass the whole array except the first element (code)
                            activeForm2.OnHistoryResponse(trozos.Skip(1).ToArray());
                            break;

                        case 4: // Leaderboard: 4/Line1/Line2...
                            activeForm2.OnLeaderboardResponse(trozos.Skip(1).ToArray());
                            break;
                            
                        case 5: // Match Results: 5/Line1/Line2...
                             activeForm2.OnMatchResultsResponse(trozos.Skip(1).ToArray());
                             break;
                    }
                }
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            // Connect to VM IP (Updated to 192.168.56.101 based on previous context)
            IPAddress direc = IPAddress.Parse("192.168.56.101");
            IPEndPoint ipep = new IPEndPoint(direc, 9050);

            // Create socket
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                server.Connect(ipep);
                this.BackColor = Color.Green;
                MessageBox.Show("Connected to UNO Server");

                // Start listener thread
                ThreadStart ts = delegate { AtenderServidor(); };
                atender = new Thread(ts);
                atender.Start();
                
                btnOpenGame.Enabled = true;
            }
            catch (SocketException ex)
            {
                MessageBox.Show("Could not connect to server: " + ex.Message);
                return;
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            // Disconnect message
            string mensaje = "0/";
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
            server.Send(msg);

            // Close connection
            if (atender != null && atender.IsAlive) atender.Abort();
            this.BackColor = Color.Gray;
            server.Shutdown(SocketShutdown.Both);
            server.Close();
            btnOpenGame.Enabled = false;
        }

        private void btnOpenGame_Click(object sender, EventArgs e)
        {
            if (activeForm2 == null || activeForm2.IsDisposed)
            {
                activeForm2 = new Form2(server);
                activeForm2.Show();
            }
            else
            {
                activeForm2.BringToFront();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (server != null && server.Connected)
            {
                string mensaje = "0/";
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
                server.Send(msg);
                if (atender != null && atender.IsAlive) atender.Abort();
                server.Shutdown(SocketShutdown.Both);
                server.Close();
            }
        }
    }
}
