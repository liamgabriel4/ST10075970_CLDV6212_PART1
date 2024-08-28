using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using Azure;
using Azure.Data.Tables;

namespace POEtest.Models
{
    public class Order : ITableEntity
    {
        [Key]
        public int Order_Id { get; set; }

        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        
        [Required(ErrorMessage = "Please select a user.")]
        public int User_ID { get; set; }

        [Required(ErrorMessage = "Please select a product.")]
        public int Product_ID { get; set; } 

        [Required(ErrorMessage = "Please select the date.")]
        public DateTime Order_Date { get; set; }

        [Required(ErrorMessage = "Please enter the location.")]
        public string? Order_Location { get; set; }
    }
}
