"""Publication des statuts d'agent vers RabbitMQ."""

import json
import logging
import threading
from datetime import datetime, timezone

import pika

from line_agent.config import RabbitMQConfig

logger = logging.getLogger(__name__)

STATUS_EXCHANGE = "agent.status"


class RabbitMQPublisher:
    """Publie les statuts des agents sur l'exchange agent.status (fanout)."""

    def __init__(self, config: RabbitMQConfig) -> None:
        self._config = config
        self._connection: pika.BlockingConnection | None = None
        self._channel: pika.adapters.blocking_connection.BlockingChannel | None = None
        self._lock = threading.Lock()

    def connect(self) -> None:
        """Ouvre la connexion et déclare l'exchange."""
        params = pika.ConnectionParameters(
            host=self._config.host,
            port=self._config.port,
            credentials=pika.PlainCredentials(self._config.username, self._config.password),
            heartbeat=60,
        )
        self._connection = pika.BlockingConnection(params)
        self._channel = self._connection.channel()
        self._channel.exchange_declare(
            exchange=STATUS_EXCHANGE,
            exchange_type="fanout",
            durable=True,
        )
        logger.info("Publisher RabbitMQ connecté à %s:%d", self._config.host, self._config.port)

    def publish_status(
        self,
        line_id: int,
        status: str,
        success: bool,
        message: str | None = None,
    ) -> None:
        """Publie un statut d'agent sur l'exchange agent.status."""
        payload = json.dumps({
            "line_id": str(line_id),
            "status": status,
            "success": success,
            "message": message,
            "timestamp": datetime.now(timezone.utc).isoformat(),
        }).encode()

        with self._lock:
            try:
                self._channel.basic_publish(
                    exchange=STATUS_EXCHANGE,
                    routing_key="",
                    body=payload,
                    properties=pika.BasicProperties(
                        content_type="application/json",
                        delivery_mode=1,
                    ),
                )
                logger.debug("Statut publié : ligne %d -> %s", line_id, status)
            except Exception:
                logger.exception("Erreur lors de la publication du statut pour ligne %d", line_id)

    def close(self) -> None:
        """Ferme la connexion."""
        try:
            if self._connection and not self._connection.is_closed:
                self._connection.close()
        except Exception:
            pass
