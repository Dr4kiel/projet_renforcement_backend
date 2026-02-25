#!/usr/bin/env python3
"""
Script de remplissage de la base de données PostgreSQL
Lit les fichiers JSON de configuration et insère les données dans la BDD
"""
import json
import os
import sys
import time
from datetime import datetime
from pathlib import Path

import psycopg2


def wait_for_db(host, port, database, user, password, max_retries=30):
    """Attend que PostgreSQL soit prêt"""
    for _i in range(max_retries):
        try:
            conn = psycopg2.connect(
                host=host,
                port=port,
                database=database,
                user=user,
                password=password
            )
            conn.close()
            print("Base de données prête !")
            return True
        except psycopg2.OperationalError:
            time.sleep(2)

    print("Impossible de se connecter à la base de données")
    return False


def check_if_data_exists(cursor):
    """Vérifie si des données existent déjà dans la table Roles"""
    try:
        cursor.execute('SELECT COUNT(*) FROM "Roles"')
        count = cursor.fetchone()[0]
        return count > 0
    except Exception as e:
        print(f"Erreur lors de la vérification des données existantes: {e}")
        return False


def load_json_file(filepath):
    """Charge un fichier JSON"""
    try:
        if not os.path.exists(filepath):
            print(f"Fichier {filepath} non trouvé, utilisation d'une liste vide")
            return []

        with open(filepath, encoding='utf-8') as f:
            data = json.load(f)
            print(f"{filepath}: {len(data)} enregistrements chargés")
            return data
    except json.JSONDecodeError as e:
        print(f"Erreur de parsing JSON dans {filepath}: {e}")
        return []
    except Exception as e:
        print(f"Erreur lors du chargement de {filepath}: {e}")
        return []


def insert_roles(cursor, roles_data):
    """Insert roles into Roles table and return id mapping by index (1-based)"""
    role_id_mapping = {}
    if not roles_data:
        return role_id_mapping

    print("\nInsertion des rôles...")
    for index, role in enumerate(roles_data, start=1):
        try:
            role_name = role.get('name')
            cursor.execute(
                'INSERT INTO "Roles" (name) VALUES (%s) ON CONFLICT DO NOTHING',
                (role_name,)
            )
            # Récupérer l'id du rôle
            cursor.execute(
                'SELECT role_id FROM "Roles" WHERE name = %s',
                (role_name,)
            )
            result = cursor.fetchone()
            if result:
                role_id_mapping[index] = result[0]
        except Exception as e:
            print(f"Erreur lors de l'insertion du rôle {role}: {e}")
    print(f"{len(roles_data)} rôles traités")
    return role_id_mapping


def insert_users(cursor, users_data, role_id_mapping):
    """Insert users into Users table using role id mapping"""
    if not users_data:
        return

    print("\nInsertion des utilisateurs...")
    for user in users_data:
        try:
            json_role_id = user.get('role')
            real_role_id = role_id_mapping.get(json_role_id)

            if real_role_id is None:
                print(f"Rôle id={json_role_id} non trouvé pour {user.get('identifiant')}, utilisateur ignoré")
                continue

            cursor.execute(
                '''INSERT INTO "Users" (identifiant, password, email, created_at, role)
                   VALUES (%s, %s, %s, %s, %s) ON CONFLICT DO NOTHING''',
                (
                    user.get('identifiant'),
                    user.get('password'),
                    user.get('email'),
                    user.get('created_at', datetime.utcnow().isoformat()),
                    real_role_id
                )
            )
        except Exception as e:
            print(f"Erreur lors de l'insertion de l'utilisateur {user.get('identifiant')}: {e}")
    print(f"{len(users_data)} utilisateurs traités")


