# Projet Dashboard de Production en Temps Réel

Tableau de bord de production affichant des métriques en temps réel. L'architecture repose sur des microservices ASP.NET Core communiquant via une API Gateway (YARP), un client React connecté en SignalR pour le temps réel, et des agents Python simulant des capteurs de production. Les données sont persistées dans PostgreSQL.

## Stack Technique

### Core
- **Backend** : ASP.NET Core 10.0 (architecture microservices)
- **API Gateway** : Traefik v3.2 (reverse proxy, load balancer)
- **Frontend** : React 19 + TypeScript + Tailwind CSS 4 + Vite 7
- **Communication temps réel** : SignalR (WebSockets)
- **Base de données** : PostgreSQL 16
- **Agents** : Python 3.11+ (psycopg2, python-dotenv)
- **Conteneurisation** : Docker & Docker Compose

### CI/CD & Qualité
- **Linters** : dotnet format, ESLint, Ruff, Black
- **Type Checking** : TypeScript, MyPy
- **Sécurité** : CodeQL, Gitleaks, Trivy
- **Automatisation** : GitHub Actions, Dependabot
- **Monitoring** : Grafana (optionnel), Traefik Dashboard

## Fonctionnalités

- Affichage en temps réel des métriques de production via SignalR
- Sélection et suivi de lignes de production
- Historique des données de production (Historian)
- Gestion back-office (CRUD) : utilisateurs, rôles, lignes, équipements, tags, OFs
- Authentification JWT
- Notifications en temps réel pour les anomalies de production
- Simulation continue de données via agents Python

## Architecture

```mermaid
graph TD
    Client[Client React :3000] -- HTTP / SignalR --> Gateway[API Gateway :5050]
    Gateway -- /api/v1/** --> Backoffice[Service Back-Office :5001]
    Gateway -- /hubs/** --> Realtime[Service Temps Réel :5002]
    Backoffice -- Lecture / Écriture --> DB[(PostgreSQL :5432)]
    Realtime -- Lecture --> DB
    Agents[Agents Python] -- Écriture --> DB
    Realtime -- SignalR Push --> Client
```

### Microservices

| Service | Port | Rôle |
|---------|------|------|
| **API Gateway** | 5050 | Point d'entrée unique, routage via YARP |
| **Back-Office** | 5001 (interne) | Opérations CRUD, authentification JWT |
| **Realtime** | 5002 (interne) | Hubs SignalR, broadcast des métriques |

### Flux de données

1. Les **agents Python** simulent des capteurs et écrivent les données dans PostgreSQL
2. Le **service Realtime** lit la base et diffuse les métriques en temps réel via SignalR
3. Le **client React** se connecte aux hubs SignalR via la gateway et affiche les métriques live
4. Le **service Back-Office** expose les API REST pour la gestion (CRUD) des entités

## Structure du Projet

```
├── gateway/                    # API Gateway (YARP reverse proxy)
│   └── ApiGateway/
├── services/
│   ├── backoffice/             # Service Back-Office (CRUD + Auth)
│   │   └── BackOfficeService/
│   │       ├── Controllers/    # Auth, Equipments, Lines, Ofs, Roles, Tags, Users
│   │       ├── DTOs/           # Objets de transfert par entité
│   │       ├── Models/         # Entités EF Core
│   │       ├── Repositories/   # Accès aux données
│   │       ├── Services/       # Logique métier
│   │       ├── Middleware/     # Gestion des erreurs
│   │       └── Data/           # DbContext + Migrations
│   └── realtime/               # Service Temps Réel (SignalR)
│       └── RealtimeService/
│           ├── Hubs/           # ProductionMetricsHub, NotificationsHub
│           ├── Services/       # MetricsBroadcastService
│           ├── Models/         # Entités en lecture seule
│           └── Data/           # RealtimeDbContext
├── server/                     # Backend monolithique legacy (remplacé par les microservices)
├── client/                     # Frontend React
│   └── src/
│       ├── pages/              # Login, Dashboard, ProductionDashboard, Backoffice (CRUD)
│       ├── components/         # UI, Auth, Layout
│       ├── services/           # Appels API + SignalR
│       ├── context/            # Contextes React
│       └── types/              # Types TypeScript
├── agents/                     # Agents Python
│   ├── seed_generation/        # Peuplement initial de la base (one-shot)
│   ├── line_agent/             # Simulation continue des lignes de production
│   └── requirements.txt
└── docker-compose.yml
```

