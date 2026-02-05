"""Simulateur de capteur de pression."""

import random

from line_agent.simulation.base import BaseSimulator

# Paramètres de simulation
NOMINAL = 6.0
MIN_VALUE = 4.0
MAX_VALUE = 8.0
ATMOSPHERIC = 1.0
NOISE_SCALE = 0.15
RECALL_FORCE = 0.1
ANOMALY_PROBABILITY = 0.02
ANOMALY_AMPLITUDE = 1.0
CHANGEOVER_DECAY = 0.12


class PressureSimulator(BaseSimulator):
    """Simule un capteur de pression (bar).

    En fonctionnement normal : marche aléatoire autour de 6.0 bar (4.0-8.0).
    En changement de série : purge progressive vers la pression atmosphérique (1.0 bar).
    """

    def _initial_value(self) -> float:
        return NOMINAL + random.uniform(-0.5, 0.5)

    def generate(self, is_changement: bool) -> float:
        if is_changement:
            self._current_value += (ATMOSPHERIC - self._current_value) * CHANGEOVER_DECAY
            self._current_value += random.gauss(0, 0.05)
            self._current_value = max(self._current_value, ATMOSPHERIC)
        else:
            if self._current_value < MIN_VALUE:
                self._current_value += (NOMINAL - self._current_value) * 0.15
                self._current_value += random.gauss(0, 0.05)
            else:
                noise = random.gauss(0, NOISE_SCALE)
                recall = (NOMINAL - self._current_value) * RECALL_FORCE
                self._current_value += noise + recall

                if random.random() < ANOMALY_PROBABILITY:
                    self._current_value += random.choice([-1, 1]) * ANOMALY_AMPLITUDE

            self._current_value = max(min(self._current_value, MAX_VALUE), MIN_VALUE)

        return round(self._current_value, 3)