def insert_equipments(cursor, equipments_data):
    """Insert equipments into Equipments table and return id mapping"""
    equipment_id_mapping = {}
    if not equipments_data:
        return equipment_id_mapping

    print("\nInsertion des équipements...")
    for index, equipment in enumerate(equipments_data, start=1):
        try:
            equipment_name = equipment.get('name')
            cursor.execute(
                'INSERT INTO "Equipments" (name) VALUES (%s) ON CONFLICT DO NOTHING',
                (equipment_name,)
            )
            # Récupérer l'id de l'équipement
            cursor.execute(
                'SELECT equipment_id FROM "Equipments" WHERE name = %s',
                (equipment_name,)
            )
            result = cursor.fetchone()
            if result:
                equipment_id_mapping[index] = result[0]
        except Exception as e:
            print(f"Erreur lors de l'insertion de l'équipement {equipment}: {e}")
    print(f"{len(equipments_data)} équipements traités")
    return equipment_id_mapping


def insert_tags(cursor, tags_data):
    """Insert tags into Tags table and return id mapping"""
    tag_id_mapping = {}
    tag_name_to_id = {}
    if not tags_data:
        return tag_id_mapping, tag_name_to_id

    print("\nInsertion des tags...")
    for index, tag in enumerate(tags_data, start=1):
        try:
            tag_name = tag.get('tag_name')
            cursor.execute(
                'INSERT INTO "Tags" (tag_name) VALUES (%s) ON CONFLICT DO NOTHING',
                (tag_name,)
            )
            # Récupérer l'id du tag
            cursor.execute(
                'SELECT tag_id FROM "Tags" WHERE tag_name = %s',
                (tag_name,)
            )
            result = cursor.fetchone()
            if result:
                tag_id_mapping[index] = result[0]
                tag_name_to_id[tag_name] = result[0]
        except Exception as e:
            print(f"Erreur lors de l'insertion du tag {tag}: {e}")
    print(f"{len(tags_data)} tags traités")
    return tag_id_mapping, tag_name_to_id


def insert_ofs(cursor, ofs_data):
    """Insert production orders into ofs table and return id mapping"""
    of_id_mapping = {}
    if not ofs_data:
        return of_id_mapping

    print("\nInsertion des ordres de fabrication...")
    for index, of in enumerate(ofs_data, start=1):
        try:
            of_name = of.get('of_')
            cursor.execute(
                """INSERT INTO ofs (of_name, produit, qte_produite, qte_totale)
                   VALUES (%s, %s, %s, %s) ON CONFLICT DO NOTHING""",
                (
                    of_name,
                    of.get('produit'),
                    of.get('qte_produite', 0),
                    of.get('qte_totale', 0)
                )
            )
            # Récupérer l'id de l'OF
            cursor.execute(
                'SELECT of_id FROM ofs WHERE of_name = %s',
                (of_name,)
            )
            result = cursor.fetchone()
            if result:
                of_id_mapping[index] = result[0]
        except Exception as e:
            print(f"Erreur lors de l'insertion de l'OF {of.get('of_')}: {e}")
    print(f"{len(ofs_data)} ordres de fabrication traités")
    return of_id_mapping


def insert_lines(cursor, lines_data, equipment_id_mapping, of_id_mapping):
    """Insert production lines into Lines table using equipment and OF id mappings"""
    if not lines_data:
        return

    print("\nInsertion des lignes de production...")
    for line in lines_data:
        try:
            json_equipment_id = line.get('equipment')
            real_equipment_id = equipment_id_mapping.get(json_equipment_id)

            json_of_suivant_id = line.get('of_suivant')
            real_of_suivant_id = of_id_mapping.get(json_of_suivant_id) if json_of_suivant_id else None

            json_of_en_cours_id = line.get('of_en_cours')
            real_of_en_cours_id = of_id_mapping.get(json_of_en_cours_id) if json_of_en_cours_id else None

            if real_equipment_id is None:
                continue

            cursor.execute(
                '''INSERT INTO "Lines" (name, is_changement, temps_changement, equipment, of_suivant, of_en_cours)
                   VALUES (%s, %s, %s, %s, %s, %s) ON CONFLICT DO NOTHING''',
                (
                    line.get('name'),
                    line.get('is_changement', False),
                    line.get('temps_changement', 0),
                    real_equipment_id,
                    real_of_suivant_id,
                    real_of_en_cours_id
                )
            )
        except Exception as e:
            print(f"Erreur lors de l'insertion de la ligne {line.get('name')}: {e}")
    print(f"{len(lines_data)} lignes de production traitées")


