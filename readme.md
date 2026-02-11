# Launch Instractions

## Docker Setup

1. **Install docker**

    [Instructions here](https://www.docker.com/get-started)

2. **Clone repository**
    
    ```cmd
    git clone https://github.com/aChainsmoker/InnoShop.git
    cd InnoShop/

    ```
3. **Setup MailService**

    For a fast setup all files with database passwords and etc are pushed here, but to make MailService work You should insert your email credentials into **appsettings.Docker.json** (or **appsettings.Development.json** if You start locally)

    **EmailSettings** section should look like

        "EmailSettings": {
            "FromName": "InnoShop",
            "FromEmail": "hello@yandex.ru",
            "ToName": "Recipient",
            "ClientHost": "smtp.yandex.ru",
            "ClientPort": 465,
            "ClientLogin": "hello@yandex.ru",
            "ClientPassword": "qwerty123",
            "UseSsl": true
        }

4. **Build and run**

    ```cmd
    docker compose up -d --build
    ```

5. **Acess SwaggerUI**

    Open [localhost:5186](http://localhost:5186/swagger/index.html) for **ProductService**
    Open [localhost:5085](http://localhost:5085/swagger/index.html) for **UserService**

**or**

5. **Acess Next.js Frontend**

    Open [localhost:3000](http://localhost:3000)

(6). **Default Roles and User**

    If You run app in Development or Docker environment it automatically creates an admin-user with credentials being:

    "Email": admin@admin.com
    "Password": admin123

        and two default roles: "Regular" and "Admin"


## Functionality

### User Service

The **User Service** is responsible for managing users and their accounts.

It provides the following capabilities:

- Basic CRUD-opeartions
- Authentication and Authorization via JWT-token
- Password recovery mechanism using Email and secure tokens
- Account confirmation mechanism using Email and secure tokens
- SoftDelete mechanism in case you dont want to completely erase data about user

- DTO validation was implementen using **FluentValidation**
- Mapping entities was implemented using **Automapper**
- Messaging to other services was implemented using **MassTransit** and **RabbitMq** technologies

### Product Service

The **Product Service** is responsible for managing products in the system.

It provides the following capabilities:

- Basic CRUD-opeartions
- Searching products by the name
- Filtering products by multiple parameters

- DTO validation was implementen using **FluentValidation**
- Mapping entities was implemented using **Automapper**
- Messaging to other services was implemented using **MassTransit** and **RabbitMq** technologies

### Mail Service

The **Mail Service** is implemented as a separate microservice dedicated to email delivery.

Its responsibilities include:

- Sending emails triggered by other services
- Handling email-related logic independently from business services
- Asynchronous communication via a message broker

- Messaging to other services was implemented using **MassTransit** and **RabbitMq** technologies

### API Gateway

The **API Gateway** acts as a single entry point to the backend system.

Main chracteristics:

- Implements a **Reverse Proxy**
- Built using **YARP (Yet Another Reverse Proxy)**
- Routes incoming requests to the appropriate backend services
- Simplifies client-side communication with multiple services

### Frontend


The frontend application is responsible for user interaction and UI rendering.

<sup> It does not implement full functionality of backend API so for full capabilities of the system better adress SwaggerUI with all endpoints. </sup>

Technologies used:

- **TypeScript**
- **React**
- **Next.js**

It features:

- Component-based architecture
- Usage of ready-made UI components from the **Mantine** library
- Interaction with backend services via HTTP APIs