﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SquareRoyal
{
    public static class GlobalFunctions
    {
        private static Random r = new Random();

        const string SPADES   = "♠";
        const string HEARTS = "♥";
        const string CLUBS = "♣";
        const string DIAMONDS = "♦";

        /// <summary>
        /// Perform a Fisher–Yates shuffle.
        /// </summary>
        /// <param name="deck">The deck to use.</param>
        public static void Shuffle(this List<Card> deck)
        {
            for (int n = deck.Count - 1; n > 0; --n)
            {
                int k = r.Next(n + 1);
                Card temp = deck[n];
                deck[n] = deck[k];
                deck[k] = temp;
            }
        }

        /// <summary>
        /// Gets the unicode character for a card suit.
        /// </summary>
        /// <param name="suit">The suit.</param>
        /// <returns>The unicode character.</returns>
        public static string GetSuitCharacter(this Suit suit)
        {
            switch (suit)
            {
                case Suit.Clubs:
                    return CLUBS;
                case Suit.Diamonds:
                    return DIAMONDS;
                case Suit.Hearts:
                    return HEARTS;
                case Suit.Spades:
                    return SPADES;
                default:
                    throw new Exception("That is not a valid suit.");
            }
        }

        /// <summary>
        /// Gets the common name for a card's number.
        /// </summary>
        /// <param name="i">The card's number.</param>
        /// <returns>The one or two character string of the number.</returns>
        public static string GetCardNumberName(this int i)
        {
            switch (i)
            {
                case 13:
                    return "K";
                case 12:
                    return "Q";
                case 11:
                    return "J";
                case 1:
                    return "A";
                default:
                    return i.ToString();
            }
        }
    }

    /// <summary>
    /// Suit of a card.
    /// </summary>
    public enum Suit
    {
        Hearts, Diamonds, Clubs, Spades
    }

    /// <summary>
    /// Represents a playing card.
    /// </summary>
    public class Card
    {
        /// <summary>
        /// Number of a card, from 1 (Ace) to 13 (King)
        /// </summary>
        public int Number { get; set; }
        /// <summary>
        /// Suit of the card.
        /// </summary>
        public Suit Suit { get; set; }

        /// <summary>
        /// Generates pre-arranged deck.
        /// </summary>
        public static List<Card> GetOrderedDeck()
        {
            List<Card> deck = new List<Card>();
            // just go through each number 1-13 and make a card of each suit for them
            for (int i = 1; i < 14; i++)
            {
                foreach (Suit suit in (Suit[])Enum.GetValues(typeof(Suit)))
                {
                    deck.Add(new Card() { Number = i, Suit = suit });
                }
            }
            return deck;
        }

        public override string ToString()
        {
            return Number.GetCardNumberName() + Suit.GetSuitCharacter();
        }
    }
}
