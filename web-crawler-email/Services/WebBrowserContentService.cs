using System;
using System.IO;
using web_crawler_email.Interfaces;

namespace web_crawler_email.Services
{
    /// <summary>
    /// Implémentation de <see cref="IWebBrowser"/> qui lit le contenu HTML à partir d'un fichier local.
    /// </summary>
    public class WebContentBrowserService : IWebBrowser
    {
        /// <summary>
        /// Récupère le contenu HTML d'une page web spécifiée par son URL.
        /// </summary>
        /// <param name="url">L'URL de la page web à récupérer.</param>
        /// <returns>Le contenu HTML de la page web.</returns>
        public string GetHtml(string url)
        {
            // Traite les URL de type "file://" pour accéder aux fichiers locaux
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
                // En cas d'erreur (fichier non trouvé, accès refusé, etc.), retourne une chaîne vide
                return string.Empty;
            }
        }
    }
}