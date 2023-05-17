using System;

namespace Notify.Core
{
    public enum NotificationType
    {
        Time,
        Location,
        WiFi
    }
    
    public class Notification
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreationDateTime { get; set; }
        public string Status { get; set; }
        public string Creator { get; set; }
        public NotificationType Type { get; set; }
        public object TypeInfo { get; set; }
        public string Target { get; set; }

        public Notification(string id, string name, string description, DateTime creationDateTime, string status, string creator, NotificationType type, object typeInfo, string target)
        {
            ID = id;
            Name = name;
            Description = description;
            CreationDateTime = creationDateTime;
            Status = status;
            Creator = creator;
            Type = type;
            TypeInfo = typeInfo;
            Target = target;
        }
    }
}