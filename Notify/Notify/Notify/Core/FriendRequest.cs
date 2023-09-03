namespace Notify.Core
{
    public class FriendRequest
    {
        public string Requester { get; set; }
        public string UserName { get; set; }
        public string RequestDate { get; set; }
        public string RequesterProfilePicture { get; set; }
        
        public FriendRequest(string requester, string userName, string requestDate, string requesterProfilePicture)
        {
            Requester = requester;
            UserName = userName;
            RequestDate = requestDate;
            RequesterProfilePicture = requesterProfilePicture;
        }
        
        public override string ToString()
        {
            return $"Username: {Requester}\nName: {UserName}\nRequest Date: {RequestDate}";
        }
    }
}
