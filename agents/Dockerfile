FROM python:3.11-slim

WORKDIR /app

# Copier les fichiers de requirements
COPY requirements.txt .

# Installer les dépendances
RUN pip install --no-cache-dir -r requirements.txt

# Copier tout le dossier seed_generation avec ses fichiers
COPY seed_generation/ ./seed_generation/

# Rendre le script exécutable
RUN chmod +x seed_generation/seed_data.py

# Exécuter le script
CMD ["python", "seed_generation/seed_data.py"]