using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
        string currentUsername = "";

        public Form2(Socket server)
        {
            InitializeComponent();
            this.server = server;
        }

        private void SendRequest(string text)
        {
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(text);
            server.Send(msg);
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string user = txtUser.Text;
            string pass = txtPass.Text;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Please enter username and password");
                return;
            }

            // Protocol: 1/User/Pass
            SendRequest("1/" + user + "/" + pass);
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
             string user = txtUser.Text;
             string pass = txtPass.Text;

             if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
             {
                 MessageBox.Show("Please enter username and password");
                 return;
             }

             // Protocol: 2/User/Pass
             // We temporarily store the username, will confirm it on server response
             currentUsername = user; 
             SendRequest("2/" + user + "/" + pass);
        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentUsername))
            {
                MessageBox.Show("You must be logged in to check history.");
                return;
            }
            // Protocol: 3/Username
            SendRequest("3/" + currentUsername);
        }

        private void btnLeaderboard_Click(object sender, EventArgs e)
        {
            // Protocol: 4/
            SendRequest("4/");
        }

        private void btnMatchResults_Click(object sender, EventArgs e)
        {
            string id = txtMatchId.Text;
             if (string.IsNullOrEmpty(id))
             {
                 MessageBox.Show("Please enter a Match ID");
                 return;
             }
             // Protocol: 5/MatchId
             SendRequest("5/" + id);
        }

        // -------------------------------------------------------------------
        // Methods called from Form1 thread (Need Invoke)
        // -------------------------------------------------------------------

        public void OnRegisterResponse(string result)
        {
            this.Invoke(new Action(() => {
                if (result == "YES")
                    MessageBox.Show("Registration Successful!");
                else
                    MessageBox.Show("Registration Failed. Name might be taken.");
            }));
        }

        public void OnLoginResponse(string result)
        {
             this.Invoke(new Action(() => {
                if (result == "YES")
                {
                    lblStatus.Text = "Logged in as: " + currentUsername;
                    lblStatus.ForeColor = Color.Green;
                    MessageBox.Show("Login Successful!");
                    
                    gbQueries.Enabled = true; // Enable queries
                }
                else
                {
                    currentUsername = ""; // Reset
                    MessageBox.Show("Login Failed. Check credentials.");
                }
            }));
        }

        public void OnHistoryResponse(string[] lines)
        {
             this.Invoke(new Action(() => {
                lstResults.Items.Clear();
                lstResults.Items.Add("--- MATCH HISTORY ---");
                foreach (string s in lines) 
                {
                    // Filter out empty strings if any
                    if(!string.IsNullOrEmpty(s)) lstResults.Items.Add(s);
                }
            }));
        }

         public void OnLeaderboardResponse(string[] lines)
        {
             this.Invoke(new Action(() => {
                lstResults.Items.Clear();
                lstResults.Items.Add("--- LEADERBOARD ---");
                foreach (string s in lines) 
                {
                    if(!string.IsNullOrEmpty(s)) lstResults.Items.Add(s);
                }
            }));
        }

         public void OnMatchResultsResponse(string[] lines)
        {
             this.Invoke(new Action(() => {
                lstResults.Items.Clear();
                lstResults.Items.Add("--- MATCH RESULTS ---");
                foreach (string s in lines) 
                {
                    if(!string.IsNullOrEmpty(s)) lstResults.Items.Add(s);
                }
            }));
        }

    }
}
