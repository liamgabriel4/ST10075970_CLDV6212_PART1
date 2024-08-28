using Azure;
using Azure.Data.Tables;
using System;
using System.ComponentModel.DataAnnotations;

namespace POEtest.Models
{
    public class Product : ITableEntity
    {
        [Key]
        public int Product_Id { get; set; }
        public string? Product_Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? Location { get; set; }

        // ITableEntity implementation
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}
