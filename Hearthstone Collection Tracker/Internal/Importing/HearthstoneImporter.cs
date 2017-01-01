using Hearthstone_Collection_Tracker.ViewModels;
using HearthMirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hearthstone_Collection_Tracker.Internal.Importing
{
    internal class HearthstoneImporter
    {
	    public async Task<List<BasicSetCollectionInfo>> Import()
        {
            var sets = SetCardsManager.CreateEmptyCollection();
            
			sets = sets.Where(s => s.SetName == "All").ToList();
			if (!sets.Any())
            {
                return sets;
            }

	        try
	        {
		        //var collection = Reflection.GetCollection();
		        var goldenCollection = Reflection.GetCollection().Where(x => x.Premium);
		        var commonCollection = Reflection.GetCollection().Where(x => !x.Premium);
		        foreach (var set in sets)
		        {
			        foreach (var card in set.Cards)
			        {
				        var amountGolden =
					        goldenCollection.Where(x => x.Id.Equals(card.CardId)).Select(x => x.Count).FirstOrDefault();
				        var amountNonGolden =
					        commonCollection.Where(x => x.Id.Equals(card.CardId)).Select(x => x.Count).FirstOrDefault();
				        card.AmountNonGolden = Math.Min(amountNonGolden, card.MaxAmountInCollection);
				        card.AmountGolden = Math.Min(amountGolden, card.MaxAmountInCollection);
			        }

		        }

	        }
	        catch (ImportingException)
	        {
		        throw;
	        }
	        catch (Exception e)
	        {
		        throw new ImportingException("Unexpected exception occured during importing", e);
	        }

            return sets;
        }
    }
}
