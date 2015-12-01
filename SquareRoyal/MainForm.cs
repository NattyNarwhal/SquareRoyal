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
            DeselectAll();
            foreach (PictureBox p in tableLayoutPanel1.Controls)
            {
                p.Image = null;
            }
            DrawNextCard();
        }

        public Image GetFace(Card card)
        {
            return (Image)Properties.Resources.ResourceManager
                .GetObject(String.Format("{0}_{1}", card.Suit, card.Number));
        }

        public void VisuallyDeselectAll()
        {
            foreach (PictureBox p in tableLayoutPanel1.Controls)
            {
                p.BackColor = Color.Transparent;
            }
        }

        public void DeselectAll()
        {
            selectingAnotherPair = false;
            selectedCard = null;
            VisuallyDeselectAll();
        }

        public void DrawNextCard()
        {
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

        public void VisuallyRemoveCard(int x, int y)
        {
            ((PictureBox)tableLayoutPanel1.GetControlFromPosition(y, x)).Image = null;
        }

        public void AttemptPlaceCard(int x, int y, Card c)
        {
            if (game.AttemptPlaceCard(x, y, c))
            {
                ((PictureBox)tableLayoutPanel1.GetControlFromPosition(y, x)).Image = GetFace(c); // picture
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
                // if we have a card highlighted, we need a buddy for it
                if (selectingAnotherPair)
                {
                    // royals and a 10 we don't check for because you can't meet c1 + c2 = 10 that way
                    // but first, check if it's the same bloody card! if so, deselect and exit
                    if (game.AttemptDiscardPair(x, y, selectedCard.Item1, selectedCard.Item2))
                    {
                        VisuallyRemoveCard(x, y);
                        VisuallyRemoveCard(selectedCard.Item1, selectedCard.Item2);
                        DeselectAll();
                    }
                    DeselectAll();
                }
                // we're gonna pick the first of the pair
                else
                {
                    if (game.CanDiscard(x, y))
                    {
                        if (game.DiscardNeedsPair(x, y))
                        {
                            selectingAnotherPair = true;
                            selectedCard = new Tuple<int, int>(x, y);
                            ((PictureBox)sender).BackColor = Color.LightGreen;
                        }
                        else
                        {
                            if (game.AttemptDiscardSingleCard(x, y))
                            {
                                VisuallyRemoveCard(x, y);
                            }
                        }
                    }
                }
            }
            // if not cleaning, we're placing
            else
            {
                // statusBar1.Text = tableLayoutPanel1.GetPositionFromControl((Control)sender).ToString();
                AttemptPlaceCard(x, y, game.Deck.Last());
            }
        }

        private void nextCard_Click(object sender, EventArgs e)
        {
            // tell the thing we're ready!
            if (game.Cleaning)
            {
                // stop cleaning and clean up our messes
                if (game.StopCleaning())
                {
                    DeselectAll();
                    DrawNextCard();
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
            game.CheatCanAlwaysDiscardCard = cheat_alwaysDiscardCard.Checked;
        }

        private void cheat_showDeck_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, String.Join(Environment.NewLine, game.Deck), "Deck (bottom to top)");
        }
    }
}
