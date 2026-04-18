using System;
using System.IO;
using web_crawler_email.Interfaces;

public class WebContentBrowserService : IWebBrowser
{
    public string GetHtml(string url)
    {
        try
        {
            // Vérifie que le fichier existe
            if (!File.Exists(url))
                return string.Empty;

            // Lecture directe du fichier HTML
            return File.ReadAllText(url);
        }
        catch
        {
            return string.Empty;
        }
    }
}