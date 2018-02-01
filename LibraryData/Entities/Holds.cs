using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryData.Entities
{
   public class Holds
    {
        public int Id { get; set; }
        public virtual LibraryAsset LibraryAssets { get; set; }
        public virtual LibraryCard LibraryCards { get; set; }
        public DateTime HoldPlace { get; set; }
    }
}
