"""Point d'entrée principal de l'agent de simulation."""

import argparse
import logging
import signal
import sys
import threading

from line_agent.agent import LineAgent
from line_agent.config import load_agent_config, load_database_config
from line_agent.database.connection import wait_for_database


def parse_args() -> argparse.Namespace:
    """Parse les arguments de la ligne de commande."""
    parser = argparse.ArgumentParser(
        description="Agent de simulation de ligne de production"
    )
    parser.add_argument(
        "--line-ids",
        type=int,
        nargs="+",
        help="IDs des lignes de production à simuler",
    )
    parser.add_argument(
        "--interval",
        type=float,
        default=None,
        help="Intervalle entre les ticks (secondes)",
    )
    parser.add_argument(
        "--log-level",
        choices=["DEBUG", "INFO", "WARNING", "ERROR"],
        default=None,
        help="Niveau de log",
    )
    return parser.parse_args()


def setup_logging(level: str) -> None:
    """Configure le logging."""
    logging.basicConfig(
        level=getattr(logging, level),
        format="%(asctime)s [%(levelname)s] %(name)s - %(message)s",
        datefmt="%Y-%m-%d %H:%M:%S",
    )


def main() -> None:
    """Fonction principale : parse les arguments, lance les agents."""
    args = parse_args()

    db_config = load_database_config()
    agent_config = load_agent_config(args.line_ids)

    if args.interval is not None:
        agent_config = agent_config.__class__(
            line_ids=agent_config.line_ids,
            interval_seconds=args.interval,
            production_increment=agent_config.production_increment,
            changeover_probability=agent_config.changeover_probability,
            changeover_duration=agent_config.changeover_duration,
            log_level=agent_config.log_level,
        )

    log_level = args.log_level or agent_config.log_level
    setup_logging(log_level)

    logger = logging.getLogger(__name__)

    if not agent_config.line_ids:
        logger.error("Aucun line_id spécifié. Utilisez --line-ids ou AGENT_LINE_IDS.")
        sys.exit(1)

    logger.info("Configuration DB : %s:%d/%s", db_config.host, db_config.port, db_config.database)
    logger.info("Lignes à simuler : %s", agent_config.line_ids)
    logger.info("Intervalle : %.1f s", agent_config.interval_seconds)

    wait_for_database(db_config)

    stop_event = threading.Event()
    threads: list[threading.Thread] = []

    def signal_handler(signum: int, _frame) -> None:
        sig_name = signal.Signals(signum).name
        logger.info("Signal %s reçu, arrêt en cours...", sig_name)
        stop_event.set()

    signal.signal(signal.SIGINT, signal_handler)
    signal.signal(signal.SIGTERM, signal_handler)

    for line_id in agent_config.line_ids:
        agent = LineAgent(line_id, db_config, agent_config, stop_event)
        thread = threading.Thread(
            target=agent.run,
            name=f"line-agent-{line_id}",
            daemon=True,
        )
        thread.start()
        threads.append(thread)
        logger.info("Thread démarré pour ligne %d", line_id)

    try:
        for thread in threads:
            while thread.is_alive():
                thread.join(timeout=1.0)
    except KeyboardInterrupt:
        logger.info("Interruption clavier, arrêt en cours...")
        stop_event.set()
        for thread in threads:
            thread.join(timeout=5.0)

    logger.info("Tous les agents arrêtés")


if __name__ == "__main__":
    main()
