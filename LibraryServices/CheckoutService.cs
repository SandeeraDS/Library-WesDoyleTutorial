﻿using LibraryData;
using System;
using System.Collections.Generic;
using System.Text;
using LibraryData.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace LibraryServices
{
    public class CheckoutService : ICheckout
    {
            private LibraryContext _context; // private field to store the context.

            public CheckoutService(LibraryContext context)
            {
                _context = context;
            }

            public void Add(Checkouts newCheckout)
            {
                _context.Add(newCheckout);
                _context.SaveChanges();
            }

            public Checkouts Get(int id)
            {
                return _context.Checkouts.FirstOrDefault(p => p.Id == id);
            }

            public IEnumerable<Checkouts> GetAll()
            {
                return _context.Checkouts;
            }

            public void CheckoutItem(int assetId, int libraryCardId)
            {
                if (IsCheckedOut(assetId))
                {
                    return;
                    // Add logic here to handle feedback to the user.
                }

                var item = _context.LibraryAssets
                    .Include(a => a.Status)
                    .FirstOrDefault(a => a.Id == assetId);

                _context.Update(item);

                item.Status = _context.Status
                    .FirstOrDefault(a => a.Name == "Checked Out");

                var now = DateTime.Now;

                var libraryCard = _context.LibraryCards
                    .Include(c => c.Checkouts)
                    .FirstOrDefault(a => a.Id == libraryCardId);

                var checkout = new Checkouts
                {
                    LibraryAsset = item,
                    LibraryCard = libraryCard,
                    Since = now,
                    Until = GetDefaultCheckoutTime(now)
                };

                _context.Add(checkout);

                var checkoutHistory = new CheckoutHistory
                {
                    CheckIn = now,
                    LibraryAsset = item,
                    LibraryCard = libraryCard
                };

                _context.Add(checkoutHistory);
                _context.SaveChanges();
            }

            public void MarkLost(int assetId)
            {
                var item = _context.LibraryAssets
                    .FirstOrDefault(a => a.Id == assetId);

                _context.Update(item);

                item.Status = _context.Status.FirstOrDefault(a => a.Name == "Lost");

                _context.SaveChanges();
            }

            public void MarkFound(int assetId)
            {
                var item = _context.LibraryAssets
                    .FirstOrDefault(a => a.Id == assetId);

                _context.Update(item);
                item.Status = _context.Status.FirstOrDefault(a => a.Name == "Available");
                var now = DateTime.Now;

                // remove any existing checkouts on the item
                var checkout = _context.Checkouts
                    .FirstOrDefault(a => a.LibraryAsset.Id == assetId);
                if (checkout != null)
                {
                    _context.Remove(checkout);
                }

                // close any existing checkout history
                var history = _context.CheckoutHistories
                    .FirstOrDefault(h =>
                    h.LibraryAsset.Id == assetId
                    && h.CheckIn == null);
                if (history != null)
                {
                    _context.Update(history);
                    history.CheckIn = now;
                }

                _context.SaveChanges();
            }

            public void PlaceHold(int assetId, int libraryCardId)
            {
                var now = DateTime.Now;

                var asset = _context.LibraryAssets
                    .Include(a => a.Status)
                    .FirstOrDefault(a => a.Id == assetId);

                var card = _context.LibraryCards
                    .FirstOrDefault(a => a.Id == libraryCardId);

                _context.Update(asset);

                if (asset.Status.Name == "Available")
                {
                    asset.Status = _context.Status.FirstOrDefault(a => a.Name == "On Hold");
                }

                var hold = new Holds
                {
                    HoldPlace = now,
                    LibraryAssets = asset,
                    LibraryCards = card
                };

                _context.Add(hold);
                _context.SaveChanges();
            }

            public void CheckInItem(int assetId)
            {
                var now = DateTime.Now;

                var item = _context.LibraryAssets
                    .FirstOrDefault(a => a.Id == assetId);

                _context.Update(item);

                // remove any existing checkouts on the item
                var checkout = _context.Checkouts
                    .Include(c => c.LibraryAsset)
                    .Include(c => c.LibraryCard)
                    .FirstOrDefault(a => a.LibraryAsset.Id == assetId);
                if (checkout != null)
                {
                    _context.Remove(checkout);
                }

                // close any existing checkout history
                var history = _context.CheckoutHistories
                    .Include(h => h.LibraryAsset)
                    .Include(h => h.LibraryCard)
                    .FirstOrDefault(h =>
                    h.LibraryAsset.Id == assetId
                    && h.CheckOut == null);
                if (history != null)
                {
                    _context.Update(history);
                    history.CheckIn = now;
                }

                // look for current holds
                var currentHolds = _context.Holds
                    .Include(a => a.LibraryAssets)
                    .Include(a => a.LibraryCards)
                    .Where(a => a.LibraryAssets.Id == assetId);

                // if there are current holds, check out the item to the earliest
                if (currentHolds.Any())
                {
                    CheckoutToEarliestHold(assetId, currentHolds);
                    return;
                }

                // otherwise, set item status to available
                item.Status = _context.Status.FirstOrDefault(a => a.Name == "Available");

                _context.SaveChanges();
            }

            private void CheckoutToEarliestHold(int assetId, IEnumerable<Holds> currentHolds)
            {
                var earliestHold = currentHolds.OrderBy(a => a.HoldPlace).FirstOrDefault();
                var card = earliestHold.LibraryCards;
                _context.Remove(earliestHold);
                _context.SaveChanges();

                CheckoutItem(assetId, card.Id);
            }

            public IEnumerable<CheckoutHistory> GetCheckoutHistory(int id)
            {
                return _context.CheckoutHistories
                    .Include(a => a.LibraryAsset)
                    .Include(a => a.LibraryCard)
                    .Where(a => a.LibraryAsset.Id == id);
            }

            public Checkouts GetLatestCheckout(int id)
            {
                return _context.Checkouts
                    .Where(c => c.LibraryAsset.Id == id)
                    .OrderByDescending(c => c.Since)
                    .FirstOrDefault();
            }

            public int GetAvailableCopies(int id)
            {
                var numberOfCopies = GetNumberOfCopies(id);

                var numberCheckedOut = _context.Checkouts
                    .Where(a => a.LibraryAsset.Id == id
                             && a.LibraryAsset.Status.Name == "Checked Out")
                             .Count();

                return numberOfCopies - numberCheckedOut;
            }

            public int GetNumberOfCopies(int id)
            {
                return _context.LibraryAssets
                    .FirstOrDefault(a => a.Id == id)
                    .NumberOfCopies;
            }

            private DateTime GetDefaultCheckoutTime(DateTime now)
            {
                return now.AddDays(30);
            }

            public bool IsCheckedOut(int id)
            {
                var isCheckedOut = _context.Checkouts.Where(a => a.LibraryAsset.Id == id).Any();

                return isCheckedOut;
            }

            public string GetCurrentHoldPatron(int holdId)
            {
                var hold = _context.Holds
                    .Include(a => a.LibraryAssets)
                    .Include(a => a.LibraryCards)
                    .Where(v => v.Id == holdId);

                var cardId = hold
                    .Include(a => a.LibraryCards)
                    .Select(a => a.LibraryCards.Id)
                    .FirstOrDefault();

                var patron = _context.Patrons
                    .Include(p => p.LibraryCard)
                    .FirstOrDefault(p => p.LibraryCard.Id == cardId);

                return patron.FirstName + " " + patron.LastName;
            }

            public string GetCurrentHoldPlaced(int holdId)
            {
                var hold = _context.Holds
                    .Include(a => a.LibraryAssets)
                    .Include(a => a.LibraryCards)
                    .Where(v => v.Id == holdId);

                return hold.Select(a => a.HoldPlace)
                    .FirstOrDefault().ToString();
            }

            public IEnumerable<Holds> GetCurrentHolds(int id)
            {
                return _context.Holds
                    .Include(h => h.LibraryAssets)
                    .Where(a => a.LibraryAssets.Id == id);
            }

            public string GetCurrentPatron(int id)
            {
                var checkout = _context.Checkouts
                    .Include(a => a.LibraryAsset)
                    .Include(a => a.LibraryCard)
                    .Where(a => a.LibraryAsset.Id == id)
                    .FirstOrDefault();

                if (checkout == null)
                {
                    return "Not checked out.";
                }

                var cardId = checkout.LibraryCard.Id;

                var patron = _context.Patrons
                    .Include(p => p.LibraryCard)
                    .Where(c => c.LibraryCard.Id == cardId)
                    .FirstOrDefault();

                return patron.FirstName + " " + patron.LastName;
            }

        }
    }