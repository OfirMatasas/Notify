using System;
using Notify.Helpers;

namespace Notify.Core
{
    public enum NotificationType
    {
        Time,
        Location,
        WiFi,
        Bluetooth,
        Dynamic
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
        public string Activation { get; set; }
        public bool IsPermanent { get; set; }
        public string Target { get; set; }
        public string ShouldBeNotified { get; set; }
        
        public bool IsPending => Status == Constants.NOTIFICATION_STATUS_PENDING;
        public bool IsRenewable => Status == Constants.NOTIFICATION_STATUS_EXPIRED && Type != NotificationType.Time;
        public bool IsEditable => Status != Constants.NOTIFICATION_STATUS_EXPIRED;
        public bool IsLocationType => Type == NotificationType.Location;
        public bool IsDynamicLocation => Type == NotificationType.Dynamic;

        public Notification()
        {
            
        }
        
        public Notification(string id, string name, string description, DateTime creationDateTime, string status, string creator, NotificationType type, object typeInfo, string target, string activation, bool permanent, string shouldBeNotified)
        {
            ID = id;
            Name = name;
            Description = description;
            CreationDateTime = creationDateTime;
            Status = status;
            Creator = creator;
            Type = type;
            TypeInfo = typeInfo;
            Activation = activation;
            IsPermanent = permanent;
            Target = target;
            ShouldBeNotified = shouldBeNotified;
        }
        
        public Notification(string id)
        {
            ID = id;
        }
    }
}
