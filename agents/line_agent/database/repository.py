"""Repository d'accès aux données pour l'agent de ligne."""

import logging

import psycopg2.extensions
from psycopg2.extras import execute_values

from line_agent.models import LineInfo, OfInfo, SensorReading, TagInfo

logger = logging.getLogger(__name__)


class LineAgentRepository:
    """Encapsule toutes les requêtes SQL de l'agent."""

    def __init__(self, conn: psycopg2.extensions.connection):
        self._conn = conn

    def get_line_info(self, line_id: int) -> LineInfo | None:
        """Charge les informations complètes d'une ligne de production."""
        cursor = self._conn.cursor()
        cursor.execute(
            """
            SELECT l.line_id, l.name, l.is_changement, l.temps_changement,
                   l.equipment, l.of_en_cours, l.of_suivant
            FROM "Lines" l
            WHERE l.line_id = %s
            """,
            (line_id,),
        )
        row = cursor.fetchone()
        cursor.close()
        if row is None:
            return None

        line_id_, name, is_changement, temps_changement, equipment_id, of_en_cours_id, of_suivant_id = row

        of_en_cours = self.get_of(of_en_cours_id) if of_en_cours_id else None
        of_suivant = self.get_of(of_suivant_id) if of_suivant_id else None
        tags = self.get_tags_for_equipment(equipment_id)

        return LineInfo(
            line_id=line_id_,
            name=name,
            is_changement=is_changement,
            temps_changement=temps_changement,
            equipment_id=equipment_id,
            of_en_cours=of_en_cours,
            of_suivant=of_suivant,
            tags=tags,
        )

    def get_tags_for_equipment(self, equipment_id: int) -> list[TagInfo]:
        """Récupère les tags associés à un équipement via la table de jonction."""
        cursor = self._conn.cursor()
        cursor.execute(
            """
            SELECT t.tag_id, t.tag_name
            FROM "Tags" t
            JOIN "Equipment_Tag" et ON et.tag_name = t.tag_id
            WHERE et.equipment = %s
            """,
            (equipment_id,),
        )
        tags = [TagInfo(tag_id=row[0], tag_name=row[1]) for row in cursor.fetchall()]
        cursor.close()
        return tags

    def get_of(self, of_id: int) -> OfInfo | None:
        """Charge un ordre de fabrication par son ID."""
        cursor = self._conn.cursor()
        cursor.execute(
            "SELECT of_id, of_name, produit, qte_produite, qte_totale FROM ofs WHERE of_id = %s",
            (of_id,),
        )
        row = cursor.fetchone()
        cursor.close()
        if row is None:
            return None

        return OfInfo(
            of_id=row[0],
            of_name=row[1],
            produit=row[2],
            qte_produite=row[3],
            qte_totale=row[4],
        )

    def insert_historian_readings_batch(self, readings: list[SensorReading]) -> None:
        """Insère un lot de lectures capteurs dans la table Historian."""
        if not readings:
            return

        cursor = self._conn.cursor()
        values = [(r.timestamp, r.value, r.tag_id) for r in readings]
        execute_values(
            cursor,
            'INSERT INTO "Historian" (timestamp, value, tag_name) VALUES %s',
            values,
        )
        cursor.close()
        logger.debug("%d lectures insérées dans Historian", len(readings))

    def increment_production(self, of_id: int, increment: int) -> int:
        """Incrémente la quantité produite d'un OF sans dépasser qte_totale.

        Returns:
            La nouvelle valeur de qte_produite.
        """
        cursor = self._conn.cursor()
        cursor.execute(
            """
            UPDATE ofs
            SET qte_produite = LEAST(qte_produite + %s, qte_totale)
            WHERE of_id = %s
            RETURNING qte_produite
            """,
            (increment, of_id),
        )
        result = cursor.fetchone()
        cursor.close()
        return result[0] if result else 0

    def start_changeover(self, line_id: int, duration: int) -> None:
        """Démarre un changement de série sur une ligne."""
        cursor = self._conn.cursor()
        cursor.execute(
            """
            UPDATE "Lines"
            SET is_changement = TRUE, temps_changement = %s
            WHERE line_id = %s
            """,
            (duration, line_id),
        )
        cursor.close()
        logger.info("Changement de série démarré sur ligne %d (durée: %d min)", line_id, duration)

    def end_changeover(self, line_id: int) -> None:
        """Termine un changement de série."""
        cursor = self._conn.cursor()
        cursor.execute(
            """
            UPDATE "Lines"
            SET is_changement = FALSE, temps_changement = 0
            WHERE line_id = %s
            """,
            (line_id,),
        )
        cursor.close()
        logger.info("Changement de série terminé sur ligne %d", line_id)

    def advance_to_next_of(self, line_id: int) -> None:
        """Fait passer of_suivant en of_en_cours et met of_suivant à NULL."""
        cursor = self._conn.cursor()
        cursor.execute(
            """
            UPDATE "Lines"
            SET of_en_cours = of_suivant, of_suivant = NULL
            WHERE line_id = %s
            """,
            (line_id,),
        )
        cursor.close()
        logger.info("Avancement OF sur ligne %d : of_suivant -> of_en_cours", line_id)

    def decrement_changeover_time(self, line_id: int, decrement: int) -> int:
        """Décrémente le temps de changement et retourne la nouvelle valeur."""
        cursor = self._conn.cursor()
        cursor.execute(
            """
            UPDATE "Lines"
            SET temps_changement = GREATEST(temps_changement - %s, 0)
            WHERE line_id = %s
            RETURNING temps_changement
            """,
            (decrement, line_id),
        )
        result = cursor.fetchone()
        cursor.close()
        return result[0] if result else 0

    def commit(self) -> None:
        """Valide la transaction en cours."""
        self._conn.commit()

    def close(self) -> None:
        """Ferme la connexion à la base de données."""
        self._conn.close()
