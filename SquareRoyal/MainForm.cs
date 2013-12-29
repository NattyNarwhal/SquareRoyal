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
        public List<Card> deck = Card.GetOrderedDeck();
        public Card[,] field;
        public bool cleaning;

        public bool selectingAnotherPair = false;
        public Tuple<int, int> selectedCard = null;

        // cheats
        public bool canAlwaysPlaceCard = false;
        public bool canAlwaysDiscardCard = false;

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
            deck = Card.GetOrderedDeck();
            deck.Shuffle();
            field = new Card[4, 4]; // it should be 3,3 but C#...
            cleaning = false;
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
            return (Image)SquareRoyal.Properties.Resources.ResourceManager.GetObject(resource);
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
            nextCard.Image = SquareRoyal.Properties.Resources.Backing;
            // update the remaining cards in deck
            cardsLeft.Text = deck.Count.ToString();
            // if we're cleaning then we don't reveal our next one
            if (cleaning)
            {
                return;
            }
            // if we have something, show something
            if (deck.Count > 0)
            {
                // nextCard.Image = SquareRoyal.Properties.Resources.Backing;
                nextCard.Image = GetFace(deck.Last());
            }
        }

        public void ShouldClean()
        {
            foreach (Card c in field)
            {
                // if we see an empty card we're obviously not cleaning
                if (c == null)
                {
                    cleaning = false;
                    statusBar1.Text = String.Empty;
                    return;
                }
            }
            cleaning = true;
            statusBar1.Text = "The board is filled - start discarding pairs that add up to 10 or cards of 10.";
        }

        public bool HasWon()
        {
            // TODO: refractor as with royal placement
            // we try and catch NullRef as failure
            // check for kings where needed
            try
            {
                if ((field[0, 0].Number == 13) & (field[0, 3].Number == 13) & (field[3, 0].Number == 13) & (field[3, 3].Number == 13))
                {
                    // then check for queens
                    if ((field[0, 1].Number == 12) & (field[0, 2].Number == 12) & (field[3, 1].Number == 12) & (field[3, 2].Number == 12))
                    {
                        // jacks last
                        if ((field[1, 0].Number == 11) & (field[1, 3].Number == 11) & (field[2, 0].Number == 11) & (field[2, 3].Number == 11))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (NullReferenceException)
            {
                
            }
            return false;
        }

        public bool IsEmptyCell(int x, int y)
        {
            return (field[x, y] == null);
        }

        public bool CanPlace(int x, int y, Card c)
        {
            // Is the user a cheating bastard?
            if (canAlwaysPlaceCard) { return true; }
            // Is the card already occupied?
            if (!IsEmptyCell(x,y))
            {
                return false;
            }
            // Is it a royal?
            // TODO: refactor it, it could be better
            if (c.Number == 11)
            {
                // jacks belong in the left and right edges
                if (x == 1 || x == 2)
                {
                    if (y == 0 || y == 3)
                    {
                        return true;
                    }
                }
                return false;
            }
            if (c.Number == 12)
            {
                // queens belong in the top and bottom edges
                if (x == 0 || x == 3)
                {
                    if (y == 1 || y == 2)
                    {
                        return true;
                    }
                }
                return false;
            }
            if (c.Number == 13)
            {
                // kings in the corners
                if (x == 0 || x == 3)
                {
                    if (y == 0 || y == 3)
                    {
                        return true;
                    }
                }
                return false;
            }
            // I guess we can then.
            return true;
        }

        public void RemoveCard(int x, int y)
        {
            field[x, y] = null;
            ((PictureBox)tableLayoutPanel1.GetControlFromPosition(y, x)).Image = null;
        }

        public void AttemptPlaceCard(int x, int y, Card c)
        {
            if (CanPlace(x, y, c))
            {
                // add and then remove
                field[x, y] = c; // set
                ((PictureBox)tableLayoutPanel1.GetControlFromPosition(y,x)).Image = GetFace(c); // picture
                deck.Remove(deck.Last());
                // empty the status bar messages
                statusBar1.Text = String.Empty;
                if (HasWon())
                {
                    if (MessageBox.Show(this, "You've won! Want to play again?", "Square Royal", MessageBoxButtons.RetryCancel, MessageBoxIcon.Information) == DialogResult.Retry)
                    {
                        NewGame();
                    }
                }
                ShouldClean();
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
            if (HasWon())
            {
                return;
            }
            // is the deck empty? if so, we can't place
            if (deck.Count == 0)
            {
                statusBar1.Text = "The deck is empty. You've probably won.";
                return;
            }
            // get our coords for later use
            int x = tableLayoutPanel1.GetPositionFromControl(((Control)sender)).Row;
            int y = tableLayoutPanel1.GetPositionFromControl(((Control)sender)).Column;
            // if cleaning, select a pair (or just a single 10)
            if (cleaning)
            {
                // check if it's empty, we can't empty the empty
                if (IsEmptyCell(x, y))
                {
                    return;
                }
                // are we a massive cheater? remember, no bugfixes for the weary
                if (canAlwaysDiscardCard)
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
                    if (field[x, y] == field[selectedCard.Item1, selectedCard.Item2])
                    {
                        selectingAnotherPair = false;
                        selectedCard = null;
                        VisuallyDeselectAll();
                        return;
                    }
                    // check if it adds to 10, and if so, remove both cards
                    if ((field[x, y].Number + field[selectedCard.Item1, selectedCard.Item2].Number) == 10)
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
                    switch (field[x, y].Number)
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
                AttemptPlaceCard(x,y, deck.Last());
            }
        }

        private void nextCard_Click(object sender, EventArgs e)
        {
            // tell the thing we're ready!
            if (cleaning)
            {
                // check if we can stop cleamiing
                foreach (Card c in field)
                {
                    // if we see an empty card we're free
                    if (c == null)
                    {
                        // stop cleaning and clean up our messes
                        cleaning = false;
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
            if (cheat_canAlwaysPlaceCard.Checked)
            {
                cheat_canAlwaysPlaceCard.Checked = false;
            }
            else
            {
                cheat_canAlwaysPlaceCard.Checked = true;
            }
            canAlwaysPlaceCard = cheat_canAlwaysPlaceCard.Checked;
        }

        private void cheat_alwaysDiscardCard_Click(object sender, EventArgs e)
        {
            if (cheat_alwaysDiscardCard.Checked)
            {
                cheat_alwaysDiscardCard.Checked = false;
            }
            else
            {
                cheat_alwaysDiscardCard.Checked = true;
            }
            canAlwaysDiscardCard = cheat_alwaysDiscardCard.Checked;
        }
    }
}
