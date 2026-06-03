namespace WindowsFormsApplication1
{
    partial class Form2
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.gbLogin = new System.Windows.Forms.GroupBox();
            this.btnLogin = new System.Windows.Forms.Button();
            this.btnRegister = new System.Windows.Forms.Button();
            this.txtPass = new System.Windows.Forms.TextBox();
            this.txtUser = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.gbQueries = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtMatchId = new System.Windows.Forms.TextBox();
            this.btnMatchResults = new System.Windows.Forms.Button();
            this.btnLeaderboard = new System.Windows.Forms.Button();
            this.btnHistory = new System.Windows.Forms.Button();
            this.lstResults = new System.Windows.Forms.ListBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.listConnected = new System.Windows.Forms.ListBox();
            this.button2 = new System.Windows.Forms.Button();
            this.btnDisconnectConsole = new System.Windows.Forms.Button();
            this.btnDeleteAccount = new System.Windows.Forms.Button();
            this.gbLogin.SuspendLayout();
            this.gbQueries.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbLogin
            // 
            this.gbLogin.Controls.Add(this.btnLogin);
            this.gbLogin.Controls.Add(this.btnRegister);
            this.gbLogin.Controls.Add(this.txtPass);
            this.gbLogin.Controls.Add(this.txtUser);
            this.gbLogin.Controls.Add(this.label2);
            this.gbLogin.Controls.Add(this.label1);
            this.gbLogin.Location = new System.Drawing.Point(20, 20);
            this.gbLogin.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbLogin.Name = "gbLogin";
            this.gbLogin.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbLogin.Size = new System.Drawing.Size(396, 214);
            this.gbLogin.TabIndex = 0;
            this.gbLogin.TabStop = false;
            this.gbLogin.Text = "Authentication";
            // 
            // btnLogin
            // 
            this.btnLogin.Location = new System.Drawing.Point(225, 148);
            this.btnLogin.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(142, 35);
            this.btnLogin.TabIndex = 5;
            this.btnLogin.Text = "Login";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // btnRegister
            // 
            this.btnRegister.Location = new System.Drawing.Point(28, 148);
            this.btnRegister.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new System.Drawing.Size(142, 35);
            this.btnRegister.TabIndex = 4;
            this.btnRegister.Text = "Register";
            this.btnRegister.UseVisualStyleBackColor = true;
            this.btnRegister.Click += new System.EventHandler(this.btnRegister_Click);
            // 
            // txtPass
            // 
            this.txtPass.Location = new System.Drawing.Point(138, 92);
            this.txtPass.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtPass.Name = "txtPass";
            this.txtPass.PasswordChar = '*';
            this.txtPass.Size = new System.Drawing.Size(228, 26);
            this.txtPass.TabIndex = 3;
            // 
            // txtUser
            // 
            this.txtUser.Location = new System.Drawing.Point(138, 43);
            this.txtUser.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtUser.Name = "txtUser";
            this.txtUser.Size = new System.Drawing.Size(228, 26);
            this.txtUser.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 97);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 20);
            this.label2.TabIndex = 1;
            this.label2.Text = "Password:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 48);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Username:";
            // 
            // gbQueries
            // 
            this.gbQueries.Controls.Add(this.label3);
            this.gbQueries.Controls.Add(this.txtMatchId);
            this.gbQueries.Controls.Add(this.btnMatchResults);
            this.gbQueries.Controls.Add(this.btnLeaderboard);
            this.gbQueries.Controls.Add(this.btnHistory);
            this.gbQueries.Enabled = false;
            this.gbQueries.Location = new System.Drawing.Point(20, 274);
            this.gbQueries.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbQueries.Name = "gbQueries";
            this.gbQueries.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbQueries.Size = new System.Drawing.Size(396, 235);
            this.gbQueries.TabIndex = 1;
            this.gbQueries.TabStop = false;
            this.gbQueries.Text = "Game Data";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(24, 180);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(78, 20);
            this.label3.TabIndex = 4;
            this.label3.Text = "Match ID:";
            // 
            // txtMatchId
            // 
            this.txtMatchId.Location = new System.Drawing.Point(111, 175);
            this.txtMatchId.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtMatchId.Name = "txtMatchId";
            this.txtMatchId.Size = new System.Drawing.Size(76, 26);
            this.txtMatchId.TabIndex = 3;
            // 
            // btnMatchResults
            // 
            this.btnMatchResults.Location = new System.Drawing.Point(198, 172);
            this.btnMatchResults.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnMatchResults.Name = "btnMatchResults";
            this.btnMatchResults.Size = new System.Drawing.Size(170, 35);
            this.btnMatchResults.TabIndex = 2;
            this.btnMatchResults.Text = "Get Match Results";
            this.btnMatchResults.UseVisualStyleBackColor = true;
            this.btnMatchResults.Click += new System.EventHandler(this.btnMatchResults_Click);
            // 
            // btnLeaderboard
            // 
            this.btnLeaderboard.Location = new System.Drawing.Point(28, 108);
            this.btnLeaderboard.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnLeaderboard.Name = "btnLeaderboard";
            this.btnLeaderboard.Size = new System.Drawing.Size(339, 35);
            this.btnLeaderboard.TabIndex = 1;
            this.btnLeaderboard.Text = "Show Leaderboard";
            this.btnLeaderboard.UseVisualStyleBackColor = true;
            this.btnLeaderboard.Click += new System.EventHandler(this.btnLeaderboard_Click);
            // 
            // btnHistory
            // 
            this.btnHistory.Location = new System.Drawing.Point(28, 48);
            this.btnHistory.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnHistory.Name = "btnHistory";
            this.btnHistory.Size = new System.Drawing.Size(339, 35);
            this.btnHistory.TabIndex = 0;
            this.btnHistory.Text = "My Match History";
            this.btnHistory.UseVisualStyleBackColor = true;
            this.btnHistory.Click += new System.EventHandler(this.btnHistory_Click);
            // 
            // lstResults
            // 
            this.lstResults.FormattingEnabled = true;
            this.lstResults.ItemHeight = 20;
            this.lstResults.Location = new System.Drawing.Point(447, 28);
            this.lstResults.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.lstResults.Name = "lstResults";
            this.lstResults.Size = new System.Drawing.Size(457, 284);
            this.lstResults.TabIndex = 2;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.ForeColor = System.Drawing.Color.Red;
            this.lblStatus.Location = new System.Drawing.Point(22, 245);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(126, 20);
            this.lblStatus.TabIndex = 3;
            this.lblStatus.Text = "Not Logged In";
            // 
            // listConnected
            // 
            this.listConnected.FormattingEnabled = true;
            this.listConnected.ItemHeight = 20;
            this.listConnected.Location = new System.Drawing.Point(447, 322);
            this.listConnected.Name = "listConnected";
            this.listConnected.Size = new System.Drawing.Size(457, 184);
            this.listConnected.TabIndex = 4;
            // 
            // button2
            // 
            this.button2.AccessibleName = "btnInvite";
            this.button2.Location = new System.Drawing.Point(288, 245);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 26);
            this.button2.TabIndex = 6;
            this.button2.Text = "Invite ";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.btnInvite_Click);
            // 
            // btnDisconnectConsole
            // 
            this.btnDisconnectConsole.Location = new System.Drawing.Point(764, 512);
            this.btnDisconnectConsole.Name = "btnDisconnectConsole";
            this.btnDisconnectConsole.Size = new System.Drawing.Size(140, 32);
            this.btnDisconnectConsole.TabIndex = 7;
            this.btnDisconnectConsole.Text = "Disconnect";
            this.btnDisconnectConsole.UseVisualStyleBackColor = true;
            this.btnDisconnectConsole.Click += new System.EventHandler(this.btnDisconnectConsole_Click);
            // 
            // btnDeleteAccount
            // 
            this.btnDeleteAccount.Location = new System.Drawing.Point(597, 512);
            this.btnDeleteAccount.Name = "btnDeleteAccount";
            this.btnDeleteAccount.Size = new System.Drawing.Size(150, 32);
            this.btnDeleteAccount.TabIndex = 8;
            this.btnDeleteAccount.Text = "Delete Account";
            this.btnDeleteAccount.UseVisualStyleBackColor = true;
            this.btnDeleteAccount.Click += new System.EventHandler(this.btnDeleteAccount_Click);
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(940, 558);
            this.Controls.Add(this.btnDeleteAccount);
            this.Controls.Add(this.btnDisconnectConsole);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.listConnected);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lstResults);
            this.Controls.Add(this.gbQueries);
            this.Controls.Add(this.gbLogin);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "Form2";
            this.Text = "UNO Player Console";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form2_FormClosing);
            this.Load += new System.EventHandler(this.Form2_Load);
            this.gbLogin.ResumeLayout(false);
            this.gbLogin.PerformLayout();
            this.gbQueries.ResumeLayout(false);
            this.gbQueries.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.GroupBox gbLogin;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Button btnRegister;
        private System.Windows.Forms.TextBox txtPass;
        private System.Windows.Forms.TextBox txtUser;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox gbQueries;
        private System.Windows.Forms.Button btnLeaderboard;
        private System.Windows.Forms.Button btnHistory;
        private System.Windows.Forms.ListBox lstResults;
        private System.Windows.Forms.TextBox txtMatchId;
        private System.Windows.Forms.Button btnMatchResults;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListBox listConnected;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button btnDisconnectConsole;
        private System.Windows.Forms.Button btnDeleteAccount;
    }
}
