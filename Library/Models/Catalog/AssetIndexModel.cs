using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Models.Catalog
{
    public class AssetIndexModel
    {
        public IEnumerable<AssetIndexListingmodel> Assets { get; set; }
    }
}
