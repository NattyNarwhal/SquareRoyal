using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SquareRoyal
{
    public class SquareRoyal
    {
        /// <summary>
        /// Gets or sets the deck.
        /// </summary>
        public List<Card> Deck { get; set; }
        /// <summary>
        /// Gets or sets the playing field.
        /// </summary>
        public Card[,] Field { get; set; }
        /// <summary>
        /// Gets or sets if we are cleaning.
        /// </summary>
        public bool Cleaning { get; set; }
        /// <summary>
        /// Gets if the game is won.
        /// </summary>
        public bool Won { get; private set; }

        // cheats
        public bool CheatCanAlwaysPlaceCard = false;
        public bool canAlwaysDiscardCard = false;

        public SquareRoyal()
        {
            Deck = CardFunctions.GetOrderedDeck();
            Deck.Shuffle();
            Field = new Card[4, 4];
            Cleaning = false;
        }

        public bool HasWon()
        {
            // TODO: refractor as with royal placement
            // we try and catch NullRef as failure
            // check for kings where needed
            try
            {
                if ((Field[0, 0].Number == 13) & (Field[0, 3].Number == 13) & (Field[3, 0].Number == 13) & (Field[3, 3].Number == 13))
                {
                    // then check for queens
                    if ((Field[0, 1].Number == 12) & (Field[0, 2].Number == 12) & (Field[3, 1].Number == 12) & (Field[3, 2].Number == 12))
                    {
                        // jacks last
                        if ((Field[1, 0].Number == 11) & (Field[1, 3].Number == 11) & (Field[2, 0].Number == 11) & (Field[2, 3].Number == 11))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (NullReferenceException)
            {
                // NREs are harmless - they're just the lack of cards.
            }
            return false;
        }

        public bool IsEmptyCell(int x, int y)
        {
            return (Field[x, y] == null);
        }

        public bool CanPlace(int x, int y, Card c)
        {
            // Is the card already occupied?
            if (!IsEmptyCell(x, y))
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

        public bool ShouldClean()
        {
            foreach (Card c in Field)
            {
                // if we see an empty card we're obviously not cleaning
                if (c == null)
                {
                    return false;
                }
            }
            return true;
        }



        public void RemoveCard(int x, int y)
        {
            Field[x, y] = null;
        }

        public bool AttemptPlaceCard(int x, int y, Card c)
        {
            if (CheatCanAlwaysPlaceCard || CanPlace(x, y, c))
            {
                // add and then remove
                Field[x, y] = c; // set
                Deck.Remove(Deck.Last());
                // empty the status bar messages
                Won = HasWon();
                Cleaning = ShouldClean();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AttemptPlaceCard(int x, int y)
        {
            return AttemptPlaceCard(x, y, Deck.Last());
        }
    }
}
