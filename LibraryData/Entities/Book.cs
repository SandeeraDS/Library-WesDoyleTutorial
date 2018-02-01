using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LibraryData.Entities
{
    public class Book : LibraryAsset

    {
        [Required]
        public string ISBN { get; set; }

        [Required]
        public String Author { get; set; }

        [Required]
        public String DeweyIndex { get; set; }

    }
}
