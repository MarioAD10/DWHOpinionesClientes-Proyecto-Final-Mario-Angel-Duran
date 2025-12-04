namespace Core.Application.DTO.DimDto
{
    public class DateSourceDto
    {
        public DateTime FullDate { get; set; }
        public byte Day { get; set; }
        public byte Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public byte Quarter { get; set; }
        public int Year { get; set; }
    }
}
