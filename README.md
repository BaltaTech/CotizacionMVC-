# HVAC Quotation System

## 📋 Project Description

A specialized web system for managing and controlling quotations in the HVAC (Heating, Ventilation, and Air Conditioning) sector. The platform enables comprehensive management of clients, equipment, and installations, generating professional quotations with automated calculations for pricing, profit margins, and taxes.

Developed following **Domain-Driven Design (DDD)** principles with rich domain models that encapsulate business logic internally.

---

## 🎯 Key Features

- **Quotation Management:** Creation, structural editing, and tracking of commercial quotations.
- **Equipment Catalog:** Management of HVAC equipment inventory with native support for MXN/USD pricing.
- **Client Management:** Centralized registration with multi-contact support and detailed addresses.
- **Multi-Company Support:** Multi-tenant architecture with configurable profit margins.
- **PDF Generation:** Automatic generation of professional documents with corporate branding using QuestPDF.
- **Access Control:** Secure authentication and role-based authorization (Administrator/Seller) via ASP.NET Core Identity.
- **Automated Calculations:** Pricing engine processing chained profits, VAT, and thermal load suggestions.

---

## 🛠️ Technology Stack

### Backend

| Technology | Purpose |
|------------|---------|
| .NET 8 | Core framework |
| ASP.NET Core MVC | Presentation architecture (Model-View-Controller) |
| Entity Framework Core | ORM for data mapping and access |
| PostgreSQL | Relational database engine |
| ASP.NET Core Identity | User management, sessions, and security policies |

### Frontend

| Technology | Purpose |
|------------|---------|
| Razor Views | Server-side dynamic templating |
| Bootstrap 5 | Responsive CSS framework |
| jQuery | DOM manipulation and async interactions |
| Font Awesome | Vector iconography |

### Document Generation

| Technology | Purpose |
|------------|---------|
| QuestPDF | Advanced PDF design and layout generation |

### Architecture & Patterns

| Pattern | Application |
|---------|-------------|
| **Domain-Driven Design (DDD)** | Rich domain models encapsulating business rules |
| **Value Objects** | Immutability applied to domain concepts (Contact, Address, Money) |
| **Repository Pattern** | Abstraction layer for data persistence decoupling |
| **Dependency Injection** | Native inversion of control for service lifecycle management |

---

## 📁 Project Structure
CotizacionMVC/
├── Controllers/ # MVC Architecture Controllers
│ ├── AutenticacionController.cs
│ ├── ClienteController.cs
│ ├── CotizacionController.cs
│ ├── EmpresaController.cs
│ ├── EquipoController.cs
│ └── UsuariosController.cs
├── Models/ # Domain Models and Logic
│ ├── Entidades/ # Rich Entities with internal logic
│ │ ├── Cliente.cs
│ │ ├── Cotizacion.cs
│ │ ├── Empresa.cs
│ │ ├── Equipo.cs
│ │ ├── Instalacion.cs
│ │ ├── ItemCotizacion.cs
│ │ ├── ItemInstalacion.cs
│ │ ├── Lead.cs
│ │ ├── Seguimiento.cs
│ │ └── Usuario.cs
│ ├── Enums/ # System-wide enumerations
│ ├── Valor/ # Immutable Value Objects
│ │ ├── Contacto.cs
│ │ ├── Dinero.cs
│ │ └── Direccion.cs
│ └── Reglas/ # Business validations and specifications
├── Data/ # Persistence Infrastructure Layer
│ └── ApplicationDbContext.cs
├── Servicios/ # Application and External Infrastructure Services
│ ├── IDocumento.cs
│ └── PdfCotizacion.cs
├── Views/ # Razor Views and Interfaces (.cshtml)
│ ├── Autenticacion/
│ ├── Cliente/
│ ├── Cotizacion/
│ ├── Empresa/
│ ├── Equipo/
│ └── Usuarios/
└── wwwroot/ # Static Application Resources
├── css/
├── js/
├── lib/
└── pdf/cotizaciones/ # Temporary PDF storage directory

text

---

## 🚀 Installation & Setup

### Prerequisites

- .NET 8 SDK
- PostgreSQL (configurable to SQL Server if preferred)
- JetBrains Rider, Visual Studio 2022, or VS Code

