# Agent de Simulation de Ligne de Production

Agent Python qui simule l'état de lignes de production en temps réel : génération de données capteurs (température, vitesse, pression, vibration), progression de la production, et gestion des changements de série.

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                       main.py                           │
│            CLI, threads, signal handling                │
└────────────────────────┬────────────────────────────────┘
                         │ 1 thread par ligne
                         ▼
┌─────────────────────────────────────────────────────────┐
│                      agent.py                           │
│          LineAgent : boucle tick principale             │
│  ┌───────────┐  ┌───────────────┐  ┌──────────────────┐ │
│  │ Capteurs  │  │  Production   │  │  Changement de   │ │
│  │ (sensors) │  │ (incrémente)  │  │  série (gestion) │ │
│  └─────┬─────┘  └───────┬───────┘  └────────┬─────────┘ │
└────────┼────────────────┼───────────────────┼───────────┘
         │                │                   │
         ▼                ▼                   ▼
┌──────────────────┐  ┌──────────────────────────────────┐
│   simulation/    │  │        database/repository.py    │
│  ┌────────────┐  │  │  ┌──────────────────────────┐    │
│  │ temperature│  │  │  │  LineAgentRepository     │    │
│  │ speed      │  │  │  │  - get_line_info()       │    │
│  │ pressure   │  │  │  │  - insert_historian()    │    │
│  │ vibration  │  │  │  │  - increment_production()│    │
│  └────────────┘  │  │  │  - start/end_changeover()│    │
│  registry.py     │  │  └──────────────────────────┘    │
└──────────────────┘  └──────────────────────────────────┘
                              │
                              ▼
                      ┌──────────────┐
                      │  PostgreSQL  │
                      └──────────────┘
```

## Structure des fichiers

```
line_agent/
├── __init__.py          # Package marker
├── __main__.py          # Exécution via python -m line_agent
├── main.py              # Point d'entrée : CLI, threads, signaux
├── config.py            # Configuration (DatabaseConfig, AgentConfig)
├── models.py            # Modèles de données (TagInfo, OfInfo, LineInfo, SensorReading)
├── agent.py             # Orchestrateur LineAgent
├── database/
│   ├── __init__.py
│   ├── connection.py    # Connexion PostgreSQL (wait + create)
│   └── repository.py    # Requêtes SQL (LineAgentRepository)
├── simulation/
│   ├── __init__.py
│   ├── base.py          # ABC BaseSimulator
│   ├── temperature.py   # Capteur température (°C)
│   ├── speed.py         # Capteur vitesse (RPM)
│   ├── pressure.py      # Capteur pression (bar)
│   ├── vibration.py     # Capteur vibration (mm/s)
│   └── registry.py      # Registre des simulateurs
└── README.md
```

## Utilisation

### Avec Docker (recommandé)

```bash
# Démarrer tous les services
docker-compose up --build

# Ou démarrer uniquement le line-agent (nécessite postgres + agents seed)
docker-compose up line-agent
```

### En local

```bash
# Prérequis : PostgreSQL en cours d'exécution avec le schéma créé et les données seed
cd agents

# Lancer l'agent pour les lignes 1 et 2
python -m line_agent --line-ids 1 2 --interval 2.0 --log-level DEBUG
```

## Arguments CLI

| Argument | Description | Défaut |
|----------|-------------|--------|
| `--line-ids` | IDs des lignes à simuler | Variable env `AGENT_LINE_IDS` |
| `--interval` | Intervalle entre les ticks (secondes) | `5.0` |
| `--log-level` | Niveau de log (`DEBUG`, `INFO`, `WARNING`, `ERROR`) | `INFO` |

## Variables d'environnement

### Base de données

| Variable | Description | Défaut |
|----------|-------------|--------|
| `DB_HOST` | Hôte PostgreSQL | `localhost` |
| `DB_PORT` | Port PostgreSQL | `5432` |
| `DB_NAME` | Nom de la base | `production_dashboard` |
| `DB_USER` | Utilisateur | `postgres` |
| `DB_PASSWORD` | Mot de passe | `postgres` |

### Agent

| Variable | Description | Défaut |
|----------|-------------|--------|
| `AGENT_LINE_IDS` | IDs des lignes (séparés par `,`) | aucun |
| `AGENT_INTERVAL` | Intervalle en secondes | `5.0` |
| `AGENT_PRODUCTION_INCREMENT` | Incrément de production par tick | `1` |
| `AGENT_CHANGEOVER_PROBABILITY` | Probabilité de changement aléatoire | `0` |
| `AGENT_CHANGEOVER_DURATION` | Durée du changement (minutes) | `30` |
| `AGENT_LOG_LEVEL` | Niveau de log | `INFO` |

## Types de capteurs

| Type | Préfixe | Valeur nominale | Plage | Unité | Comportement en changement |
|------|---------|-----------------|-------|-------|---------------------------|
| Température | `TEMP` | 75.0 | 60-90 | °C | Décroissance vers 22°C (ambiant) |
| Vitesse | `SPEED` | 1500.0 | 1200-1800 | RPM | Chute rapide vers 0 |
| Pression | `PRESSURE` | 6.0 | 4.0-8.0 | bar | Purge vers 1.0 (atmosphérique) |
| Vibration | `VIBRATION` | 2.8 | 1.0-4.5 | mm/s | Chute vers 0.2 (bruit de fond) |

Tous les capteurs utilisent un modèle de **marche aléatoire avec force de rappel** vers la valeur nominale, avec des anomalies occasionnelles (probabilité 2%).

## Logique de production

A chaque tick (si la ligne n'est pas en changement de série) :
1. Les valeurs capteurs sont générées et insérées dans `"Historian"`
2. `qte_produite` de l'OF en cours est incrémentée (sans dépasser `qte_totale`)
3. Si l'OF est terminé, un changement de série est déclenché

## Logique de changement de série

Un changement de série peut être déclenché par :
- **OF terminé** : `qte_produite >= qte_totale`
- **Aléatoirement** : probabilité configurable à chaque tick

Pendant le changement :
- Les capteurs passent en mode idle (valeurs convergent vers les valeurs de repos)
- La production est arrêtée
- Le compteur `temps_changement` décrémente à chaque tick

A la fin du changement :
- `of_suivant` devient `of_en_cours`
- `of_suivant` est mis à `NULL`
- Les capteurs reprennent leur comportement nominal

## Extension : ajouter un nouveau type de capteur

1. Créer `simulation/nouveau_type.py` avec une classe héritant de `BaseSimulator`
2. Implémenter `_initial_value()` et `generate(is_changement)`
3. Ajouter l'entrée dans `SIMULATOR_REGISTRY` dans `simulation/registry.py`
4. Ajouter les tags correspondants dans la base de données (table `"Tags"` + `"Equipment_Tag"`)

## Dépannage

**L'agent ne trouve pas la ligne**
- Vérifiez que le seed a bien été exécuté (`docker-compose up agents`)
- Vérifiez les IDs de ligne avec : `SELECT * FROM "Lines";`

**Aucune insertion dans Historian**
- Vérifiez que les tags sont associés à l'équipement de la ligne
- Lancez en mode `--log-level DEBUG` pour voir les détails

**Erreur de connexion à la base**
- En local : `DB_HOST=localhost` (pas `postgres`)
- En Docker : `DB_HOST=postgres`
