
namespace Notify.Core
{
    public class FriendRequest
    {
        public string Requester { get; set; }
        public string UserName { get; set; }
        public string RequestDate { get; set; }
        
        public override string ToString()
        {
            return $"Username: {Requester}\nName: {UserName}\nRequest Date: {RequestDate}";
        }
    }
}