"""Registre des simulateurs de capteurs."""

from line_agent.simulation.base import BaseSimulator
from line_agent.simulation.pressure import PressureSimulator
from line_agent.simulation.speed import SpeedSimulator
from line_agent.simulation.temperature import TemperatureSimulator
from line_agent.simulation.vibration import VibrationSimulator

SIMULATOR_REGISTRY: dict[str, type[BaseSimulator]] = {
    "TEMP": TemperatureSimulator,
    "SPEED": SpeedSimulator,
    "PRESSURE": PressureSimulator,
    "VIBRATION": VibrationSimulator,
}


def get_simulator(tag_name: str) -> BaseSimulator:
    """Instancie le simulateur correspondant au tag_name.

    Extrait le préfixe (partie avant le premier '_') et cherche dans le registre.

    Raises:
        ValueError: Si le préfixe n'est pas reconnu.
    """
    prefix = tag_name.split("_")[0] if "_" in tag_name else tag_name
    simulator_class = SIMULATOR_REGISTRY.get(prefix)

    if simulator_class is None:
        raise ValueError(
            f"Préfixe de tag inconnu : '{prefix}' (tag_name='{tag_name}'). "
            f"Préfixes supportés : {list(SIMULATOR_REGISTRY.keys())}"
        )

    return simulator_class(tag_name)
