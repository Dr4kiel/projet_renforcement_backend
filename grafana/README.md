# Grafana Configuration

Ce répertoire contient la configuration Grafana pour le Dashboard de Production en Temps Réel.

## Structure

```
grafana/
├── README.md                           # Ce fichier
├── provisioning/                       # Configuration de provisioning automatique
│   ├── datasources/
│   │   └── postgres.yml               # Configuration de la datasource PostgreSQL
│   └── dashboards/
│       └── dashboard.yml              # Configuration du provisioning des dashboards
└── dashboards/
    └── production-dashboard.json      # Dashboard principal de production
```

## Accès à Grafana

Une fois le projet démarré avec `docker-compose up`, accédez à Grafana via :

- **URL**: http://localhost:3001
- **Username**: admin
- **Password**: admin

## Datasource PostgreSQL

La datasource PostgreSQL est automatiquement configurée au démarrage avec les paramètres suivants :

- **Name**: PostgreSQL Production Dashboard
- **Type**: PostgreSQL
- **Host**: postgres:5432
- **Database**: production_dashboard
- **User**: postgres
- **SSL Mode**: disable

La connexion est établie automatiquement grâce au fichier `provisioning/datasources/postgres.yml`.

## Dashboard Principal

Le dashboard "Dashboard de Production en Temps Réel" est automatiquement chargé et contient :

### Panels

1. **Historian - Données temps réel**
   - Type: Time series
   - Description: Visualisation en temps réel de toutes les données des capteurs
   - Source: Table `historian`
   - Actualisation: Auto (5 secondes)

2. **Lignes de Production Actives**
   - Type: Stat
   - Description: Nombre de lignes de production ayant un OF en cours
   - Requête: `SELECT COUNT(*) FROM lines WHERE current_of_id IS NOT NULL`

3. **Ordres de Fabrication Totaux**
   - Type: Stat
   - Description: Nombre total d'ordres de fabrication dans le système
   - Requête: `SELECT COUNT(*) FROM ofs`

4. **État des Lignes de Production**
   - Type: Table
   - Description: Vue détaillée de chaque ligne avec son équipement et OF actuel
   - Colonnes: ID Ligne, Nom Ligne, Équipement, OF en cours, Quantité cible

5. **Métriques par Tag**
   - Type: Time series
   - Description: Données temps réel groupées par nom de tag
   - Statistiques: Moyenne, Dernière valeur, Maximum
   - Source: Tables `historian` et `tags` (jointure)

6. **Équipements et Tags Associés**
   - Type: Table
   - Description: Liste des équipements avec le nombre de tags associés
   - Colonnes: ID, Nom Équipement, Type, Nombre de Tags

### Paramètres du Dashboard

- **Rafraîchissement**: 5 secondes (automatique)
- **Plage temporelle par défaut**: 6 dernières heures
- **Thème**: Dark
- **Tags**: production, realtime

## Personnalisation

### Modifier le Dashboard Existant

