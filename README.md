# Web-Crawler-de-Mail
## Introduction

Ce projet implémente un web crawler simple permettant d’extraire toutes les adresses e-mail distinctes présentes dans le HTML d’une page web ainsi que dans les pages qu’elle référence.

Le crawler explore les liens jusqu’à une profondeur configurable et priorise les pages les plus proches de la page de départ en utilisant un parcours en largeur (Breadth-First Search - BFS).

La récupération du HTML est abstraite via l’interface IWebBrowser, ce qui facilite les tests et rend la solution flexible.

## Fonctionnalités

- Extraction des adresses e-mail depuis les liens mailto:
- Parcours récursif des pages liées
- Profondeur de recherche configurable
- Évitement des boucles infinies (détection des pages déjà visitées)
- Suppression des doublons
- Tests unitaires avec mock

## Fonctionnement

1- Démarrer à partir d’une URL initiale

2- Extraire :

   - les e-mails (mailto:)
   - les liens (href)
     
3- Parcourir les pages liées jusqu’à la profondeur définie

4- Éviter de revisiter les pages déjà traitées

5- Retourner une liste d’adresses e-mail uniques

## Choix techniques

- Utilisation d’un parcours BFS pour respecter la priorité des pages proches
- Utilisation de HashSet pour garantir l’unicité des e-mails
- Utilisation de mocks pour isoler les tests
