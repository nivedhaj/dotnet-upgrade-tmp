namespace Ct.Scheduler.Domain;

public enum CalendarEventCategory { Event = 10, Holiday = 20, WeeklyHoliday = 30, Exam = 40 }
public enum CalendarRuleStatus { Active = 1, Disabled = 99 }
public enum EventOccurrenceStatus { Active = 1, Deleted = 99 }

public class CalendarRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OwnerId { get; set; } = "";
    public string Title { get; set; } = "";
    public string? CronExpression { get; set; }
    public CalendarRuleStatus Status { get; set; } = CalendarRuleStatus.Active;
    public CalendarEventCategory Category { get; set; } = CalendarEventCategory.Event;
    public int Weight { get; set; }
    public decimal Duration { get; set; }
    public string? TopicId { get; set; }
    public string? UserId { get; set; }
    public bool CreateMeetingLink { get; set; }
    public List<DayOfWeek>? WeeklyDays { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}

public class CalendarEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OwnerId { get; set; } = "";
    public string? InstId { get; set; }
    public string EventName { get; set; } = "";
    public CalendarEventCategory Category { get; set; } = CalendarEventCategory.Event;
    public int Priority { get; set; }
    public List<string> Audience { get; set; } = [];
    public List<string> ConductedBy { get; set; } = [];
    public List<EventOccurrence> Occurrences { get; set; } = [];
    public Guid? RuleId { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
}

public class EventOccurrence
{
    public DateTime EventDate { get; set; }
    public EventOccurrenceStatus Status { get; set; } = EventOccurrenceStatus.Active;
    public int Duration { get; set; }
}
