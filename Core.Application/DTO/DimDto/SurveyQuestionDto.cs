namespace Core.Application.DTO.DimDto
{
    public class SurveyQuestionDto
    {
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public int ScaleMin { get; set; }
        public int ScaleMax { get; set; }
    }
}
