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
        public Friend Sender { get; set; }
        public DateTime RequestDate { get; set; }
        public StatusType Status { get; set; }
    }
}