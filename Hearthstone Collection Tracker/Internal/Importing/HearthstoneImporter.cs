﻿using Hearthstone_Collection_Tracker.ViewModels;
using HearthMirror;
using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Collection_Tracker.Internal.Importing
{
    internal class HearthstoneImporter
    {
        public int ImportStepDelay { get; set; }

        public List<BasicSetCollectionInfo> Import(string importingSet)
        {
            var sets = SetCardsManager.CreateEmptyCollection();
            if(!string.IsNullOrEmpty(importingSet))
            {
                sets = sets.Where(s => s.SetName == importingSet).ToList();
            }

            if(!sets.Any())
            {
                return sets;
            }

            try
            {
                var collection = Reflection.GetCollection();
                var goldenCollection = collection.Where(x => x.Premium);
                var commonCollection = collection.Where(x => x.Premium == false);
                foreach(var set in sets)
                {
                    foreach(var card in set.Cards)
                    {
                        var amountGolden = goldenCollection.Where(x => x.Id.Equals(card.CardId)).Select(x => x.Count).FirstOrDefault();
                        var amountNonGolden = commonCollection.Where(x => x.Id.Equals(card.CardId)).Select(x => x.Count).FirstOrDefault();

                        card.AmountNonGolden = Math.Min(amountNonGolden, card.MaxAmountInCollection);
                        card.AmountGolden = Math.Min(amountGolden, card.MaxAmountInCollection);
                    }

                }

            }
            catch(ImportingException e)
            {
                Log.Error("COLLECTION TRACKER: import exception");
                throw;
            }
            catch(Exception e)
            {
                Log.Error("COLLECTION TRACKER: Random exception when importing");
                Log.Error(e);
            }

            return sets;
        }
    }
}
