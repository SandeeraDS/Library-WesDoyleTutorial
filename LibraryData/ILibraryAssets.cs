using LibraryData.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryData
{
    public interface ILibraryAssets
    {

        IEnumerable<LibraryAsset> GetAll();
        LibraryAsset GetByID(int Id);
        void Add(LibraryAsset NewAsset);
        string GetAuthorOrDirector(int Id);
        string GetDeweyIndex(int Id);
        string GetType(int Id);
        string GetImageUrl(int Id);
        string GetTittle(int Id);
        string GetIsbn(int Id);
        LibraryBranch GetCurrentLocation(int Id);
        string StatusName(int Id);


    }
}
