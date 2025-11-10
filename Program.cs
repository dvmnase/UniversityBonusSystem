using UniversityBonusSystem.Extensions;
using UniversityBonusSystem.Models;
using UniversityBonusSystem.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq; 


namespace UniversityBonusSystem
{
    class Program
    {
        private static LoggerService _logger;
        private static ReportService _reportService;
        private static IdempotencyService _idempotencyService;
        private static FileService _fileService;
        private static BonusService _bonusService;
        private static Department _department;

        static void Main(string[] args)
        {
            InitializeServices();
            ShowMainMenu();
        }

        static void InitializeServices()
        {
            _logger = new LoggerService();
            _idempotencyService = new IdempotencyService();
            _fileService = new FileService();
            _bonusService = new BonusService(_idempotencyService, _logger);
            _reportService = new ReportService(_logger);
            _department = CreateTestDepartment();

             _bonusService.LoadTransactionsFromFile();

            // Подписка на событие
            _bonusService.BonusAwarded += (sender, e) =>
            {
                Console.WriteLine($"  СОБЫТИЕ: Карта {e.CardNo} - {e.Status}, Бонусы: {e.BonusAmount}");
            };
        }

        static void ShowMainMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("====================================");
                Console.WriteLine("  СИСТЕМА МАССОВОГО НАЧИСЛЕНИЯ БОНУСОВ");
                Console.WriteLine("====================================");
                Console.WriteLine("1 - Массовое начисление бонусов");
                Console.WriteLine("2 - Просмотр протокола операций");
                Console.WriteLine("3 - Очистка истории операций");
                Console.WriteLine("4 - Создать пример XML файла");
                Console.WriteLine("5 - Показать информацию о кафедре");
                Console.WriteLine("6 - Демонстрация функционала");
                Console.WriteLine("7 - Генерация отчетов (LINQ)");          
                Console.WriteLine("8 - Чтение XML через LINQ to XML");     
                Console.WriteLine("9 - Модификация XML файла");    
                Console.WriteLine("0 - Выход");
                Console.WriteLine("====================================");
                Console.Write("Выберите действие: ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ProcessMassBonusAward();
                        break;
                    case "2":
                        ShowOperationLog();
                        break;
                    case "3":
                        ClearOperationHistory();
                        break;
                    case "4":
                        CreateSampleXmlFile();
                        break;
                    case "5":
                        ShowDepartmentInfo();
                        break;
                    case "6":
                        DemonstrateFunctionality();
                        break;
                    case "7":
                        GenerateLinqReports();
                        break;
                     case "8":
                        ReadWithLinqToXml();
                        break;
                     case "9":
                        ModifyXmlFile();
                        break;
                    case "0":
                        Console.WriteLine("Выход из программы...");
                        return;
                    default:
                        Console.WriteLine("Неверный выбор! Нажмите любую клавишу...");
                        Console.ReadKey();
                        break;
                }
            }
        }

static void ModifyXmlFile()
{
    Console.Clear();
    Console.WriteLine("МОДИФИКАЦИЯ XML ФАЙЛА");
    Console.WriteLine("=====================");

    string xmlFilePath = "purchases.xml";

    try
    {
        if (!File.Exists(xmlFilePath))
        {
            Console.WriteLine($"Файл {xmlFilePath} не найден!");
            Console.WriteLine("Создайте файл через меню (пункт 4)");
            WaitForUser();
            return;
        }

        Console.WriteLine($"Работа с файлом: {xmlFilePath}");
        Console.WriteLine();

        // Загрузка XML документа
        var doc = XDocument.Load(xmlFilePath);
        var originalCount = doc.Root?.Elements("PurchaseData").Count() ?? 0;

        Console.WriteLine($"Текущее количество записей: {originalCount}");

        // МЕНЮ модификации
        Console.WriteLine("\nВЫБЕРИТЕ ОПЕРАЦИЮ:");
        Console.WriteLine("1 - Добавить новые записи");
        Console.WriteLine("2 - Удалить записи по критерию");
        Console.WriteLine("3 - Обновить существующие записи");
        Console.WriteLine("4 - Добавить статистику в XML");
        Console.Write("Ваш выбор: ");

        var choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                AddNewRecords(doc, xmlFilePath);
                break;
            case "2":
                DeleteRecords(doc, xmlFilePath);
                break;
            case "3":
                UpdateRecords(doc, xmlFilePath);
                break;
            case "4":
                AddStatisticsToXml(doc, xmlFilePath);
                break;
            default:
                Console.WriteLine("Неверный выбор!");
                break;
        }

    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Ошибка при модификации XML: {ex.Message}");
        _logger.LogError("Ошибка модификации XML", ex);
    }

    WaitForUser();
}

