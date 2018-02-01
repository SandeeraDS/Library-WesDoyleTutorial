using LibraryData;
using LibraryData.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraryServices
{
    public class LibraryAssetService : ILibraryAssets
    {

        private LibraryContext _context;

        public LibraryAssetService(LibraryContext context)
        {
            _context = context;
        }


        public string StatusName(int Id)
        {
            return _context.Status.FirstOrDefault(name => name.Id == Id).Name;
        }
        public void Add(LibraryAsset NewAsset)
        {
            _context.Add(NewAsset);
            _context.SaveChanges();
        }

        public IEnumerable<LibraryAsset> GetAll()
        {
            return _context.LibraryAssets
                .Include(asset => asset.Status)
                .Include(asset => asset.Location);
        }

        public string GetAuthorOrDirector(int Id)
        {
            var isBook = _context.LibraryAssets.OfType<Book>().Where(asset => asset.Id == Id).Any();

            var isVideo = _context.LibraryAssets.OfType<Video>().Where(asset => asset.Id == Id).Any();

            return isBook ? _context.Books.FirstOrDefault(book => book.Id == Id).Author : _context.Videos.FirstOrDefault(video => video.Id == Id).Director
                ?? "Unknown";
        }

        public LibraryAsset GetByID(int Id)
        {
            return GetAll().FirstOrDefault(asset => asset.Id == Id);

            /*_context.LibraryAssets
           .Include(asset => asset.Status)
           .Include(asset => asset.Location).FirstOrDefault(asset => asset.Id==Id);*/

        }

        public LibraryBranch GetCurrentLocation(int Id)
        {
            return GetByID(Id).Location;
        }

        public string GetDeweyIndex(int Id)
        {
            if (_context.Books.Any(book => book.Id == Id))
            {
                return _context.Books.FirstOrDefault(book => book.Id == Id).DeweyIndex;
            }
            else
                return "";
        }

        public string GetIsbn(int Id)
        {
            if (_context.Books.Any(book => book.Id == Id))
            {
                return _context.Books.FirstOrDefault(book => book.Id == Id).ISBN;
            }
            else
                return "";
        }

        public string GetTittle(int Id)
        {
            return _context.LibraryAssets.FirstOrDefault(title => title.Id == Id).Title;
        }

        public string GetType(int Id)
        {
            var Book = _context.LibraryAssets.OfType<Book>().Where(b => b.Id == Id);

            return Book.Any() ? "Book" : "Video";
        }

        public string GetImageUrl(int Id)
        {
            return _context.LibraryAssets.FirstOrDefault(image => image.Id == Id).ImageUrl;

        }
    }
}
