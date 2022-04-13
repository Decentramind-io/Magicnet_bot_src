using Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unsplasharp;
using WebClientEx;

//https://unsplash.com

namespace NSUnsplashImageBank
{
    class UnsplashImageBank
    {
        private UnsplasharpClient _client;
        private List<string> _apikeyList;
        private List<string> _photoCollectionsList;
        private int _currApikeyInd = -1;
        private Random _rand = new Random();

        public UnsplashImageBank()
        {
            _initApikeys();
            _initPhotoCollections();
            _reInitClient(_getNextApikey());
        }

        public MemoryStream getRandomPhoto()
        {
            string searchQuery = _getNextPhotoCollection();

            var collections = _client.SearchCollections(searchQuery).GetAwaiter().GetResult();
            if ((collections == null) || (collections.Count == 0))
            {
                _reInitClient(_getNextApikey());
                collections = _client.SearchCollections(searchQuery).GetAwaiter().GetResult();
            }

            var randomPhoto = _client.GetRandomPhoto(collections[_rand.Next(0, collections.Count - 1)].Id).GetAwaiter().GetResult();
            if (randomPhoto == null)
            {
                _reInitClient(_getNextApikey());
                randomPhoto = _client.GetRandomPhoto().GetAwaiter().GetResult();
            }

            var link = _client.GetPhotoDownloadLink(randomPhoto.Id).GetAwaiter().GetResult();

            TWebClientEx wc = new TWebClientEx();
            wc.Timeout = 10000;

            return new MemoryStream(wc.DownloadData(link));
        }

        private void _initApikeys()
        {
            //https://unsplash.com apikeys

            _apikeyList = new List<string>()
            {
                "ADSfvsdSvas-adfggS3FDgver5dFSFVsdVfdfgDSSd3",
            };
        }

        private void _initPhotoCollections()
        {
            _photoCollectionsList = new List<string>()
            {
                "mountains",
                "scenery",
                "cars",
                "cats",
                "dogs",
                "road",
                "sea",
                "surfing",
                "food",
                "moon",
            };
        }

        private string _getNextPhotoCollection()
        {
            _currApikeyInd++;

            if (_currApikeyInd >= _apikeyList.Count) _currApikeyInd = 0;

            return _photoCollectionsList[_rand.Next(0, _photoCollectionsList.Count - 1)];
        }

        private string _getNextApikey()
        {
            _currApikeyInd++;

            if (_currApikeyInd >= _apikeyList.Count) _currApikeyInd = 0;

            return _apikeyList[_currApikeyInd];
        }

        private void _reInitClient(string apikey)
        {
            _client = new UnsplasharpClient(apikey);

            TLog.ToLogAsync("UnsplashImageBank._reInitClient");
        }
    }
}
