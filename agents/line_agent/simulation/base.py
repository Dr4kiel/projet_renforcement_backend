"""Classe abstraite pour les simulateurs de capteurs."""

from abc import ABC, abstractmethod


class BaseSimulator(ABC):
    """Interface commune pour tous les simulateurs de capteurs."""

    def __init__(self, tag_name: str):
        self.tag_name = tag_name
        self._current_value = self._initial_value()

    @abstractmethod
    def _initial_value(self) -> float:
        """Retourne la valeur initiale du capteur."""

    @abstractmethod
    def generate(self, is_changement: bool) -> float:
        """Génère la prochaine valeur du capteur.

        Args:
            is_changement: True si la ligne est en changement de série.

        Returns:
            La valeur simulée du capteur.
        """
