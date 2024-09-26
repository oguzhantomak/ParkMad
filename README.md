# ParkMad - Parking Management System

ParkMad is a microservice-based parking management system that handles operations such as vehicle registration, parking duration tracking, and dynamic pricing calculations.

## Project Structure

The project follows a microservice architecture for modularity and scalability:

- **Parking.API**: Handles parking operations like registering and unregistering vehicles.
- **Pricing.API**: Manages dynamic pricing calculations based on parking zones and durations.
- **Vehicle.API**: Manages vehicles, getting and setting.
- **Messaging System**: Asynchronous event-based communication using RabbitMQ between Parking and Pricing APIs.

## Design Patterns & Approaches

- **CQRS (Command Query Responsibility Segregation)**: Separates commands (modifications) and queries (data retrieval) for better scalability.
- **Event-Driven Architecture**: Ensures loose coupling between services via message events.
- **Strategy Pattern**: Used in the Pricing.API to handle different pricing strategies based on parking zones.
- **Repository Pattern**: Encapsulates data access logic, promoting maintainability.
- **Dependency Injection**: Ensures loose coupling by injecting dependencies.
- **Unit of Work**: Manages multiple database changes within a transaction.

## Technologies & Libraries & Tools Used

- **.NET Core 8**: Framework for building scalable microservices.
- **Entity Framework Core**: ORM used for database interactions.
- **MassTransit**: Implements RabbitMQ messaging for service communication.
- **RabbitMQ**: Message broker for event-driven architecture.
- **Redis**: Used for caching frequently accessed data to improve performance.
- **PostgreSQL**: Database used by the Pricing.API for persistent storage.
- **MSSQL**: Database used by the Parking.API for persistent storage.
- **Swagger**: Provides API documentation for testing and debugging.
- **AutoMapper**: Maps data between different layers, simplifying DTO and model conversion.
- **Docker**: Containerizes the entire system for consistent environment setup.

## Running the Project

### Prerequisites

- [.NET Core 8 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/products/docker-desktop)

### Running with Docker

1. Clone the repository:

   ```bash
   git clone https://github.com/oguzhantomak/ParkMad.git

2. Navigate to the project directory:

   ```bash
   cd ParkMad
3. Build and start the Docker containers:

   ```bash
   docker-compose up --build

You dont need to migrate the project and databases. Databases will be migrated when the project first runs.

# ParkMad API Endpoints Usage Step Guied

This document provides details on how to use the available API endpoints in ParkMad for managing parking and pricing operations.

## 1. Vehicle API

#### Used to register the vehicle in the database. While the vehicle is being registered, a service in Parking.API listens for the registration of the vehicle and register it to the most suitable parking spot.
#### This is the first step in our case.

```http
  POST https://localhost:6060/vehicles
```

| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `PlateNumber` | `string` | **Required**. License plate number. Ex: 34ABC123 |
| `VehicleSize` | `string` | **Required**. Car vehicle size |

| VehicleSize | Description                |
| :--------  | :------------------------- |
| `"Small"` | For small cars |
| `"Medium"` | For medium cars |
| `"Large"` | For large cars |



## Usage/Example - cURL

```cURL
curl --location 'http://localhost:5003/vehicleservice/api/vehicle' \
--header 'Content-Type: application/json' \
--data '{
    "LicensePlate": "34ABC123",
    "VehicleSize": "Medium"
}'
```

## Response

```cURL
{
    "response": {
        "spotId": 2,
        "zoneName": "Zone B",
        "assignedAt": "2024-09-26T18:53:23.9035745Z",
        "plateNumber": "34ABC123"
    }
}
```


--------------------------

#### Get the vehicle by plate number

```http
  GET http://localhost:6000/vehicles/39VYM573
```

| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `PlateNumber` | `string` | **Required**. License plate number. Ex: 39VYM573 |

## Usage/Example - cURL

```cURL
curl --location 'http://localhost:6000/vehicles/39VYM573'
```

## Response

```cURL
{
    "vehicle": {
        "id": "ec5fde79-c9c2-4568-8c17-564a00970074",
        "plateNumber": "39VYM573",
        "vehicleSize": 2
    }
}
```

-------------------------------

## 2. Parking API

#### Unregisters a vehicle and sending an event and triggers parking fee calculation.

```http
  POST https://localhost:6061/api/parking/unassign
```

| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `PlateNumber` | `string` | **Required**. License plate number. Ex: 34ABC123 |



## Usage/Example - cURL

```cURL
curl --location 'https://localhost:6061/api/parking/unassign' \
--header 'Content-Type: application/json' \
--data '{
  "PlateNumber": "34ABC123"
}'
```

## Response

```cURL
{
    "price": 553.8569755805550,
    "totalHours": 11.077139511611112
}
```


--------------------------