### Installation Steps

**1. Clone the repository:**

```bash
git clone https://github.com/BaltaTech/CotizacionMVC-.git
cd CotizacionMVC
2. Configure the database:

Modify the appsettings.json file in the root directory with your local connection string:

json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=CotizacionDB;Username=postgres;Password=your-password"
  }
}
3. Restore dependencies and apply migrations:

bash
# Restore NuGet packages
dotnet restore

# Create and apply the initial database structure
dotnet ef migrations add InitialCreate
dotnet ef database update
4. Run the project:

bash
dotnet run
The application will start on the following ports:

HTTP: http://localhost:5000

HTTPS: https://localhost:5001

🔐 Authentication & Roles
Default Credentials (Seed Data)
Email	Password	Assigned Role
admin@empresa.com	Admin123!	Administrator
Authorization Policies
Role	Permissions
Administrator	Full access to global configurations, catalogs, audits, and user management
Seller	Operational access: equipment viewing, client creation, and quotation issuance
Anonymous User	Restricted access exclusively to the Login portal
📊 Workflow & Business Rules
Quotation Lifecycle
Stage	Description
Client Selection	Direct linking from the master catalog
Equipment Loading	Selection of pre-configured technical items
Installation Services	Optional inclusion of labor and expenses
Price Calculation	Domain core processes the cascade:
Final Price = Base Price × (1 + Company Profit%) × (1 + Seller Profit%)
Taxes & Totals	Automated application of VAT (16%) and base currency conversion
Issuance	Clean PDF compilation ready for commercial delivery
Technical Rules Matrix
Entity / Flow	Business Rule	Technical Description
Equipment	Currency restricted by manufacturer	Trane and York brands are quoted strictly in USD. Other brands in MXN.
Clients	Mandatory Contact	Client registration is not allowed without at least one valid contact method (Email/Phone).
Finance	Numeric Validations	All base costs and list prices must be strictly greater than zero (
>
0
>0).
Evolution	State Machine	Quotation states follow a linear forward flow; backward phase transitions are not permitted.
⚙️ Advanced Configuration
Financial Exchange Rate
The system centralizes conversion rates within the quotation entity to ensure historical transactional consistency:

csharp
// Located in: Models/Entidades/Cotizacion.cs
public decimal ObtenerTipoCambioActual()
{
    return 20.50m; // Centralized and configurable per business needs
}
Default Margins:

Corporate Profit: 20%

Sales Commission: 10%

🧪 Software Testing
The project includes an automated test suite to ensure the calculation core does not suffer regressions:

bash
# Run the complete test suite
dotnet test

# Filter execution exclusively for the domain (Unit Tests)
dotnet test --filter "Category=Unit"
🚀 Deployment
Local / IIS Publishing
bash
dotnet publish --configuration Release --output ./publish
Azure Cloud Deployment
bash
# Package production artifacts
dotnet publish --configuration Release

# Direct deployment using Azure CLI
az webapp deployment source config-zip --resource-group <your-resource-group> --name <app-name> --src publish.zip
🤝 Contribution & Best Practices
If you wish to collaborate on the quotation module development, please follow the structured Git workflow:

Fork the repository

Create a clean development branch (git checkout -b feature/new-functionality)

Commit your changes with descriptive conventions (git commit -m 'feat: Add new calculation rule')

Push your branch to the remote origin (git push origin feature/new-functionality)

Open a Pull Request targeting the main branch of the main repository

Internal Code Style Guide
Aspect	Guideline
Presentation	Razor Views and web controllers are named in descriptive Spanish to maintain cohesion with the business language
Documentation	All public methods in services or repositories must include XML structured comments
DDD	It is strictly forbidden to place transactional or business logic inside Controllers or ViewModels; this must live encapsulated within the domain entities
📄 License
This project is distributed under the MIT License. See the LICENSE file for details.

"Enterprise-grade software development with Domain-Driven Design, Clean Architecture, and SOLID principles."

text

---

## ¿Qué necesitas hacer?

1. **Reemplaza** el contenido de tu `README.md` con el texto de arriba
2. **Guarda el archivo**
3. **Sube el cambio:**

```bash
git add README.md
git commit -m "docs: Update README to English version with same structure"
git push origin main
