"""Consommation des commandes de contrôle depuis RabbitMQ."""

from __future__ import annotations

import json
import logging
import threading
from typing import TYPE_CHECKING

import pika

from line_agent.config import RabbitMQConfig
from line_agent.rabbitmq.publisher import RabbitMQPublisher

if TYPE_CHECKING:
    from line_agent.agent import LineAgent

logger = logging.getLogger(__name__)

COMMANDS_EXCHANGE = "agent.commands"


class RabbitMQCommandConsumer:
    """Consomme les commandes start/stop depuis l'exchange agent.commands (topic).

    Routing key attendue : line.<line_id>
    """

    def __init__(
        self,
        config: RabbitMQConfig,
        agents: dict[int, LineAgent],
        publisher: RabbitMQPublisher,
        stop_event: threading.Event,
    ) -> None:
        self._config = config
        self._agents = agents
        self._publisher = publisher
        self._stop_event = stop_event

    def run(self) -> None:
        """Boucle principale du consumer - à exécuter dans un thread dédié."""
        while not self._stop_event.is_set():
            try:
                self._connect_and_consume()
            except Exception:
                if self._stop_event.is_set():
                    break
                logger.warning(
                    "Connexion RabbitMQ perdue, nouvelle tentative dans 5s...",
                    exc_info=True,
                )
                self._stop_event.wait(5)

        logger.info("Consumer RabbitMQ arrêté")

    def _connect_and_consume(self) -> None:
        """Se connecte à RabbitMQ et démarre la consommation des messages."""
        params = pika.ConnectionParameters(
            host=self._config.host,
            port=self._config.port,
            credentials=pika.PlainCredentials(self._config.username, self._config.password),
            heartbeat=60,
        )
        connection = pika.BlockingConnection(params)
        channel = connection.channel()

        channel.exchange_declare(
            exchange=COMMANDS_EXCHANGE,
            exchange_type="topic",
            durable=True,
        )

        # Queue exclusive auto-delete : liée au cycle de vie de ce process
        result = channel.queue_declare(queue="", exclusive=True, auto_delete=True)
        queue_name = result.method.queue

        # Reçoit les commandes pour toutes les lignes
        channel.queue_bind(
            exchange=COMMANDS_EXCHANGE,
            queue=queue_name,
            routing_key="line.*",
        )

        channel.basic_consume(
            queue=queue_name,
            on_message_callback=self._on_message,
            auto_ack=True,
        )

        logger.info(
            "Consumer RabbitMQ connecté à %s:%d, en attente de commandes...",
            self._config.host,
            self._config.port,
        )

        # Boucle de consommation avec vérification du stop_event
        while not self._stop_event.is_set():
            connection.process_data_events(time_limit=1.0)

        connection.close()

    def _on_message(self, ch, method, properties, body: bytes) -> None:
        """Traite un message de commande reçu."""
        try:
            data = json.loads(body.decode())
            line_id = int(data.get("line_id", 0))
            command = str(data.get("command", "")).lower()

            agent = self._agents.get(line_id)
            if agent is None:
                logger.warning(
                    "Commande '%s' ignorée : ligne %d inconnue (lignes actives: %s)",
                    command, line_id, list(self._agents.keys()),
                )
                return

            if command == "start":
                agent.resume()
                self._publisher.publish_status(line_id, "running", True)
            elif command == "stop":
                agent.pause()
                self._publisher.publish_status(line_id, "stopped", True)
            else:
                logger.warning("Commande inconnue '%s' pour ligne %d", command, line_id)

        except Exception:
            logger.exception("Erreur lors du traitement d'une commande RabbitMQ")
