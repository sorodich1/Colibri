using System;

namespace Colibri.WebApi.Models;

public class LogEntry
{
        public int Id { get; set; }
        public string Level { get; set; }
        public string User { get; set; }
        public string Message { get; set; }
        public string Logger { get; set; }
        public DateTime Timestamp { get; set; }
        
        // Свойство для отображения сокращенного сообщения
        public string ShortMessage 
        { 
            get
            {
                if (string.IsNullOrEmpty(Message))
                    return "";
                    
                return Message.Length > 100 ? Message.Substring(0, 100) + "..." : Message;
            }
        }
        
        // Цвет для уровня логирования
        public string LevelColor
        {
            get
            {
                return Level?.ToLower() switch
                {
                    "error" => "danger",
                    "warning" => "warning",
                    "information" => "info",
                    "debug" => "secondary",
                    _ => "light"
                };
            }
        }
        
        // Форматированная дата - исправленная версия
        public string FormattedTimestamp => Timestamp.ToString("dd.MM.yyyy HH:mm:ss");
        
        // Форматированная дата только для даты
        public string FormattedDate => Timestamp.ToString("dd.MM.yyyy");
        
        // Форматированная дата для input type="date"
        public string DateForInput => Timestamp.ToString("yyyy-MM-dd");
}
