using System;

namespace Notify.Core
{
    public enum StatusType
    {
        Pending,
        Accepted,
        Rejected
    }
    
    public class FriendRequest
    {
        public string Requester { get; set; }
        public string UserName { get; set; }
        public string RequestDate { get; set; }
        public StatusType Status { get; set; }
    }
}