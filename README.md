# Web-Crawler-de-Mail
## Introduction

Un algorithme pour web crawler qui cherche tous les mails distincts dans le html d'une page web (mailto dans les href) et les pages web qu'elle référence.

- Le navigateur pour obtenir les pages html est fourni.
- La profondeur de recherche est configurable car le web crawler doit chercher en priorités dans les pages "les plus proches" de la page de départ et éviter d'aller voir des liens trop loin en profondeur de recherche (On peut aussi mettre maximumDepth à -1 pour tout explorer).

Pour cet exercice on considère que html est du xml valide.