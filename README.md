# CotizacionMVC

**A Professional HVAC Quotation Management System**

---

## 🚀 Overview

CotizacionMVC is a comprehensive web application designed for the HVAC (Heating, Ventilation, and Air Conditioning) industry. It streamlines the entire quotation lifecycle—from client registration and equipment selection to PDF generation and sales pipeline management.

Built with **Domain-Driven Design (DDD)**, **Clean Architecture**, and **SOLID principles**, this system demonstrates enterprise-grade software development with a strong focus on maintainability, scalability, and business logic encapsulation.

---

## 🎯 Key Features

### Core Business Features
- **Quotation Management** – Create, edit, and track commercial quotes.
- **Equipment Catalog** – Manage HVAC equipment inventory with support for MXN/USD pricing.
- **Client Management** – Centralized client registry with multi-contact support.
- **Sales Pipeline** – Lead management, follow-ups, and opportunity tracking.
- **Multi-Company Support** – Configurable profit margins per company.
- **PDF Generation** – Professional documents with corporate branding.
- **Role-Based Access Control** – Administrator, Seller, Receptionist.
- **Real-Time Notifications** – SignalR-powered alerts.
- **Dashboards** – Receptionist and seller performance dashboards.

### Technical Highlights
- **Rich Domain Model** – Business logic encapsulated within entities, not services.
- **Value Objects** – Immutable concepts like `Money`, `Contact`, `Address`.
- **Strategy Pattern** – Price calculation per brand (Trane, York, Hisense, TCL, Standard).
- **Repository Pattern** – Abstraction of the persistence layer.
- **Dependency Injection** – Native .NET IoC container.
- **Background Services** – Automated reminders and scheduled tasks.

---

## 🛠️ Technology Stack

| Layer | Technology |
|-------|------------|
| **Backend** | .NET 8, ASP.NET Core MVC, Entity Framework Core, PostgreSQL |
| **Authentication** | ASP.NET Core Identity, Role-Based Authorization |
| **Real-Time** | SignalR |
| **Document Generation** | QuestPDF |
| **Frontend** | Razor Views, Bootstrap 5, jQuery, Font Awesome |
| **Testing** | xUnit, FluentAssertions, Moq |
| **Architecture** | Clean Architecture, DDD, SOLID |

---

## 🏗️ Architecture Overview

### Clean Architecture + DDD
┌─────────────────────────────────────────────────────────────┐
│ PRESENTATION LAYER │
│ (Controllers, Views, ViewModels) │
│ │
│ MVC Controllers │ Razor Views │ SignalR Hubs │
└─────────────────────────────────────────────────────────────┘
│
▼
┌─────────────────────────────────────────────────────────────┐
│ APPLICATION LAYER │
│ (Services, DTOs, Interfaces) │
│ │
│ Application Services │ Dependency Injection │
└─────────────────────────────────────────────────────────────┘
│
▼
┌─────────────────────────────────────────────────────────────┐
│ DOMAIN LAYER │
│ (Entities, Value Objects, Rules) │
│ │
│ Rich Entities │ Value Objects │ Business Rules │
│ OCP Compliant │ Encapsulated │ Strategy Pattern │
└─────────────────────────────────────────────────────────────┘
│
▼
┌─────────────────────────────────────────────────────────────┐
│ INFRASTRUCTURE LAYER │
│ (Repositories, Data, External Services) │
│ │
│ Repository Pattern │ EF Core │ PostgreSQL │
│ Importers │ Background Services │
└─────────────────────────────────────────────────────────────┘

text

### 📐 Architecture Diagrams

| Diagram | Description |
|---------|-------------|
| [Architecture General](./docs/diagramas/01-arquitectura-general.png) | High-level system overview |
| [Layers & Dependencies](./docs/diagramas/02-capas-dependencias.png) | Clean Architecture layer separation |
| [Domain & Relationships](./docs/diagramas/03-dominio-relaciones.png) | Rich domain model with entities and value objects |
| [API & JWT](./docs/diagramas/04-api-jwt.png) | REST API structure and authentication flow |

---

## 🔐 Authentication & Roles

### Default Credentials

| Email | Password | Role |
|-------|----------|------|
| `admin@empresa.com` | `Admin123!` | Administrator |

### Authorization Policies

| Role | Permissions |
|------|-------------|
| **Administrator** | Full system access |
| **Seller** | Quotation, client, and follow-up management |
| **Receptionist** | Client registration and seller assignment |

---

## 💰 Pricing Engine (Strategy Pattern)

The system uses a **Strategy Pattern** for price calculation per brand:
Final Price = Base Price × Price Factor × Profit Factor

// Trane Equipment
Price (USD) = Base Price × 0.31 × 1.18
Price (MXN) = Price (USD) × Exchange Rate

// Hisense / TCL Equipment
Price (MXN) = Base Price (list price)
Price (USD) = Price (MXN) / Exchange Rate

// Other Brands
Price (MXN) = Base Price × (1 + Company Profit %)
Price (USD) = Price (MXN) / Exchange Rate

text

### Total Calculation
Equipment Subtotal (USD) + City Surcharge % → Total Equipment (USD)
Total Equipment (USD) → Convert to MXN

Installations (MXN)
= Subtotal (MXN)

16% IVA (VAT)
= Final Total (MXN)

text

---

## 📊 Business Rules

