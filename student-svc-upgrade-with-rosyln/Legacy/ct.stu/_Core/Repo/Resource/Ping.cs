using System;

namespace ct._Core.Repo.Resource
{
    public class Ping : Entity
    {
        public string Service { get; set; }
        public string Message { get; set; }
        public DateTime At { get; }
        public Ping()
        {
            At = DateTime.UtcNow;
        }
        public bool IsValid()
        {
            var result = false;
            result = !string.IsNullOrEmpty(Service) && !string.IsNullOrEmpty(Message);
            return result;
        }

    }
}
