FROM python:3.11-slim

WORKDIR /app

# Copier les fichiers de requirements
COPY requirements.txt .

# Installer les dépendances
RUN pip install --no-cache-dir -r requirements.txt

# Copier les modules
COPY seed_generation/ ./seed_generation/
COPY line_agent/ ./line_agent/

# Rendre le script exécutable
RUN chmod +x seed_generation/seed_data.py

# Exécuter le script
CMD ["python", "seed_generation/seed_data.py"]