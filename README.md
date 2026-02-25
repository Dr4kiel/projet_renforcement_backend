# Projet Dashboard de Production en Temps Réel

Ce projet est une application de monitoring de production en temps réel, conçue pour afficher les métriques clés d'une ligne de production industrielle. Il utilise une architecture moderne basée sur une API Gateway (Traefik) pour gérer les requêtes, un backend ASP.NET Core pour l'API REST et la diffusion en temps réel via SignalR, un frontend React pour l'interface utilisateur, et une base de données PostgreSQL pour stocker les données de production.

## Rendu + Expériences

| Branche | État | Commentaire |
|---|---|---|
| stable | Fonctionnel | Projet complet |
| add_grafana | Fonctionnel | Ajout d'un container grafana avec un dashboard simple des données de prod |
| rabbitmq | Fonctionnel | Ajout d'un container rabbitmq avec un back C# permettant de contrôler via le front, les lignes de production (marche/arrêt) |

## Credentials

- **PostgreSQL** : `postgres` / `postgres` (base : `production_dashboard`)
- **pgAdmin** : `admin@example.com` / `admin`
- **Connexion site admin** : `admin` / `admin123`
- **Connexion site viewer** : `viewer` / `admin123`
- **Grafana** : `admin` / `admin`

## Stack Technique

- **Gateway** : Traefik v3.2 — routage, ForwardAuth JWT
- **BackOffice** : ASP.NET Core 10.0 — API REST CRUD + authentification JWT
- **Realtime** : ASP.NET Core 10.0 + SignalR — diffusion temps réel
- **Frontend** : React + TypeScript + Tailwind CSS (Vite)
- **Agents** : Scripts Python simulant des capteurs de production
- **Base de données** : PostgreSQL 16
- **Conteneurisation** : Docker + Docker Compose

## Architecture

```mermaid
flowchart TD
    Client["🖥️ Client React<br/>:3000"]

    subgraph GW["🔀 API Gateway — Traefik :5050"]
        direction TB
        R_public["Routeur public<br/>POST /api/v1/auth/login<br/>GET /health<br/>OPTIONS /api/v1/**"]
        R_protected["Routeur protégé<br/>/api/v1/**"]
        R_hubs["Routeur SignalR<br/>/hubs/**"]
        R_health["Routeur health<br/>/realtime/health"]
        FA["⚙️ Middleware<br/>ForwardAuth JWT"]
        SP["⚙️ Middleware<br/>StripPrefix /realtime"]

        R_protected -->|"valide le token"| FA
        R_health --> SP
    end

    subgraph BO["⚙️ BackOffice — :5001<br/>ASP.NET Core"]
        Auth["Auth<br/>POST /api/v1/auth/login<br/>GET /api/v1/auth/validate"]
        CRUD["CRUD<br/>/api/v1/lines<br/>/api/v1/users<br/>/api/v1/tags<br/>..."]
    end

    subgraph RT["📡 Realtime — :5002<br/>ASP.NET Core + SignalR"]
        Hubs["Hubs<br/>/hubs/production<br/>/hubs/historian"]
        Poll["Polling PostgreSQL<br/>toutes les 5s"]
    end

    subgraph DB_block["🗄️ PostgreSQL — :5432"]
        DB[("production_dashboard")]
    end

    subgraph Agents["🤖 Agents Python"]
        Seed["seed_generation<br/>one-shot"]
        LineAgent["line_agent<br/>continu"]
    end

    Client -->|"REST"| R_public
    Client -->|"REST + Bearer token"| R_protected
    Client -->|"WebSocket"| R_hubs
    Client -->|"GET"| R_health

    R_public --> Auth
    FA -->|"200 → transmis"| CRUD
    FA -->|"401 → bloqué"| Client
    R_hubs --> Hubs
    SP --> RT

    Auth --> DB
    CRUD --> DB
    Poll --> DB
    Hubs -->|"push temps réel"| Client

    Seed -->|"init"| DB
    LineAgent -->|"métriques"| DB
```

## Fonctionnalités

- Authentification JWT avec validation au niveau gateway (ForwardAuth)
- Affichage en temps réel des métriques de production via SignalR
- Sélection de la ligne de production à surveiller
- Historique des données de production
- Notifications en temps réel pour les anomalies
- Simulation de données via des agents Python

## Routes exposées (port 5050)

| Route | Méthode | JWT | Destination |
|---|---|---|---|
| `/api/v1/auth/login` | POST | Non | backoffice:5001 |
| `/health` | GET | Non | backoffice:5001 |
| `/api/v1/**` | * | Oui (ForwardAuth) | backoffice:5001 |
| `/hubs/**` | WS | Non* | realtime:5002 |
| `/realtime/health` | GET | Non | realtime:5002 |

*JWT géré par le RealtimeService via query parameter.

## Structure du Projet

```
.
├── .env                        # Variables d'environnement (gitignored)
├── docker-compose.yml
├── traefik/
│   ├── traefik.yml             # Config statique Traefik (entrypoints, provider)
│   └── dynamic/
│       └── config.yml          # Config dynamique (routers, middlewares, services)
├── services/
│   ├── backoffice/             # API REST + auth JWT (ASP.NET Core :5001)
│   └── realtime/               # SignalR hub (ASP.NET Core :5002)
├── client/                     # Frontend React + TypeScript + Tailwind
└── agents/                     # Scripts Python de simulation
    ├── seed_generation/        # Seeding initial de la BDD (one-shot)
    └── line_agent/             # Simulation continue des lignes de production
```

## Démarrage

### Prérequis

- Docker + Docker Compose

### Configuration

```bash
# Copier et adapter le fichier d'environnement
cp .env.example .env  # puis éditer JWT_SECRET_KEY, mots de passe, etc.
```

Variables clés dans `.env` :

| Variable | Description |
|---|---|
| `POSTGRES_PASSWORD` | Mot de passe PostgreSQL |
| `JWT_SECRET_KEY` | Clé secrète de signature JWT (min. 32 caractères) |
| `CORS_ALLOWED_ORIGINS` | Origines autorisées (ex: `http://localhost:3000`) |
| `GATEWAY_PORT` | Port d'écoute de Traefik (défaut: `5050`) |

### Lancement

```bash
# Tout démarrer
docker-compose up --build

# Services seuls (dev)
docker-compose up --build gateway backoffice realtime

# Base de données uniquement
docker-compose up postgres pgadmin
```

## Schéma de Base de Données

| Table | Colonnes principales |
|---|---|
| `"Users"` | id, username, password_hash, role, created_at |
| `"Roles"` | role_id, name |
| `"Lines"` | id, name, equipment, of_en_cours, of_suivant, is_changement |
| `"Equipments"` | id, name |
| `"Tags"` | tag_id, name, unit |
| `"Historian"` | id, tag_name (FK Tags), timestamp, value |
| `"Equipment_Tag"` | equipment (FK), tag_name (FK) |
| `ofs` | id, name, quantity |


## Auteurs

- Cyprien.G
- Tristan.G
