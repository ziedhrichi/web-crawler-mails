using Moq;
using web_crawler_email.Interfaces;
using web_crawler_email.Services;

namespace web_crawler_email.test
{
    public class EmailCrawlerServiceTests
    {
        #region Imports

        private readonly Mock<IWebBrowser> _browserMock;
        private readonly IAmTheTest _service;

        #endregion

        #region Constructeur

        public EmailCrawlerServiceTests()
        {
            _browserMock = new Mock<IWebBrowser>();
            _service = new EmailCrawlerService();
        }

        #endregion

        #region Méthodes d'aide

        private void SetupBrowser(Dictionary<string, string> pages)
        {
            foreach (var page in pages)
            {
                _browserMock
                    .Setup(b => b.GetHtml(page.Key))
                    .Returns(page.Value);
            }
        }

        private Dictionary<string, string> CreateStandardPages()
        {
            return new Dictionary<string, string>
            {
                ["file:///C:/TestHtml/index.html"] =
                    "<html>" +
                    "<a href=\"./child1.html\">child1</a>" +
                    "<a href=\"mailto:nullepart@mozilla.org\">mail</a>" +
                    "</html>",

                ["file:///C:/TestHtml/child1.html"] =
                    "<html>" +
                    "<a href=\"./index.html\">index</a>" +
                    "<a href=\"./child2.html\">child2</a>" +
                    "<a href=\"mailto:ailleurs@mozilla.org\">mail1</a>" +
                    "<a href=\"mailto:nullepart@mozilla.org\">mail2</a>" +
                    "</html>",

                ["file:///C:/TestHtml/child2.html"] =
                    "<html>" +
                    "<a href=\"./index.html\">index</a>" +
                    "<a href=\"mailto:loin@mozilla.org\">mail</a>" +
                    "<a href=\"mailto:nullepart@mozilla.org\">mail2</a>" +
                    "</html>"
            };
        }

        #endregion

        #region Tests

        [Fact]
        public void Should_Crawl_All_Pages_And_Extract_All_Emails()
        {
            SetupBrowser(CreateStandardPages());

            var result = _service.GetEmailsInPageAndChildPages(
                _browserMock.Object,
                "file:///C:/TestHtml/index.html",
                2);

            Assert.Equal(3, result.Count);
            Assert.Contains("nullepart@mozilla.org", result);
            Assert.Contains("ailleurs@mozilla.org", result);
            Assert.Contains("loin@mozilla.org", result);
        }

        [Fact]
        public void Should_Respect_MaxDepth()
        {
            SetupBrowser(CreateStandardPages());

            var result = _service.GetEmailsInPageAndChildPages(
                _browserMock.Object,
                "file:///C:/TestHtml/index.html",
                0);

            Assert.Single(result);
            Assert.Contains("nullepart@mozilla.org", result);
        }

        [Fact]
        public void Should_Stop_At_Depth_One()
        {
            SetupBrowser(CreateStandardPages());

            var result = _service.GetEmailsInPageAndChildPages(
                _browserMock.Object,
                "file:///C:/TestHtml/index.html",
                1);

            Assert.Contains("nullepart@mozilla.org", result);
            Assert.Contains("ailleurs@mozilla.org", result);
            Assert.DoesNotContain("loin@mozilla.org", result);
        }

        [Fact]
        public void Should_Explore_All_When_Depth_Is_Minus_One()
        {
            SetupBrowser(CreateStandardPages());

            var result = _service.GetEmailsInPageAndChildPages(
                _browserMock.Object,
                "file:///C:/TestHtml/index.html",
                -1);

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void Should_Not_Loop_When_Pages_Reference_Each_Other()
        {
            var pages = new Dictionary<string, string>
            {
                ["file:///A.html"] =
                    "<a href=\"file:///B.html\"></a>" +
                    "<a href=\"mailto:a@test.com\"></a>",

                ["file:///B.html"] =
                    "<a href=\"file:///A.html\"></a>" +
                    "<a href=\"mailto:b@test.com\"></a>"
            };

            SetupBrowser(pages);

            var result = _service.GetEmailsInPageAndChildPages(
                _browserMock.Object,
                "file:///A.html",
                10);

            Assert.Equal(2, result.Count);
            Assert.Contains("a@test.com", result);
            Assert.Contains("b@test.com", result);
        }

        [Fact]
        public void Should_Not_Return_Duplicate_Emails()
        {
            SetupBrowser(CreateStandardPages());

            var result = _service.GetEmailsInPageAndChildPages(
                _browserMock.Object,
                "file:///C:/TestHtml/index.html",
                2);

            Assert.Equal(result.Count, result.ToHashSet().Count);
        }

        [Fact]
        public void Should_Handle_Missing_Page()
        {
            var pages = CreateStandardPages();
            pages.Remove("file:///C:/TestHtml/child2.html");

            SetupBrowser(pages);

            var result = _service.GetEmailsInPageAndChildPages(
                _browserMock.Object,
                "file:///C:/TestHtml/index.html",
                5);

            Assert.Contains("nullepart@mozilla.org", result);
            Assert.Contains("ailleurs@mozilla.org", result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void Should_Return_Empty_When_Start_Page_Is_Null()
        {
            _browserMock
                .Setup(b => b.GetHtml("index"))
                .Returns((string)null);

            var result = _service.GetEmailsInPageAndChildPages(
                _browserMock.Object,
                "index",
                2);

            Assert.Empty(result);
        }

        [Fact]
        public void Should_Return_Empty_When_No_Email_Found()
        {
            var pages = new Dictionary<string, string>
            {
                ["file:///index.html"] = "<a href=\"child.html\"></a>",
                ["file:///child.html"] = "<html></html>"
            };

            SetupBrowser(pages);

            var result = _service.GetEmailsInPageAndChildPages(
                _browserMock.Object,
                "file:///index.html",
                2);

            Assert.Empty(result);
        }

        [Fact]
        public void Should_Ignore_Invalid_Links()
        {
            var pages = new Dictionary<string, string>
            {
                ["file:///index.html"] =
                    "<a href=\"#\"></a>" +
                    "<a href=\"javascript:void(0)\"></a>" +
                    "<a href=\"mailto:test@test.com\"></a>"
            };

            SetupBrowser(pages);

            var result = _service.GetEmailsInPageAndChildPages(
                _browserMock.Object,
                "file:///index.html",
                1);

            Assert.Single(result);
            Assert.Contains("test@test.com", result);
        }

        [Fact]
        public void Should_Resolve_Relative_Urls()
        {
            var pages = new Dictionary<string, string>
            {
                ["file:///root/index.html"] =
                    "<a href=\"./child.html\"></a>",

                ["file:///root/child.html"] =
                    "<a href=\"mailto:test@test.com\"></a>"
            };

            SetupBrowser(pages);

            var result = _service.GetEmailsInPageAndChildPages(
                _browserMock.Object,
                "file:///root/index.html",
                1);

            Assert.Contains("test@test.com", result);
        }

        [Fact]
        public void Should_Ignore_Invalid_Mailto()
        {
            var pages = new Dictionary<string, string>
            {
                ["file:///index.html"] =
                    "<a href=\"mailto:\"></a>" +
                    "<a href=\"mailto:valid@test.com\"></a>"
            };

            SetupBrowser(pages);

            var result = _service.GetEmailsInPageAndChildPages(
                _browserMock.Object,
                "file:///index.html",
                1);

            Assert.Single(result);
            Assert.Contains("valid@test.com", result);
        }

        #endregion
    }
}