"""Orchestrateur principal de l'agent de simulation de ligne."""

import logging
import random
import threading
from datetime import datetime, timezone

from line_agent.config import AgentConfig, DatabaseConfig
from line_agent.database.connection import create_connection
from line_agent.database.repository import LineAgentRepository
from line_agent.models import LineInfo, SensorReading
from line_agent.simulation.base import BaseSimulator
from line_agent.simulation.registry import get_simulator

logger = logging.getLogger(__name__)


class LineAgent:
    """Agent de simulation pour une ligne de production unique.

    Gère la boucle principale : génération de données capteurs,
    progression de la production, et changements de série.
    """

    def __init__(
        self,
        line_id: int,
        db_config: DatabaseConfig,
        agent_config: AgentConfig,
        stop_event: threading.Event,
    ):
        self.line_id = line_id
        self._db_config = db_config
        self._agent_config = agent_config
        self._stop_event = stop_event
        self._simulators: dict[int, BaseSimulator] = {}
        self._repo: LineAgentRepository | None = None
        self._line_info: LineInfo | None = None

    def run(self) -> None:
        """Boucle principale de l'agent."""
        try:
            self._initialize()
            logger.info(
                "Agent démarré pour ligne %d (%s) - %d tags",
                self.line_id,
                self._line_info.name,
                len(self._line_info.tags),
            )

            while not self._stop_event.is_set():
                self._tick()
                self._stop_event.wait(self._agent_config.interval_seconds)

        except Exception:
            logger.exception("Erreur fatale dans l'agent pour ligne %d", self.line_id)
        finally:
            if self._repo:
                self._repo.close()
                logger.info("Agent arrêté pour ligne %d", self.line_id)

    def _initialize(self) -> None:
        """Initialise la connexion DB, charge les infos de la ligne et crée les simulateurs."""
        conn = create_connection(self._db_config)
        self._repo = LineAgentRepository(conn)
        self._refresh_line_info()

        for tag in self._line_info.tags:
            try:
                self._simulators[tag.tag_id] = get_simulator(tag.tag_name)
            except ValueError:
                logger.warning("Tag non supporté ignoré : %s", tag.tag_name)

    def _refresh_line_info(self) -> None:
        """Recharge les informations de la ligne depuis la base de données."""
        self._line_info = self._repo.get_line_info(self.line_id)
        if self._line_info is None:
            raise ValueError(f"Ligne {self.line_id} introuvable en base de données")

    def _tick(self) -> None:
        """Exécute un cycle de simulation complet."""
        try:
            self._refresh_line_info()

            readings = self._tick_sensors()
            if readings:
                self._repo.insert_historian_readings_batch(readings)

            if not self._line_info.is_changement:
                self._tick_production()
                self._check_changeover_trigger()
            else:
                self._tick_changeover()

            self._repo.commit()

        except Exception:
            logger.exception("Erreur lors du tick pour ligne %d", self.line_id)

    def _tick_sensors(self) -> list[SensorReading]:
        """Génère une lecture pour chaque capteur."""
        now = datetime.now(timezone.utc)
        readings = []

        for tag_id, simulator in self._simulators.items():
            value = simulator.generate(self._line_info.is_changement)
            readings.append(SensorReading(tag_id=tag_id, timestamp=now, value=value))

        if readings:
            tag_values = ", ".join(
                f"{self._get_tag_name(r.tag_id)}={r.value}" for r in readings
            )
            logger.debug("Ligne %d - Capteurs : %s", self.line_id, tag_values)

        return readings

    def _tick_production(self) -> None:
        """Incrémente la production de l'OF en cours."""
        of_en_cours = self._line_info.of_en_cours
        if of_en_cours is None:
            return

        if of_en_cours.is_complete:
            logger.info(
                "Ligne %d - OF %s terminé (%d/%d), déclenchement changement de série",
                self.line_id,
                of_en_cours.of_name,
                of_en_cours.qte_produite,
                of_en_cours.qte_totale,
            )
            self._start_changeover()
            return

        new_qty = self._repo.increment_production(
            of_en_cours.of_id, self._agent_config.production_increment
        )
        logger.debug(
            "Ligne %d - Production %s : %d/%d",
            self.line_id,
            of_en_cours.of_name,
            new_qty,
            of_en_cours.qte_totale,
        )

    def _check_changeover_trigger(self) -> None:
        """Vérifie si un changement de série aléatoire doit être déclenché."""
        if random.random() < self._agent_config.changeover_probability:
            logger.info(
                "Ligne %d - Changement de série aléatoire déclenché",
                self.line_id,
            )
            self._start_changeover()

    def _start_changeover(self) -> None:
        """Démarre un changement de série."""
        self._repo.start_changeover(self.line_id, self._agent_config.changeover_duration)

    def _tick_changeover(self) -> None:
        """Gère le décompte du changement de série."""
        remaining = self._repo.decrement_changeover_time(self.line_id, 1)
        logger.debug(
            "Ligne %d - Changement de série en cours, temps restant : %d min",
            self.line_id,
            remaining,
        )

        if remaining <= 0:
            self._end_changeover()

    def _end_changeover(self) -> None:
        """Termine le changement de série et avance l'OF."""
        self._repo.end_changeover(self.line_id)
        if self._line_info.of_suivant:
            self._repo.advance_to_next_of(self.line_id)
            logger.info(
                "Ligne %d - Nouvel OF en cours : %s",
                self.line_id,
                self._line_info.of_suivant.of_name,
            )
        else:
            logger.info(
                "Ligne %d - Changement terminé, pas d'OF suivant",
                self.line_id,
            )

    def _get_tag_name(self, tag_id: int) -> str:
        """Retourne le nom du tag pour un tag_id donné."""
        for tag in self._line_info.tags:
            if tag.tag_id == tag_id:
                return tag.tag_name
        return f"tag_{tag_id}"
