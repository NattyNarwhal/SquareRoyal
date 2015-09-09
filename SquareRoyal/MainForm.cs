using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SquareRoyal
{
    public partial class MainForm : Form
    {
        public SquareRoyal game = new SquareRoyal();

        public bool selectingAnotherPair = false;
        public Tuple<int, int> selectedCard = null;

        public bool CheatCanAlwaysDiscardCard = false;

        public MainForm()
        {
            InitializeComponent();
            NewGame();
        }

        public void AllowCheating()
        {
            cheatMenu.Visible = true;
            cheatMenu.Enabled = true;
            foreach (MenuItem m in cheatMenu.MenuItems)
            {
                m.Visible = true;
                m.Enabled = true;
            }
        }

        public void NewGame()
        {
            statusBar1.Text = String.Empty;
            game = new SquareRoyal();
            selectingAnotherPair = false;
            VisuallyDeselectAll();
            foreach (PictureBox p in tableLayoutPanel1.Controls)
            {
                p.Image = null;
            }
            DrawNextCard();
        }

        public Image GetFace(Card card)
        {
            string resource = String.Empty;
            switch (card.Suit)
            {
                case Suit.Clubs:
                    resource += "Clubs";
                    break;
                case Suit.Diamonds:
                    resource += "Diamonds";
                    break;
                case Suit.Hearts:
                    resource += "Hearts";
                    break;
                case Suit.Spades:
                    resource += "Spades";
                    break;
                default: break;
            }
            resource += "_";
            resource += card.Number;
            return (Image)Properties.Resources.ResourceManager.GetObject(resource);
        }

        public void VisuallyDeselectAll()
        {
            foreach (PictureBox p in tableLayoutPanel1.Controls)
            {
                p.BackColor = Color.Transparent;
            }
        }
        
        public void DrawNextCard() {
            // no matter what get SOMETHING
            nextCard.Image = Properties.Resources.Backing;
            // update the remaining cards in deck
            cardsLeft.Text = game.Deck.Count.ToString();
            // if we're cleaning then we don't reveal our next one
            if (game.Cleaning)
            {
                return;
            }
            // if we have something, show something
            if (game.Deck.Count > 0)
            {
                // nextCard.Image = SquareRoyal.Properties.Resources.Backing;
                nextCard.Image = GetFace(game.Deck.Last());
            }
        }

        public void RemoveCard(int x, int y)
        {
            game.RemoveCard(x, y);
            ((PictureBox)tableLayoutPanel1.GetControlFromPosition(y, x)).Image = null;
        }

        public void AttemptPlaceCard(int x, int y, Card c)
        {
            if (game.AttemptPlaceCard(x, y, c))
            {
                ((PictureBox)tableLayoutPanel1.GetControlFromPosition(y,x)).Image = GetFace(c); // picture
                // empty the status bar messages
                statusBar1.Text = String.Empty;
                if (game.Won)
                {
                    if (MessageBox.Show(this, "You've won! Want to play again?", "Square Royal", MessageBoxButtons.RetryCancel, MessageBoxIcon.Information) == DialogResult.Retry)
                    {
                        NewGame();
                    }
                }
                if (game.Cleaning)
                {
                    statusBar1.Text = "The board is filled - start discarding pairs that add up to 10 or cards of 10.";
                }
                DrawNextCard();
            }
            else
            {
                statusBar1.Text = "You can't place that card there.";
            }
        }

        private void card_click(object sender, EventArgs e)
        {
            // don't do anything if we've won
            if (game.Won)
            {
                return;
            }
            // is the deck empty? if so, we can't place
            if (game.Deck.Count == 0)
            {
                statusBar1.Text = "The deck is empty. You've probably won.";
                return;
            }
            // get our coords for later use
            int x = tableLayoutPanel1.GetPositionFromControl(((Control)sender)).Row;
            int y = tableLayoutPanel1.GetPositionFromControl(((Control)sender)).Column;
            // if cleaning, select a pair (or just a single 10)
            if (game.Cleaning)
            {
                // check if it's empty, we can't empty the empty
                if (game.IsEmptyCell(x, y))
                {
                    return;
                }
                // are we a massive cheater? remember, no bugfixes for the weary
                if (CheatCanAlwaysDiscardCard)
                {
                    RemoveCard(x, y);
                    selectingAnotherPair = false;
                    selectedCard = null;
                    VisuallyDeselectAll();
                    return;
                }
                // if we have a card highlighted, we need a buddy for it
                if (selectingAnotherPair)
                {
                    // royals and a 10 we don't check for because you can't meet c1 + c2 = 10 that way
                    // but first, check if it's the same bloody card! if so, deselect and exit
                    if (game.Field[x, y] == game.Field[selectedCard.Item1, selectedCard.Item2])
                    {
                        selectingAnotherPair = false;
                        selectedCard = null;
                        VisuallyDeselectAll();
                        return;
                    }
                    // check if it adds to 10, and if so, remove both cards
                    if ((game.Field[x, y].Number + game.Field[selectedCard.Item1, selectedCard.Item2].Number) == 10)
                    {
                        RemoveCard(x, y);
                        RemoveCard(selectedCard.Item1, selectedCard.Item2);
                        selectingAnotherPair = false;
                        selectedCard = null;
                        VisuallyDeselectAll();
                    }
                }
                // we're gonna pick the first of the pair
                else
                {
                    // if 10, we delete immediately, if royal, we return immediately
                    switch (game.Field[x, y].Number)
                    {
                        case 13:
                        case 12:
                        case 11:
                            break;
                        case 10:
                            RemoveCard(x, y);
                            break;
                        default:
                            selectingAnotherPair = true;
                            selectedCard = new Tuple<int, int>(x, y);
                            ((PictureBox)sender).BackColor = Color.LightGreen;
                            break;
                    }
                }
            }
            // if not cleaning, we're placing
            else
            {
                // statusBar1.Text = tableLayoutPanel1.GetPositionFromControl((Control)sender).ToString();
                AttemptPlaceCard(x,y, game.Deck.Last());
            }
        }

        private void nextCard_Click(object sender, EventArgs e)
        {
            // tell the thing we're ready!
            if (game.Cleaning)
            {
                // check if we can stop cleamiing
                foreach (Card c in game.Field)
                {
                    // if we see an empty card we're free
                    if (c == null)
                    {
                        // stop cleaning and clean up our messes
                        game.Cleaning = false;
                        selectedCard = null;
                        VisuallyDeselectAll();
                        DrawNextCard();
                    }
                }
                // if not, we return
            }
        }

        private void newMenuItem_Click(object sender, EventArgs e)
        {
            NewGame();
        }

        private void quitMenuItem_Click(object sender, EventArgs e)
        {
            // someone's mad ;)
            Application.Exit();
        }

        private void aboutMenuItem_Click(object sender, EventArgs e)
        {
            new AboutForm().ShowDialog(this);
        }

        private void cheat_canAlwaysPlaceCard_Click(object sender, EventArgs e)
        {
            cheat_canAlwaysPlaceCard.Checked = !cheat_canAlwaysPlaceCard.Checked;
            game.CheatCanAlwaysPlaceCard = cheat_canAlwaysPlaceCard.Checked;
        }

        private void cheat_alwaysDiscardCard_Click(object sender, EventArgs e)
        {
            cheat_alwaysDiscardCard.Checked = !cheat_alwaysDiscardCard.Checked;
            CheatCanAlwaysDiscardCard = cheat_alwaysDiscardCard.Checked;
        }

        private void cheat_showDeck_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, String.Join(Environment.NewLine, game.Deck), "Deck (bottom to top)");
        }
    }
}
