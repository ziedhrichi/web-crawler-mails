using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace web_crawler_email.Interfaces
{
    /// <summary>
    /// Interface définissant le contrat pour un navigateur web capable 
    /// de récupérer le contenu HTML d'une page à partir d'une URL.
    /// </summary>
    public interface IWebBrowser
    {
        /// <summary>
        /// Récupère le contenu HTML d'une page web spécifiée par son URL.
        /// </summary>
        /// <param name="url">L'URL de la page web à récupérer.</param>
        /// <returns>Le contenu HTML de la page web.</returns>
        string GetHtml(string url);
    }
}
