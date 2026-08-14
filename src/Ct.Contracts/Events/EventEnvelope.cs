using System;
using System.Collections.Generic;

namespace Ct.Contracts.Events
{
    /// <summary>
    /// Typed replacement for the ad-hoc EventLog { Event, EventData, EventDataType } shape
    /// StudentCmd.ApplyEvents currently parses out of a CSV-packed string. Every module that
    /// publishes or consumes cross-module events should use this envelope so the shape is
    /// consistent regardless of which two modules are talking.
    /// </summary>
    public class EventEnvelope<TPayload>
    {
        public string EventType { get; set; }
        public DateTime OccurredOn { get; set; }
        public string SourceModule { get; set; }
        public TPayload Payload { get; set; }
    }

    /// <summary>
    /// Example payload for the existing attendance-ingestion flow, given a proper shape
    /// instead of being packed into a comma-separated string.
    /// </summary>
    public class StudentAttendanceTakenPayload
    {
        public string CourseId { get; set; }
        public string TermName { get; set; }
        public string SectionName { get; set; }
        public DateTime Date { get; set; }
        public int PeriodNo { get; set; }
        public string SubjectName { get; set; }
        public bool IsOptionalSubject { get; set; }
        public string StudentId { get; set; }
        public string StaffId { get; set; }
        public string Attendance { get; set; }
    }

    /// <summary>
    /// Example payload for a Fees -> Student notification (not yet consumed anywhere,
    /// included as the template for the next event this pair of modules needs).
    /// </summary>
    public class FeePaymentReceivedPayload
    {
        public string StudentId { get; set; }
        public string InstId { get; set; }
        public string TxId { get; set; }
        public decimal Amount { get; set; }
    }

    public static class EventTypes
    {
        public const string StudentAttendanceTaken = "Student_Attendance_Taken";
        public const string FeePaymentReceived = "Fee_Payment_Received";
    }

    public class EventBatch
    {
        public List<object> Events { get; set; } = new List<object>();
    }
}
