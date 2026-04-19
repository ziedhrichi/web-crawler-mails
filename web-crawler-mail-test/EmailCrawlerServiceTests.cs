using Moq;
using web_crawler_email.Interfaces;
using web_crawler_email.Services;

namespace web_crawler_email.Tests
{
    public class EmailCrawlerServiceTests
    {
        #region Imports

        private readonly Mock<IWebBrowser> _browserMock;
        private readonly IAmTheTest _service;

        #endregion

        #region Coonstructeur
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

        #region Méthodes de test

        /// <summary>
        /// Teste que le service peut parcourir plusieurs pages et extraire les adresses e‑mail de chacune d'elles.
        /// </summary>
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
            Assert.Contains("nullepart@mozilla.org", result);
        }

        /// <summary>
        /// Teste que le service respecte la profondeur maximale spécifiée et n'extrait 
        /// pas les adresses e‑mail des pages enfants au-delà de cette profondeur.
        /// </summary>
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

        /// <summary>
        /// Teste que le service ne boucle pas indéfiniment lorsque les pages 
        /// font référence les unes aux autres,
        /// </summary>
        [Fact]
        public void Should_Not_Loop_When_Pages_Reference_Each_Other()
        {
            SetupBrowser(CreateStandardPages());

            var result = _service.GetEmailsInPageAndChildPages(
                _browserMock.Object,
                "file:///C:/TestHtml/index.html",
                10);

            Assert.Equal(3, result.Count);
        }

        /// <summary>
        /// Teste que le service ne retourne pas d'adresses e‑mail en double même si elles sont présentes sur 
        /// plusieurs pages ou plusieurs fois sur la même page.
        /// </summary>
        [Fact]
        public void Should_Not_Return_Duplicate_Emails()
        {
            SetupBrowser(CreateStandardPages());

            var result = _service.GetEmailsInPageAndChildPages(
                _browserMock.Object,
                "file:///C:/TestHtml/index.html",
                2);

            Assert.Equal(result.Count, result.Distinct().Count());
        }

        /// <summary>
        /// Teste que le service gère correctement les pages manquantes 
        /// ou les erreurs de récupération de HTML
        /// </summary>
        [Fact]
        public void Should_Handle_Missing_Page()
        {
            var pages = CreateStandardPages();
            pages.Remove("file:///C:/TestHtml/child2.html"); // page manquante

            SetupBrowser(pages);

            var result = _service.GetEmailsInPageAndChildPages(
                _browserMock.Object,
                "file:///C:/TestHtml/index.html",
                5);

            Assert.Contains("nullepart@mozilla.org", result);
            Assert.Contains("ailleurs@mozilla.org", result);
            Assert.Equal(2, result.Count);
        }

        /// <summary>
        /// Teste que le service s'arrête correctement à la profondeur spécifiée et n'extrait pas les adresses e‑mail 
        /// des pages enfants au-delà de cette profondeur.
        /// </summary>
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

        #endregion
    }
}