| Entity | Rule | Technical Implementation |
|--------|------|--------------------------|
| **Equipment** | Currency restricted by brand | Trane/York → USD, Hisense/TCL → MXN |
| **Equipment** | Capacity required | `CapacidadToneladas > 0` validation |
| **Equipment** | Complete details required | Type, Voltage, Technology mandatory |
| **Client** | Contact required | Must have phone, mobile, or email |
| **Client** | Address required | Must have registered address |
| **Quotation** | Valid area required | `AreaMetrosCuadrados > 0` |
| **Quotation** | City surcharge | Only applies to Trane equipment |
| **Quotation** | Linear state progression | No backward state transitions allowed |

---

## 📁 Project Structure
CotizacionMVC/
├── Controllers/ # MVC Controllers
│ ├── AutenticacionController.cs
│ ├── ClienteController.cs
│ ├── CotizacionController.cs
│ ├── EmpresaController.cs
│ ├── EquipoController.cs
│ └── ... (13 controllers)
│
├── Models/ # Domain Layer
│ ├── Entidades/ # Rich Entities (DDD)
│ │ ├── Cliente.cs
│ │ ├── Cotizacion.cs
│ │ ├── Equipo.cs
│ │ ├── Lead.cs
│ │ ├── Seguimiento.cs
│ │ └── ... (10 entities)
│ ├── Valor/ # Value Objects
│ │ ├── Dinero.cs
│ │ ├── Contacto.cs
│ │ └── Direccion.cs
│ ├── Enums/ # System Enums
│ │ ├── EstadoCotizacion.cs
│ │ ├── EstadoCliente.cs
│ │ ├── TipoMarca.cs
│ │ └── ... (14 enums)
│ └── Reglas/ # Business Rules
│ ├── ReglasNegocio.cs
│ ├── ICalculadoraPrecio.cs
│ └── CalculadoraPrecioTrane.cs
│
├── Servicios/ # Application Layer
│ ├── Aplicacion/
│ │ ├── Interfaces/ # Service Contracts
│ │ │ ├── IClienteServicio.cs
│ │ │ ├── ICotizacionServicio.cs
│ │ │ └── ... (8 interfaces)
│ │ └── Servicios/ # Service Implementations
│ │ ├── ClienteServicio.cs
│ │ ├── CotizacionServicio.cs
│ │ └── ... (8 services)
│ └── Infraestructura/
│ ├── NotificacionServicio.cs
│ └── RecordatorioBackgroundService.cs
│
├── Data/ # Infrastructure Layer
│ ├── ApplicationDbContext.cs
│ ├── Repositorios/
│ │ ├── Interfaces/ # Repository Contracts
│ │ └── Implementaciones/ # Repository Implementations
│ │ ├── BaseRepository.cs
│ │ ├── ClienteRepository.cs
│ │ └── ... (7 repositories)
│ └── Importadores/
│ ├── ImportadorEquipos.cs
│ └── ImportadorInstalaciones.cs
│
├── Views/ # Razor Views (.cshtml)
├── Hubs/ # SignalR Hubs
├── Tests/ # Unit & Integration Tests
├── Program.cs # Entry Point
└── appsettings.json # Configuration

text

---

## 🧪 Testing

| Test Type | Coverage |
|-----------|----------|
| **Unit Tests** | Domain entities, Value Objects, Services |
| **Integration Tests** | Repository operations, End-to-end scenarios |

**Tested Scenarios:**
- ✅ Quotation total calculation
- ✅ Equipment capacity validation
- ✅ State transitions
- ✅ Money conversions (USD/MXN)
- ✅ Contact validation
- ✅ Follow-up registration

---

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK
- PostgreSQL 15+
- Visual Studio 2022 / VS Code

### Clone & Run

```bash
# Clone the repository
git clone https://github.com/yourusername/CotizacionMVC.git

# Navigate to the project
cd CotizacionMVC

# Restore dependencies
dotnet restore

# Update database (create PostgreSQL DB)
dotnet ef database update

# Run the application
dotnet run
Default Login
text
Email: admin@empresa.com
Password: Admin123!
📄 Documentation
Document	Description
Architecture Diagrams	UML diagrams (PlantUML)
API Documentation	Swagger/OpenAPI (runs locally)
🔧 Development Workflow
bash
# Create a feature branch
git checkout -b feature/new-feature

# Commit with descriptive message
git commit -m "feat: Add new calculation rule"

# Push and create Pull Request
git push origin feature/new-feature
📝 License
This project is for portfolio demonstration purposes.

👤 Author
Airey Baltazar
GitHub • LinkedIn

📊 Key Metrics
Architecture: Clean Architecture + DDD

Patterns: Repository, DI, Strategy, Unit of Work, Background Service

SOLID: ✅ All five principles applied

Testing: ✅ Unit + Integration tests

Documentation: ✅ Swagger + UML diagrams

Security: ✅ JWT + Role-based authorization

"This project demonstrates enterprise-grade software development with a focus on domain-driven design, clean architecture, and maintainable code."

text

---

## Subir a GitHub

```bash
# 1. Agregar todos los archivos
git add docs/
git add README.md

# 2. Commit
git commit -m "docs: Add professional README and UML diagrams

- Added README.md in English with project overview
- Added 4 key UML diagrams:
  - Architecture General
  - Layers & Dependencies
  - Domain & Relationships
  - API & JWT
- Added documentation for technology stack, architecture, and business rules"

# 3. Push
git push origin feature/jwt-implementation
