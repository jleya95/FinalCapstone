﻿using Capstone.DAO.Interfaces;
using Capstone.Exceptions;
using Capstone.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Capstone.DAO
{
    public class SearchSqlDao : ISearchDao
    {
        private readonly string connectionString;
        public SearchSqlDao(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

        public List<int> WildcardSearchDatabaseForRecords(SearchRequest requestObject)
        {
            List<int> recordIDs = new List<int>();

            string sql = "SELECT records.discogs_id " +
                "FROM records " +
                "JOIN records_artists ON records.discogs_id = records_artists.discogs_id " +
                "JOIN artists ON records_artists.artist_id = artists.artist_id " +
                "JOIN records_labels ON records.discogs_id = records_labels.discogs_id " +
                "JOIN labels ON records_labels.label_id = labels.label_id " +
                "JOIN barcodes ON records.discogs_id = barcodes.discogs_id " +
                "JOIN records_genres ON records.discogs_id = records_genres.discogs_id " +
                "JOIN genres ON records_genres.genre_id = genres.genre_id " +
                "WHERE (records.title LIKE @recordsTitle OR @recordsTitle = '') " +
                "AND (artists.name LIKE @artistsName OR @artistsName = '') " +
                "AND (genres.name LIKE @genresName OR @genresName = '') " +
                "AND (records.released LIKE @recordsReleased OR @recordsReleased = '') " +
                "AND (records.country LIKE @recordsCountry OR @recordsCountry = '') " +
                "AND (labels.name LIKE @labelsName OR @labelsName = '') " +
                "AND (barcodes.value LIKE @barcodesValue OR @barcodesValue = '')";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@recordsTitle", SearchStringWildcardAdder(requestObject.Title));
                    cmd.Parameters.AddWithValue("@artistsName", SearchStringWildcardAdder(requestObject.Artist));
                    cmd.Parameters.AddWithValue("@genresName", SearchStringWildcardAdder(requestObject.Genre));
                    cmd.Parameters.AddWithValue("@recordsReleased", SearchStringWildcardAdder(requestObject.Year));
                    cmd.Parameters.AddWithValue("@recordsCountry", SearchStringWildcardAdder(requestObject.Country));
                    cmd.Parameters.AddWithValue("@labelsName", SearchStringWildcardAdder(requestObject.Label));
                    cmd.Parameters.AddWithValue("@barcodesValue", SearchStringWildcardAdder(requestObject.Barcode));
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        int idToAdd = Convert.ToInt32(reader["discogs_id"]);
                        if (!recordIDs.Contains(idToAdd))
                        {
                            recordIDs.Add(idToAdd);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DaoException("Sql exception occured", ex);
            }


            return recordIDs;
        }

        protected string SearchStringWildcardAdder(string query)
        {
            return string.IsNullOrEmpty(query) ? "" : "%" + query + "%";
        }
        //        SELECT*
        //FROM records
        //JOIN records_artists ON records.discogs_id = records_artists.discogs_id
        //        JOIN artists ON records_artists.artist_id = artists.artist_id
        //JOIN records_extra_artists ON records.discogs_id = records_extra_artists.discogs_id
        //JOIN artists AS extra_artists ON records_extra_artists.extra_artist_id = extra_artists.artist_id
        //JOIN images ON records.discogs_id = images.discogs_id
        //JOIN records_labels ON records.discogs_id = records_labels.discogs_id
        //JOIN labels ON records_labels.label_id = labels.label_id
        //JOIN tracks ON records.discogs_id = tracks.discogs_id
        //JOIN barcodes ON records.discogs_id = barcodes.discogs_id
        //JOIN records_formats ON records.discogs_id = records_formats.discogs_id
        //JOIN formats ON records_formats.format_id = formats.format_id
        //JOIN records_genres ON records.discogs_id = records_genres.discogs_id
        //JOIN genres ON records_genres.genre_id = genres.genre_id















    }
}