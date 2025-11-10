using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UniversityBonusSystem.Models;

namespace UniversityBonusSystem.Services
{
    public class ReportService
    {
        private readonly LoggerService _logger;

        public ReportService(LoggerService logger)
        {
            _logger = logger;
        }

        // Генерация всех отчетов
        public void GenerateReports(List<BonusTransaction> transactions, List<Department> departments)
        {
            try
            {
                Directory.CreateDirectory("reports");

                // Создаем различные отчеты
                GenerateTxtReport(transactions, departments);
                GenerateCsvReport(transactions);
                GenerateXmlReport(transactions);

                _logger.LogInfo("Все отчеты успешно сгенерированы");
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка генерации отчетов", ex);
                throw;
            }
        }

        private void GenerateTxtReport(List<BonusTransaction> transactions, List<Department> departments)
        {
            var reportPath = "reports/summary_report.txt";

            using (var writer = new StreamWriter(reportPath))
            {
                writer.WriteLine("ОТЧЕТ ПО НАЧИСЛЕНИЮ БОНУСОВ");
                writer.WriteLine("============================");
                writer.WriteLine($"Дата генерации: {DateTime.Now:dd.MM.yyyy HH:mm}");
                writer.WriteLine();

                // Статистика по статусам
                var statusStats = transactions
                    .GroupBy(t => t.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() });

                writer.WriteLine("СТАТИСТИКА ПО СТАТУСАМ:");
                foreach (var stat in statusStats)
                {
                    writer.WriteLine($"  {stat.Status}: {stat.Count} операций");
                }
                writer.WriteLine();

                // Успешные операции по дням
                var dailyStats = transactions
                    .Where(t => t.IsProcessed && t.Status == "Успешно")
                    .GroupBy(t => t.TransactionDate.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Count = g.Count(),
                        TotalAmount = g.Sum(t => t.Amount),
                        TotalBonus = g.Sum(t => t.BonusAmount)
                    })
                    .OrderBy(x => x.Date);

                writer.WriteLine("СТАТИСТИКА ПО ДНЯМ (успешные операции):");
                foreach (var day in dailyStats)
                {
                    writer.WriteLine($"  {day.Date:dd.MM.yyyy}: {day.Count} операций, " +
                                   $"Сумма: {day.TotalAmount:F2} руб, " +
                                   $"Бонусы: {day.TotalBonus:F2}");
                }
                writer.WriteLine();

                // Топ карт по бонусам
                var topCards = transactions
                    .Where(t => t.IsProcessed && t.Status == "Успешно")
                    .GroupBy(t => t.CardNo)
                    .Select(g => new
                    {
                        CardNo = g.Key,
                        TotalBonus = g.Sum(t => t.BonusAmount),
                        Transactions = g.Count()
                    })
                    .OrderByDescending(x => x.TotalBonus)
                    .Take(10);

                writer.WriteLine("ТОП-10 КАРТ ПО БОНУСАМ:");
                int rank = 1;
                foreach (var card in topCards)
                {
                    writer.WriteLine($"  {rank}. {card.CardNo}: {card.TotalBonus:F2} бонусов ({card.Transactions} операций)");
                    rank++;
                }
            }
        }

        private void GenerateCsvReport(List<BonusTransaction> transactions)
        {
            var reportPath = "reports/detailed_report.csv";

            using (var writer = new StreamWriter(reportPath))
            {
                writer.WriteLine("Дата;Номер карты;Сумма покупки;Бонусы;Статус");

                foreach (var transaction in transactions.Where(t => t.IsProcessed))
                {
                    writer.WriteLine($"{transaction.TransactionDate:dd.MM.yyyy};" +
                                   $"{transaction.CardNo};" +
                                   $"{transaction.Amount:F2};" +
                                   $"{transaction.BonusAmount:F2};" +
                                   $"{transaction.Status}");
                }
            }
        }

        private void GenerateXmlReport(List<BonusTransaction> transactions)
        {
            var reportPath = "reports/transactions_report.xml";

            var xmlReport = new XElement("BonusReport",
                new XElement("GeneratedDate", DateTime.Now),
                new XElement("TotalTransactions", transactions.Count),
                new XElement("SuccessfulTransactions", transactions.Count(t => t.IsProcessed && t.Status == "Успешно")),
                new XElement("TotalBonusAmount", transactions.Where(t => t.IsProcessed).Sum(t => t.BonusAmount)),
                
                new XElement("Transactions",
                    transactions.Select(t => new XElement("Transaction",
                        new XElement("CardNo", t.CardNo),
                        new XElement("Amount", t.Amount),
                        new XElement("BonusAmount", t.BonusAmount),
                        new XElement("Date", t.TransactionDate),
                        new XElement("Status", t.Status)
                    ))
                )
            );

            xmlReport.Save(reportPath);
        }

        // LINQ to XML: Чтение с валидацией
        public List<PurchaseData> ReadPurchasesWithLinqXml(string filePath)
        {
            try
            {
                var doc = XDocument.Load(filePath);
                
                // Валидация структуры XML
                if (doc.Root?.Name != "Purchases")
                    throw new InvalidOperationException("Неверная структура XML. Ожидается корневой элемент 'Purchases'");

                var purchases = doc.Root.Elements("PurchaseData")
                    .Select(p => new PurchaseData
                    {
                        CardNo = (string)p.Element("CardNo"),
                        Amount = (decimal)p.Element("Amount"),
                        Date = (DateTime)p.Element("Date")
                    })
                    .ToList();

                _logger.LogInfo($"LINQ to XML: прочитано {purchases.Count} покупок");
                return purchases;
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка чтения XML через LINQ", ex);
                throw;
            }
        }

        // LINQ to XML: Модификация XML
        public void UpdatePurchaseFile(string filePath, List<PurchaseData> newPurchases)
        {
            try
            {
                var doc = XDocument.Load(filePath);
                var root = doc.Root;

                foreach (var purchase in newPurchases)
                {
                    root.Add(new XElement("PurchaseData",
                        new XElement("CardNo", purchase.CardNo),
                        new XElement("Amount", purchase.Amount),
                        new XElement("Date", purchase.Date)
                    ));
                }

                doc.Save(filePath);
                _logger.LogInfo($"XML файл обновлен: добавлено {newPurchases.Count} записей");
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка модификации XML", ex);
                throw;
            }
        }
    }
}