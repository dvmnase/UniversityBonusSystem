using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UniversityBonusSystem.Models;

namespace UniversityBonusSystem.Services
{
    public class FileService
    {
        public List<PurchaseData> ReadPurchasesFromXml(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Файл не найден: {filePath}");
                }
                
                var serializer = new XmlSerializer(typeof(List<PurchaseData>), new XmlRootAttribute("Purchases"));
                
                using (var reader = new StreamReader(filePath))
                {
                    return (List<PurchaseData>)serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка чтения XML файла: {ex.Message}", ex);
            }
        }
    }
}