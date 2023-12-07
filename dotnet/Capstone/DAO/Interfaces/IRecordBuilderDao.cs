﻿using Capstone.Models;

namespace Capstone.DAO
{
    public interface IRecordBuilderDao
    {
        public RecordTableData GetRecordByDiscogsId(int discogsId);
        public RecordTableData GetRecordByRecordId(int recordId);
        public RecordTableData AddRecord(RecordClient input);
        public RecordTableData ActivateRecord(int discogsId);
        public RecordTableData UpdateRecord(RecordClient input);
    }
}