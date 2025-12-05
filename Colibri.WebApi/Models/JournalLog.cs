using System;

namespace Colibri.WebApi.Models;

public class JournalLog
{
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
    public string Unit { get; set; }
}
