"""Simulateur de capteur de température."""

import random

from line_agent.simulation.base import BaseSimulator

# Paramètres de simulation
NOMINAL = 75.0
MIN_VALUE = 60.0
MAX_VALUE = 90.0
AMBIENT = 22.0
NOISE_SCALE = 1.5
RECALL_FORCE = 0.1
ANOMALY_PROBABILITY = 0.02
ANOMALY_AMPLITUDE = 8.0
CHANGEOVER_DECAY = 0.15


class TemperatureSimulator(BaseSimulator):
    """Simule un capteur de température (°C).

    En fonctionnement normal : marche aléatoire autour de 75°C (60-90°C).
    En changement de série : décroissance progressive vers la température ambiante (22°C).
    """

    def _initial_value(self) -> float:
        return NOMINAL + random.uniform(-3.0, 3.0)

    def generate(self, is_changement: bool) -> float:
        if is_changement:
            self._current_value += (AMBIENT - self._current_value) * CHANGEOVER_DECAY
            self._current_value += random.gauss(0, 0.3)
        else:
            noise = random.gauss(0, NOISE_SCALE)
            recall = (NOMINAL - self._current_value) * RECALL_FORCE
            self._current_value += noise + recall

            if random.random() < ANOMALY_PROBABILITY:
                self._current_value += random.choice([-1, 1]) * ANOMALY_AMPLITUDE

        self._current_value = max(min(self._current_value, MAX_VALUE), MIN_VALUE if not is_changement else AMBIENT)
        return round(self._current_value, 2)
