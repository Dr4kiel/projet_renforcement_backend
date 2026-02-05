"""Configuration de l'agent de simulation de ligne de production."""

import os
from dataclasses import dataclass


@dataclass(frozen=True)
class DatabaseConfig:
    """Configuration de connexion à la base de données."""

    host: str
    port: int
    database: str
    user: str
    password: str


@dataclass(frozen=True)
class AgentConfig:
    """Configuration de l'agent de simulation."""

    line_ids: list[int]
    interval_seconds: float = 5.0
    production_increment: int = 1
    changeover_probability: float = 0.02
    changeover_duration: int = 30
    log_level: str = "INFO"


def load_database_config() -> DatabaseConfig:
    """Charge la configuration de la base de données depuis les variables d'environnement."""
    return DatabaseConfig(
        host=os.getenv("DB_HOST", "localhost"),
        port=int(os.getenv("DB_PORT", "5432")),
        database=os.getenv("DB_NAME", "production_dashboard"),
        user=os.getenv("DB_USER", "postgres"),
        password=os.getenv("DB_PASSWORD", "postgres"),
    )


def load_agent_config(cli_line_ids: list[int] | None = None) -> AgentConfig:
    """Charge la configuration de l'agent depuis les variables d'environnement et les arguments CLI."""
    line_ids = cli_line_ids or []

    if not line_ids:
        env_ids = os.getenv("AGENT_LINE_IDS", "")
        if env_ids:
            line_ids = [int(x.strip()) for x in env_ids.split(",") if x.strip()]

    return AgentConfig(
        line_ids=line_ids,
        interval_seconds=float(os.getenv("AGENT_INTERVAL", "5.0")),
        production_increment=int(os.getenv("AGENT_PRODUCTION_INCREMENT", "1")),
        changeover_probability=float(os.getenv("AGENT_CHANGEOVER_PROBABILITY", "0")),
        changeover_duration=int(os.getenv("AGENT_CHANGEOVER_DURATION", "30")),
        log_level=os.getenv("AGENT_LOG_LEVEL", "INFO"),
    )
