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

        public ILibraryAssets _assets;

        public CatalogController(ILibraryAssets assets)
        {
            _assets = assets;
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
                ISBN = _assets.GetIsbn(id)
            };

            return View(model);
        }

    }
}
