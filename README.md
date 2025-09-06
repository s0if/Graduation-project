# 🏘️ Aqaraty - Real Estate & Professional Services Platform (Back-End)

This is the back-end implementation of **Aqaraty**: a graduation project developed at **Palestine Polytechnic University**, Faculty of Information Technology and Computer Engineering (2024–2025).

The system aims to connect real estate owners and service providers (electricians, plumbers, engineers...) with users across Palestine using a centralized platform.

---

## 📌 Project Info

- 🎓 Developed by: **Saif Al-Din Komi** (Back-End)
- 👨‍💻 Front-End by: Hazem Amayreh (React.js)
- 🧑‍🏫 Supervised by: Dr. Ezdehar Jawabreh
- 🌐 Live Website: [https://aqaraty.netlify.app](https://aqaraty.netlify.app)

---

## 🛠️ Technologies Used

### 💻 Back-End Stack

- **ASP.NET Core 8.0**
- **Entity Framework Core 8.0**
- **JWT Authentication**
- **Azure Blob Storage**
- **SignalR** (Real-time messaging)
- **Swagger API Docs**
- **Serilog** for logging
- **Mapster** for object mapping
- **RestSharp** for HTTP requests
- **Scalar.AspNetCore** for validations

---

## 📁 Project Structure

```
Graduation/
├── Controllers/           → API endpoints
├── Data/                  → EF Core DbContext
├── DTOs/                  → Data Transfer Objects
├── Error/                 → Global exception handling
├── Helpers/               → Utility classes (e.g., FileSettings)
├── Model/                 → Entity models
├── Service/               → Business logic & services
│   ├── AuthServices.cs
│   ├── ChatHub.cs
│   ├── EmailSetting.cs
│   ├── WhatsAppService.cs
│   └── ExtractClaims.cs
├── appsettings.json       → Configuration & connection strings
├── launchSettings.json    → Project run profiles
└── Program.cs             → Startup & DI
```

---

## 🔌 Database Connection

To run this project locally, you need to have **SQL Server** installed.

Update your connection string in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnections": "Server=YOUR_SERVER_NAME;Database=Graduation;Trusted_Connection=True;TrustServerCertificate=true"
}
```

> 🔁 Replace `YOUR_SERVER_NAME` with your local SQL Server name.  
You can find it in **SQL Server Management Studio (SSMS)** when you connect to your local instance.

---

## 🚀 Running the Project

1. Clone the repo:
   ```bash
   git clone https://github.com/s0if/Graduation-project.git
   ```

2. Update the connection string in `appsettings.json`.

3. Apply EF Core migrations:
   ```bash
   dotnet ef database update
   ```

4. Run the project:
   ```bash
   dotnet run
   ```

5. Visit the Swagger UI at:
   - [http://localhost:5100/swagger](http://localhost:5100/swagger)
   - or as configured in `launchSettings.json`

---

## 📡 API Endpoints

| Method | Endpoint                                 | Description                             |
|--------|------------------------------------------|-----------------------------------------|
| POST   | `/Auth/Register`                         | Register a new user (WhatsApp/Email)    |
| POST   | `/Auth/Login`                            | Login and receive JWT token             |
| GET    | `/ServiceToProject/AllService`           | Get all services                        |
| GET    | `/PropertyToProject/AllProperty`         | Get all properties                      |
| GET    | `/Advertisement/AllAdvertisement`        | Get all ads                             |
| GET    | `/UserOperations/AllUser`                | Admin: View all users                   |
| GET    | `/Complaints/AllComplaint`               | Admin: View all complaints              |

✅ Most endpoints are protected by JWT and role-based policies.

---

## 🔧 Configuration (`launchSettings.json`)

```json
"profiles": {
  "http": {
    "commandName": "Project",
    "launchBrowser": true,
    "launchUrl": "swagger",
    "applicationUrl": "http://localhost:5100",
    "environmentVariables": {
      "ASPNETCORE_ENVIRONMENT": "Development"
    }
  },
  "https": {
    "commandName": "Project",
    "launchBrowser": true,
    "launchUrl": "swagger",
    "applicationUrl": "https://localhost:7017;http://localhost:5100",
    "environmentVariables": {
      "ASPNETCORE_ENVIRONMENT": "Development"
    }
  }
}
```

---

## 📄 Docs

- 🔗 **Swagger UI**: [View Docs](https://lnkd.in/dWNnHrEn)
- 🔗 **Postman Collection**: [API Postman](https://lnkd.in/dvZhYcm8)
- 🔗 **Project Report (Arabic)**: [View Report](https://s0if.github.io/Aqaraty_Graduation_Project_Report_Arabic/)

---

## ✅ Contributions

This repository is part of an academic project. If you wish to reuse it, please cite the authors and supervisor properly.

---

## 🙏 Acknowledgments

Thanks to our university, our supervisor, and all who supported us during the journey.

---

## 📜 License

This project is distributed for academic purposes. Not licensed for commercial use.
