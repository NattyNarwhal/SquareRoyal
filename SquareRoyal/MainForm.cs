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
            Message(String.Empty);
            game = new SquareRoyal();
            game.CheatCanAlwaysDiscardCard = cheat_alwaysDiscardCard.Checked;
            game.CheatCanAlwaysPlaceCard = cheat_canAlwaysPlaceCard.Checked;
            DeselectAll();
            foreach (PictureBox p in tableLayoutPanel1.Controls)
            {
                p.Image = (Image)Properties.Resources.ResourceManager.GetObject("Blank");
            }
            DrawNextCard();
        }

        public Image GetFace(Card card)
        {
            return (Image)Properties.Resources.ResourceManager
                .GetObject(String.Format("{0}_{1}", card.Suit, card.Number));
        }

        public void Message(string msg)
        {
            statusBar1.Text = msg;
        }

        public void NewGameMessage(string msg)
        {
            if (MessageBox.Show(this, msg, this.Text, MessageBoxButtons.YesNo,
                MessageBoxIcon.Information) == DialogResult.Yes)
            {
                NewGame();
            }
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
            ((PictureBox)tableLayoutPanel1.GetControlFromPosition(y, x)).Image =
                (Image)Properties.Resources.ResourceManager.GetObject("Blank");
        }

        public void VisuallyPlaceCard(int x, int y, Card c)
        {
            ((PictureBox)tableLayoutPanel1.GetControlFromPosition(y, x)).Image = GetFace(c);
        }

        public void CheckGameState()
        {
            if (game.Won)
            {
                Message("You've won!");
                NewGameMessage("You've won! Want to play again?");
                return;
            }
            if (game.Cleaning)
            {
                Message("The board is filled - start discarding pairs that add up to 10 or cards of 10.");
            }
            if (game.CheckIfStuck())
            {
                Message("No more possible moves.");
                NewGameMessage("There are no more possible moves. Do you want to start a new game?");
            }
            DrawNextCard();
        }

        private void card_click(object sender, EventArgs e)
        {
            // don't do anything if we've won or lost
            if (game.Won || game.CheckIfStuck())
            {
                return;
            }
            // is the deck empty? if so, we can't place
            if (game.Deck.Count == 0)
            {
                Message("The deck is empty.");
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
                    if (game.Field[x,y] != game.Field[selectedCard.Item1, selectedCard.Item2]
                        & game.AttemptDiscardPair(x, y, selectedCard.Item1, selectedCard.Item2))
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
                Card c = game.Deck.Last(); // for VisuallyPlaceCard
                if (game.AttemptPlaceCard(x, y))
                {
                    VisuallyPlaceCard(x, y, c);
                    Message(String.Empty);
                }
                else
                {
                    Message("You can't place that card there.");
                }
            }
            CheckGameState();
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
            MessageBox.Show(this, String.Join(Environment.NewLine, game.Deck.Reverse<Card>()), "Deck");
        }
    }
}
