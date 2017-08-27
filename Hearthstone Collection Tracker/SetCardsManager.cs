using Hearthstone_Collection_Tracker.Internal;
using Hearthstone_Collection_Tracker.ViewModels;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Collection_Tracker
{
    internal static class SetCardsManager
    {
        public static readonly string[] CollectableSets = { "Classic", "Goblins vs Gnomes", "The Grand Tournament", "Whispers of the Old Gods", "Mean Streets of Gadgetzan", "Journey to Un'Goro", "Knights of the Frozen Throne" };

        public static readonly string[] StandardSets = { "Classic", "Whispers of the Old Gods", "Mean Streets of Gadgetzan", "Journey to Un'Goro", "Knights of the Frozen Throne" };

        public static List<BasicSetCollectionInfo> LoadSetsInfo(string collectionStoragePath)
        {
            List<BasicSetCollectionInfo> collection = null;
            try
            {
                var setInfos = XmlManager<List<BasicSetCollectionInfo>>.Load(collectionStoragePath);
                if (setInfos != null)
                {
                    var cards = Database.GetActualCards();
                    var cardsInDecks = GetCardsInDecks();
                    collection = setInfos;
                    foreach (var set in CollectableSets)
                    {
                        var currentSetCards = cards.Where(c => c.Set.Equals(set, StringComparison.InvariantCultureIgnoreCase));
                        var setInfo = setInfos.FirstOrDefault(si => si.SetName.Equals(set, StringComparison.InvariantCultureIgnoreCase));
                        if (setInfo == null)
                        {
                            collection.Add(new BasicSetCollectionInfo()
                            {
                                SetName = set,
                                Cards = currentSetCards.Select(c => new CardInCollection(c, cardsInDecks.Where(cid => cid.Key == c.Name).Select(p => p.Value).FirstOrDefault())).ToList()
                            });
                        }
                        else
                        {
                            foreach (var card in currentSetCards)
                            {
                                var savedCard = setInfo.Cards.FirstOrDefault(c => c.CardId == card.Id);
                                if (savedCard == null)
                                {
                                    setInfo.Cards.Add(new CardInCollection(card, cardsInDecks.Where(cid => cid.Key == card.Name).Select(p => p.Value).FirstOrDefault()));
                                }
                                else
                                {
                                    savedCard.Card = card;
                                    savedCard.AmountGolden = savedCard.AmountGolden.Clamp(0, savedCard.MaxAmountInCollection);
                                    savedCard.AmountNonGolden = savedCard.AmountNonGolden.Clamp(0, savedCard.MaxAmountInCollection);
                                    savedCard.CopiesInDecks = cardsInDecks.ContainsKey(card.Name) ? cardsInDecks[card.Name] : 0;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("File with your collection information is corrupted.", ex);
            }
            return collection;
        }

        public static List<BasicSetCollectionInfo> CreateEmptyCollection()
        {
            var cards = Database.GetActualCards();
            var cardsInDecks = GetCardsInDecks();
            var setCards = CollectableSets.Select(set => new BasicSetCollectionInfo()
            {
                SetName = set,
                Cards = cards.Where(c => c.Set == set)
                        .Select(c => new CardInCollection(c, cardsInDecks.Where(cid => cid.Key == c.Name).Select(p => p.Value).FirstOrDefault()))
                        .ToList()
            }).ToList();
            return setCards;
        }

        public static SortedDictionary<string, int> GetCardsInDecks()
        {
            var cardsInDecks = new SortedDictionary<string, int>();
            var deckList = DeckList.Instance.Decks.Where(d => !d.IsArenaDeck && !d.IsBrawlDeck).ToList();
            foreach (var deck in deckList)
            {
                foreach (var card in deck.Cards)
                {
                    if (cardsInDecks.ContainsKey(card.Name))
                    {
                        int copiesOfCardInDeck = cardsInDecks[card.Name];
                        cardsInDecks[card.Name] = Math.Max(card.Count, copiesOfCardInDeck);
                    }
                    else
                    {
                        cardsInDecks.Add(card.Name, card.Count);
                    }
                }
            }
            return cardsInDecks;
        }

        public static void SaveCollection(List<BasicSetCollectionInfo> collections, string saveFilePath)
        {
            XmlManager<List<BasicSetCollectionInfo>>.Save(saveFilePath, collections);
        }
    }
}