1. Connectez-vous à Grafana (http://localhost:3001)
2. Ouvrez le dashboard "Dashboard de Production en Temps Réel"
3. Cliquez sur l'icône d'engrenage (⚙️) en haut à droite
4. Effectuez vos modifications
5. Cliquez sur "Save dashboard"

**Note**: Les modifications faites dans l'interface sont persistées dans le volume Docker `grafana_data`, mais pour les rendre permanentes dans le code source :

1. Dans Grafana, cliquez sur "Share" puis "Export"
2. Cochez "Export for sharing externally"
3. Cliquez sur "Save to file"
4. Remplacez le contenu de `grafana/dashboards/production-dashboard.json` avec le fichier exporté

### Créer un Nouveau Dashboard

**Option 1 - Via l'interface Grafana** :
1. Cliquez sur "+" → "Create Dashboard"
2. Ajoutez vos panels et configurez vos requêtes
3. Sauvegardez le dashboard
4. Exportez-le (Share → Export → Save to file)
5. Placez le fichier JSON dans `grafana/dashboards/`

**Option 2 - Manuellement** :
1. Créez un nouveau fichier JSON dans `grafana/dashboards/`
2. Utilisez `production-dashboard.json` comme template
3. Redémarrez le service Grafana : `docker-compose restart grafana`

### Ajouter une Nouvelle Datasource

1. Créez un nouveau fichier YAML dans `grafana/provisioning/datasources/`
2. Utilisez le format suivant :

```yaml
apiVersion: 1

datasources:
  - name: Ma Nouvelle Datasource
    type: [postgres|mysql|prometheus|...]
    access: proxy
    url: host:port
    database: database_name
    user: username
    secureJsonData:
      password: password
    jsonData:
      # Options spécifiques au type de datasource
    isDefault: false
    editable: true
```

3. Redémarrez Grafana : `docker-compose restart grafana`

## Requêtes SQL Utiles

Voici quelques exemples de requêtes SQL pour créer vos propres panels :

### Données Historian avec Noms de Tags

```sql
SELECT
  h.timestamp as time,
  h.value,
  t.name as metric
FROM historian h
JOIN tags t ON h.tag_id = t.id
WHERE $__timeFilter(h.timestamp)
ORDER BY h.timestamp
```

### Statistiques par Ligne de Production

```sql
SELECT
  l.name as "Ligne",
  COUNT(h.id) as "Nombre de mesures",
  AVG(h.value) as "Moyenne"
FROM historian h
JOIN equipment_tag et ON h.tag_id = et.tag_id
JOIN equipment e ON et.equipment_id = e.id
JOIN lines l ON l.equipment_id = e.id
WHERE $__timeFilter(h.timestamp)
GROUP BY l.name
```

### Production par OF

```sql
SELECT
  o.name as "OF",
  o.target_quantity as "Objectif",
  COUNT(l.id) as "Lignes actives"
FROM ofs o
LEFT JOIN lines l ON l.current_of_id = o.id
GROUP BY o.id, o.name, o.target_quantity
```

## Variables Grafana

Le dashboard utilise les variables Grafana suivantes :

- `$__timeFilter(column)`: Filtre temporel automatique basé sur la plage sélectionnée
- `time`: Colonne de temps pour les time series (doit être nommée `time` dans le SELECT)
- `metric`: Label pour grouper les séries (optionnel)

## Troubleshooting

### La datasource ne se connecte pas

1. Vérifiez que PostgreSQL est démarré : `docker-compose ps postgres`
2. Vérifiez les logs Grafana : `docker-compose logs grafana`
3. Testez la connexion depuis Grafana UI : Configuration → Data Sources → PostgreSQL Production Dashboard → Save & Test

### Le dashboard ne s'affiche pas

1. Vérifiez que le fichier JSON est valide (utilisez un validateur JSON en ligne)
2. Vérifiez les logs : `docker-compose logs grafana`
3. Rechargez la configuration : `docker-compose restart grafana`

### Aucune donnée n'apparaît

1. Vérifiez que les agents Python ont inséré des données : `docker-compose logs agents`
2. Vérifiez la table historian : `docker exec production-dashboard-db psql -U postgres -d production_dashboard -c "SELECT COUNT(*) FROM historian;"`
3. Ajustez la plage temporelle dans Grafana (en haut à droite)

### Les permissions ne fonctionnent pas

Si vous rencontrez des erreurs de permissions sur les fichiers de configuration :

```bash
# Sur Linux/Mac
chmod -R 755 grafana/

# Sur Windows (depuis PowerShell en admin)
icacls grafana /grant Everyone:F /t
```

## Ressources

- [Documentation Grafana](https://grafana.com/docs/grafana/latest/)
- [PostgreSQL Datasource](https://grafana.com/docs/grafana/latest/datasources/postgres/)
- [Dashboard Provisioning](https://grafana.com/docs/grafana/latest/administration/provisioning/#dashboards)
- [Panel Types](https://grafana.com/docs/grafana/latest/panels-visualizations/)
