using web_crawler_email.Interfaces;

namespace web_crawler_email.Services
{
    public class EmailCrawler : IAmTheTest
    {
        public List<string> GetEmailsInPageAndChildPages(IWebBrowser browser, string url, int maximumDepth)
        {
            throw new NotImplementedException();
        }
    }
}
