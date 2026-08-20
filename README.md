# CotizacionMVC

**A Professional HVAC Quotation Management System**

---

## 🏗️ Architecture Overview

### Clean Architecture + DDD

![Arquitectura General](./docs/diagramas/01-arquitectura-general.png)

### Layers & Dependencies

![Capas y Dependencias](./docs/diagramas/02-capas-dependencias.png)

### Domain & Relationships

![Dominio y Relaciones](./docs/diagramas/03-dominio-relaciones.png)

### API REST & JWT

![API REST y JWT](./docs/diagramas/04-api-jwt.png)

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

| Email | Password | Role |
|-------|----------|------|
| `admin@empresa.com` | `Admin123!` | Administrator |

---

## 💰 Pricing Engine (Strategy Pattern)

The system uses a **Strategy Pattern** for price calculation per brand:

- **Trane:** `Price (USD) = Base Price × 0.31 × 1.18`
- **Hisense / TCL:** `Price (MXN) = Base Price (list price)`
- **Other Brands:** `Price (MXN) = Base Price × (1 + Company Profit %)`

---

## 📊 Business Rules

| Entity | Rule |
|--------|------|
| **Equipment** | Currency restricted by brand (Trane/York → USD, Hisense/TCL → MXN) |
| **Equipment** | Capacity required (`CapacidadToneladas > 0`) |
| **Client** | Contact required (phone, mobile, or email) |
| **Client** | Address required |
| **Quotation** | Valid area required (`AreaMetrosCuadrados > 0`) |
| **Quotation** | Linear state progression (no backward transitions) |

---

## 🚀 Getting Started

```bash
# Clone
git clone https://github.com/yourusername/CotizacionMVC.git

# Restore dependencies
dotnet restore

# Update database
dotnet ef database update

# Run
dotnet run
Default Login: admin@empresa.com / Admin123!

📄 Documentation
UML Diagrams (PlantUML)

Swagger API (runs locally)

👤 Author
Airey Baltazar
GitHub • LinkedIn

"Enterprise-grade software development with Domain-Driven Design, Clean Architecture, and SOLID principles."

text

---

## Resumen de cambios

| Antes | Ahora |
|-------|-------|
| Texto plano mal formateado | Imágenes profesionales de UML |
| Estructura confusa | Diagramas claros y organizados |
| README poco atractivo | README visual y profesional |
| No se ve la arquitectura | Se ve la arquitectura de un vistazo |

---

## Subir a GitHub

```bash
# 1. Generar imágenes PNG desde PlantText
# 2. Guardar en docs/diagramas/

git add docs/diagramas/*.png
git add docs/uml/*.puml
git add README.md

git commit -m "docs: Add UML diagrams as images and professional README"

git push origin feature/jwt-implementation
