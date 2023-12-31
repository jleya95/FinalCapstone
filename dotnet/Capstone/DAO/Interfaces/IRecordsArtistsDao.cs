﻿using Capstone.Models;

namespace Capstone.DAO
{
    public interface IRecordsArtistsDao
    {
        public bool AddRecordArtist(int discogsId, int artistId);
        public bool GetRecordArtistByRecordIdAndArtistId(int discogsId, int artistId);
    }
}
