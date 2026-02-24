"""Simulateur de capteur de vibration."""

import random

from line_agent.simulation.base import BaseSimulator

# Paramètres de simulation
NOMINAL = 2.8
MIN_VALUE = 1.0
MAX_VALUE = 4.5
BACKGROUND_NOISE = 0.2
NOISE_SCALE = 0.2
RECALL_FORCE = 0.1
ANOMALY_PROBABILITY = 0.02
ANOMALY_AMPLITUDE = 1.0
CHANGEOVER_DECAY = 0.18


class VibrationSimulator(BaseSimulator):
    """Simule un capteur de vibration (mm/s).

    En fonctionnement normal : marche aléatoire autour de 2.8 mm/s (1.0-4.5).
    En changement de série : chute vers le bruit de fond (0.2 mm/s).
    """

    def _initial_value(self) -> float:
        return NOMINAL + random.uniform(-0.3, 0.3)

    def generate(self, is_changement: bool) -> float:
        if is_changement:
            self._current_value += (BACKGROUND_NOISE - self._current_value) * CHANGEOVER_DECAY
            self._current_value += random.gauss(0, 0.02)
            self._current_value = max(self._current_value, 0.0)
        else:
            if self._current_value < MIN_VALUE:
                self._current_value += (NOMINAL - self._current_value) * 0.12
                self._current_value += random.gauss(0, 0.05)
            else:
                noise = random.gauss(0, NOISE_SCALE)
                recall = (NOMINAL - self._current_value) * RECALL_FORCE
                self._current_value += noise + recall

                if random.random() < ANOMALY_PROBABILITY:
                    self._current_value += random.choice([-1, 1]) * ANOMALY_AMPLITUDE

            self._current_value = max(min(self._current_value, MAX_VALUE), MIN_VALUE)

        return round(self._current_value, 3)
