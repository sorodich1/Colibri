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
}
