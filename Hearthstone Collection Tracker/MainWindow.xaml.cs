﻿using Hearthstone_Collection_Tracker.Internal;
using Hearthstone_Collection_Tracker.ViewModels;
using Hearthstone_Deck_Tracker;
using HearthDb.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Hearthstone_Collection_Tracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public Thickness TitleBarMargin
        {
            get { return new Thickness(0, TitlebarHeight, 0, 0); }
        }

        public static void Refresh()
        {
            if (Application.Current.Windows.OfType<MainWindow>().Any())
            {
                MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                if (mainWindow.SetStats != null)
                {
                    mainWindow.SetStats.ItemsSource = null;
                    mainWindow.GetSetsInfo();
                    mainWindow.SetStats.ItemsSource = mainWindow.SetsInfo;
                }
            }
        }

        public IEnumerable<SetDetailInfoViewModel> SetsInfo { get; set; }

        public MainWindow()
        {
	        try
	        {
				this.GetSetsInfo();

				this.MaxHeight = SystemParameters.PrimaryScreenHeight;
				InitializeComponent();

				Filter.PropertyChanged += (sender, args) =>
				{
					HandleFilterChange(sender, args);
				};

				string activeAccount = HearthstoneCollectionTrackerPlugin.Settings.ActiveAccount;
				Title = "Collection Tracker";
				if(!string.IsNullOrEmpty(activeAccount))
				{
					Title += " (" + activeAccount + ")";
				}
            }
			catch(Exception e)
			{
				var f = MessageBox.Show("Your Collection config file seems to be corrupted or out of date. \nReset it now?\n sorry for any inconvenience.", "Hearthstone Collection Tracker", MessageBoxButton.YesNo);
				if(f == MessageBoxResult.Yes)
				{
					foreach(
						var file in
						Directory.GetFiles(HearthstoneCollectionTrackerPlugin.PluginDataDir, "Collection_*.xml",
							SearchOption.TopDirectoryOnly))
					{
						File.Delete(file);

					}
					Core.MainWindow.Restart();
				}
			}
		}

        private void EditCollection(SetDetailInfoViewModel setInfo)
        {
            OpenCollectionForEditing(setInfo.SetCards);
        }

        #region Collection management

        public FilterSettings Filter { get; set; } = new FilterSettings();

        private void OpenCollectionForEditing(TrulyObservableCollection<CardInCollection> cards)
        {
            CardCollectionEditor.ItemsSource = cards;
            ListCollectionView view = (ListCollectionView)CollectionViewSource.GetDefaultView(cards);
            view.Filter = CardsFilter;
            if (!view.GroupDescriptions.Any())
            {
                view.GroupDescriptions.Add(new PropertyGroupDescription("CardClass"));
            }
            view.CustomSort = new CardInCollectionComparer();

            FlyoutCollection.IsOpen = true;
        }

        private bool CardsFilter(object card)
        {
            CardInCollection c = card as CardInCollection;
            if (Filter.OnlyDesired)
            {
                if ((Filter.GoldenCards && c.AmountGolden >= c.ActualDesiredAmount)
                    || (!Filter.GoldenCards && c.AmountNonGolden >= c.ActualDesiredAmount))
                {
                    return false;
                }
            }
            if (Filter.OnlyMissing)
            {
                if ((Filter.GoldenCards && c.AmountGolden >= c.MaxAmountInCollection)
                    || (!Filter.GoldenCards && c.AmountNonGolden >= c.MaxAmountInCollection))
                {
                    return false;
                }
            }
            if (Filter.FormattedText == string.Empty)
                return true;

            Rarity rarity;
            bool filteringByRarity = Enum.TryParse(Filter.FormattedText, true, out rarity);
            if (filteringByRarity)
            {
                return c.Card.Rarity == rarity;
            }

            var cardName = Helper.RemoveDiacritics(c.Card.LocalizedName, true);
            return cardName.Contains(Filter.FormattedText, StringComparison.OrdinalIgnoreCase)
                || c.Card.AlternativeNames.Any(n => Helper.RemoveDiacritics(n, true).Contains(Filter.FormattedText, StringComparison.OrdinalIgnoreCase));
        }

        private CancellationTokenSource _filterCancel = new CancellationTokenSource();

        private async Task HandleFilterChange(object sender, PropertyChangedEventArgs args)
        {
            if (_filterCancel != null && !_filterCancel.IsCancellationRequested)
            {
                _filterCancel.Cancel();
            }

            if (args.PropertyName == "Text")
            {
                if (Filter.Text.Length < 4)
                {
                    // wait 300 ms before filtering
                    _filterCancel = new CancellationTokenSource();
                    Task t = Task.Delay(TimeSpan.FromMilliseconds(300), _filterCancel.Token);
                    await t;
                }
                FilterCollection();
            }
            else
            {
                FilterCollection();
            }
        }

        private void FilterCollection()
        {
            if (CardCollectionEditor.ItemsSource != null)
            {
                CollectionViewSource.GetDefaultView(CardCollectionEditor.ItemsSource).Refresh();
            }
        }

        private void TextBoxCollectionFilter_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var index = CardCollectionEditor.SelectedIndex;
            CardInCollection card = null;
            switch (e.Key)
            {
                case Key.Enter:
                    if (CardCollectionEditor.SelectedItem != null)
                        card = (CardInCollection)CardCollectionEditor.SelectedItem;
                    else if (CardCollectionEditor.Items.Count > 0)
                        card = (CardInCollection)CardCollectionEditor.Items[0];
                    break;
                case Key.D1:
                    if (CardCollectionEditor.Items.Count > 0)
                        card = (CardInCollection)CardCollectionEditor.Items[0];
                    break;
                case Key.D2:
                    if (CardCollectionEditor.Items.Count > 1)
                        card = (CardInCollection)CardCollectionEditor.Items[1];
                    break;
                case Key.D3:
                    if (CardCollectionEditor.Items.Count > 2)
                        card = (CardInCollection)CardCollectionEditor.Items[2];
                    break;
                case Key.D4:
                    if (CardCollectionEditor.Items.Count > 3)
                        card = (CardInCollection)CardCollectionEditor.Items[3];
                    break;
                case Key.D5:
                    if (CardCollectionEditor.Items.Count > 4)
                        card = (CardInCollection)CardCollectionEditor.Items[4];
                    break;
                case Key.Down:
                    if (index < CardCollectionEditor.Items.Count - 1)
                        CardCollectionEditor.SelectedIndex += 1;
                    break;
                case Key.Up:
                    if (index > 0)
                        CardCollectionEditor.SelectedIndex -= 1;
                    break;
            }
            if (card != null)
            {
                UpdateCardsAmount(card, 1);
                e.Handled = true;
            }
        }

        private void UpdateCardsAmount(CardInCollection card, int difference)
        {
            if (Filter.GoldenCards)
            {
                int newValue = card.AmountGolden + difference;
                newValue = newValue.Clamp(0, card.MaxAmountInCollection);
                card.AmountGolden = newValue;
            }
            else
            {
                int newValue = card.AmountNonGolden + difference;
                newValue = newValue.Clamp(0, card.MaxAmountInCollection);
                card.AmountNonGolden = newValue;
            }
        }

        private void CardCollectionEditor_OnKeyDown(object sender, KeyEventArgs e)
        {
            int? amount = null;
            if (e.Key == Key.Enter)
            {
                amount = 1;
            }
            else if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                amount = -1;
            }
            if (amount.HasValue)
            {
                CardInCollection card = (CardInCollection)CardCollectionEditor.SelectedItem;
                if (card == null || string.IsNullOrEmpty(card.Card.Name))
                    return;

                UpdateCardsAmount(card, amount.Value);
            }
        }

        private void CardCollectionEditor_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var originalSource = (DependencyObject)e.OriginalSource;
            while ((originalSource != null) && !(originalSource is ListViewItem))
                originalSource = VisualTreeHelper.GetParent(originalSource);

            if (originalSource != null)
            {
                var card = (CardInCollection)CardCollectionEditor.SelectedItem;
                if (card == null)
                    return;
                UpdateCardsAmount(card, 1);
            }
        }

        private void CardCollectionEditor_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var originalSource = (DependencyObject)e.OriginalSource;
            while ((originalSource != null) && !(originalSource is ListViewItem))
                originalSource = VisualTreeHelper.GetParent(originalSource);

            if (originalSource != null)
            {
                var card = (CardInCollection)CardCollectionEditor.SelectedItem;
                if (card == null)
                    return;
                UpdateCardsAmount(card, -1);
            }
        }

        #endregion

        private void MainWindow_OnContentRendered(object sender, EventArgs e)
        {
            this.SizeToContent = SizeToContent.Manual;
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            HearthstoneCollectionTrackerPlugin.Settings.SaveCurrentAccount(SetsInfo.Select(s => new BasicSetCollectionInfo
            {
                SetName = s.SetName,
                Cards = s.SetCards.ToList()
            }).ToList());
        }

        private void FlyoutCollection_OnIsOpenChanged(object sender, RoutedEventArgs e)
        {
            // Change Show Desired filter settings
            ShowOnlyDesired.Visibility = HearthstoneCollectionTrackerPlugin.Settings.EnableDesiredCardsFeature ? Visibility.Visible : Visibility.Collapsed;

            if (FlyoutCollection.IsOpen)
                TextBoxCollectionFilter.Focus();

            MainWrapPanel.HorizontalAlignment = FlyoutCollection.IsOpen
                ? HorizontalAlignment.Left : HorizontalAlignment.Center;
        }

        private void GetSetsInfo()
        {
            CardsInDecks.Instance.UpdateCardsInDecks();
            this.SetsInfo = HearthstoneCollectionTrackerPlugin.Settings.ActiveAccountSetsInfo.Select(set => new SetDetailInfoViewModel
            {
                SetName = set.SetName,
                SetCards = new TrulyObservableCollection<CardInCollection>(set.Cards.ToList())
            }).ToList();
        }

        private void ManageAllCards_Click(object sender, RoutedEventArgs e)
        {
            var collection = new TrulyObservableCollection<CardInCollection>(SetsInfo.SelectMany(si => si.SetCards).ToList());
            OpenCollectionForEditing(collection);
        }

        private void ManageStandardCards_Click(object sender, RoutedEventArgs e)
        {
            var collection = new TrulyObservableCollection<CardInCollection>(SetsInfo.Where(s => s.IsStandardSet).SelectMany(si => si.SetCards).ToList());
            OpenCollectionForEditing(collection);
        }
    }

    internal class CardInCollectionComparer : IComparer
    {
        private const string Neutral = "Neutral";
        public int Compare(object x, object y)
        {
            if (x is CardInCollection && y is CardInCollection)
            {
                CardInCollection cardX = (CardInCollection)x;
                CardInCollection cardY = (CardInCollection)y;
                // workaround to put neutral cards last
                bool xIsNeutral = cardX.CardClass == Neutral;
                bool yIsNeutral = cardY.CardClass == Neutral;
                if (xIsNeutral && !yIsNeutral)
                    return 1;
                if (!xIsNeutral && yIsNeutral)
                    return -1;
                int cardClassCompare = cardX.CardClass.CompareTo(cardY.CardClass);
                if (cardClassCompare != 0)
                    return cardClassCompare;
                int manaCostCompare = cardX.Card.Cost.CompareTo(cardY.Card.Cost);
                if (manaCostCompare != 0)
                    return manaCostCompare;
                return cardX.Card.LocalizedName.CompareTo(cardY.Card.LocalizedName);
            }
            else
            {
                return 1;
            }
        }
    }
}
