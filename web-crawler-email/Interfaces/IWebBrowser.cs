using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace web_crawler_email.Interfaces
{
    public interface IWebBrowser
    {
        string GetHtml(string url);
    }
}
