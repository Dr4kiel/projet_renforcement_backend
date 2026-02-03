FROM python:3.11-slim

WORKDIR /app

# Copier les fichiers de requirements
COPY requirements.txt .

# Installer les dépendances
RUN pip install --no-cache-dir -r requirements.txt

# Copier le script et les fichiers de config
COPY seed_data.py .
COPY config/ ./config/

# Rendre le script exécutable
RUN chmod +x seed_data.py

# Exécuter le script
CMD ["python", "seed_data.py"]
