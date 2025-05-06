namespace CalendarApp.Models;

public class CalendarDay
{
    public DateTime Date { get; set; }
    public bool IsCurrentMonth { get; set; }
    public bool IsToday { get; set; }
}

public class App
{
    public string? Date { get; set; }
    public string? Description { get; set; }
    public string? HolidayType { get; set; }
}