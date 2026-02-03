# Agents Python - Remplissage de la Base de Données

Ce dossier contient les scripts Python pour remplir automatiquement la base de données PostgreSQL avec des données initiales.

## Structure

```
agents/
├── seed_data.py          # Script principal d'insertion
├── requirements.txt      # Dépendances Python
├── Dockerfile           # Dockerfile pour containerisation
└── config/              # Fichiers JSON de configuration
    ├── roles.json
    ├── users.json
    ├── equipments.json
    ├── tags.json
    ├── ofs.json
    ├── lines.json
    ├── historian.json
    └── equipment_tags.json
```

## Configuration des Données

Modifiez les fichiers JSON dans le dossier `config/` pour définir les données à insérer :

### 1. **roles.json** - Rôles utilisateurs
```json
[
  { "name": "Admin" },
  { "name": "Operator" }
]
```

### 2. **users.json** - Utilisateurs
```json
[
  {
    "id": 1,
    "identifiant": "admin",
    "password": "admin123",
    "email": "admin@production.com",
    "created_at": "2024-01-01T00:00:00",
    "role": 1
  }
]
```

### 3. **equipments.json** - Équipements
```json
[
  { "name": "Machine A" }
]
```

### 4. **tags.json** - Tags de capteurs
```json
[
  { "tag_name": "TEMP_MACHINE_A" }
]
```

### 5. **ofs.json** - Ordres de fabrication
```json
[
  {
    "of_": "OF-2024-001",
    "produit": "Produit A",
    "qte_produite": 150,
    "qte_totale": 1000
  }
]
```

### 6. **lines.json** - Lignes de production
```json
[
  {
    "name": "Ligne 1",
    "is_changement": false,
    "temps_changement": 0,
    "equipment": 1,
    "of_suivant": 2,
    "of_en_cours": 1
  }
]
```

### 7. **historian.json** - Données historiques
```json
[
  {
    "timestamp_": "2024-01-15T10:00:00",
    "value_": 75.5,
    "tag_name": 1
  }
]
```

### 8. **equipment_tags.json** - Association équipement-tag
```json
[
  {
    "equipment": 1,
    "tag_name": 1
  }
]
```

## Ordre d'Insertion

Le script insère les données dans cet ordre pour respecter les contraintes de clés étrangères :

1. Roles
2. Users
3. Equipments
4. Tags
5. OFs
6. Lines
7. Historian
8. Equipment_Tag

## Utilisation

### Avec Docker Compose (recommandé)

```bash
# Lancer tous les services y compris les agents
docker-compose up --build

# Lancer uniquement les agents
docker-compose up --build agents
```

Le conteneur `agents` s'exécute **une seule fois** puis s'arrête automatiquement.

### En local (pour tests)

```bash
cd agents

# Installer les dépendances
pip install -r requirements.txt

# Définir les variables d'environnement
export DB_HOST=localhost
export DB_PORT=5432
export DB_NAME=production_dashboard
export DB_USER=postgres
export DB_PASSWORD=postgres

# Exécuter le script
python seed_data.py
```

## Variables d'Environnement

| Variable | Description | Valeur par défaut |
|----------|-------------|-------------------|
| `DB_HOST` | Hôte PostgreSQL | `postgres` |
| `DB_PORT` | Port PostgreSQL | `5432` |
| `DB_NAME` | Nom de la base | `production_dashboard` |
| `DB_USER` | Utilisateur | `postgres` |
| `DB_PASSWORD` | Mot de passe | `postgres` |

## Fonctionnalités

- ✅ Attend que PostgreSQL soit prêt avant d'insérer
- ✅ Gère les conflits avec `ON CONFLICT DO NOTHING` (idempotent)
- ✅ Logs détaillés de l'insertion
- ✅ Rollback automatique en cas d'erreur
- ✅ Support des fichiers JSON vides ou manquants
- ✅ Validation des données JSON

## Notes Importantes

- Les IDs dans les fichiers JSON doivent correspondre aux clés étrangères
- Les fichiers JSON manquants sont ignorés (liste vide utilisée)
- Le script est idempotent : relancer plusieurs fois n'insère pas de doublons
- Les erreurs d'insertion sont loggées mais ne bloquent pas le processus complet
