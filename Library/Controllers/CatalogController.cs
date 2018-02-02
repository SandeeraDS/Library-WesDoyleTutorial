using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Library.Models.Catalog;
using LibraryData;

namespace Library.Controllers
{
    public class CatalogController:Controller
    {

        private ILibraryAssets _assets;
        private ICheckout _checkouts;


        public CatalogController(ILibraryAssets assets,ICheckout checkouts)
        {
            _assets = assets;
            _checkouts = checkouts;
        }

        //show Library Items
        public IActionResult Index()
        {

            var assetModels = _assets.GetAll();

            var ListingResult = assetModels.Select(result => new AssetIndexListingmodel
            {
                Id = result.Id,
                ImageUrl = result.ImageUrl,
                AuthorOrDirector = _assets.GetAuthorOrDirector(result.Id),
                DeweyCallNumber = _assets.GetDeweyIndex(result.Id),
                Title = result.Title,
                Type = _assets.GetType(result.Id)

            });

            var model = new AssetIndexModel
            {
                Assets = ListingResult

            };
            return View(model);
        }

        //get Library Item By Id
        public IActionResult Detail(int id)
        {

            var asset = _assets.GetByID(id);

            var currentHolds = _checkouts.GetCurrentHolds(id)
                .Select(a => new AssetHoldModel
                {
                    HoldPlaced = _checkouts.GetCurrentHoldPlaced(a.Id).ToString("d"),
                    PatronName = _checkouts.GetCurrentHoldPatron(a.Id)
                });

            var model = new AssetDetailModel
            {
                AssetId = id,
                Title = asset.Title,
                Year = asset.Year,
                Cost = asset.Cost,
                Status = _assets.StatusName(id),
                ImageUrl = asset.ImageUrl,
                AuthorOrDirector = _assets.GetAuthorOrDirector(id),
                CurrentLocation = _assets.GetCurrentLocation(id).Name,
                DeweyCallNumber = _assets.GetDeweyIndex(id),
                ISBN = _assets.GetIsbn(id),
                CheckoutHistory = _checkouts.GetCheckoutHistory(id),
              //  CurrentAssociatedLibraryCard = _assets.GetLibraryCardByAssetId(id),
                LatestCheckout =_checkouts.GetLatestCheckout(id),
                PatronName=_checkouts.GetCurrentPatron(id),
                CurrentHolds=currentHolds
            };

            return View(model);
        }

    }
}
