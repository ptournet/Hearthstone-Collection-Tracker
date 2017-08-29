using Hearthstone_Deck_Tracker;
using System;
using System.Collections.Generic;
using System.Linq;

/*
 * A Singleton class to manage how many copies of a card there are in all of
 * the decks the user has created. Be sure to call the Update method whenever
 * we need to refresh the deck lists.
 */

namespace Hearthstone_Collection_Tracker.Internal
{
    public sealed class CardsInDecks
    {
        private static volatile CardsInDecks instance;
        private static object syncRoot = new Object();

        private SortedDictionary<string, int> _cards;

        private CardsInDecks()
        {
            this.UpdateCardsInDecks();
        }

        public static CardsInDecks Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new CardsInDecks();
                        }
                    }
                }

                return instance;
            }
        }

        public SortedDictionary<string, int> Cards
        {
            get { return _cards; }
            set { _cards = value; }
        }

        public int CopiesInDecks(string cardName)
        {
            if (Cards.ContainsKey(cardName))
            {
                return Cards[cardName];
            }

            return 0;
        }

        public void UpdateCardsInDecks()
        {
            this.Cards = new SortedDictionary<string, int>();
            var deckList = DeckList.Instance.Decks.Where(d => !d.IsArenaDeck && !d.IsBrawlDeck).ToList();
            foreach (var deck in deckList)
            {
                foreach (var card in deck.Cards)
                {
                    if (this.Cards.ContainsKey(card.Name))
                    {
                        int copiesOfCardInDeck = this.Cards[card.Name];
                        this.Cards[card.Name] = Math.Max(card.Count, copiesOfCardInDeck);
                    }
                    else
                    {
                        this.Cards.Add(card.Name, card.Count);
                    }
                }
            }
        }
    }
}
