namespace WindowsFormsApplication1
{
    partial class Form3
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.rtbChat = new System.Windows.Forms.RichTextBox();
            this.txtChatInput = new System.Windows.Forms.TextBox();
            this.btnSendChat = new System.Windows.Forms.Button();
            this.btnDrawCard = new System.Windows.Forms.Button();
            this.lblGameAction = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // rtbChat
            // 
            this.rtbChat.Location = new System.Drawing.Point(34, 148);
            this.rtbChat.Name = "rtbChat";
            this.rtbChat.Size = new System.Drawing.Size(225, 179);
            this.rtbChat.TabIndex = 0;
            this.rtbChat.Text = "";
            // 
            // txtChatInput
            // 
            this.txtChatInput.Location = new System.Drawing.Point(34, 347);
            this.txtChatInput.Name = "txtChatInput";
            this.txtChatInput.Size = new System.Drawing.Size(154, 26);
            this.txtChatInput.TabIndex = 1;
            this.txtChatInput.TextChanged += new System.EventHandler(this.txtChatInput_TextChanged);
            // 
            // btnSendChat
            // 
            this.btnSendChat.Location = new System.Drawing.Point(194, 347);
            this.btnSendChat.Name = "btnSendChat";
            this.btnSendChat.Size = new System.Drawing.Size(65, 24);
            this.btnSendChat.TabIndex = 2;
            this.btnSendChat.Text = "Send";
            this.btnSendChat.UseVisualStyleBackColor = true;
            this.btnSendChat.Click += new System.EventHandler(this.btnSendChat_Click);
            // 
            // btnDrawCard
            // 
            this.btnDrawCard.Location = new System.Drawing.Point(528, 148);
            this.btnDrawCard.Name = "btnDrawCard";
            this.btnDrawCard.Size = new System.Drawing.Size(121, 53);
            this.btnDrawCard.TabIndex = 3;
            this.btnDrawCard.Text = "Draw Card";
            this.btnDrawCard.UseVisualStyleBackColor = true;
            this.btnDrawCard.Click += new System.EventHandler(this.btnDrawCard_Click);
            // 
            // lblGameAction
            // 
            this.lblGameAction.AutoSize = true;
            this.lblGameAction.Location = new System.Drawing.Point(524, 285);
            this.lblGameAction.Name = "lblGameAction";
            this.lblGameAction.Size = new System.Drawing.Size(137, 20);
            this.lblGameAction.TabIndex = 4;
            this.lblGameAction.Text = "Game Activity Log";
            // 
            // Form3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.lblGameAction);
            this.Controls.Add(this.btnDrawCard);
            this.Controls.Add(this.btnSendChat);
            this.Controls.Add(this.txtChatInput);
            this.Controls.Add(this.rtbChat);
            this.Name = "Form3";
            this.Text = "Form3";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbChat;
        private System.Windows.Forms.TextBox txtChatInput;
        private System.Windows.Forms.Button btnSendChat;
        private System.Windows.Forms.Button btnDrawCard;
        private System.Windows.Forms.Label lblGameAction;
    }
}