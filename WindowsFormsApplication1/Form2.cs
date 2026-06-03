using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;

namespace WindowsFormsApplication1
{
    public partial class Form2 : Form
    {
        Socket server;
        Form1 launcher;
        public string currentUsername = "";
        bool loginPending = false;
        bool isLoggedIn = false;
        bool isDisconnecting = false;
        bool isClosing = false;

        public Form2(Socket server, Form1 launcher)
        {
            InitializeComponent();
            this.server = server;
            this.launcher = launcher;
            listConnected.SelectionMode = SelectionMode.MultiSimple;
            ApplyLobbyStyle();
        }

        private void ApplyLobbyStyle()
        {
            this.BackColor = Color.FromArgb(18, 115, 65);
            this.Font = new Font("Segoe UI", 9F);
            gbLogin.ForeColor = Color.White;
            gbQueries.ForeColor = Color.White;
            lblStatus.ForeColor = Color.FromArgb(245, 199, 34);
            lstResults.BackColor = Color.FromArgb(31, 31, 31);
            lstResults.ForeColor = Color.White;
            listConnected.BackColor = Color.FromArgb(31, 31, 31);
            listConnected.ForeColor = Color.White;

            StyleButton(btnRegister, Color.FromArgb(0, 103, 179), Color.White);
            StyleButton(btnLogin, Color.FromArgb(238, 42, 36), Color.White);
            StyleButton(btnHistory, Color.FromArgb(245, 199, 34), Color.Black);
            StyleButton(btnLeaderboard, Color.FromArgb(0, 146, 70), Color.White);
            StyleButton(btnMatchResults, Color.FromArgb(31, 31, 31), Color.White);
            StyleButton(button2, Color.FromArgb(238, 42, 36), Color.White);
            StyleButton(btnDisconnectConsole, Color.FromArgb(31, 31, 31), Color.White);
            StyleButton(btnDeleteAccount, Color.FromArgb(120, 20, 20), Color.White);
        }

        private void StyleButton(Button button, Color backColor, Color foreColor)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }

