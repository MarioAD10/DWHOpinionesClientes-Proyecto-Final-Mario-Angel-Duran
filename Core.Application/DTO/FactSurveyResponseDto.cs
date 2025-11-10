using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTO
{
    public class FactSurveyResponseDto
    {
        public int SurveyQuestionKey { get; set; }
        public int CustomerKey { get; set; }
        public int DateKey { get; set; }
        public int ProductKey { get; set; }
        public int SourceKey { get; set; }
        public int ChannelKey { get; set; }
        public int ETLBatchKey { get; set; }

        public decimal ResponseValue { get; set; }
        public int ResponseTimeSec { get; set; }
        public bool IsValidResponse { get; set; }
        public string? ResponseText { get; set; } = string.Empty;
    }
}
