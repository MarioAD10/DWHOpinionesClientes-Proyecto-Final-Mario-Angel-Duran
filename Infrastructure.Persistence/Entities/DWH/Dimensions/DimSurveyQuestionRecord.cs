namespace Infrastructure.Persistence.Entities.DWH.Dimensions
{
    /// <summary>
    /// Dimensión que almacena las preguntas de encuestas de satisfacción.
    /// </summary>
    public class DimSurveyQuestionRecord
    {
        public int SurveyQuestionKey { get; set; }        
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public int ScaleMin { get; set; }
        public int ScaleMax { get; set; }
    }
}
