# Copilot Instructions for Docker Game Kadai

## Overview
This project is a game server built using Flask and MySQL, containerized with Docker. The architecture consists of a Flask application that interacts with a MySQL database, with data stored in JSON format. The server is designed to handle game logic and data management.

## Architecture
- **Main Components**:
  - **Flask Application**: Located in the `server/` directory, this is the core of the application.
  - **Database**: MySQL is used for data storage, configured through environment variables.
  - **Docker**: The application is containerized using Docker, with configurations in `Dockerfile` and `docker-compose.yml`.

- **Data Flow**:
  - The Flask app communicates with the MySQL database to retrieve and store game data.
  - JSON data files are used for initial data loading and are located in the `json_data/` directory.

## Developer Workflows
- **Building and Running**:
  - Use `docker-compose up` to build and start the application. This command will set up the Flask server and the MySQL database.

- **Testing**:
  - Ensure that the database is initialized before running tests. Use the `init_db()` function in `app.py` to set up the database schema.

- **Debugging**:
  - Logs can be viewed in the terminal where the Docker container is running. Ensure to check for any initialization errors in the database.

## Project Conventions
- **Environment Variables**: Configuration for the database connection is managed through environment variables defined in `docker-compose.yml`.
- **File Structure**: Follow the existing directory structure for adding new features or components. Place new JSON data files in the `json_data/` directory.

## Integration Points
- **External Dependencies**: The project relies on Flask and MySQL. Ensure that the required packages are listed in `requirements.txt` and installed in the Docker container.
- **Cross-Component Communication**: The Flask app communicates with the MySQL database using the `mysql.connector` library, with connection pooling managed in `models.py`.

## Key Files
- **`Dockerfile`**: Defines the Docker image for the application.
- **`docker-compose.yml`**: Manages the multi-container setup for the application and database.
- **`server/app.py`**: Main application logic and API endpoints.
- **`server/models.py`**: Database connection and pooling logic.
- **`initialize.sql`**: SQL script for initializing the database schema.

---

This document serves as a guide for AI coding agents to understand the structure and workflows of the Docker Game Kadai project. Please provide feedback on any unclear or incomplete sections for further iteration.