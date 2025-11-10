using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Models
{
    public class ExtractionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int RecordsRead { get; set; }
        public int RecordsInserted { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime ExecutionDate { get; set; } = DateTime.UtcNow;
        public string? ErrorDetails { get; set; }
    }
}
