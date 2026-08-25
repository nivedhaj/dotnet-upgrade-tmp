using ct._Core.Repo.Resource;

namespace Repo.Resource
{
    /// <summary>
    /// Event(s) to be applied to an aggregate / document root in batched manner. Reduces DB round trip.
    /// </summary>
    public class EventLog : Entity
    {
        public string Event { get; set; }
        public object TimeStamp { get; set; }
        public string EventDataType { get; set; }
        public string EventData { get; set; }
    }
}