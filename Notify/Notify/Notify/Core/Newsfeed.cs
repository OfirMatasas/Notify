namespace Notify.Core
{
    public class Newsfeed
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        
        public Newsfeed(string id, string title, string content)
        {
            ID = id;
            Title = title;
            Content = content;
        }
    }
}
