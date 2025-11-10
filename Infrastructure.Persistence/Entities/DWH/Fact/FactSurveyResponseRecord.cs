namespace Infrastructure.Persistence.Entities.DWH.Fact
{
    public class FactSurveyResponseRecord
    {
        public int SurveyResponseKey { get; set; }      
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
        public DateTime IngestionTimestamp { get; set; }
    }
}
