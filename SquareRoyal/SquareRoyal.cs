﻿using System;
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
        /// <summary>
        /// If the user can always place a card, even in invalid positions.
        /// </summary>
        public bool CheatCanAlwaysPlaceCard { get; set; }
        /// <summary>
        /// If the user can always discord a card, even without a valid pair.
        /// </summary>
        public bool CheatCanAlwaysDiscardCard { get; set; }

        /// <summary>
        /// Starts a new game.
        /// </summary>
        public SquareRoyal()
        {
            // these get 
            CheatCanAlwaysDiscardCard = false;
            CheatCanAlwaysPlaceCard = false;
            Deck = CardFunctions.GetOrderedDeck();
            Deck.Shuffle();
            Field = new Card[4, 4];
            Cleaning = false;
        }

        /// <summary>
        /// Checks if the game is in a win state.
        /// </summary>
        /// <returns>If the game has been won.</returns>
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

        /// <summary>
        /// Checks if a slot is empty.
        /// </summary>
        /// <param name="x">The x position of the slot.</param>
        /// <param name="y">The y position of the slot.</param>
        /// <returns>If the slot is empty.</returns>
        public bool IsEmptyCell(int x, int y)
        {
            return (Field[x, y] == null);
        }

        /// <summary>
        /// Checks if a card can be placed into a slot.
        /// </summary>
        /// <param name="x">The x position of the slot.</param>
        /// <param name="y">The y position of the slot.</param>
        /// <param name="c">The card to try to place.</param>
        /// <returns>If the card can be placed or not.</returns>
        public bool CanPlace(int x, int y, Card c)
        {
            if (Cleaning)
            {
                return false;
            }
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

        /// <summary>
        /// Checks if a card has another card to pair with, to add to ten.
        /// </summary>
        /// <param name="x">The x position of the card.</param>
        /// <param name="y">The y position of the card.</param>
        /// <returns>If the card has a pair or not.</returns>
        public bool HasPair(int x, int y)
        {
            if (IsEmptyCell(x, y) || Field[x, y].Number > 9)
            {
                return false; // no point
            }
            for (int ix = 0; ix < 4; ix++)
            {
                for (int iy = 0; iy < 4; iy++)
                {
                    if (ix == x & iy == y)
                    {
                        continue;
                    }
                    if (!IsEmptyCell(ix, iy))
                    {
                        if (Field[ix, iy].Number + Field[x, y].Number == 10)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Checks to see if the game in stuck, either by a lack of cards pairs
        /// to clean, or a lack of available slots to place cards into.
        /// </summary>
        /// <returns>If the game is stuck or not.</returns>
        public bool CheckIfStuck()
        {
            if (Cleaning)
            {
                if (CheatCanAlwaysDiscardCard)
                {
                    return false;
                }
                for (int x = 0; x < 4; x++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        if (IsEmptyCell(x, y))
                        {
                            return false;
                        }
                        if (Field[x,y].Number == 10 || (DiscardNeedsPair(x,y) & HasPair(x, y)))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            else
            {
                if (CheatCanAlwaysPlaceCard)
                {
                    return false;
                }
                switch (Deck.Last().Number)
                {
                    case 13:
                        // (Field[0, 0].Number == 13) & (Field[0, 3].Number == 13) & (Field[3, 0].Number == 13) & (Field[3, 3].Number == 13)
                        return !(IsEmptyCell(0, 0) || IsEmptyCell(0, 3) || IsEmptyCell(3, 0) || IsEmptyCell(3, 3));
                    case 12:
                        // (Field[0, 1].Number == 12) & (Field[0, 2].Number == 12) & (Field[3, 1].Number == 12) & (Field[3, 2].Number == 12)
                        return !(IsEmptyCell(0, 1) || IsEmptyCell(0, 2) || IsEmptyCell(3, 1) || IsEmptyCell(3, 2));
                    case 11:
                        return !(IsEmptyCell(1, 0) || IsEmptyCell(1, 3) || IsEmptyCell(2, 0) || IsEmptyCell(2, 3));
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Checks to see if the game should enter the cleaning phase, due to a
        /// lack of space on the board.
        /// </summary>
        /// <returns>If the cleaning phase should be entered.</returns>
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

        /// <summary>
        /// Attempts to exit the cleaning phase.
        /// </summary>
        /// <remarks>
        /// If the game isn't in the cleaning phase, or there's still a lack of
        /// empty space, then this will return false.
        /// </remarks>
        /// <returns>If the cleaning phase was exited.</returns>
        public bool StopCleaning()
        {
            if (Cleaning & !ShouldClean())
            {
                Cleaning = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Removes a card from the board.
        /// </summary>
        /// <param name="x">The x position of the card to remove.</param>
        /// <param name="y">The y position of the card to remove.</param>
        public void RemoveCard(int x, int y)
        {
            Field[x, y] = null;
        }

        /// <summary>
        /// Attempts to place a card on to the board.
        /// </summary>
        /// <param name="x">The x position to place the card in.</param>
        /// <param name="y">The y position to place the card in.</param>
        /// <param name="c">The card to place.</param>
        /// <returns>If the card was placed or not.</returns>
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

        /// <summary>
        /// Attempts to place the current card in the deck on to the board.
        /// </summary>
        /// <param name="x">The x position to place the card in.</param>
        /// <param name="y">The y position to place the card in.</param>
        /// <returns>If the card was placed or not.</returns>
        public bool AttemptPlaceCard(int x, int y)
        {
            return AttemptPlaceCard(x, y, Deck.Last());
        }

        public bool CanDiscard(int x, int y)
        {
            return CheatCanAlwaysDiscardCard ||
                (!IsEmptyCell(x, y) & Field[x, y].Number < 11);
        }

        /// <summary>
        /// Sees if a card needs to be matched in a pair.
        /// </summary>
        /// <param name="x">The x position of the card to check.</param>
        /// <param name="y">The y position of the card to check.</param>
        /// <returns>
        /// If the card needs another card to be paired with.
        /// </returns>
        public bool DiscardNeedsPair(int x, int y)
        {
            if (CheatCanAlwaysDiscardCard)
            {
                return false;
            }
            else
            {
                return !IsEmptyCell(x, y) & Field[x, y].Number < 10;
            }
        }

        /// <summary>
        /// Tries to see if a card is valued at 10 to be discarded, and does so
        /// if possible.
        /// </summary>
        /// <param name="x">The x position of the card to remove.</param>
        /// <param name="y">The y position of the card to remove.</param>
        /// <returns>If the card was removed.</returns>
        public bool AttemptDiscardSingleCard(int x, int y)
        {
            if (CheatCanAlwaysDiscardCard ||
                (!IsEmptyCell(x, y) & Field[x, y].Number == 10))
            {
                RemoveCard(x, y);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Try to see if two card pairs add up to ten, so they can be
        /// discarded, and does so if possible.
        /// </summary>
        /// <param name="x1">
        /// The x position of the first card to remove.
        /// </param>
        /// <param name="y1">
        /// The y position of the first card to remove.
        /// </param>
        /// <param name="x2">
        /// The x position of the second card to remove.
        /// </param>
        /// <param name="y2">
        /// The y position of the second card to remove.
        /// </param>
        /// <returns>If the pair was removed.</returns>
        public bool AttemptDiscardPair(int x1, int y1, int x2, int y2)
        {
            if (CheatCanAlwaysDiscardCard || (Cleaning & !IsEmptyCell(x1, y1) & !IsEmptyCell(x2, y2)
                & Field[x1, y1] != Field[x2, y2] & Field[x1, y1].Number + Field[x2, y2].Number == 10))
            {
                RemoveCard(x1, y1);
                RemoveCard(x2, y2);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
