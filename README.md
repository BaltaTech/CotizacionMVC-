# CotizacionMVC

**A Professional HVAC Quotation Management System**

---

## 📋 Overview

CotizacionMVC is a comprehensive web application for the HVAC (Heating, Ventilation, and Air Conditioning) industry. It streamlines the entire quotation lifecycle—from client registration and equipment selection to PDF generation and sales pipeline management.

Built with **Domain-Driven Design (DDD)** , **Clean Architecture**, and **SOLID principles**, this system demonstrates enterprise-grade software development with a strong focus on maintainability, scalability, and business logic encapsulation.

---

## 🎯 Key Features

| Feature | Description |
|---------|-------------|
| **Quotation Management** | Create, edit, and track commercial quotes |
| **Equipment Catalog** | Manage HVAC equipment with MXN/USD pricing support |
| **Client Management** | Centralized registry with multi-contact support |
| **Sales Pipeline** | Lead management, follow-ups, and opportunity tracking |
| **Multi-Company** | Configurable profit margins per company |
| **PDF Generation** | Professional documents with corporate branding |
| **Role-Based Access** | Administrator, Seller, Receptionist roles |
| **Real-Time Notifications** | SignalR-powered alerts |
| **Dashboards** | Receptionist and seller performance dashboards |

---

## 🏗️ Architecture Overview

### Clean Architecture + DDD

The system follows a layered architecture with clear separation of concerns:

| Layer | Responsibility |
|-------|----------------|
| **Presentation** | MVC Controllers, Razor Views, ViewModels, SignalR Hubs |
| **Application** | Services, DTOs, Interfaces, Dependency Injection |
| **Domain** | Entities, Value Objects, Business Rules, Strategy Pattern |
| **Infrastructure** | Repositories, EF Core, PostgreSQL, Background Services |

### Architecture Diagrams

| Diagram | Description |
|---------|-------------|
| ![Arquitectura General](./docs/diagramas/01-arquitectura-general.puml.png) | High-level system overview with actors and external systems |
| ![Capas y Dependencias](./docs/diagramas/02-capas-dependencias.puml.png) | Clean Architecture layer separation and dependencies |
| ![Dominio y Relaciones](./docs/diagramas/03-dominio-relaciones.puml.png) | Rich domain model with entities, value objects, and relationships |
| ![API REST y JWT](./docs/diagramas/04-api-jwt.puml.png) | REST API structure, JWT authentication flow, and endpoints |

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

| Brand | Formula |
|-------|---------|
| **Trane** | `Price (USD) = Base Price × 0.31 × 1.18` |
| **Hisense / TCL** | `Price (MXN) = Base Price (list price)` |
| **Other Brands** | `Price (MXN) = Base Price × (1 + Company Profit %)` |

### Total Calculation
Equipment Subtotal (USD) + City Surcharge % → Total Equipment (USD)
Total Equipment (USD) → Convert to MXN

Installations (MXN)
= Subtotal (MXN)

16% VAT
= Final Total (MXN)

text

---

## 📊 Business Rules

| Entity | Rule |
|--------|------|
| **Equipment** | Currency restricted by brand (Trane/York → USD, Hisense/TCL → MXN) |
| **Equipment** | Capacity required (`CapacidadToneladas > 0`) |
| **Equipment** | Complete details required (Type, Voltage, Technology) |
| **Client** | Contact required (phone, mobile, or email) |
| **Client** | Address required |
| **Quotation** | Valid area required (`AreaMetrosCuadrados > 0`) |
| **Quotation** | Linear state progression (no backward transitions) |

---

## 🚀 Getting Started

### Prerequisites

- .NET 8 SDK
- PostgreSQL 15+

### Installation

```bash
# Clone the repository
git clone https://github.com/BaltaTech/CotizacionMVC-.git
cd CotizacionMVC

# Restore dependencies
dotnet restore

# Update database
dotnet ef database update

# Run the application
dotnet run
Default Login
text
Email: admin@empresa.com
Password: Admin123!
API Documentation
Once running, access Swagger at:

text
https://localhost:7271/swagger/index.html
📄 Documentation
Document	Location
UML Diagrams	./docs/diagramas/
PlantUML Source	./docs/uml/
API Documentation	/swagger (local)
🧪 Testing
bash
# Run all tests
dotnet test

# Run only unit tests
dotnet test --filter "Category=Unit"
👤 Author
Airey Baltazar

GitHub: @BaltaTech

LinkedIn: linkedin.com/in/...

📝 License
MIT License

"Enterprise-grade software development with Domain-Driven Design, Clean Architecture, and SOLID principles."

text

---

## Instrucciones

1. **Reemplaza** el contenido de tu `README.md` con el texto de arriba
2. **Guarda** el archivo
3. **Sube** los cambios:

```bash
git add README.md
git commit -m "docs: Update README with professional format and correct image names"
git push origin main