// Метод для добавления новых записей
static void AddNewRecords(XDocument doc, string filePath)
{
    Console.WriteLine("\n--- ДОБАВЛЕНИЕ НОВЫХ ЗАПИСЕЙ ---");

    Console.Write("Сколько записей добавить? ");
    if (!int.TryParse(Console.ReadLine(), out int count) || count <= 0)
    {
        Console.WriteLine("Неверное количество!");
        return;
    }

    var newPurchases = new List<PurchaseData>();
    var random = new Random();

    for (int i = 0; i < count; i++)
    {
        newPurchases.Add(new PurchaseData
        {
            CardNo = $"AUTO{random.Next(100000, 999999)}",
            Amount = random.Next(100, 5000),
            Date = DateTime.Now.AddDays(-random.Next(0, 30))
        });
    }

    // LINQ to XML: Добавление элементов
    foreach (var purchase in newPurchases)
    {
        doc.Root.Add(new XElement("PurchaseData",
            new XElement("CardNo", purchase.CardNo),
            new XElement("Amount", purchase.Amount),
            new XElement("Date", purchase.Date)
        ));
    }

    doc.Save(filePath);
    Console.WriteLine($"✅ Добавлено {count} новых записей!");
    Console.WriteLine("Пример добавленных записей:");
    foreach (var purchase in newPurchases.Take(3))
    {
        Console.WriteLine($"   Карта: {purchase.CardNo}, Сумма: {purchase.Amount}, Дата: {purchase.Date:dd.MM.yyyy}");
    }
}

// Метод для удаления записей
static void DeleteRecords(XDocument doc, string filePath)
{
    Console.WriteLine("\n--- УДАЛЕНИЕ ЗАПИСЕЙ ---");
    Console.WriteLine("1 - Удалить по номеру карты");
    Console.WriteLine("2 - Удалить по минимальной сумме");
    Console.WriteLine("3 - Удалить невалидные записи");
    Console.Write("Ваш выбор: ");

    var choice = Console.ReadLine();
    var elementsToRemove = new List<XElement>();

    switch (choice)
    {
        case "1":
            Console.Write("Введите номер карты для удаления: ");
            var cardToDelete = Console.ReadLine();
            elementsToRemove = doc.Root.Elements("PurchaseData")
                .Where(p => (string)p.Element("CardNo") == cardToDelete)
                .ToList();
            break;

        case "2":
            Console.Write("Введите минимальную сумму: ");
            if (decimal.TryParse(Console.ReadLine(), out decimal minAmount))
            {
                elementsToRemove = doc.Root.Elements("PurchaseData")
                    .Where(p => (decimal?)p.Element("Amount") < minAmount)
                    .ToList();
            }
            break;

        case "3":
            elementsToRemove = doc.Root.Elements("PurchaseData")
                .Where(p => string.IsNullOrEmpty((string)p.Element("CardNo")) || 
                           ((decimal?)p.Element("Amount") ?? 0) <= 0)
                .ToList();
            break;

        default:
            Console.WriteLine("Неверный выбор!");
            return;
    }

    // LINQ to XML: Удаление элементов
    elementsToRemove.ForEach(e => e.Remove());
    doc.Save(filePath);

    Console.WriteLine($"✅ Удалено записей: {elementsToRemove.Count}");
    if (elementsToRemove.Any())
    {
        Console.WriteLine("Удаленные записи:");
        foreach (var element in elementsToRemove.Take(5))
        {
            Console.WriteLine($"   Карта: {element.Element("CardNo")?.Value}, " +
                            $"Сумма: {element.Element("Amount")?.Value}");
        }
    }
}

// Метод для обновления записей
static void UpdateRecords(XDocument doc, string filePath)
{
    Console.WriteLine("\n--- ОБНОВЛЕНИЕ ЗАПИСЕЙ ---");
    
    // Находим записи с маленькими суммами и увеличиваем их
    var smallAmounts = doc.Root.Elements("PurchaseData")
        .Where(p => (decimal?)p.Element("Amount") < 100)
        .ToList();

    if (!smallAmounts.Any())
    {
        Console.WriteLine("Записей с суммами менее 100 не найдено");
        return;
    }

    Console.WriteLine($"Найдено записей с суммами < 100: {smallAmounts.Count}");
    Console.Write("Увеличить суммы в 2 раза? (y/n): ");

    if (Console.ReadLine()?.ToLower() == "y")
    {
        foreach (var element in smallAmounts)
        {
            var currentAmount = (decimal)element.Element("Amount");
            element.Element("Amount").Value = (currentAmount * 2).ToString();
        }

        doc.Save(filePath);
        Console.WriteLine($"✅ Обновлено записей: {smallAmounts.Count}");
    }
}

