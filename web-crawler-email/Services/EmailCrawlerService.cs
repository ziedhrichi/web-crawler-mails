using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using web_crawler_email.Interfaces;

/// <summary>
/// Parcourt une page web et ses pages enfants pour découvrir des adresses e‑mail.
/// </summary>
/// <remarks>
/// Effectue un parcours en largeur (BFS) à partir d'une URL de départ, collecte les
/// adresses trouvées dans les attributs <c>mailto:</c> et résout les liens relatifs
/// par rapport à leur URL de base. Utilisez <see cref="GetEmailsInPageAndChildPages"/> pour lancer le crawl.
/// </remarks>
public class EmailCrawlerService : IAmTheTest
{
    /// <summary>
    /// Parcourt l'URL spécifiée et ses pages enfants jusqu'à une profondeur donnée et renvoie les adresses e‑mail découvertes.
    /// </summary>
    /// <param name="browser">Une implémentation de <see cref="IWebBrowser"/> utilisée pour récupérer le HTML de chaque URL.</param>
    /// <param name="url">L'URL de départ du crawl. Doit être une URL absolue.</param>
    /// <param name="maximumDepth">
    /// Profondeur maximale des pages enfants à parcourir.
    /// Utilisez 0 pour ne récupérer que la page de départ.
    /// Utilisez -1 pour désactiver la limite de profondeur et parcourir tant qu'il reste des liens non visités.
    /// </param>
    /// <returns>
    /// Une liste d'adresses e‑mail uniques découvertes lors du crawl. Chaque adresse est renvoyée sans le préfixe <c>mailto:</c>.
    /// </returns>
    public List<string> GetEmailsInPageAndChildPages(IWebBrowser browser, string url, int maximumDepth)
    {
        // Utiliser des HashSet pour éviter les doublons d'emails et de visites
        HashSet<string> emails = [];
        HashSet<string> visited = [];

        // Utiliser une file pour le parcours en largeur (BFS)
        Queue<(string url, int depth)> queue = new();

        // Enqueue l'URL de départ avec une profondeur initiale de 0
        queue.Enqueue((url, 0));

        // Tant qu'il y a des URLs à visiter dans la file
        while (queue.Count > 0)
        {
            var (currentUrl, depth) = queue.Dequeue();

            // Vérifier si l'URL a déjà été visitée pour éviter les boucles infinies
            if (visited.Contains(currentUrl))
                continue;

            // Marquer l'URL comme visitée
            visited.Add(currentUrl);

            // Récupérer le HTML de la page
            string html = browser.GetHtml(currentUrl);
            if (html == null)
                continue;

            // 1. Extraire les emails
            foreach (var email in ExtractEmails(html))
            {
                emails.Add(email);
            }

            // 2. Stop si profondeur max atteinte
            if (maximumDepth != -1 && depth >= maximumDepth)
                continue;

            // 3. Extraire les liens
            foreach (var link in ExtractLinks(html, currentUrl))
            {
                // Enqueue les liens non visités avec une profondeur incrémentée
                if (!visited.Contains(link))
                {
                    // Enqueue le lien avec une profondeur incrémentée
                    queue.Enqueue((link, depth + 1));
                }
            }
        }

        // Convertir le HashSet d'emails en une liste et la retourner
        return [.. emails];
    }

    /// <summary>
    /// Extrait les adresses e‑mail depuis le contenu HTML en recherchant les attributs <c>href="mailto:"</c>.
    /// </summary>
    /// <param name="html">Le contenu HTML à analyser.</param>
    /// <returns>Un énumérable d'adresses e‑mail (sans le préfixe <c>mailto:</c>).</returns>
    private IEnumerable<string> ExtractEmails(string html)
    {
        var matches = Regex.Matches(html, @"href\s*=\s*""mailto:([^""]+)""", RegexOptions.IgnoreCase);
        foreach (Match match in matches)
        {
            yield return match.Groups[1].Value;
        }
    }

    /// <summary>
    /// Extrait les liens depuis le HTML et résout les liens relatifs par rapport à l'URL de base fournie.
    /// Les liens <c>mailto:</c> sont ignorés.
    /// </summary>
    /// <param name="html">Le contenu HTML contenant les liens.</param>
    /// <param name="baseUrl">L'URL de base utilisée pour résoudre les liens relatifs.</param>
    /// <returns>Un énumérable d'URL absolues trouvées dans le HTML.</returns>
    private IEnumerable<string> ExtractLinks(string html, string baseUrl)
    {
        var matches = Regex.Matches(html, @"href\s*=\s*""([^""]+)""", RegexOptions.IgnoreCase);

        foreach (Match match in matches)
        {
            var link = match.Groups[1].Value;

            // Ignorer mailto
            if (link.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
                continue;

            yield return NormalizeUrl(baseUrl, link);
        }
    }

    /// <summary>
    /// Normalise et résout un lien par rapport à une URL de base. Si le lien est absolu, il est renvoyé tel quel.
    /// </summary>
    /// <param name="baseUrl">L'URL de base pour résoudre les liens relatifs.</param>
    /// <param name="link">Le lien à normaliser (absolu ou relatif).</param>
    /// <returns>Une chaîne représentant l'URL absolue résolue.</returns>
    private string NormalizeUrl(string baseUrl, string link)
    {
        if (Uri.IsWellFormedUriString(link, UriKind.Absolute))
            return link;

        var baseUri = new Uri(baseUrl);
        var resolved = new Uri(baseUri, link);
        return resolved.ToString();
    }
}