        private bool SendRequest(string text)
        {
            try
            {
                if (server == null || !server.Connected) return false;
                if (!text.EndsWith("\n")) text += "\n";
                byte[] msg = Encoding.ASCII.GetBytes(text);
                server.Send(msg);
                return true;
            }
            catch
            {
                MessageBox.Show("Connection lost.");
                return false;
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            if (isLoggedIn || loginPending) return;
            string user = txtUser.Text.Trim();
            string pass = txtPass.Text;
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass)) return;
            SendRequest("1/" + user + "/" + pass);
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (isLoggedIn || loginPending) return;
            string user = txtUser.Text.Trim();
            string pass = txtPass.Text;
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass)) return;

            currentUsername = user;
            loginPending = true;
            btnLogin.Enabled = false;
            btnRegister.Enabled = false;

            if (!SendRequest("2/" + user + "/" + pass))
            {
                loginPending = false;
                btnLogin.Enabled = true;
                btnRegister.Enabled = true;
                currentUsername = "";
            }
        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentUsername)) return;
            SendRequest("3/" + currentUsername);
        }

        private void btnLeaderboard_Click(object sender, EventArgs e)
        {
            SendRequest("4/");
        }

        private void btnMatchResults_Click(object sender, EventArgs e)
        {
            string id = txtMatchId.Text.Trim();
            if (string.IsNullOrEmpty(id)) return;
            SendRequest("5/" + id);
        }

        public void OnRegisterResponse(string result)
        {
            SafeUi(delegate
            {
                if (result == "YES") MessageBox.Show("Registration successful!");
                else MessageBox.Show("Registration failed.");
            });
        }

        public void OnLoginResponse(string result)
        {
            SafeUi(delegate
            {
                loginPending = false;
                if (result == "YES")
                {
                    isLoggedIn = true;
                    lblStatus.Text = "Logged in as: " + currentUsername;
                    lblStatus.ForeColor = Color.FromArgb(245, 199, 34);
                    gbQueries.Enabled = true;
                    txtUser.Enabled = false;
                    txtPass.Enabled = false;
                    btnLogin.Enabled = false;
                    btnRegister.Enabled = false;
                    MessageBox.Show("Login successful!");
                }
                else
                {
                    isLoggedIn = false;
                    currentUsername = "";
                    btnLogin.Enabled = true;
                    btnRegister.Enabled = true;
                    MessageBox.Show(result == "ALREADY" ? "That user is already logged in." : "Login failed.");
                }
            });
        }

        public void OnDeleteAccountResponse(string result, string message)
        {
            SafeUi(delegate
            {
                MessageBox.Show(message);
                if (result == "1")
                {
                    isDisconnecting = true;
                    foreach (Form3 gameBoard in Application.OpenForms.OfType<Form3>().ToArray())
                    {
                        if (!gameBoard.IsDisposed) gameBoard.Close();
                    }

                    if (launcher != null)
                    {
                        launcher.OnConsoleRequestedDisconnect();
                    }
                    else
                    {
                        Close();
                    }
                }
            });
        }

        public void UpdateConnectedList(string[] noms)
        {
            SafeUi(delegate
            {
                HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
                listConnected.Items.Clear();
                foreach (string n in noms)
                {
                    string nomPropre = Clean(n);
                    if (string.IsNullOrEmpty(nomPropre)) continue;
                    if (string.Equals(nomPropre, currentUsername, StringComparison.Ordinal)) continue;
                    if (seen.Add(nomPropre)) listConnected.Items.Add(nomPropre);
                }
            });
        }

        public void OnHistoryResponse(string[] lines)
        {
            FillResults("--- MATCH HISTORY ---", lines);
        }

        public void OnLeaderboardResponse(string[] lines)
        {
            FillResults("--- LEADERBOARD ---", lines);
        }

        public void OnMatchResultsResponse(string[] lines)
        {
            FillResults("--- MATCH RESULTS ---", lines);
        }

        private void FillResults(string title, string[] lines)
        {
            SafeUi(delegate
            {
                lstResults.Items.Clear();
                lstResults.Items.Add(title);
                foreach (string s in lines)
                {
                    string line = Clean(s);
                    if (!string.IsNullOrEmpty(line)) lstResults.Items.Add(line);
                }
            });
        }

        private void btnInvite_Click(object sender, EventArgs e)
        {
            if (!isLoggedIn)
            {
                MessageBox.Show("Log in before inviting players.");
                return;
            }
            if (listConnected.SelectedItems.Count == 0) return;

            List<string> invitees = new List<string>();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in listConnected.SelectedItems)
            {
                string name = Clean(item.ToString());
                if (name.Length == 0 || name == currentUsername) continue;
                if (seen.Add(name)) invitees.Add(name);
            }
            if (invitees.Count == 0) return;

            string msg = "7/" + currentUsername + "/" + string.Join("/", invitees.ToArray());
            if (SendRequest(msg))
            {
                MessageBox.Show("Invitation sent. Waiting for response...");
            }
        }

        public void OnInvitationReceived(string inviter)
        {
            SafeUi(delegate
            {
                inviter = Clean(inviter);
                if (string.IsNullOrEmpty(inviter) || string.IsNullOrEmpty(currentUsername)) return;

                DialogResult res = MessageBox.Show(inviter + " has invited you to play UNO! Do you accept?", "Game Invitation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (res == DialogResult.Yes) SendRequest("9/" + inviter + "/" + currentUsername + "/1");
                else SendRequest("9/" + inviter + "/" + currentUsername + "/0");
            });
        }

        public void OnInvitationResult(string result, int gameId)
        {
            SafeUi(delegate
            {
                if (result == "1")
                {
                    this.Hide();
                    Form3 gameBoard = Application.OpenForms.OfType<Form3>().FirstOrDefault(f => f.GameId == gameId);
                    if (gameBoard == null || gameBoard.IsDisposed)
                    {
                        gameBoard = new Form3(server, currentUsername, gameId);
                        gameBoard.FormClosed += delegate { if (!isDisconnecting && !isClosing && !IsDisposed) this.Show(); };
                        gameBoard.Show();
                    }
                    else
                    {
                        gameBoard.BringToFront();
                    }
                    gameBoard.ShowServerMessage("Invitation accepted. Waiting for the first deal...");
                }
                else
                {
                    MessageBox.Show("The invitation was declined. Game cancelled.", "Matchmaking failed");
                }
            });
        }

        private void btnDisconnectConsole_Click(object sender, EventArgs e)
        {
            isDisconnecting = true;
            if (launcher != null)
            {
                launcher.OnConsoleRequestedDisconnect();
            }
            else
            {
                Close();
            }
        }

        private void btnDeleteAccount_Click(object sender, EventArgs e)
        {
            if (!isLoggedIn || string.IsNullOrEmpty(currentUsername))
            {
                MessageBox.Show("Log in before deleting your account.");
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Delete account '" + currentUsername + "'? This disables future login but keeps old match history.",
                "Delete Account",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            string pass = txtPass.Text;
            if (string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Password is required to delete the account.");
                return;
            }

            SendRequest("19/" + currentUsername + "/" + pass);
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            isClosing = true;
            foreach (Form3 gameBoard in Application.OpenForms.OfType<Form3>().ToArray())
            {
                if (!gameBoard.IsDisposed) gameBoard.Close();
            }
        }

        private void Form2_Load(object sender, EventArgs e) { }

        private void SafeUi(Action action)
        {
            if (IsDisposed) return;
            try
            {
                if (InvokeRequired) BeginInvoke(action);
                else action();
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        }

        private string Clean(string text)
        {
            return (text ?? "").Replace("\0", "").Trim();
        }
    }
}
