# 🚀 TaskManagement API

[Go to French Version / Aller à la version française](#version-française)

A robust backend solution developed with **ASP.NET Core (.Net7)**, designed for enterprise task management with seamless communication between administrators and employees.

## 📌 Key Features
### 🔐 Auth & Security
* **Identity Framework**: Full user and role management (Admin, Employee).
* **JWT Authentication**: Securing endpoints via JSON Web Tokens.

### 📝 Task Management (CRUD)
* Create, Visualize, edit, delete and assign tasks with the possibility to upload or to download documents.
* **Email Notifications (SMTP)**: Automated emails on task assignment.

### 💬 Real-Time Communication (SignalR)
* **Private Chat**: Instant messaging between Admin and Employees via SignlR(WebSockets).
* **Message History**: Chat persistence in SQL Server database.
* **Presence Status**: Real-time Online/Offline indicator for each user.

### 🤖 Artificial Intelligence
* **Content Analysis**: Integration of an AI service for task description generation.

## 🛠 Tech Stack
* **Framework**: ASP.NET Core Web API
* **Database**: SQL Server (Entity Framework Core)
* **Real-time**: SignalR
* **Security**: ASP.NET Core Identity + JWT
* **Architecture**: RESTful API

## 🚀 Quick Start
1. **Clone the project**: `git clone https://github.com/yossrlouati/TaskManagement.git`
2. **Rename** `appsettings.Example.json` to `appsettings.json`.
3. **Configure Settings** in `appsettings.json` (Database DefaultConnection).
4. **Apply migrations**: `dotnet ef database update`
5. **Run the application**: `dotnet run`

## 🏗 Architecture & Scalability (Vision Pro)
Although the project currently uses SQL Server for all data, the architecture is designed to scale using:
* **Redis**: For message caching and SignalR backplane.
* **MongoDB**: For high-volume chat history archiving.

## 💡 Improvements
* **User Preferences (Optional)**: System allowing users to enable/disable email notifications.
* **Google Auth (Optional)**: Configured support for OAuth2 external authentication.

## 🧪 Validation & Tests
### Completed Tests
* **Integration Tests (xUnit)**: Setup of `SimpleAuthApi.Tests` project with initial login logic tests (using `WebApplicationFactory` for in-memory test server). Validation of the full authentication flow.
* **Functional/API Tests (Postman)**: Manual End-to-End (E2E) / Black-box testing:
    * **Authentication**: Full Register/Login flow and JWT token generation.
    * **Task Management**: Task creation and assignment.
    * **Email Service**: Verification of actual SMTP notification delivery on task assignment.

### Upcoming Tests (Roadmap)
* **Unit Testing**: Implement tests for isolated business logic in services (Task assignment logic, AI description processing).
* **Load Testing**: Verify SignalR stability and performance under multiple concurrent connections.
* **Database Integration**: Expand integration tests to cover all CRUD operations and data persistence.

---

<a name="version-française"></a>

# 🚀 TaskManagement API (Version Française)

Une solution backend robuste développée avec **ASP.NET Core (.Net7)**, conçue pour la gestion de tâches en entreprise avec une communication fluide entre administrateurs et employés.

## 📌 Fonctionnalités Clés
### 🔐 Authentification & Sécurité
* **Identity Framework** : Gestion complète des utilisateurs et des rôles (Admin, Employee).
* **JWT Authentication** : Sécurisation des endpoints via JSON Web Tokens.

### 📝 Gestion des Tâches (CRUD)
* Créer, visualiser, modifier, supprimer et attribuer des tâches avec la possibilité de télécharger ou de téléverser des documents..
* **Notifications Email (SMTP)** : Envoi automatique d'emails lors de l'assignation de tâches.

### 💬 Communication Temps Réel (SignalR)
* **Chat Privé** : Messagerie instantanée entre Admin et Employés via SignlR(WebSockets).
* **Historique des messages** : Persistance des conversations en base de données SQL Server.
* **Statut de présence** : Indicateur Online/Offline en temps réel pour chaque utilisateur.

### 🤖 Intelligence Artificielle
* **Analyse de contenu** : Intégration d'un service d'IA pour la génération de description de tâches.

## 🛠 Stack Technique
* **Framework** : ASP.NET Core Web API
* **Base de données** : SQL Server (Entity Framework Core)
* **Temps réel** : SignalR
* **Sécurité** : ASP.NET Core Identity + JWT
* **Architecture** : RESTful API

## 🚀 Installation Rapide
1. **Cloner le projet** : `git clone https://github.com/yossrlouati/TaskManagement.git`
2. **Renommer** `appsettings.Example.json` en `appsettings.json`.
3. **Modifier les Configurations** dans le fichier `appsettings.json` (DefaultConnection de DB).
4. **Appliquer les migrations** : `dotnet ef database update`
5. **Lancer l'application** : `dotnet run`

## 🏗 Architecture & Scalabilité (Vision Pro)
Bien que le projet utilise actuellement SQL Server pour l'ensemble des données, l'architecture a été pensée pour évoluer vers :
* **Redis** : Pour le caching des messages et le backplane SignalR.
* **MongoDB** : Pour l'archivage massif des logs de chat.

## 💡 Amélioration:
* **Préférences Utilisateur (Optionnel)** : Système permettant aux utilisateurs d'activer/désactiver leurs notifications mail.
* **Google Auth (Optionnel)** : Support configuré pour l'authentification externe OAuth2.

## 🧪 Validation & Tests
### Tests Effectués
* **Tests d'Intégration (xUnit)** : Mise en place d'un projet de test `SimpleAuthApi.Tests` avec des premiers tests sur la logique de Login (WebApplicationFactory pour créer un serveur de test en mémoire). Validation du flux d'authentification complet (de la requête à la réponse).
* **Tests Fonctionnels/API (Postman)** : Black-box Testing / Tests de bout en bout (End-to-End ou E2E) manuels :
    * **Authentification** : Test complet du flux Register/Login et génération du token JWT.
    * **Gestion des Tâches** : Création et assignation de tâches.
    * **Service Email** : Vérification de l'envoi réel des notifications SMTP lors de l'assignation d'une tâche.

### Tests à venir (Roadmap)
* **Tests Unitaires** : Implémenter des tests pour la logique métier isolée dans les services (Logique d'assignation, traitement IA).
* **Tests de Charge** : Vérifier la stabilité et la performance de SignalR lors de connexions simultanées massives.
* **Intégration de la Base de Données** : Étendre les tests d'intégration pour couvrir l'ensemble des opérations CRUD et la persistance des données.
