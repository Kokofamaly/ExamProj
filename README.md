# ExamProject — Word Cards

Full-stack приложение для изучения английских слов с использованием карточек.

## 🛠 Technologies

### Backend

* ASP.NET Core Web API
* .NET 10
* C#
* MongoDB

### Frontend

* React
* TypeScript
* Node.js 26.8.1
* Vite
* React Router
* TanStack Query

### Infrastructure

* Docker
* Docker Compose

---

## 📋 Requirements

Before running the project, make sure you have:

* .NET 10 SDK
* Node.js 26.8.1
* npm
* MongoDB 7.0
* Docker Desktop (for Docker Compose launch)

---

## 🚀 Running with Docker Compose

The easiest way to run the complete application is Docker Compose.

From the project root directory:

```bash
docker compose up --build
```

After the containers start:

* Frontend: http://localhost:5173
* Backend API: http://localhost:5071
* MongoDB: localhost:27017

To run the containers in the background:

```bash
docker compose up -d
```

To stop the containers:

```bash
docker compose down
```

---

## 🔧 Backend

The backend is an ASP.NET Core Web API application running on .NET 10.

### Run locally

Go to the backend directory:

```bash
cd Api/WordCardsApi
```

Restore dependencies:

```bash
dotnet restore
```

Run the application:

```bash
dotnet run
```

The API will be available on the configured ASP.NET Core URL.

### MongoDB connection

The backend connects to MongoDB using the following configuration:

```text
MongoDb__ConnectionURI=mongodb://mongodb:27017
MongoDb__DatabaseName=WordCardsApi
```

When running through Docker Compose, `mongodb` is the name of the MongoDB service inside the Docker network.

When running the backend locally outside Docker, the MongoDB connection string should point to the MongoDB instance running on the host machine.

---

## 🎨 Frontend

The frontend is a React + TypeScript application built with Vite.

### Run locally

Go to the frontend directory:

```bash
cd React/my-app
```

Install dependencies:

```bash
npm install
```

Run the development server:

```bash
npm run dev
```

The frontend will be available at:

http://localhost:5173

---

## 🗄 MongoDB

MongoDB is used as the application's database.

When using Docker Compose, MongoDB runs in a separate container:

```yaml
mongodb:
  image: mongo:7.0
```

The database data is stored in a named Docker volume:

```yaml
volumes:
  - mongodb_data:/data/db
```

This allows MongoDB data to persist when the container is recreated.

The volume is managed by Docker and is not stored in the Git repository.

---

## 🔐 Environment Variables

The backend uses the following environment variables:

```text
ASPNETCORE_ENVIRONMENT=Development
MongoDb__ConnectionURI=mongodb://mongodb:27017
MongoDb__DatabaseName=WordCardsApi
```

When using Docker Compose, these variables are configured in `compose.yaml`.

---

## 👤 Test User

If user registration is not implemented, use the following test account:

```text
Email: test@gmail.com
Password: 12345
```

---

## 🏗 Architecture

The application consists of three main parts:

```text
React Frontend
       ↓
ASP.NET Core Web API
       ↓
MongoDB
```

### Frontend

The frontend is responsible for the user interface and communication with the backend API.

React Router is used for navigation and protected routes.

TanStack Query is used for server state management and API requests.

### Backend

The backend is implemented using ASP.NET Core Web API.

It is responsible for:

* authentication and authorization;
* business logic;
* communication with MongoDB;
* providing REST API endpoints for the frontend.

### Database

MongoDB is used as a NoSQL database.

The backend communicates with MongoDB through the MongoDB .NET driver.

### Docker

Docker Compose is used to run the frontend, backend and MongoDB as separate services.

Each service has its own container and the services communicate through the Docker Compose network.

MongoDB uses a named volume to persist database data between container restarts.

---

## 🐳 Useful Docker Commands

Start the project:

```bash
docker compose up
```

Start in background:

```bash
docker compose up -d
```

Rebuild images:

```bash
docker compose up --build
```

Stop containers:

```bash
docker compose down
```

Check container status:

```bash
docker compose ps
```

View logs:

```bash
docker compose logs
```

View backend logs:

```bash
docker compose logs backend
```

View MongoDB logs:

```bash
docker compose logs mongodb
```

View Docker volumes:

```bash
docker volume ls
```
