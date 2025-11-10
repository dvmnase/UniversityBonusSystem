using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniversityBonusSystem.Extensions;
using UniversityBonusSystem.Models;

namespace UniversityBonusSystem.Services
{
    public class IdempotencyService
    {
        private readonly string _storagePath;
        private readonly HashSet<string> _processedKeys;
        
        public IdempotencyService(string storagePath = "processed_transactions.txt")
        {
            _storagePath = storagePath;
            _processedKeys = LoadProcessedKeys();
        }
        
        public string GenerateIdempotencyKey(PurchaseData purchase)
        {
            var keyData = $"{purchase.Date:yyyyMMdd}_{purchase.CardNo}_{purchase.Amount}";
            return keyData.ToSha256Hash();
        }
        
        public bool IsAlreadyProcessed(string idempotencyKey)
        {
            return _processedKeys.Contains(idempotencyKey);
        }
        
        public void MarkAsProcessed(string idempotencyKey)
        {
            if (!_processedKeys.Contains(idempotencyKey))
            {
                _processedKeys.Add(idempotencyKey);
                SaveProcessedKeys();
            }
        }
        
        private HashSet<string> LoadProcessedKeys()
        {
            try
            {
                if (File.Exists(_storagePath))
                {
                    return new HashSet<string>(File.ReadAllLines(_storagePath).Where(line => !string.IsNullOrWhiteSpace(line)));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки обработанных ключей: {ex.Message}");
            }
            
            return new HashSet<string>();
        }
        
        private void SaveProcessedKeys()
        {
            try
            {
                File.WriteAllLines(_storagePath, _processedKeys);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения обработанных ключей: {ex.Message}");
            }
        }
    }
}