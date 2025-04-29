```md
# API – User Management System

## Overview
This project provides an API for managing user information, including creating, reading, updating, and deleting user data. It is built with **ASP.NET Core** and uses a **PostgreSQL** database.

## Features
- Create users (with or without a school class)
- Retrieve user information by UID or name
- Update user details by ID
- Delete users by UID
- Test API via Swagger UI

## Prerequisites
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [PostgreSQL](https://www.postgresql.org/download/)
- [Npgsql](https://www.npgsql.org/) – .NET data provider for PostgreSQL
- [DBeaver](https://dbeaver.io/) – optional, for viewing the database

## Setup

### Database Configuration
Make sure PostgreSQL is running. You can use **DBeaver** to explore the database.

Sample connection details for DBeaver:
```
Host: server-ip
Port: 5432
Database: db_name
User: user_name
Password: db_password
```

### Running the API

1. Connect to the server via SSH:
   ```bash
   ssh username@server-ip
   ```

2. Start the latest version of the application:
   ```bash
   start-server
   ```
   This command fetches the newest version from GitHub and automatically starts the container.

## Usage

### Swagger UI
Once running, you can test endpoints using Swagger UI:
```
http://server-name:server-port/swagger/index.html
```

## Preview

Here’s a look at the Swagger UI interface provided for testing API endpoints:

![Swagger UI Screenshot](images/swagger-ui.png)

## API Endpoints

### User Management

#### Create User With Class
- **POST** `/api/user/create/with-class`


#### Create User Without Class
- **POST** `/api/user/create/without-class`

#### Get User by UID
- **GET** `/api/user/read/by-uid`

#### Get User by Name
- **GET** `/api/user/read/by-name`

#### Update User by ID
- **PUT** `/api/user/{id}`


#### Delete User by UID
- **DELETE** `/api/user/delete/{uid}`

## Contributing
This project was developed by students at **IT-HTL Ybbs** and is intended for educational use only. Contributions are limited to students of the institution.

## License
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Contact
For questions or support, contact: [sz-laeuft@sz-ybbs.ac.at](mailto:sz-laeuft@sz-ybbs.ac.at)
```

---