// Метод для добавления статистики в XML
static void AddStatisticsToXml(XDocument doc, string filePath)
{
    Console.WriteLine("\n--- ДОБАВЛЕНИЕ СТАТИСТИКИ В XML ---");

    // Вычисляем статистику через LINQ
    var purchases = doc.Root.Elements("PurchaseData");
    var totalAmount = purchases.Sum(p => (decimal?)p.Element("Amount") ?? 0);
    var avgAmount = purchases.Average(p => (decimal?)p.Element("Amount") ?? 0);
    var count = purchases.Count();

    // Добавляем элемент статистики
    var statsElement = new XElement("Statistics",
        new XElement("TotalRecords", count),
        new XElement("TotalAmount", totalAmount),
        new XElement("AverageAmount", avgAmount),
        new XElement("GeneratedDate", DateTime.Now),
        new XElement("RecordCountByCard",
            purchases.GroupBy(p => (string)p.Element("CardNo"))
                .Select(g => new XElement("Card",
                    new XElement("CardNo", g.Key),
                    new XElement("Count", g.Count()),
                    new XElement("Total", g.Sum(p => (decimal?)p.Element("Amount") ?? 0))
                ))
        )
    );

    // Удаляем старую статистику если есть
    doc.Root.Elements("Statistics").Remove();
    
    // Добавляем новую статистику
    doc.Root.Add(statsElement);
    doc.Save(filePath);

    Console.WriteLine("✅ Статистика добавлена в XML файл!");
    Console.WriteLine($"   Всего записей: {count}");
    Console.WriteLine($"   Общая сумма: {totalAmount:F2}");
    Console.WriteLine($"   Средний чек: {avgAmount:F2}");
}
static void GenerateLinqReports()
{
    Console.Clear();
    Console.WriteLine("ГЕНЕРАЦИЯ ОТЧЕТОВ (LINQ)");
    Console.WriteLine("=========================");

    try
    {
        // Загружаем транзакции из сервиса
        _bonusService.LoadTransactionsFromFile();
        var transactions = _bonusService.AllTransactions;

        // Исправляем проверку количества - вызываем Count() как метод
        if (transactions == null || transactions.Count == 0)
        {
            Console.WriteLine("❌ Нет данных для генерации отчетов.");
            Console.WriteLine("   Сначала выполните успешное начисление бонусов через пункт меню 1");
            WaitForUser();
            return;
        }

        Console.WriteLine($"📊 Найдено {transactions.Count} транзакций для анализа...");

        // Создаем тестовые кафедры для демонстрации
        var departments = new List<Department>
        {
            CreateTestDepartment(),
            new Department { DepartmentId = "MATH", Name = "Математика" },
            new Department { DepartmentId = "PHYS", Name = "Физика" }
        };

        // LINQ to Objects: СЛОЖНЫЕ ЗАПРОСЫ

        // 1. ГРУППИРОВКА по статусам операций
        var statusGroups = transactions
            .GroupBy(t => t.Status)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count(),
                TotalAmount = g.Sum(t => t.Amount),
                AvgAmount = g.Average(t => t.Amount)
            })
            .OrderByDescending(x => x.Count);

        Console.WriteLine("\n1. 📈 СТАТИСТИКА ПО СТАТУСАМ:");
        foreach (var group in statusGroups)
        {
            Console.WriteLine($"   📌 {group.Status}: {group.Count} операций, " +
                            $"Сумма: {group.TotalAmount:F2} руб, " +
                            $"Среднее: {group.AvgAmount:F2} руб");
        }

        // 2. АГРЕГАТЫ по дням (только успешные операции)
        var successfulTransactions = transactions.Where(t => t.IsProcessed && t.Status == "Успешно").ToList();
        
        if (successfulTransactions.Count > 0)
        {
            var dailyStats = successfulTransactions
                .GroupBy(t => t.TransactionDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalAmount = g.Sum(t => t.Amount),
                    TotalBonus = g.Sum(t => t.BonusAmount),
                    Count = g.Count(),
                    AvgTransaction = g.Average(t => t.Amount),
                    MaxTransaction = g.Max(t => t.Amount),
                    MinTransaction = g.Min(t => t.Amount)
                })
                .OrderBy(x => x.Date);

            Console.WriteLine("\n2. 📅 СТАТИСТИКА ПО ДНЯМ (успешные операции):");
            foreach (var day in dailyStats)
            {
                Console.WriteLine($"   🗓️  {day.Date:dd.MM.yyyy}: {day.Count} операций");
                Console.WriteLine($"      💰 Сумма: {day.TotalAmount:F2} руб, Бонусы: {day.TotalBonus:F2}");
                Console.WriteLine($"      📊 Среднее: {day.AvgTransaction:F2} руб, Диапазон: {day.MinTransaction:F2}-{day.MaxTransaction:F2} руб");
            }

            // 3. ПРОЕКЦИЯ + СЛОЖНЫЕ ВЫЧИСЛЕНИЯ (топ карт)
            var topCards = successfulTransactions
                .GroupBy(t => t.CardNo)
                .Select(g => new
                {
                    CardNo = g.Key,
                    TotalAmount = g.Sum(t => t.Amount),
                    TotalBonus = g.Sum(t => t.BonusAmount),
                    Transactions = g.Count(),
                    AvgBonusPerTransaction = g.Average(t => t.BonusAmount)
                })
                .OrderByDescending(x => x.TotalBonus)
                .Take(5);

            Console.WriteLine("\n3. 🏆 ТОП-5 КАРТ ПО БОНУСАМ:");
            int rank = 1;
            foreach (var card in topCards)
            {
                Console.WriteLine($"   {rank}. 🎫 {card.CardNo}:");
                Console.WriteLine($"      💎 Бонусы: {card.TotalBonus:F2} ({card.Transactions} операций)");
                Console.WriteLine($"      💰 Сумма покупок: {card.TotalAmount:F2} руб");
                Console.WriteLine($"      📈 Средний бонус: {card.AvgBonusPerTransaction:F2} за операцию");
                rank++;
            }

            // Генерация файлов отчетов
            _reportService.GenerateReports(transactions, departments);

            Console.WriteLine("\n✅ Отчеты успешно сгенерированы!");
            Console.WriteLine("   📄 summary_report.txt - текстовый отчет");
            Console.WriteLine("   📊 detailed_report.csv - CSV данные");
            Console.WriteLine("   📋 transactions_report.xml - XML отчет");
        }
        else
        {
            Console.WriteLine("\n❌ Нет успешных операций для детального анализа.");
            Console.WriteLine("   Выполните начисление бонусов с валидными данными.");
        }

    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Ошибка при генерации отчетов: {ex.Message}");
        _logger.LogError("Ошибка генерации отчетов", ex);
    }

    WaitForUser();
}
        // Вспомогательный метод для чтения транзакций из лога
        static List<BonusTransaction> ReadTransactionsFromLog()
        {
            var transactions = new List<BonusTransaction>();

            try
            {
                if (File.Exists("batch_log.txt"))
                {
                    var logLines = File.ReadAllLines("batch_log.txt");
                    var successLines = logLines.Where(line => line.Contains("Успешно") && line.Contains("бонусов для карты"));

                    foreach (var line in successLines)
                    {
                        // Парсим строку лога для демонстрации
                        // В реальной системе лучше хранить историю в структурированном виде
                        var parts = line.Split(' ');
                        if (parts.Length > 10)
                        {
                            transactions.Add(new BonusTransaction
                            {
                                CardNo = parts[10].Replace("карты", "").Trim(),
                                BonusAmount = decimal.Parse(parts[3]),
                                Amount = decimal.Parse(parts[3]) * 100, // Примерная сумма
                                TransactionDate = DateTime.Parse(parts[0] + " " + parts[1]),
                                Status = "Успешно",
                                IsProcessed = true
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка чтения истории: {ex.Message}");
            }

            return transactions;
        }
static void ReadWithLinqToXml()
{
    Console.Clear();
    Console.WriteLine("ЧТЕНИЕ XML ЧЕРЕЗ LINQ TO XML");
    Console.WriteLine("=============================");

    string xmlFilePath = "purchases.xml";

    try
    {
        if (!File.Exists(xmlFilePath))
        {
            Console.WriteLine($"Файл {xmlFilePath} не найден!");
            Console.WriteLine("Создайте файл через меню (пункт 4)");
            WaitForUser();
            return;
        }

        Console.WriteLine($"Чтение файла: {xmlFilePath}");
        Console.WriteLine();

        // LINQ TO XML: Загрузка и анализ документа
        var doc = XDocument.Load(xmlFilePath);

        // 1. ВАЛИДАЦИЯ структуры XML
        Console.WriteLine("1. ВАЛИДАЦИЯ СТРУКТУры XML:");
        if (doc.Root == null)
        {
            Console.WriteLine("   ❌ Ошибка: Корневой элемент не найден");
            return;
        }

        Console.WriteLine($"   ✅ Корневой элемент: {doc.Root.Name}");
        Console.WriteLine($"   ✅ Атрибуты корня: {doc.Root.Attributes().Count()}");

        // Проверка обязательных элементов
        var hasPurchases = doc.Root.Elements("PurchaseData").Any();
        Console.WriteLine($"   ✅ Наличие PurchaseData: {hasPurchases}");

        // 2. ЧТЕНИЕ данных с LINQ
        Console.WriteLine("\n2. ЧТЕНИЕ ДАННЫХ:");

        var purchases = doc.Root.Elements("PurchaseData")
            .Select((p, index) => new
            {
                Index = index + 1,
                CardNo = (string)p.Element("CardNo"),
                Amount = (decimal?)p.Element("Amount") ?? 0,
                Date = (DateTime?)p.Element("Date") ?? DateTime.MinValue,
                IsValid = !string.IsNullOrEmpty((string)p.Element("CardNo")) && 
                         ((decimal?)p.Element("Amount") ?? 0) > 0
            })
            .ToList();

        // 3. СТАТИСТИКА данных
        Console.WriteLine($"   Всего записей: {purchases.Count}");
        Console.WriteLine($"   Валидных записей: {purchases.Count(p => p.IsValid)}");
        Console.WriteLine($"   Невалидных записей: {purchases.Count(p => !p.IsValid)}");

        // 4. ВЫВОД данных с фильтрацией
        Console.WriteLine("\n3. ДЕТАЛЬНЫЙ ПРОСМОТР ДАННЫХ:");

        var validPurchases = purchases.Where(p => p.IsValid);
        var invalidPurchases = purchases.Where(p => !p.IsValid);

        if (validPurchases.Any())
        {
            Console.WriteLine("\n   ✅ ВАЛИДНЫЕ ЗАПИСИ:");
            foreach (var purchase in validPurchases.Take(5)) // Показываем первые 5
            {
                Console.WriteLine($"      {purchase.Index}. Карта: {purchase.CardNo}, " +
                                $"Сумма: {purchase.Amount:F2}, Дата: {purchase.Date:dd.MM.yyyy}");
            }
            if (validPurchases.Count() > 5)
                Console.WriteLine($"      ... и еще {validPurchases.Count() - 5} записей");
        }

        if (invalidPurchases.Any())
        {
            Console.WriteLine("\n   ❌ НЕВАЛИДНЫЕ ЗАПИСИ:");
            foreach (var purchase in invalidPurchases)
            {
                var issues = new List<string>();
                if (string.IsNullOrEmpty(purchase.CardNo)) issues.Add("отсутствует карта");
                if (purchase.Amount <= 0) issues.Add("неверная сумма");
                if (purchase.Date == DateTime.MinValue) issues.Add("отсутствует дата");

                Console.WriteLine($"      {purchase.Index}. Проблемы: {string.Join(", ", issues)}");
            }
        }

        // 5. АНАЛИЗ через LINQ
        Console.WriteLine("\n4. АНАЛИТИКА ДАННЫХ:");

        if (validPurchases.Any())
        {
            var totalAmount = validPurchases.Sum(p => p.Amount);
            var avgAmount = validPurchases.Average(p => p.Amount);
            var minAmount = validPurchases.Min(p => p.Amount);
            var maxAmount = validPurchases.Max(p => p.Amount);

            var dateRange = validPurchases
                .Where(p => p.Date != DateTime.MinValue)
                .Select(p => p.Date);

            var minDate = dateRange.Any() ? dateRange.Min() : DateTime.MinValue;
            var maxDate = dateRange.Any() ? dateRange.Max() : DateTime.MinValue;

            Console.WriteLine($"   Общая сумма: {totalAmount:F2} руб");
            Console.WriteLine($"   Средний чек: {avgAmount:F2} руб");
            Console.WriteLine($"   Минимальный чек: {minAmount:F2} руб");
            Console.WriteLine($"   Максимальный чек: {maxAmount:F2} руб");
            
            if (minDate != DateTime.MinValue)
                Console.WriteLine($"   Период данных: {minDate:dd.MM.yyyy} - {maxDate:dd.MM.yyyy}");

            // Группировка по дням
            var dailyGroups = validPurchases
                .Where(p => p.Date != DateTime.MinValue)
                .GroupBy(p => p.Date.Date)
                .Select(g => new { Date = g.Key, Count = g.Count(), Total = g.Sum(p => p.Amount) })
                .OrderBy(g => g.Date);

            Console.WriteLine("\n   СТАТИСТИКА ПО ДНЯМ:");
            foreach (var day in dailyGroups)
            {
                Console.WriteLine($"      {day.Date:dd.MM.yyyy}: {day.Count} операций, {day.Total:F2} руб");
            }
        }

        // 6. ПРЕОБРАЗОВАНИЕ в доменные объекты
        Console.WriteLine("\n5. ПРЕОБРАЗОВАНИЕ В ОБЪЕКТЫ:");
        
        var domainPurchases = doc.Root.Elements("PurchaseData")
            .Where(p => !string.IsNullOrEmpty((string)p.Element("CardNo")) && 
                       ((decimal?)p.Element("Amount") ?? 0) > 0)
            .Select(p => new PurchaseData
            {
                CardNo = (string)p.Element("CardNo"),
                Amount = (decimal)p.Element("Amount"),
                Date = (DateTime)p.Element("Date")
            })
            .ToList();

        Console.WriteLine($"   Создано объектов PurchaseData: {domainPurchases.Count}");

    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Ошибка при чтении XML: {ex.Message}");
        _logger.LogError("Ошибка чтения XML через LINQ", ex);
    }

    WaitForUser();
}
        static void ProcessMassBonusAward()
        {
            Console.Clear();
            Console.WriteLine("МАССОВОЕ НАЧИСЛЕНИЕ БОНУСОВ");
            Console.WriteLine("============================");

            string xmlFilePath = "purchases.xml";

            try
            {
                // Проверка существования файла
                if (!File.Exists(xmlFilePath))
                {
                    Console.WriteLine($"Файл {xmlFilePath} не найден!");
                    Console.WriteLine("Создайте файл через меню (пункт 4) или поместите XML файл в папку с программой.");
                    WaitForUser();
                    return;
                }

                _logger.LogInfo($"Чтение данных из {xmlFilePath}");
                Console.WriteLine($"Чтение данных из {xmlFilePath}...");

                // Чтение покупок из XML
                var purchases = _fileService.ReadPurchasesFromXml(xmlFilePath);
                _logger.LogInfo($"Прочитано {purchases.Count} покупок");
                Console.WriteLine($"Прочитано {purchases.Count} покупок");

                if (purchases.Count == 0)
                {
                    Console.WriteLine("Файл не содержит данных для обработки!");
                    WaitForUser();
                    return;
                }

                // Показываем данные для обработки
                Console.WriteLine("\nДанные для обработки:");
                Console.WriteLine("---------------------");
                foreach (var purchase in purchases)
                {
                    Console.WriteLine($"Карта: {purchase.CardNo}, Сумма: {purchase.Amount}, Дата: {purchase.Date:dd.MM.yyyy}");
                }

                Console.Write("\nНачать обработку? (y/n): ");
                var confirm = Console.ReadLine();

                if (confirm?.ToLower() != "y")
                {
                    Console.WriteLine("Обработка отменена пользователем.");
                    WaitForUser();
                    return;
                }

                // Массовое начисление бонусов
                _logger.LogInfo("Начало массового начисления бонусов");
                Console.WriteLine("\nНачало обработки...");

                var results = _bonusService.ProcessMassBonusAward(purchases, _department);

                // Статистика
                var successful = results.Count(r => r.IsProcessed && r.Status == "Успешно");
                var skipped = results.Count(r => r.IsProcessed && r.Status.Contains("Пропущено"));
                var errors = results.Count(r => !r.IsProcessed);

                _logger.LogInfo($"Обработка завершена. Успешно: {successful}, Пропущено: {skipped}, Ошибок: {errors}");

                // Вывод результатов
                Console.WriteLine("\n=== РЕЗУЛЬТАТЫ ОБРАБОТКИ ===");
                foreach (var result in results)
                {
                    var statusIcon = result.Status == "Успешно" ? "✓" : result.Status.Contains("Пропущено") ? "↷" : "✗";
                    var color = result.Status == "Успешно" ? ConsoleColor.Green :
                               result.Status.Contains("Пропущено") ? ConsoleColor.Yellow : ConsoleColor.Red;

                    Console.ForegroundColor = color;
                    Console.WriteLine($"{statusIcon} Карта: {result.CardNo.Truncate(10)}, Сумма: {result.Amount}, Бонусы: {result.BonusAmount}, Статус: {result.Status}");
                    Console.ResetColor();
                }

                Console.WriteLine($"\nИтоги: Успешно: {successful}, Пропущено: {skipped}, Ошибок: {errors}");
                Console.WriteLine($"\nПротокол операций сохранен в: batch_log.txt");
                Console.WriteLine($"Ключи идемпотентности сохранены в: processed_transactions.txt");

                // Демонстрация идемпотентности
                Console.WriteLine("\n=== ПРОВЕРКА ИДЕМПОТЕНТНОСТИ ===");
                Console.WriteLine("Повторный запуск с тем же файлом не создаст дубликатов!");
                Console.WriteLine("Попробуйте запустить обработку еще раз для демонстрации.");

            }
            catch (Exception ex)
            {
                _logger.LogError("Критическая ошибка при обработке", ex);
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            WaitForUser();
        }

        static void ShowOperationLog()
        {
            Console.Clear();
            Console.WriteLine("ПРОТОКОЛ ОПЕРАЦИЙ");
            Console.WriteLine("==================");

            string logFilePath = "batch_log.txt";

            try
            {
                if (!File.Exists(logFilePath))
                {
                    Console.WriteLine("Файл протокола не найден. Сначала выполните операции.");
                    WaitForUser();
                    return;
                }

                var logLines = File.ReadAllLines(logFilePath);
                
                if (logLines.Length == 0)
                {
                    Console.WriteLine("Протокол пуст.");
                }
                else
                {
                    foreach (var line in logLines)
                    {
                        if (line.Contains("[ERROR]"))
                            Console.ForegroundColor = ConsoleColor.Red;
                        else if (line.Contains("[SUCCESS]"))
                            Console.ForegroundColor = ConsoleColor.Green;
                        else if (line.Contains("[INFO]"))
                            Console.ForegroundColor = ConsoleColor.Cyan;
                        
                        Console.WriteLine(line);
                        Console.ResetColor();
                    }
                    Console.WriteLine($"\nВсего записей: {logLines.Length}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка чтения протокола: {ex.Message}");
            }

            WaitForUser();
        }

        static void ClearOperationHistory()
        {
            Console.Clear();
            Console.WriteLine("ОЧИСТКА ИСТОРИИ ОПЕРАЦИЙ");
            Console.WriteLine("========================");

            try
            {
                var filesToClear = new[] { "batch_log.txt", "processed_transactions.txt","transactions_history.json" };
                int clearedCount = 0;

                foreach (var file in filesToClear)
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                        Console.WriteLine($"Удален: {file}");
                        clearedCount++;
                    }
                }
 // Также очищаем транзакции в памяти
        _bonusService.AllTransactions.Clear();
                if (clearedCount > 0)
                {
                    Console.WriteLine($"\nУдалено файлов: {clearedCount}");
                    Console.WriteLine("История операций очищена. Теперь можно тестировать идемпотентность заново.");
                }
                else
                {
                    Console.WriteLine("Файлы для очистки не найдены.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при очистке: {ex.Message}");
            }

            WaitForUser();
        }

        static void CreateSampleXmlFile()
        {
            Console.Clear();
            Console.WriteLine("СОЗДАНИЕ ПРИМЕРА XML ФАЙЛА");
            Console.WriteLine("==========================");

            string xmlFilePath = "purchases.xml";

            try
            {
                if (File.Exists(xmlFilePath))
                {
                    Console.Write($"Файл {xmlFilePath} уже существует. Перезаписать? (y/n): ");
                    var confirm = Console.ReadLine();
                    if (confirm?.ToLower() != "y")
                    {
                        Console.WriteLine("Создание отменено.");
                        WaitForUser();
                        return;
                    }
                }

                var samplePurchases = new List<PurchaseData>
                {
                    new PurchaseData { CardNo = "CARD123456", Amount = 1000.00m, Date = DateTime.Now.AddDays(-1) },
                    new PurchaseData { CardNo = "CARD789012", Amount = 2500.50m, Date = DateTime.Now.AddDays(-2) },
                    new PurchaseData { CardNo = "CARD345678", Amount = 500.00m, Date = DateTime.Now.AddDays(-3) },
                    new PurchaseData { CardNo = "INVALID", Amount = 100.00m, Date = DateTime.Now }, // Невалидная карта
                    new PurchaseData { CardNo = "CARD123456", Amount = 1000.00m, Date = DateTime.Now.AddDays(-1) } // Дубликат для демонстрации идемпотентности
                };

                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<PurchaseData>), new System.Xml.Serialization.XmlRootAttribute("Purchases"));
                using (var writer = new System.IO.StreamWriter(xmlFilePath))
                {
                    serializer.Serialize(writer, samplePurchases);
                }

                Console.WriteLine($"Создан файл: {xmlFilePath}");
                Console.WriteLine("\nСодержимое файла:");
                Console.WriteLine("------------------");
                foreach (var purchase in samplePurchases)
                {
                    Console.WriteLine($"  Карта: {purchase.CardNo}, Сумма: {purchase.Amount}, Дата: {purchase.Date:dd.MM.yyyy}");
                }
                Console.WriteLine("\nПримечание: файл содержит дубликат для демонстрации идемпотентности.");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка создания файла: {ex.Message}");
            }

            WaitForUser();
        }

        static void ShowDepartmentInfo()
        {
            Console.Clear();
            Console.WriteLine("ИНФОРМАЦИЯ О КАФЕДРЕ");
            Console.WriteLine("=====================");

            Console.WriteLine(_department.GetDepartmentInfo());
            Console.WriteLine("\nКурсы кафедры:");
            foreach (var course in _department.Courses)
            {
                Console.WriteLine($"  - {course.GetCourseInfo()}");
            }

            Console.WriteLine($"\nБонусная политика: {_department.CalculateTotalBonus(1000):F2} бонусов с 1000 руб.");

            WaitForUser();
        }

        static void DemonstrateFunctionality()
        {
            Console.Clear();
            Console.WriteLine("ДЕМОНСТРАЦИЯ ФУНКЦИОНАЛА");
            Console.WriteLine("========================");

            // Демонстрация partial класса Student
            Console.WriteLine("\n1. ДЕМОНСТРАЦИЯ PARTIAL КЛАССА STUDENT:");
            Console.WriteLine("--------------------------------------");
            
            var student = new Student
            {
                StudentId = "S001",
                FullName = "Иван Петров",
                CardNo = "CARD123456",
                TotalBonus = 100.50m
            };
            
            Console.WriteLine(student.GetStudentInfo());
            Console.WriteLine(Student.GetUniversityInfo());
            Console.WriteLine($"Минимальный бонус: {Student.MIN_BONUS_AMOUNT}");
            Console.WriteLine($"Создан: {student.CreatedDate}");

            // Демонстрация методов расширения
            Console.WriteLine("\n2. ДЕМОНСТРАЦИЯ МЕТОДОВ РАСШИРЕНИЯ:");
            Console.WriteLine("----------------------------------");
            
            var cardHash = student.CardNo.ToSha256Hash();
            Console.WriteLine($"Хэш карты: {cardHash.Truncate(20)}...");
            Console.WriteLine($"Валидность карты '{student.CardNo}': {student.CardNo.IsValidCardNumber()}");
            Console.WriteLine($"Валидность карты 'SHORT': {"SHORT".IsValidCardNumber()}");

            // Демонстрация идемпотентности
            Console.WriteLine("\n3. ДЕМОНСТРАЦИЯ ИДЕМПОТЕНТНОСТИ:");
            Console.WriteLine("-------------------------------");
            
            var testPurchase = new PurchaseData { CardNo = "TEST123", Amount = 1000m, Date = DateTime.Now };
            var key1 = _idempotencyService.GenerateIdempotencyKey(testPurchase);
            var key2 = _idempotencyService.GenerateIdempotencyKey(testPurchase);
            
            Console.WriteLine($"Ключ 1: {key1.Truncate(30)}...");
            Console.WriteLine($"Ключ 2: {key2.Truncate(30)}...");
            Console.WriteLine($"Ключи идентичны: {key1 == key2}");

            WaitForUser();
        }

        static void WaitForUser()
        {
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        static Department CreateTestDepartment()
        {
            var department = new Department
            {
                DepartmentId = "CS",
                Name = "Компьютерные науки"
            };
            
            department.AddCourse(new Course { CourseId = "CS101", Title = "Основы программирования", Credits = 4 });
            department.AddCourse(new Course { CourseId = "CS201", Title = "Алгоритмы и структуры данных", Credits = 5 });
            department.AddCourse(new Course { CourseId = "CS301", Title = "Базы данных", Credits = 4 });
            
            return department;
        }
    }
}