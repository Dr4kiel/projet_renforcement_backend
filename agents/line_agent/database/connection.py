"""Gestion de la connexion à la base de données PostgreSQL."""

import logging
import time

import psycopg2

from line_agent.config import DatabaseConfig

logger = logging.getLogger(__name__)


def wait_for_database(db_config: DatabaseConfig, max_retries: int = 30) -> None:
    """Attend que PostgreSQL soit prêt avant de continuer.

    Raises:
        ConnectionError: Si la base de données n'est pas disponible après max_retries tentatives.
    """
    for attempt in range(1, max_retries + 1):
        try:
            conn = psycopg2.connect(
                host=db_config.host,
                port=db_config.port,
                database=db_config.database,
                user=db_config.user,
                password=db_config.password,
            )
            conn.close()
            logger.info("Base de données prête")
            return
        except psycopg2.OperationalError:
            logger.debug("Tentative %d/%d - base de données non disponible", attempt, max_retries)
            time.sleep(2)

    raise ConnectionError(
        f"Impossible de se connecter à la base de données après {max_retries} tentatives"
    )


def create_connection(db_config: DatabaseConfig) -> psycopg2.extensions.connection:
    """Crée et retourne une connexion psycopg2."""
    conn = psycopg2.connect(
        host=db_config.host,
        port=db_config.port,
        database=db_config.database,
        user=db_config.user,
        password=db_config.password,
    )
    conn.autocommit = False
    return conn
