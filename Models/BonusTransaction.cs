using System;
using UniversityBonusSystem.Extensions; 

namespace UniversityBonusSystem.Models
{
    public class BonusTransaction
    {
        public string TransactionId { get; set; }
        public string CardNo { get; set; }
        public decimal Amount { get; set; }
        public decimal BonusAmount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string IdempotencyKey { get; set; }
        public bool IsProcessed { get; set; }
        public string Status { get; set; }
    }
}