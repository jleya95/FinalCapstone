﻿using Capstone.DAO;
using Capstone.DAO.Interfaces;
using Capstone.Models;
using Capstone.Service;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Capstone.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TestController : CommonController
    {

        public TestController(IArtistsDao artistsDao, IBarcodesDao barcodesDao, ICollectionsDao collectionsDao, IFormatsDao formatsDao,
            IFriendsDao friendsDao, IGenresDao genresDao, IImagesDao imagesDao, ILabelsDao labelsDao, ILibrariesDao librariesDao,
            IRecordBuilderDao recordBuilderDao, IRecordsArtistsDao recordsArtistsDao, IRecordsExtraArtistsDao recordsExtraArtistsDao,
            IRecordsFormatsDao recordsFormatsDao, IRecordsGenresDao recordsGenresDao, IRecordsLabelsDao recordsLabelsDao,
            IRecordService recordService, ITracksDao tracksDao, IUserDao userDao, ISearchDao searchDao)
            : base(artistsDao, barcodesDao, collectionsDao, formatsDao, friendsDao, genresDao, imagesDao, labelsDao, librariesDao,
                  recordBuilderDao, recordsArtistsDao, recordsExtraArtistsDao, recordsFormatsDao, recordsGenresDao, recordsLabelsDao,
                  recordService, tracksDao, userDao, searchDao)
        {
        }

        [HttpGet("AddRecordToDb/{discogsId}")]
        public ActionResult<RecordClient> AddRecordToDbById(int discogsId)
        {
            // this might be atrocious for performance but I am not sure how else to do this
            RecordClient output = null;

            try
            {
                // get the record from the client
                RecordClient clientSuppliedRecord = _recordService.GetRecord(discogsId);

                // need to make sure the client supplied record is at least on vinyl once
                int vinylCount = 0;
                foreach (Format item in clientSuppliedRecord.Formats)
                {
                    // go through each of the formats and count
                    if (item.Name.ToLower() == "vinyl")
                    {
                        vinylCount++;
                    }
                }
                // if there isn't at least one vinyl format, return a bad request 
                if (vinylCount < 1)
                {
                    return BadRequest("This record does not contain any vinyl. We're a vinyl only shop. Try again.");
                }

                // check if it's in the record table first
                // if yes, just need to check if we should update the database
                //      check if the date_changes is different in discogs from the last time we pulled
                //      if yes, update
                // if not in our database, add to it

                // need to refactor this so that if a downstream table load fails, it'll still go through the new created pathway
                // maybe extra column that says "fully loaded 1/0" and that only gets updated once all the things parse?
                RecordTableData existingRecord = _recordBuilderDao.GetRecordByDiscogsId(discogsId);

                // checks if you have an existing record
                // if you don't, add
                // if you do, check if it's active. If not active, assumed something in here failed
                if (existingRecord == null || existingRecord.Is_Active == false)
                {
                    // do the add if it doesn't exist
                    // can build this out first
                    RecordTableData newRecord = null;
                    if (existingRecord == null)
                    {
                        newRecord = _recordBuilderDao.AddRecord(clientSuppliedRecord);
                    }

                    // build out the preceding primary tables
                    // if count is zero, no need for the sql dao for this particular table
                    if (clientSuppliedRecord.Genres.Count != 0)
                    {
                        foreach (string genre in clientSuppliedRecord.Genres)
                        {
                            _genresDao.AddGenre(genre);
                            Genre genreReturned = _genresDao.GetGenre(genre);
                            _recordsGenresDao.AddRecordGenre(clientSuppliedRecord.Id, genreReturned.Genre_Id);
                        }
                    }
                    if (clientSuppliedRecord.Styles.Count != 0)
                    {
                        // we're just lumping genres and styles (sub genre it seems) into one
                        // so repeate the method call
                        foreach (string style in clientSuppliedRecord.Styles)
                        {
                            _genresDao.AddGenre(style);
                            Genre genreReturned = _genresDao.GetGenre(style);
                            _recordsGenresDao.AddRecordGenre(clientSuppliedRecord.Id, genreReturned.Genre_Id);
                        }
                    }
                    if (clientSuppliedRecord.Labels.Count != 0)
                    {
                        foreach (Label item in clientSuppliedRecord.Labels)
                        {
                            _labelsDao.AddLabel(item);
                            Label returnedLabel = _labelsDao.GetLabel(item);
                            _recordsLabelsDao.AddRecordLabel(clientSuppliedRecord.Id, returnedLabel.Label_Id);
                        }
                    }
                    if (clientSuppliedRecord.Formats.Count != 0)
                    {
                        foreach (Format item in clientSuppliedRecord.Formats)
                        {
                            // want only the vinyl formats...
                            if (item.Name.ToLower() == "vinyl")
                            {
                                // then go through the descriptions array and add it to the format table
                                foreach (string description in item.Descriptions)
                                {
                                    _formatsDao.AddFormat(description);
                                    Format returnedFormat = _formatsDao.GetFormat(description);
                                    _recordsFormatsDao.AddRecordFormat(clientSuppliedRecord.Id, returnedFormat.Format_Id);
                                }
                            }
                        }
                    }
                    if (clientSuppliedRecord.Artists.Count != 0)
                    {
                        foreach (Artist artist in clientSuppliedRecord.Artists)
                        {
                            _artistsDao.AddArtist(artist);
                            Artist returnedArtist = _artistsDao.GetArtist(artist);
                            _recordsArtistsDao.AddRecordArtist(clientSuppliedRecord.Id, returnedArtist.Artist_Id);
                        }
                    }
                    if (clientSuppliedRecord.ExtraArtists.Count != 0)
                    {
                        // same thing for extra artists
                        foreach (Artist extraArtist in clientSuppliedRecord.ExtraArtists)
                        {
                            _artistsDao.AddArtist(extraArtist);
                            Artist returnedArtist = _artistsDao.GetArtist(extraArtist);
                            _recordsExtraArtistsDao.AddRecordExtraArtist(clientSuppliedRecord.Id, returnedArtist.Artist_Id);
                        }
                    }

                    // then do the other downstream builds
                    if (clientSuppliedRecord.Identifiers.Count != 0)
                    {
                        foreach (Identifier identifier in clientSuppliedRecord.Identifiers)
                        {
                            identifier.Discogs_Id = clientSuppliedRecord.Id;
                            _barcodesDao.AddIdentifier(identifier);
                        }
                    }
                    if (clientSuppliedRecord.Images.Count != 0)
                    {
                        foreach (Image image in clientSuppliedRecord.Images)
                        {
                            image.Discogs_Id = clientSuppliedRecord.Id;
                            _imagesDao.AddImage(image);
                        }
                    }
                    if (clientSuppliedRecord.Tracklist.Count != 0)
                    {
                        foreach (Track track in clientSuppliedRecord.Tracklist)
                        {
                            track.Discogs_Id = clientSuppliedRecord.Id;
                            _tracksDao.AddTrack(track);
                        }
                    }
                    // if you get here, assume that everything went well
                    // activate the record
                    // if you have an existing record, activate that
                    // if you have a new record, activate that
                    if (existingRecord != null)
                    {
                        return Created("https://localhost:44315/", _recordBuilderDao.ActivateRecord(existingRecord.Discogs_Id));
                    }
                    else
                    {
                        return Created("https://localhost:44315/", _recordBuilderDao.ActivateRecord(newRecord.Discogs_Id));
                    }

                }
                else if (clientSuppliedRecord.Date_Changed != existingRecord.Discogs_Date_Changed)
                {
                    // do an update
                    // update record (but doesn't update the discog update column until it reaches the end, in case something fails)
                    _recordBuilderDao.UpdateRecord(clientSuppliedRecord);

                    // update genre
                    // not much to change. If you find the name, there are no other fields to change
                    // so check if the genre exists, if not, add it
                    if (clientSuppliedRecord.Genres.Count != 0)
                    {
                        foreach (string genre in clientSuppliedRecord.Genres)
                        {
                            // get the genre so we have the ID/check if it exists
                            Genre genreReturned = _genresDao.GetGenre(genre);

                            if (genreReturned == null)
                            {
                                // if it doesn't exist, create it
                                _genresDao.AddGenre(genre);
                            }
                            // then check if the association is there
                            if (!_recordsGenresDao.GetRecordGenreByRecordIdAndGenreId(clientSuppliedRecord.Id, genreReturned.Genre_Id))
                            {
                                // if not there, make the association
                                _recordsGenresDao.AddRecordGenre(clientSuppliedRecord.Id, genreReturned.Genre_Id);
                            }
                            // if already associated with this record, and genre in database, get to this point and do nothing
                        }
                    }
                    // same method for style
                    if (clientSuppliedRecord.Styles.Count != 0)
                    {
                        foreach (string style in clientSuppliedRecord.Styles)
                        {
                            Genre styleReturned = _genresDao.GetGenre(style);

                            if (styleReturned == null)
                            {
                                _genresDao.AddGenre(style);
                            }
                            if (!_recordsGenresDao.GetRecordGenreByRecordIdAndGenreId(clientSuppliedRecord.Id, styleReturned.Genre_Id))
                            {
                                _recordsGenresDao.AddRecordGenre(clientSuppliedRecord.Id, styleReturned.Genre_Id);
                            }
                        }
                    }
                    // label is a little different as it also has the uri
                    // if they change the name, it'll create a new entry and keep the old one
                    //      this will still happen even if they correct a typo - I don't know how else to check the name field for updates
                    if (clientSuppliedRecord.Labels.Count != 0)
                    {
                        foreach (Label label in clientSuppliedRecord.Labels)
                        {
                            Label labelReturned = _labelsDao.GetLabel(label);

                            if (labelReturned == null)
                            {
                                // if you don't find the label, create it
                                _labelsDao.AddLabel(label);
                            }
                            else
                            {
                                // if you do find it, then you have to update the properties for the uri in case it has changed
                                Label updatedLabel = _labelsDao.UpdateLabel(label);
                                // then check if the association is there
                                if (!_recordsLabelsDao.GetRecordLabelByLabelIdAndDiscogsId(clientSuppliedRecord.Id, updatedLabel.Label_Id))
                                {
                                    // if not, add it
                                    _recordsLabelsDao.AddRecordLabel(clientSuppliedRecord.Id, updatedLabel.Label_Id);
                                }
                            }
                        }
                    }
                    // format update method is the same pattern as genre/style
                    if (clientSuppliedRecord.Formats.Count != 0)
                    {
                        foreach (Format format in clientSuppliedRecord.Formats)
                        {
                            // except it has a nested list... ouch, my performance!
                            foreach (string description in format.Descriptions)
                            {
                                Format formatReturned = _formatsDao.GetFormat(description);
                                if (formatReturned == null)
                                {
                                    _formatsDao.AddFormat(description);
                                }
                                if (!_recordsFormatsDao.GetRecordFormatByRecordIdAndFormatId(clientSuppliedRecord.Id, formatReturned.Format_Id))
                                {
                                    _recordsFormatsDao.GetRecordFormatByRecordIdAndFormatId(clientSuppliedRecord.Id, formatReturned.Format_Id);
                                }
                            }


                        }
                    }
                    // similar method for artists and extra artists to genre/style
                    if (clientSuppliedRecord.Artists.Count != 0)
                    {
                        foreach (Artist artist in clientSuppliedRecord.Artists)
                        {
                            Artist artistReturned = _artistsDao.GetArtist(artist);

                            if (artistReturned == null)
                            {
                                _artistsDao.AddArtist(artist);
                            }
                            if (!_recordsArtistsDao.GetRecordArtistByRecordIdAndArtistId(clientSuppliedRecord.Id, artistReturned.Artist_Id))
                            {
                                _recordsArtistsDao.AddRecordArtist(clientSuppliedRecord.Id, artistReturned.Artist_Id);
                            }
                        }
                    }
                    if (clientSuppliedRecord.ExtraArtists.Count != 0)
                    {
                        foreach (Artist artist in clientSuppliedRecord.ExtraArtists)
                        {
                            Artist artistReturned = _artistsDao.GetArtist(artist);

                            if (artistReturned == null)
                            {
                                _artistsDao.AddArtist(artist);
                            }
                            if (!_recordsExtraArtistsDao.GetRecordExtraArtistByRecordIdAndExtraArtistId(clientSuppliedRecord.Id, artistReturned.Artist_Id))
                            {
                                _recordsExtraArtistsDao.AddRecordExtraArtist(clientSuppliedRecord.Id, artistReturned.Artist_Id);
                            }
                        }
                    }

                    // this is slightly different to the other update methods. So will track/img
                    if (clientSuppliedRecord.Identifiers.Count != 0)
                    {
                        foreach (Identifier identifier in clientSuppliedRecord.Identifiers)
                        {
                            identifier.Discogs_Id = clientSuppliedRecord.Id;
                            Identifier identifierReturned = _barcodesDao.GetIdentifier(identifier);

                            if (identifierReturned == null)
                            {
                                // if the identifier doesn't exist, add it
                                identifier.Discogs_Id = clientSuppliedRecord.Id;
                                _barcodesDao.AddIdentifier(identifier);
                            }
                            // if you find it, then some other pieces may have changed, so have to do an update
                            else
                            {
                                // need to include the discogs IDs
                                identifier.Discogs_Id = clientSuppliedRecord.Id;
                                _barcodesDao.UpdateIdentifier(identifierReturned);
                            }
                            // don't need to do another association, it's already tied through the discogs_id
                        }
                    }
                    if (clientSuppliedRecord.Tracklist.Count != 0)
                    {
                        foreach (Track track in clientSuppliedRecord.Tracklist)
                        {
                            track.Discogs_Id = clientSuppliedRecord.Id;
                            Track trackReturned = _tracksDao.GetTrack(track);

                            if (trackReturned == null)
                            {
                                track.Discogs_Id = clientSuppliedRecord.Id;
                                _tracksDao.AddTrack(track);
                            }
                            else
                            {
                                // need to include the discogs IDs
                                track.Discogs_Id = clientSuppliedRecord.Id;
                                _tracksDao.UpdateTrack(track);
                            }
                        }
                    }
                    if (clientSuppliedRecord.Images.Count != 0)
                    {
                        // this will avoid stale images:
                        // get all the current images
                        List<Image> allCurrentImages = _imagesDao.GetAllImagesByDiscogsId(clientSuppliedRecord.Id);

                        foreach (Image image in clientSuppliedRecord.Images)
                        {
                            image.Discogs_Id = clientSuppliedRecord.Id;
                            Image imageReturned = _imagesDao.GetImageInfoExact(image);

                            // if image url is not there, create one
                            if (imageReturned == null)
                            {
                                image.Discogs_Id = clientSuppliedRecord.Id;
                                _imagesDao.AddImage(image);
                            }
                            else if (imageReturned != null)
                            {
                                // if you find an image with an exact match
                                // find it on the list and take it off
                                int indexPosition = allCurrentImages.FindIndex(p => p.Uri == imageReturned.Uri);
                                allCurrentImages.RemoveAt(indexPosition);
                            }
                        }
                        // then all the ones on the list left don't exist in the new list, but are still in the database
                        // so delete them from the database
                        foreach (Image image in allCurrentImages)
                        {
                            _imagesDao.DeleteImage(image.Image_Id);
                        }
                    }

                    // if you get here, all your updates are successful, so you can update the discogs date in the database
                    // so you won't come down this path again until discogs updates its api

                    _recordBuilderDao.UpdateRecordDiscogsDateChanged(clientSuppliedRecord.Id, clientSuppliedRecord.Date_Changed);

                    return Ok("Updated record");
                }
                else
                {
                    return Ok("This record already exists in our database");
                }

            }
            catch (Exception e)
            {

                return BadRequest($"Something went wrong adding your record, id {discogsId}. Please contact an admin");
            }
        }




        //[HttpGet("search")]
        //public ActionResult<SearchResult> Search(string q, string artist, string title, string genre, string year, string country, string label, string barcode, int pageNumber = 1)
        //{
        //    // need the username to search the library
        //    string username = User.Identity.Name;
        //    username = "user";
        //    if (username == null)
        //    {
        //        return BadRequest("You must be logged in to search a library");
        //    }

        //    SearchRequest searchRequest = _recordService.GenerateRequestObject(q, artist, title, genre, year, country, label, barcode);

        //    SearchResult output = null;
        //        try
        //        {
        //            output = _recordService.SearchForRecordsDiscogs(searchRequest, pageNumber);
        //            if (output != null)
        //            {
        //                return Ok(output);
        //            }
        //            else
        //            {
        //                return NotFound();
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            return BadRequest(e.Message);
        //        }

        //    return output;
        //}

        [HttpGet("searchDatabase")]
        public ActionResult<List<RecordClient>> SearchLibrary(string q, string artist, string title, string genre, string year, string country, string label, string barcode, int pageNumber = 1)
        {
            string username = User.Identity.Name;
            username = "user";
            if (username == null)
            {
                return BadRequest("You must be logged in to search a library");
            }

            SearchRequest searchRequest = _recordService.GenerateRequestObject(q, artist, title, genre, year, country, label, barcode);

            List<RecordClient> output = new List<RecordClient>();

            try
            {
                List<int> recordIds = new List<int>();
                if (string.IsNullOrEmpty(searchRequest.Artist) && string.IsNullOrEmpty(searchRequest.Title) && string.IsNullOrEmpty(searchRequest.Genre) && string.IsNullOrEmpty(searchRequest.Year) && string.IsNullOrEmpty(searchRequest.Country) && string.IsNullOrEmpty(searchRequest.Label) && string.IsNullOrEmpty(searchRequest.Barcode))
                {
                    recordIds = _searchDao.WildcardSearchDatabaseForRecords(searchRequest.Query, username);
                }
                else
                {
                    recordIds = _searchDao.WildcardAdvancedSearchDatabaseForRecords(searchRequest, username);
                }

                if (recordIds.Count == 0)
                {
                    return NotFound();
                }

                foreach (int discogId in recordIds)
                {
                    // this is much neater
                    // refactored and put in the parent CommonController class as a helper method
                    RecordClient newFullRecord = BuildFullRecord(discogId);

                    if (newFullRecord != null)
                    {
                        output.Add(newFullRecord);
                    }
                }

                if (output != null)
                {
                    return Ok(output);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }

        [HttpGet("search")]
        public ActionResult<List<SearchResult>> SearchAll(string q, string artist, string title, string genre, string year, string country, string label, string barcode, int pageNumber = 1)
        {
            List<SearchResult> allResults = new List<SearchResult>();
            // need the username to search the library
            //TODO
            string username = User.Identity.Name;
            username = "user";
            if (username == null)
            {
                return BadRequest("You must be logged in to search a library");
            }

            SearchRequest searchRequest = _recordService.GenerateRequestObject(q, artist, title, genre, year, country, label, barcode);
            SearchResult discogsResult = null;
            SearchResult libraryResult = null;
            List<RecordClient> recordsInLibrary = null;
            SearchResult collectionsResult = null;

            if (string.IsNullOrEmpty(searchRequest.Query) && string.IsNullOrEmpty(searchRequest.Artist) && string.IsNullOrEmpty(searchRequest.Title) && string.IsNullOrEmpty(searchRequest.Genre) && string.IsNullOrEmpty(searchRequest.Year) && string.IsNullOrEmpty(searchRequest.Country) && string.IsNullOrEmpty(searchRequest.Label) && string.IsNullOrEmpty(searchRequest.Barcode))
            {
                return BadRequest("Please enter a valid search");
            }

            try
            {
                discogsResult = _recordService.SearchForRecordsDiscogs(searchRequest, pageNumber);

                if (discogsResult != null)
                {
                    return Ok(discogsResult);
                    //allResults.Add(discogsResult);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }




            return allResults;
        }

    }
}
