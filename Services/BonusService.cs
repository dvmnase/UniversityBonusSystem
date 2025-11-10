using System;
using System.Collections.Generic;
using UniversityBonusSystem.Extensions;
using UniversityBonusSystem.Models; 

namespace UniversityBonusSystem.Services
{
    // Делегат для события начисления бонусов
    public delegate void BonusAwardedEventHandler(object sender, BonusAwardedEventArgs e);


    public class BonusAwardedEventArgs : EventArgs
    {
        public string CardNo { get; set; }
        public decimal Amount { get; set; }
        public decimal BonusAmount { get; set; }
        public string Status { get; set; }
        public BonusAwardedEventArgs(string cardNo, decimal amount, decimal bonusAmount, string status)
        {
            CardNo = cardNo;
            Amount = amount;
            BonusAmount = bonusAmount;
            Status = status;
        }
    }
    
    public class BonusService
    {
        private readonly IdempotencyService _idempotencyService;
        private readonly LoggerService _logger;

        // Событие
        public event BonusAwardedEventHandler BonusAwarded;
        public List<BonusTransaction> AllTransactions { get; private set; } = new List<BonusTransaction>();
        
        public BonusService(IdempotencyService idempotencyService, LoggerService logger)
        {
            _idempotencyService = idempotencyService;
            _logger = logger;
        }

        public List<BonusTransaction> ProcessMassBonusAward(List<PurchaseData> purchases, Department department)
        {
            var results = new List<BonusTransaction>();

            foreach (var purchase in purchases)
            {
                try
                {
                    // Проверка валидности номера карты с помощью метода расширения
                    if (!purchase.CardNo.IsValidCardNumber())
                    {
                        var errorTransaction = new BonusTransaction
                        {
                            CardNo = purchase.CardNo,
                            Amount = purchase.Amount,
                            BonusAmount = 0,
                            TransactionDate = DateTime.Now,
                            Status = "Ошибка: Неверный номер карты",
                            IsProcessed = false
                        };
                        results.Add(errorTransaction);

                        _logger.LogError($"Неверный номер карты: {purchase.CardNo}");
                        OnBonusAwarded(purchase.CardNo, purchase.Amount, 0, "Ошибка");
                        continue;
                    }

                    // Генерация ключа идемпотентности
                    var idempotencyKey = _idempotencyService.GenerateIdempotencyKey(purchase);

                    // Проверка идемпотентности
                    if (_idempotencyService.IsAlreadyProcessed(idempotencyKey))
                    {
                        var skippedTransaction = new BonusTransaction
                        {
                            CardNo = purchase.CardNo,
                            Amount = purchase.Amount,
                            BonusAmount = 0,
                            TransactionDate = DateTime.Now,
                            IdempotencyKey = idempotencyKey,
                            Status = "Пропущено: Уже обработано",
                            IsProcessed = true
                        };
                        results.Add(skippedTransaction);

                        _logger.LogInfo($"Пропущено (уже обработано): {purchase.CardNo}");
                        continue;
                    }

                    // Расчет бонусов
                    decimal bonusAmount = department.CalculateTotalBonus(purchase.Amount);

                    var transaction = new BonusTransaction
                    {
                        TransactionId = Guid.NewGuid().ToString(),
                        CardNo = purchase.CardNo,
                        Amount = purchase.Amount,
                        BonusAmount = bonusAmount,
                        TransactionDate = DateTime.Now,
                        IdempotencyKey = idempotencyKey,
                        Status = "Успешно",
                        IsProcessed = true
                    };

                    results.Add(transaction);
                    AllTransactions.Add(transaction);

                    // Отметка как обработанного
                    _idempotencyService.MarkAsProcessed(idempotencyKey);

                    _logger.LogSuccess($"Начислено {bonusAmount} бонусов для карты {purchase.CardNo}");

                    // Вызов события
                    OnBonusAwarded(purchase.CardNo, purchase.Amount, bonusAmount, "Успешно");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Ошибка обработки покупки для карты {purchase.CardNo}", ex);

                    var errorTransaction = new BonusTransaction
                    {
                        CardNo = purchase.CardNo,
                        Amount = purchase.Amount,
                        BonusAmount = 0,
                        TransactionDate = DateTime.Now,
                        Status = $"Ошибка: {ex.Message}",
                        IsProcessed = false
                    };
                    results.Add(errorTransaction);

                    OnBonusAwarded(purchase.CardNo, purchase.Amount, 0, "Ошибка");
                }
            }
            SaveTransactionsToFile();
            return results;
        }

        protected virtual void OnBonusAwarded(string cardNo, decimal amount, decimal bonusAmount, string status)
        {
            BonusAwarded?.Invoke(this, new BonusAwardedEventArgs(cardNo, amount, bonusAmount, status));
        }
        
        // Метод для сохранения транзакций в файл
    private void SaveTransactionsToFile()
    {
        try
        {
            var transactionsPath = "transactions_history.json";
            var json = System.Text.Json.JsonSerializer.Serialize(AllTransactions, new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            File.WriteAllText(transactionsPath, json);
            _logger.LogInfo($"Сохранено {AllTransactions.Count} транзакций для отчетов");
        }
        catch (Exception ex)
        {
            _logger.LogError("Ошибка сохранения транзакций", ex);
        }
    }
    
    // Метод для загрузки транзакций из файла
    public void LoadTransactionsFromFile()
    {
        try
        {
            var transactionsPath = "transactions_history.json";
            if (File.Exists(transactionsPath))
            {
                var json = File.ReadAllText(transactionsPath);
                AllTransactions = System.Text.Json.JsonSerializer.Deserialize<List<BonusTransaction>>(json) ?? new List<BonusTransaction>();
                _logger.LogInfo($"Загружено {AllTransactions.Count} транзакций из истории");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Ошибка загрузки транзакций", ex);
        }
    }

    }
}