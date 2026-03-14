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
            this.txtMatchId = new System.Windows.Forms.TextBox();
            this.btnMatchResults = new System.Windows.Forms.Button();
            this.btnLeaderboard = new System.Windows.Forms.Button();
            this.btnHistory = new System.Windows.Forms.Button();
            this.lstResults = new System.Windows.Forms.ListBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
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
            this.gbLogin.Location = new System.Drawing.Point(13, 13);
            this.gbLogin.Name = "gbLogin";
            this.gbLogin.Size = new System.Drawing.Size(264, 139);
            this.gbLogin.TabIndex = 0;
            this.gbLogin.TabStop = false;
            this.gbLogin.Text = "Authentication";
            // 
            // btnLogin
            // 
            this.btnLogin.Location = new System.Drawing.Point(150, 96);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(95, 23);
            this.btnLogin.TabIndex = 5;
            this.btnLogin.Text = "Login";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // btnRegister
            // 
            this.btnRegister.Location = new System.Drawing.Point(19, 96);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new System.Drawing.Size(95, 23);
            this.btnRegister.TabIndex = 4;
            this.btnRegister.Text = "Register";
            this.btnRegister.UseVisualStyleBackColor = true;
            this.btnRegister.Click += new System.EventHandler(this.btnRegister_Click);
            // 
            // txtPass
            // 
            this.txtPass.Location = new System.Drawing.Point(92, 60);
            this.txtPass.Name = "txtPass";
            this.txtPass.PasswordChar = '*';
            this.txtPass.Size = new System.Drawing.Size(153, 20);
            this.txtPass.TabIndex = 3;
            // 
            // txtUser
            // 
            this.txtUser.Location = new System.Drawing.Point(92, 28);
            this.txtUser.Name = "txtUser";
            this.txtUser.Size = new System.Drawing.Size(153, 20);
            this.txtUser.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Password:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
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
            this.gbQueries.Location = new System.Drawing.Point(13, 178);
            this.gbQueries.Name = "gbQueries";
            this.gbQueries.Size = new System.Drawing.Size(264, 153);
            this.gbQueries.TabIndex = 1;
            this.gbQueries.TabStop = false;
            this.gbQueries.Text = "Game Data";
            // 
            // txtMatchId
            // 
            this.txtMatchId.Location = new System.Drawing.Point(74, 114);
            this.txtMatchId.Name = "txtMatchId";
            this.txtMatchId.Size = new System.Drawing.Size(52, 20);
            this.txtMatchId.TabIndex = 3;
            // 
            // btnMatchResults
            // 
            this.btnMatchResults.Location = new System.Drawing.Point(132, 112);
            this.btnMatchResults.Name = "btnMatchResults";
            this.btnMatchResults.Size = new System.Drawing.Size(113, 23);
            this.btnMatchResults.TabIndex = 2;
            this.btnMatchResults.Text = "Get Match Results";
            this.btnMatchResults.UseVisualStyleBackColor = true;
            this.btnMatchResults.Click += new System.EventHandler(this.btnMatchResults_Click);
            // 
            // btnLeaderboard
            // 
            this.btnLeaderboard.Location = new System.Drawing.Point(19, 70);
            this.btnLeaderboard.Name = "btnLeaderboard";
            this.btnLeaderboard.Size = new System.Drawing.Size(226, 23);
            this.btnLeaderboard.TabIndex = 1;
            this.btnLeaderboard.Text = "Show Leaderboard";
            this.btnLeaderboard.UseVisualStyleBackColor = true;
            this.btnLeaderboard.Click += new System.EventHandler(this.btnLeaderboard_Click);
            // 
            // btnHistory
            // 
            this.btnHistory.Location = new System.Drawing.Point(19, 31);
            this.btnHistory.Name = "btnHistory";
            this.btnHistory.Size = new System.Drawing.Size(226, 23);
            this.btnHistory.TabIndex = 0;
            this.btnHistory.Text = "My Match History";
            this.btnHistory.UseVisualStyleBackColor = true;
            this.btnHistory.Click += new System.EventHandler(this.btnHistory_Click);
            // 
            // lstResults
            // 
            this.lstResults.FormattingEnabled = true;
            this.lstResults.Location = new System.Drawing.Point(298, 18);
            this.lstResults.Name = "lstResults";
            this.lstResults.Size = new System.Drawing.Size(306, 316);
            this.lstResults.TabIndex = 2;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.ForeColor = System.Drawing.Color.Red;
            this.lblStatus.Location = new System.Drawing.Point(15, 159);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(84, 13);
            this.lblStatus.TabIndex = 3;
            this.lblStatus.Text = "Not Logged In";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 117);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Match ID:";
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(627, 347);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lstResults);
            this.Controls.Add(this.gbQueries);
            this.Controls.Add(this.gbLogin);
            this.Name = "Form2";
            this.Text = "UNO Player Console";
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
    }
}
