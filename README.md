# CotizacionMVC

**Enterprise-Grade HVAC Quotation Management System**

---

## 🎯 What Makes This Project Senior-Level?

This isn't just a CRUD application. It demonstrates a deep understanding of enterprise patterns and clean code practices:

| Senior Skill | Application in Project |
| :--- | :--- |
| **Domain-Driven Design** | Rich entities encapsulating business logic, avoiding anemic domain models. |
| **Clean Architecture** | Strict separation of concerns (Presentation → Application → Domain → Infrastructure). |
| **SOLID Principles** | Every principle applied intentionally to ensure maintainability and scalability. |
| **Strategy Pattern** | Pluggable, dynamic pricing engines per brand (Trane, York, Hisense, TCL). |
| **Repository Pattern** | Generic repository base with highly specific implementations. |
| **JWT Authentication** | Stateless, secure authentication flow including refresh tokens. |
| **Background Services** | Automated internal processes for reminders and scheduled tasks. |
| **Real-Time Comm.** | SignalR integration for instant user notifications. |
| **Unit Testing** | Robust coverage utilizing xUnit, FluentAssertions, and Moq. |
| **Documentation** | Interactive Swagger UI and comprehensive UML architecture diagrams. |

> **This project proves the capability to design, build, and document a production-ready system from scratch.**

---

## 📋 Features

### Business Features
* **Quotation Management:** Create, edit, track, and export professional HVAC quotes.
* **Equipment Catalog:** Full CRUD operations with dynamic multi-currency support (MXN/USD).
* **Client Management:** Centralized registry supporting multiple contacts, addresses, and full history.
* **Sales Pipeline:** Lead tracking with structured follow-ups and state transitions.
* **Multi-Company:** Configurable, isolated profit margins, branding, and rules per company.
* **PDF Generation:** Professional document rendering featuring corporate colors and logos.
* **Real-Time Alerts:** SignalR-powered notifications for follow-ups and critical reminders.
* **Role-Based Access:** Tiered access control (Administrator, Seller, Receptionist).

### Technical Features
* **JWT Authentication:** Secure, stateless identity verification.
* **Swagger/OpenAPI:** Interactive and documented API endpoints.
* **Background Jobs:** Automated system triggers (e.g., reminders every 15 minutes).
* **Unit & Integration Tests:** High reliability across domain, services, and repositories.
* **UML Diagrams:** Clear visualization of architecture, layers, domain relationships, and API flows.
* **Clean Architecture:** The core domain remains completely free of external dependencies.

---

## 🏗️ Architecture at a Glance

### Clean Architecture + DDD

| Layer | Responsibility | Key Components |
| :--- | :--- | :--- |
| **Presentation** | User Interface | MVC Controllers, Razor Views, SignalR Hubs |
| **Application** | Use Cases | Services, DTOs, Interfaces |
| **Domain** | Business Logic | Entities, Value Objects, Business Rules, Strategy Pattern |
| **Infrastructure** | Technical Details | Repositories, EF Core, PostgreSQL, Background Services |

### Architecture Diagrams

| Diagram | Description |
| :--- | :--- |
| `01-arquitectura-general.puml.png` | Full system overview with actors and external systems |
| `02-capas-dependencias.puml.png` | Layer separation and dependency flow |
| `03-dominio-relaciones.puml.png` | Rich domain model with entities, value objects, and relationships |
| `04-api-jwt.puml.png` | REST API structure, JWT authentication, and endpoints |

---

## 🔐 Authentication & Authorization

### Default Credentials

| Email | Password | Role |
| :--- | :--- | :--- |
| `admin@empresa.com` | `Admin123!` | Administrator |

### Role-Based Access

| Role | Permissions |
| :--- | :--- |
| **Administrator** | Full system access and configuration |
| **Seller** | Quotations, clients, and follow-up management |
| **Receptionist** | Client registration and seller assignment |

---

## 💰 Pricing Engine

The system uses a **Strategy Pattern** for dynamic price calculation based on the manufacturer:

| Brand | Calculation Formula |
| :--- | :--- |
| **Trane** | `Price (USD) = Base × 0.31 × 1.18` |
| **Hisense / TCL** | `Price (MXN) = Base (list price)` |
| **Other Brands** | `Price (MXN) = Base × (1 + Company Profit %)` |

### Total Calculation Flow

```text
1. Equipment Subtotal (USD) + City Surcharge → Total Equipment (USD)
2. Total Equipment (USD) → Convert to MXN
3. Add Installations (MXN)
--------------------------------------------------
= Subtotal (MXN)
+ 16% VAT
--------------------------------------------------
= Final Total (MXN)
🧠 Business RulesEntityValidation RuleEquipmentCurrency is strictly restricted by brand (Trane/York → USD, Hisense/TCL → MXN).EquipmentCapacity is mandatory (CapacidadToneladas > 0).ClientAt least one valid contact method is required (phone, mobile, or email).ClientA physical address is required.QuotationApplication area must be greater than zero (AreaMetrosCuadrados > 0).QuotationState progression is strictly linear (no backward transitions allowed).🛠️ Technology StackComponentTechnologyBackend.NET 8, ASP.NET Core MVC, Entity Framework Core, PostgreSQLAuthenticationASP.NET Core Identity, JWT, Role-Based AuthorizationReal-TimeSignalRDocument Gen.QuestPDFFrontendRazor Views, Bootstrap 5, jQuery, Font AwesomeTestingxUnit, FluentAssertions, MoqArchitectureClean Architecture, DDD, SOLID📁 Project StructurePlaintextCotizacionMVC/
├── Controllers/
│   ├── MVC/                  # HTML Razor Views Controllers
│   └── API/                  # REST API Controllers (JWT protected)
├── Models/                   # Domain Layer
│   ├── Entidades/            # Rich Entities (DDD)
│   ├── Valor/                # Value Objects
│   ├── Enums/                # System Enums
│   └── Reglas/               # Business Rules + Strategy Pattern Implementations
├── Servicios/                # Application Layer
│   ├── Aplicacion/           # Services + Interfaces
│   └── Infraestructura/      # External Services & Integrations
├── Data/                     # Infrastructure Layer
│   ├── ApplicationDbContext.cs
│   └── Repositorios/         # Repository Pattern Implementations
└── Tests/                    # Unit + Integration Tests
🚀 Quick StartBash# 1. Clone the repository
git clone [https://github.com/BaltaTech/CotizacionMVC-.git](https://github.com/BaltaTech/CotizacionMVC-.git)
cd CotizacionMVC

# 2. Restore dependencies
dotnet restore

# 3. Apply Database Migrations (PostgreSQL required)
dotnet ef database update

# 4. Run the application
dotnet run
Development Access:Login: admin@empresa.com / Admin123!Swagger (API Docs): https://localhost:7271/swagger🧪 TestingBash# Run all tests (Unit & Integration)
dotnet test

# Run unit tests only
dotnet test --filter "Category=Unit"
👤 AuthorAirey BaltazarGitHub: @BaltaTechLinkedIn: linkedin.com/in/...📄 LicenseMIT License
