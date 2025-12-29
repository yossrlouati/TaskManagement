# Fichier: Dockerfile

# [ÉTAPE 1: BUILD] Utilise l'image SDK .NET pour compiler l'application
# 'AS base' donne un nom de référence à cette étape
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copie le fichier .csproj et lance la restauration des dépendances NuGet
# C'est fait séparément pour optimiser le cache de Docker
COPY ["SimpleAuthApi.csproj", "."] 
RUN dotnet restore "SimpleAuthApi.csproj"

# Copie le reste des fichiers du projet
COPY . .

# Publie l'application en mode "Release" dans le dossier "out"
RUN dotnet publish "SimpleAuthApi.csproj" -c Release -o /app/out

# ----------------------------------------------------------------------

# [ÉTAPE 2: FINAL] Utilise l'image runtime .NET (beaucoup plus légère) pour exécuter l'application
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app

# Copie les fichiers publiés (seulement les binaires) de l'étape 'build'
COPY --from=build /app/out .

# Configuration de l'environnement (IMPORTANT : Pour la sécurité et la santé)
# L'API s'exécutera sur le port 8080 dans le conteneur
ENV ASPNETCORE_URLS=http://+:8080 

# Définit le point d'entrée : la commande pour lancer l'API
ENTRYPOINT ["dotnet", "SimpleAuthApi.dll"]