def insert_equipment_tags(cursor, equipment_tags_data, equipment_id_mapping, tag_id_mapping):
    """Insert equipment-tag associations into Equipment_Tag table using mappings"""
    if not equipment_tags_data:
        return

    print("\nInsertion des associations équipement-tag...")
    for assoc in equipment_tags_data:
        try:
            json_equipment_id = assoc.get('equipment')
            real_equipment_id = equipment_id_mapping.get(json_equipment_id)

            json_tag_id = assoc.get('tag_name')
            real_tag_id = tag_id_mapping.get(json_tag_id)

            if real_equipment_id is None or real_tag_id is None:
                continue

            cursor.execute(
                '''INSERT INTO "Equipment_Tag" (equipment, tag_name)
                   VALUES (%s, %s) ON CONFLICT DO NOTHING''',
                (
                    real_equipment_id,
                    real_tag_id
                )
            )
        except Exception as e:
            print(f"Erreur lors de l'insertion de l'association: {e}")
    print(f"{len(equipment_tags_data)} associations traitées")


def main():
    """Fonction principale"""
    print("=" * 60)
    print("Démarrage du script de remplissage de la base de données")
    print("=" * 60)

    # Configuration de la connexion
    db_config = {
        'host': os.getenv('DB_HOST', 'postgres'),
        'port': int(os.getenv('DB_PORT', 5432)),
        'database': os.getenv('DB_NAME', 'production_dashboard'),
        'user': os.getenv('DB_USER', 'postgres'),
        'password': os.getenv('DB_PASSWORD', 'postgres')
    }

    print("\nConfiguration:")
    print(f"  Host: {db_config['host']}")
    print(f"  Port: {db_config['port']}")
    print(f"  Database: {db_config['database']}")
    print(f"  User: {db_config['user']}")

    if not wait_for_db(**db_config):
        sys.exit(1)

    config_dir = Path(__file__).parent / 'config'

    # Charger les données JSON
    print("\nChargement des fichiers de configuration...")
    roles_data = load_json_file(config_dir / 'roles.json')
    users_data = load_json_file(config_dir / 'users.json')
    equipments_data = load_json_file(config_dir / 'equipments.json')
    tags_data = load_json_file(config_dir / 'tags.json')
    ofs_data = load_json_file(config_dir / 'ofs.json')
    lines_data = load_json_file(config_dir / 'lines.json')
    equipment_tags_data = load_json_file(config_dir / 'equipment_tags.json')

    # Connexion et insertion
    try:
        print("\nConnexion à la base de données...")
        conn = psycopg2.connect(**db_config)
        cursor = conn.cursor()

        print("Connecté avec succès !")

        print("\nVérification de l'état de la base de données...")
        if check_if_data_exists(cursor):
            print("=" * 60)
            print("Des rôles existent déjà dans la base de données.")
            print("Aucune insertion ne sera effectuée pour éviter les doublons.")
            print("=" * 60)
            return

        print("Aucune donnée existante détectée. Démarrage de l'insertion...")

        # Insérer les données dans le bon ordre
        role_id_mapping = insert_roles(cursor, roles_data)
        insert_users(cursor, users_data, role_id_mapping)
        equipment_id_mapping = insert_equipments(cursor, equipments_data)
        tag_id_mapping, tag_name_to_id = insert_tags(cursor, tags_data)
        of_id_mapping = insert_ofs(cursor, ofs_data)
        insert_lines(cursor, lines_data, equipment_id_mapping, of_id_mapping)
        insert_equipment_tags(cursor, equipment_tags_data, equipment_id_mapping, tag_id_mapping)

        # Commit des changements
        conn.commit()
        print("\n" + "=" * 60)
        print("Toutes les données ont été insérées avec succès !")
        print("=" * 60)

    except Exception as e:
        print(f"\nErreur: {e}")
        if conn:
            conn.rollback()
        sys.exit(1)
    finally:
        if cursor:
            cursor.close()
        if conn:
            conn.close()
            print("\nConnexion fermée")


if __name__ == "__main__":
    main()
