using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form3 : Form
    {
        Socket server;
        string currentUsername;
        int gameId;
        string currentPlayer = "";
        readonly List<string> handCards = new List<string>();

        Panel tablePanel;
        FlowLayoutPanel handPanel;
        FlowLayoutPanel opponentsPanel;
        Label lblTopCard;
        Label lblTurn;
        Label lblDeck;
        Label lblPlayers;
        Button btnUno;

        public int GameId
        {
            get { return gameId; }
        }

        public Form3(Socket server, string username, int gameId)
        {
            InitializeComponent();
            this.server = server;
            this.currentUsername = username;
            this.gameId = gameId;

            BuildUnoLayout();
            lblGameAction.Text = "Waiting for the server to deal the cards...";
        }

        private void BuildUnoLayout()
        {
            this.Text = "UNO Table - Room " + gameId;
            this.ClientSize = new Size(1120, 720);
            this.MinimumSize = new Size(980, 640);
            this.BackColor = Color.FromArgb(18, 115, 65);
            this.Font = new Font("Segoe UI", 10F);

            rtbChat.Location = new Point(845, 80);
            rtbChat.Size = new Size(250, 485);
            rtbChat.BackColor = Color.FromArgb(31, 31, 31);
            rtbChat.ForeColor = Color.White;
            rtbChat.BorderStyle = BorderStyle.None;

            txtChatInput.Location = new Point(845, 585);
            txtChatInput.Size = new Size(170, 30);
            txtChatInput.BackColor = Color.White;

            btnSendChat.Location = new Point(1025, 584);
            btnSendChat.Size = new Size(70, 32);
            StyleButton(btnSendChat, Color.FromArgb(245, 199, 34), Color.Black);

            btnDrawCard.Location = new Point(500, 300);
            btnDrawCard.Size = new Size(140, 52);
            btnDrawCard.Text = "DRAW";
            StyleButton(btnDrawCard, Color.FromArgb(245, 199, 34), Color.Black);

            lblGameAction.Location = new Point(28, 632);
            lblGameAction.Size = new Size(790, 42);
            lblGameAction.ForeColor = Color.White;
            lblGameAction.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblGameAction.AutoSize = false;

            Label title = new Label();
            title.Text = "UNO";
            title.Font = new Font("Segoe UI Black", 36F, FontStyle.Bold);
            title.ForeColor = Color.FromArgb(238, 42, 36);
            title.BackColor = Color.Transparent;
            title.Location = new Point(28, 18);
            title.Size = new Size(170, 64);
            Controls.Add(title);

            lblTurn = NewTableLabel(new Point(210, 32), new Size(350, 30), "Turn: -");
            lblDeck = NewTableLabel(new Point(590, 32), new Size(220, 30), "Deck: -");
            lblPlayers = NewTableLabel(new Point(845, 32), new Size(250, 30), "Chat");

            tablePanel = new Panel();
            tablePanel.Location = new Point(28, 92);
            tablePanel.Size = new Size(790, 505);
            tablePanel.BackColor = Color.FromArgb(11, 92, 52);
            tablePanel.BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(tablePanel);

            opponentsPanel = new FlowLayoutPanel();
            opponentsPanel.Location = new Point(20, 18);
            opponentsPanel.Size = new Size(748, 72);
            opponentsPanel.BackColor = Color.Transparent;
            tablePanel.Controls.Add(opponentsPanel);

            Label discardTitle = NewInnerLabel(new Point(315, 118), new Size(150, 25), "Discard pile");
            tablePanel.Controls.Add(discardTitle);

            lblTopCard = new Label();
            lblTopCard.Location = new Point(312, 150);
            lblTopCard.Size = new Size(156, 218);
            lblTopCard.TextAlign = ContentAlignment.MiddleCenter;
            lblTopCard.Font = new Font("Segoe UI Black", 22F, FontStyle.Bold);
            lblTopCard.ForeColor = Color.White;
            lblTopCard.BackColor = Color.FromArgb(238, 42, 36);
            lblTopCard.BorderStyle = BorderStyle.FixedSingle;
            tablePanel.Controls.Add(lblTopCard);

            btnDrawCard.Parent = tablePanel;
            btnDrawCard.Location = new Point(505, 232);

            btnUno = new Button();
            btnUno.Text = "UNO!";
            btnUno.Location = new Point(505, 296);
            btnUno.Size = new Size(140, 52);
            StyleButton(btnUno, Color.FromArgb(238, 42, 36), Color.White);
            btnUno.Click += delegate { ShowServerMessage("UNO called! Keep playing until your last card is accepted."); };
            tablePanel.Controls.Add(btnUno);

            handPanel = new FlowLayoutPanel();
            handPanel.Location = new Point(20, 390);
            handPanel.Size = new Size(748, 95);
            handPanel.AutoScroll = true;
            handPanel.Visible = true;
            handPanel.BackColor = Color.FromArgb(9, 75, 42);
            tablePanel.Controls.Add(handPanel);
            handPanel.BringToFront();
        }

        private Label NewTableLabel(Point location, Size size, string text)
        {
            Label label = new Label();
            label.Location = location;
            label.Size = size;
            label.Text = text;
            label.ForeColor = Color.White;
            label.BackColor = Color.Transparent;
            label.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            Controls.Add(label);
            return label;
        }

        private Label NewInnerLabel(Point location, Size size, string text)
        {
            Label label = new Label();
            label.Location = location;
            label.Size = size;
            label.Text = text;
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.ForeColor = Color.White;
            label.BackColor = Color.Transparent;
            label.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            return label;
        }

        private void StyleButton(Button button, Color backColor, Color foreColor)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        }

        private void txtChatInput_TextChanged(object sender, EventArgs e) { }

        private void btnSendChat_Click(object sender, EventArgs e)
        {
            if (txtChatInput.Text.Trim() != "")
            {
                string msg = "11/" + gameId + "/" + currentUsername + "/" + txtChatInput.Text.Replace("/", " ");
                SendRequest(msg);
                txtChatInput.Text = "";
            }
        }

        private void btnDrawCard_Click(object sender, EventArgs e)
        {
            if (!IsMyTurn())
            {
                ShowServerMessage("It is not your turn.");
                return;
            }

            SendRequest("14/" + gameId + "/" + currentUsername);
        }

        private bool SendRequest(string text)
        {
            try
            {
                if (server == null || !server.Connected) return false;
                if (!text.EndsWith("\n")) text += "\n";
                server.Send(Encoding.ASCII.GetBytes(text));
                return true;
            }
            catch
            {
                ShowServerMessage("Connection lost.");
                return false;
            }
        }

        public void UpdateGameState(string[] parts)
        {
            this.Invoke(new Action(() => {
                Debug.WriteLine("DEBUG 12 raw parts length=" + parts.Length);
                if (parts.Length < 10)
                {
                    lblGameAction.Text = "Incomplete game state received from server.";
                    return;
                }

                currentPlayer = Clean(parts[3]);
                string topCard = Clean(parts[2]);
                string receivedGameId = Clean(parts[1]);
                string direction = Clean(parts[4]) == "1" ? "clockwise" : "counter-clockwise";
                string deckCount = Clean(parts[5]);
                string activeColor = Clean(parts[6]);
                string hand = Clean(parts[7]);
                string opponents = Clean(parts[8]);
                string lastAction = Clean(parts[9]);

                handCards.Clear();
                if (hand.Length > 0)
                {
                    handCards.AddRange(hand.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                }

                Debug.WriteLine("DEBUG 12 gameId=" + receivedGameId + " currentPlayer=" + currentPlayer + " handField=" + hand + " parsedHandCount=" + handCards.Count);
                Console.WriteLine("DEBUG 12 gameId=" + receivedGameId + " currentPlayer=" + currentPlayer + " handField=" + hand + " parsedHandCount=" + handCards.Count);
                rtbChat.AppendText("[DEBUG] 12 game=" + receivedGameId + " turn=" + currentPlayer + " handField=" + hand + " count=" + handCards.Count + "\n");

                lblTurn.Text = "Turn: " + currentPlayer + (IsMyTurn() ? " (you)" : "");
                lblDeck.Text = "Deck: " + deckCount + " | " + direction;
                lblTopCard.Text = CardText(topCard);
                lblTopCard.BackColor = CardBackColor(activeColor.Length > 0 ? activeColor : topCard);
                lblGameAction.Text = handCards.Count == 0 ? "No cards received from server. Check server BroadcastGameState." : lastAction;

                btnDrawCard.Enabled = IsMyTurn();
                btnUno.Enabled = IsMyTurn() && handCards.Count == 1;

                RenderOpponents(opponents);
                RenderHand();
            }));
        }

        private void RenderOpponents(string opponents)
        {
            opponentsPanel.Controls.Clear();
            if (opponents.Length == 0) return;

            string[] entries = opponents.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string entry in entries)
            {
                Label badge = new Label();
                badge.Text = entry.Replace(":", "  cards: ");
                badge.Size = new Size(175, 42);
                badge.Margin = new Padding(5);
                badge.TextAlign = ContentAlignment.MiddleCenter;
                badge.ForeColor = Color.White;
                badge.BackColor = Color.FromArgb(238, 42, 36);
                badge.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                opponentsPanel.Controls.Add(badge);
            }
        }

        private void RenderHand()
        {
            handPanel.Visible = true;
            handPanel.BringToFront();
            handPanel.Controls.Clear();
            for (int index = 0; index < handCards.Count; index++)
            {
                string card = handCards[index];
                Button cardButton = new Button();
                cardButton.Tag = index;
                cardButton.Text = CardText(card);
                cardButton.Size = new Size(82, 78);
                cardButton.Margin = new Padding(6, 8, 6, 6);
                cardButton.BackColor = CardBackColor(card);
                cardButton.ForeColor = Color.White;
                cardButton.Font = new Font("Segoe UI Black", 11F, FontStyle.Bold);
                cardButton.FlatStyle = FlatStyle.Flat;
                cardButton.FlatAppearance.BorderColor = Color.White;
                cardButton.FlatAppearance.BorderSize = 2;
                cardButton.Enabled = IsMyTurn();
                cardButton.Click += CardButton_Click;
                handPanel.Controls.Add(cardButton);
            }
            handPanel.Refresh();
        }

        private void CardButton_Click(object sender, EventArgs e)
        {
            if (!IsMyTurn()) return;

            Button cardButton = sender as Button;
            int index = (int)cardButton.Tag;
            string chosenColor = "";
            if (handCards[index].StartsWith("W-"))
            {
                chosenColor = AskWildColor();
                if (chosenColor == "") return;
            }

            SendRequest("13/" + gameId + "/" + currentUsername + "/" + index + "/" + chosenColor);
        }

        private string AskWildColor()
        {
            using (Form picker = new Form())
            {
                picker.Text = "Choose color";
                picker.ClientSize = new Size(330, 90);
                picker.FormBorderStyle = FormBorderStyle.FixedDialog;
                picker.StartPosition = FormStartPosition.CenterParent;
                picker.MaximizeBox = false;
                picker.MinimizeBox = false;

                string[] result = new string[] { "" };
                AddColorChoice(picker, "R", Color.FromArgb(238, 42, 36), 12, result);
                AddColorChoice(picker, "Y", Color.FromArgb(245, 199, 34), 90, result);
                AddColorChoice(picker, "G", Color.FromArgb(0, 146, 70), 168, result);
                AddColorChoice(picker, "B", Color.FromArgb(0, 103, 179), 246, result);

                picker.ShowDialog(this);
                return result[0];
            }
        }

        private void AddColorChoice(Form picker, string colorCode, Color color, int x, string[] result)
        {
            Button button = new Button();
            button.Text = colorCode;
            button.Location = new Point(x, 18);
            button.Size = new Size(64, 52);
            button.BackColor = color;
            button.ForeColor = Color.White;
            button.Font = new Font("Segoe UI Black", 14F, FontStyle.Bold);
            button.FlatStyle = FlatStyle.Flat;
            button.Tag = colorCode;
            button.Click += delegate {
                result[0] = colorCode;
                picker.Close();
            };
            picker.Controls.Add(button);
        }

        private bool IsMyTurn()
        {
            return string.Equals(currentPlayer, currentUsername, StringComparison.OrdinalIgnoreCase);
        }

        private string Clean(string text)
        {
            return (text ?? "").Replace("\0", "").Trim();
        }

        private string CardText(string card)
        {
            string[] pieces = card.Split('-');
            if (pieces.Length < 2) return card;
            if (pieces[1] == "DRAW2") return "+2";
            if (pieces[1] == "WILD4") return "+4";
            if (pieces[1] == "WILD") return "WILD";
            if (pieces[1] == "SKIP") return "SKIP";
            if (pieces[1] == "REV") return "REV";
            return pieces[1];
        }

        private Color CardBackColor(string cardOrColor)
        {
            string colorCode = cardOrColor.Contains("-") ? cardOrColor.Split('-')[0] : cardOrColor;
            if (colorCode == "R") return Color.FromArgb(238, 42, 36);
            if (colorCode == "Y") return Color.FromArgb(245, 199, 34);
            if (colorCode == "G") return Color.FromArgb(0, 146, 70);
            if (colorCode == "B") return Color.FromArgb(0, 103, 179);
            return Color.FromArgb(35, 35, 35);
        }

        public void UpdateChat(string user, string message)
        {
            this.Invoke(new Action(() => {
                rtbChat.AppendText("[" + user + "]: " + message + "\n");
                rtbChat.SelectionStart = rtbChat.Text.Length;
                rtbChat.ScrollToCaret();
            }));
        }

        public void OnGameOver(string winner)
        {
            this.Invoke(new Action(() => {
                lblGameAction.Text = winner + " wins this UNO match!";
                MessageBox.Show(winner + " wins this UNO match!", "Game over");
                btnDrawCard.Enabled = false;
                btnUno.Enabled = false;
                foreach (Control control in handPanel.Controls) control.Enabled = false;
            }));
        }

        public void ShowServerMessage(string message)
        {
            this.Invoke(new Action(() => {
                lblGameAction.Text = message;
                rtbChat.AppendText("[SERVER]: " + message + "\n");
            }));
        }
    }
}
