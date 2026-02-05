"""Modèles de données pour l'agent de simulation."""

from dataclasses import dataclass, field
from datetime import datetime


@dataclass
class TagInfo:
    """Informations sur un tag capteur."""

    tag_id: int
    tag_name: str

    @property
    def prefix(self) -> str:
        """Extrait le préfixe du tag_name (ex: TEMP_MACHINE_A -> TEMP)."""
        return self.tag_name.split("_")[0] if "_" in self.tag_name else self.tag_name


@dataclass
class OfInfo:
    """Informations sur un ordre de fabrication."""

    of_id: int
    of_name: str
    produit: str
    qte_produite: int
    qte_totale: int

    @property
    def is_complete(self) -> bool:
        """Vérifie si l'OF est terminé."""
        return self.qte_produite >= self.qte_totale


@dataclass
class LineInfo:
    """Informations complètes sur une ligne de production."""

    line_id: int
    name: str
    is_changement: bool
    temps_changement: int
    equipment_id: int
    of_en_cours: OfInfo | None
    of_suivant: OfInfo | None
    tags: list[TagInfo] = field(default_factory=list)


@dataclass
class SensorReading:
    """Lecture d'un capteur à écrire dans l'historique."""

    tag_id: int
    timestamp: datetime
    value: float
