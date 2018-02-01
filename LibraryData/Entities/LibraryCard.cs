using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryData.Entities
{
    public class LibraryCard
    {
        public int Id { get; set; }

        public decimal Fees { get; set; }

        public DateTime Created { get; set; }

        public virtual IEnumerable<Checkouts> Checkouts { get; set; }
    }
}