## Schéma de Base de Données

```mermaid
erDiagram
    Roles ||--o{ Users : "1:N"
    Roles {
        int role_id PK
        varchar name
    }
    Users {
        int user_id PK
        varchar identifiant UK
        varchar password
        varchar email
        int role FK
        timestamp created_at
        timestamp updated_at
    }
    Equipments ||--|| Lines : "1:1"
    Equipments ||--o{ Equipment_Tag : "M:N"
    Equipments {
        int equipment_id PK
        varchar name
    }
    Tags ||--o{ Equipment_Tag : "M:N"
    Tags ||--o{ Historian : "1:N"
    Tags {
        int tag_id PK
        varchar tag_name
    }
    Equipment_Tag {
        int equipment FK
        int tag_name FK
    }
    ofs ||--o{ Lines : "of_en_cours"
    ofs ||--o{ Lines : "of_suivant"
    ofs {
        int of_id PK
        varchar of_name
        varchar produit
        int qte_produite
        int qte_totale
    }
    Lines {
        int line_id PK
        varchar name
        int equipment FK
        int of_en_cours FK
        int of_suivant FK
        bool is_changement
        int temps_changement
    }
    Historian {
        int historian_id PK
        int tag_name FK
        timestamp timestamp
        decimal value
    }
```

## Instructions de Déploiement

### Avec Docker (recommandé)

```bash
# Lancer l'ensemble des services
docker-compose up --build

# Lancer uniquement la base de données
docker-compose up postgres pgadmin
```

### Accès

| Service | URL | Description |
|---------|-----|-------------|
| **Frontend** | http://localhost:3000 | Application React principale |
| **API Gateway** | http://localhost:5050 | Point d'entrée unique (Traefik) |
| **Traefik Dashboard** | http://localhost:8081 | Monitoring de l'API Gateway |
| **Grafana** | http://localhost:3001 | Dashboards de visualisation (si configuré) |
| **pgAdmin** | http://localhost:8080 | Interface de gestion PostgreSQL |
| **PostgreSQL** | localhost:5432 | Base de données |

**Note** : Les ports peuvent varier selon votre fichier `.env`

### Identifiants par défaut

- **PostgreSQL** : `postgres` / `postgres` (base : `production_dashboard`)
- **pgAdmin** : `admin@example.com` / `admin`
- **Grafana** (si configuré) : `admin` / `admin`
- **Connexion site admin** : `admin` / `admin123`
- **Connexion site viewer** : `viewer` / `admin123`

### Développement local

```bash
# Backend (depuis services/backoffice/BackOfficeService ou services/realtime/RealtimeService)
dotnet restore && dotnet run

# Frontend
cd client && npm install && npm run dev    # http://localhost:5173

# Agents
cd agents && pip install -r requirements.txt
python -m seed_generation                   # Peuplement initial
python -m line_agent --line-ids 1 2         # Simulation continue
```

## CI/CD - Pipelines

Le projet utilise GitHub Actions pour garantir la qualité du code.

### Linters

| Technologie | Outil | Pipeline | Déclenchement |
|-------------|-------|----------|---------------|
| **Backend .NET** | dotnet format | `backend-lint.yml` | Push/PR sur `services/`, `gateway/` |
| **Frontend React** | ESLint | `frontend-lint.yml` | Push/PR sur `client/` |
| **Python Agents** | Ruff | `python-lint.yml` | Push/PR sur `agents/` |

### Tests

| Projet | Pipeline | Déclenchement |
|--------|----------|---------------|
| **BackOffice Tests** | `backoffice-unittests.yml` | Push/PR sur `services/backoffice/` |

### Exécuter localement

```bash
# Linters
dotnet format services/backoffice/BackOfficeService/BackOfficeService.csproj --verify-no-changes
dotnet format services/realtime/RealtimeService/RealtimeService.csproj --verify-no-changes
dotnet format gateway/ApiGateway/ApiGateway.csproj --verify-no-changes
cd client && npm run lint
cd agents && ruff check .

# Tests
dotnet test services/backoffice/BackOfficeService.Tests/BackOfficeService.Tests.csproj
```

## Auteurs

- Cyprien.G
- Tristan.G