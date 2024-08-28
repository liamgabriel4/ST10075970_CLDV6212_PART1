﻿using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace POEtest.Models
{
    public class User : ITableEntity
    {
        [Key]
        public int User_Id { get; set; }
        public string? User_Name { get; set; }
        public string? email { get; set; }
        public string? password { get; set; }

        // ITableEntity implementation
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}
