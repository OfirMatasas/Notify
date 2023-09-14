namespace Notify.Core
{
    public class Newsfeed
    {
        public string Username { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        
        public Newsfeed(string username, string title, string content)
        {
            Username = username;
            Title = title;
            Content = content;
        }
    }
}
