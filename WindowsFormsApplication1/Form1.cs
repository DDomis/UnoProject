using System;
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
        Form2 activeForm2 = null;
        bool isListening = false;
        bool isDisconnecting = false;
        bool consoleRequestedDisconnect = false;
        string receiveBuffer = "";

        public Form1()
        {
            InitializeComponent();
            ApplyLauncherStyle();
        }

        private void Form1_Load(object sender, EventArgs e) { }

        private void ApplyLauncherStyle()
        {
            this.BackColor = Color.FromArgb(18, 115, 65);
            this.Font = new Font("Segoe UI", 9F);
            lblTitle.ForeColor = Color.White;
            lblTitle.Font = new Font("Segoe UI Black", 18F, FontStyle.Bold);
            StyleButton(btnConnect, Color.FromArgb(238, 42, 36), Color.White);
            StyleButton(btnOpenGame, Color.FromArgb(245, 199, 34), Color.Black);
            StyleButton(btnDisconnect, Color.FromArgb(31, 31, 31), Color.White);
        }

        private void StyleButton(Button button, Color backColor, Color foreColor)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = backColor;
            button.ForeColor = foreColor;
        }

        private void AtenderServidor()
        {
            while (isListening)
            {
                try
                {
                    byte[] msg2 = new byte[2048];
                    int bytesRec = server.Receive(msg2);
                    if (bytesRec == 0)
                    {
                        BeginInvoke((MethodInvoker)delegate { HandleServerClosed(); });
                        break;
                    }

                    receiveBuffer += Encoding.ASCII.GetString(msg2, 0, bytesRec);
                    int lineEnd;
                    while ((lineEnd = receiveBuffer.IndexOf('\n')) >= 0)
                    {
                        string message = receiveBuffer.Substring(0, lineEnd).Trim('\r');
                        receiveBuffer = receiveBuffer.Substring(lineEnd + 1);
                        if (message.Length == 0) continue;
                        DispatchServerMessage(message);
                    }
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (SocketException)
                {
                    if (isListening)
                    {
                        BeginInvoke((MethodInvoker)delegate { HandleServerClosed(); });
                    }
                    break;
                }
                catch
                {
                    break;
                }
            }
        }

        private void DispatchServerMessage(string receivedData)
        {
            string[] trozos = receivedData.Split('/');

            int codigo;
            if (trozos.Length == 0 || !int.TryParse(Clean(trozos[0]), out codigo)) return;

            BeginInvoke((MethodInvoker)delegate
            {
                if (activeForm2 == null || activeForm2.IsDisposed) return;

                switch (codigo)
                {
                    case 1:
                        activeForm2.OnRegisterResponse(trozos.Length > 1 ? Clean(trozos[1]) : "NO");
                        break;
                    case 2:
                        activeForm2.OnLoginResponse(trozos.Length > 1 ? Clean(trozos[1]) : "NO");
                        break;
                    case 3:
                        activeForm2.OnHistoryResponse(trozos.Skip(1).ToArray());
                        break;
                    case 4:
                        activeForm2.OnLeaderboardResponse(trozos.Skip(1).ToArray());
                        break;
                    case 5:
                        activeForm2.OnMatchResultsResponse(trozos.Skip(1).ToArray());
                        break;
                    case 6:
                        activeForm2.UpdateConnectedList(trozos.Skip(1).ToArray());
                        break;
                    case 8:
                        activeForm2.OnInvitationReceived(trozos.Length > 1 ? Clean(trozos[1]) : "");
                        break;
                    case 10:
                        if (trozos.Length > 1 && Clean(trozos[1]) == "1")
                        {
                            int gId = -1;
                            if (trozos.Length > 2) int.TryParse(Clean(trozos[2]), out gId);
                            activeForm2.OnInvitationResult("1", gId);
                        }
                        else
                        {
                            activeForm2.OnInvitationResult("0", -1);
                        }
                        break;
                    case 11:
                        Form3 actGame = null;
                        string chatUser = "";
                        string chatText = "";
                        int chatGameId = -1;
                        if (trozos.Length > 3 && int.TryParse(Clean(trozos[1]), out chatGameId))
                        {
                            actGame = FindGameForm(chatGameId);
                            chatUser = Clean(trozos[2]);
                            chatText = Clean(trozos[3]);
                        }
                        else if (trozos.Length > 2)
                        {
                            actGame = Application.OpenForms.OfType<Form3>().FirstOrDefault();
                            chatUser = Clean(trozos[1]);
                            chatText = Clean(trozos[2]);
                        }
                        if (actGame != null) actGame.UpdateChat(chatUser, chatText);
                        break;
                    case 12:
                        int stateGameId = -1;
                        if (trozos.Length > 1) int.TryParse(Clean(trozos[1]), out stateGameId);
                        Form3 stateGame = FindGameForm(stateGameId);
                        if (stateGame != null)
                        {
                            stateGame.UpdateGameState(trozos);
                        }
                        break;
                    case 16:
                        int finishedGameId = -1;
                        if (trozos.Length > 2) int.TryParse(Clean(trozos[1]), out finishedGameId);
                        Form3 finishedGame = FindGameForm(finishedGameId) ?? Application.OpenForms.OfType<Form3>().FirstOrDefault();
                        if (finishedGame != null)
                        {
                            finishedGame.OnGameOver(trozos.Length > 2 ? Clean(trozos[2]) : (trozos.Length > 1 ? Clean(trozos[1]) : "Unknown"));
                        }
                        break;
                    case 17:
                        int errorGameId = -1;
                        if (trozos.Length > 2) int.TryParse(Clean(trozos[1]), out errorGameId);
                        Form3 errorGame = FindGameForm(errorGameId) ?? Application.OpenForms.OfType<Form3>().FirstOrDefault();
                        if (errorGame != null)
                        {
                            errorGame.ShowServerMessage(trozos.Length > 2 ? Clean(trozos[2]) : (trozos.Length > 1 ? Clean(trozos[1]) : "Invalid action"));
                        }
                        break;
                    case 18:
                        Form3 disconnectedGame = Application.OpenForms.OfType<Form3>().FirstOrDefault();
                        string message = trozos.Length > 1 ? Clean(trozos[1]) : "The other player disconnected.";
                        if (disconnectedGame != null) disconnectedGame.ShowServerMessage(message);
                        MessageBox.Show(message);
                        break;
                    case 19:
                        activeForm2.OnDeleteAccountResponse(
                            trozos.Length > 1 ? Clean(trozos[1]) : "0",
                            trozos.Length > 2 ? Clean(trozos[2]) : "Delete account failed");
                        break;
                }
            });
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (IsSocketUsable())
            {
                MessageBox.Show("Already connected to UNO Server");
                return;
            }

            // IPAddress direc = IPAddress.Parse("192.168.56.101");
            // IPAddress direc = IPAddress.Parse("10.4.119.5");

            // Correct if you are using:
            // ssh -L 9050:127.0.0.1:9050 domenico.lenzerini@shiva
            IPAddress direc = IPAddress.Parse("127.0.0.1");

            IPEndPoint ipep = new IPEndPoint(direc, 9050);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                server.Connect(ipep);
                isListening = true;
                isDisconnecting = false;
                receiveBuffer = "";
                this.BackColor = Color.FromArgb(0, 146, 70);
                btnOpenGame.Enabled = true;

                atender = new Thread(AtenderServidor);
                atender.IsBackground = true;
                atender.Start();

                MessageBox.Show("Connected to UNO Server");
            }
            catch (SocketException ex)
            {
                MessageBox.Show("Connection failed: " + ex.Message);
            }
        }

        private Form3 FindGameForm(int gameId)
        {
            return Application.OpenForms.OfType<Form3>().FirstOrDefault(f => f.GameId == gameId);
        }

        private void btnOpenGame_Click(object sender, EventArgs e)
        {
            if (!IsSocketUsable())
            {
                MessageBox.Show("Connect to the server first.");
                return;
            }

            if (activeForm2 == null || activeForm2.IsDisposed)
            {
                activeForm2 = new Form2(server, this);
                activeForm2.FormClosed += ActiveForm2_FormClosed;
                activeForm2.Show();
                this.Hide();
            }
            else
            {
                activeForm2.BringToFront();
            }
        }

        private void ActiveForm2_FormClosed(object sender, FormClosedEventArgs e)
        {
            activeForm2 = null;
            ResetLauncherState();
            if (!IsDisposed && (IsSocketUsable() || consoleRequestedDisconnect)) this.Show();
            consoleRequestedDisconnect = false;
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            DisconnectFromServer(true);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DisconnectFromServer(true);
        }

        public bool IsSocketUsable()
        {
            try
            {
                return server != null && server.Connected;
            }
            catch
            {
                return false;
            }
        }

        public void DisconnectFromServer(bool notifyServer)
        {
            if (isDisconnecting) return;
            isDisconnecting = true;
            isListening = false;

            try
            {
                if (server != null && server.Connected)
                {
                    if (notifyServer) server.Send(Encoding.ASCII.GetBytes("0/\n"));
                    server.Shutdown(SocketShutdown.Both);
                }
            }
            catch { }

            try
            {
                if (server != null) server.Close();
            }
            catch { }

            ResetLauncherState();
            receiveBuffer = "";
            isDisconnecting = false;
        }

        public void OnConsoleRequestedDisconnect()
        {
            consoleRequestedDisconnect = true;
            DisconnectFromServer(true);
            if (activeForm2 != null && !activeForm2.IsDisposed)
            {
                activeForm2.Close();
            }
            this.Show();
        }

        private void HandleServerClosed()
        {
            DisconnectFromServer(false);
            if (activeForm2 != null && !activeForm2.IsDisposed)
            {
                activeForm2.Close();
            }
            MessageBox.Show("The server disconnected.");
            this.Show();
        }

        private void ResetLauncherState()
        {
            this.BackColor = Color.FromArgb(18, 115, 65);
            btnOpenGame.Enabled = IsSocketUsable();
        }

        private string Clean(string text)
        {
            return (text ?? "").Replace("\0", "").Trim();
        }
    }
}
