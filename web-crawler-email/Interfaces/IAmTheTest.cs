using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace web_crawler_email.Interfaces
{
    /// <summary>
    /// Interface définissant le contrat pour le test de découverte d'adresses e‑mail 
    /// dans une page web et ses pages enfants.
    /// </summary>
    public interface IAmTheTest
    {
        /// <summary>
        /// Récupère les adresses e‑mail présentes dans une page web spécifiée par son URL.
        /// </summary>
        /// <param name="browser">Le navigateur web utilisé pour récupérer le contenu HTML des pages.</param>
        /// <param name="url">L'URL de la page web à analyser.</param>
        /// <param name="maximumDepth">La profondeur maximale de parcours des pages enfants.</param>
        /// <returns>Une liste d'adresses e‑mail trouvées.</returns>
        List<string> GetEmailsInPageAndChildPages(IWebBrowser browser, string url, int maximumDepth);
    }
}
