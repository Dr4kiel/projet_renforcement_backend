"""Simulateur de capteur de vitesse."""

import random

from line_agent.simulation.base import BaseSimulator

# Paramètres de simulation
NOMINAL = 1500.0
MIN_VALUE = 1200.0
MAX_VALUE = 1800.0
NOISE_SCALE = 20.0
RECALL_FORCE = 0.08
ANOMALY_PROBABILITY = 0.02
ANOMALY_AMPLITUDE = 100.0
CHANGEOVER_DECAY = 0.25
RAMP_UP_RATE = 0.1


class SpeedSimulator(BaseSimulator):
    """Simule un capteur de vitesse (RPM).

    En fonctionnement normal : marche aléatoire autour de 1500 RPM (1200-1800).
    En changement de série : chute rapide vers 0, remontée progressive à la reprise.
    """

    def _initial_value(self) -> float:
        return NOMINAL + random.uniform(-50.0, 50.0)

    def generate(self, is_changement: bool) -> float:
        if is_changement:
            self._current_value *= (1.0 - CHANGEOVER_DECAY)
            self._current_value += random.gauss(0, 2.0)
            self._current_value = max(self._current_value, 0.0)
        else:
            if self._current_value < MIN_VALUE:
                self._current_value += (NOMINAL - self._current_value) * RAMP_UP_RATE
                self._current_value += random.gauss(0, 5.0)
            else:
                noise = random.gauss(0, NOISE_SCALE)
                recall = (NOMINAL - self._current_value) * RECALL_FORCE
                self._current_value += noise + recall

                if random.random() < ANOMALY_PROBABILITY:
                    self._current_value += random.choice([-1, 1]) * ANOMALY_AMPLITUDE

            self._current_value = min(self._current_value, MAX_VALUE)

        return round(self._current_value, 1)
