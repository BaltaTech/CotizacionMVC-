# CotizacionMVC

<p align="center">
  <strong>Un Sistema de Gestión e Ingeniería de Cotizaciones HVAC de Nivel Empresarial</strong>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet" alt=".NET 8" />
  <img src="https://img.shields.io/badge/Architecture-Clean%20%2B%20DDD-blue?style=for-the-badge" alt="Clean Architecture" />
  <img src="https://img.shields.io/badge/Database-PostgreSQL-4169E1?style=for-the-badge&logo=postgresql" alt="PostgreSQL" />
  <img src="https://img.shields.io/badge/License-MIT-green?style=for-the-badge" alt="License MIT" />
</p>

---

## 📋 Resumen Ejecutivo

**CotizacionMVC** es una plataforma web integral diseñada para la industria HVAC (*Heating, Ventilation, and Air Conditioning*). Automatiza y optimiza todo el ciclo de vida comercial: desde la prospección y registro de clientes, pasando por la selección paramétrica de equipos y cálculo dinámico de precios, hasta la generación de documentos PDF de alta calidad y la analítica en tiempo real del embudo de ventas.

El sistema fue desarrollado bajo **Clean Architecture**, **Domain-Driven Design (DDD)** y los principios **SOLID**, garantizando un bajo acoplamiento, alta mantenibilidad y testabilidad end-to-end.

---

## 🎯 Capacidades Principales

| Módulo | Descripción |
| :--- | :--- |
| **Gestión de Cotizaciones** | Flujo de estados con reglas estricta de transición comercial y motor de cálculo dinámico. |
| **Catálogo de Equipamiento** | Administración multi-marca con soporte dinámico para multimoneda (**USD / MXN**). |
| **Gestión de Clientes (CRM)** | Registro centralizado con soporte multi-contacto y jerarquías organizacionales. |
| **Pipeline de Ventas** | Seguimiento de *leads*, historial de oportunidades y métricas de conversión. |
| **Estructura Multi-Empresa** | Parametrización de márgenes de utilidad y políticas comerciales por entidad. |
| **Motor de Documentos PDF** | Renderizado dinámico de propuestas comerciales mediante **QuestPDF**. |
| **Seguridad y RBAC** | Control de acceso basado en roles (*Administrator*, *Seller*, *Receptionist*). |
| **Notificaciones en T. Real** | Canal de alertas y eventos distribuidos con **SignalR**. |

---

## 🏗️ Arquitectura de Software

La aplicación implementa una separación estricta de responsabilidades en cuatro capas principales, asegurando que la lógica de negocio permanezca independiente de infraestructura o marcos web.

   ┌─────────────────────────────────────────────────────────┐
   │                   Presentation Layer                    │
   │         (MVC Controllers, Views, SignalR Hubs)          │
   └───────────────────────────┬─────────────────────────────┘
                               │
   ┌───────────────────────────▼─────────────────────────────┐
   │                    Application Layer                    │
   │             (Services, DTOs, Contracts)                 │
   └───────────────────────────┬─────────────────────────────┘
                               │
   ┌───────────────────────────▼─────────────────────────────┐
   │                      Domain Layer                       │
   │       (Rich Entities, Value Objects, Domain Rules)      │
   └───────────────────────────▲─────────────────────────────┘
                               │
   ┌───────────────────────────┴─────────────────────────────┐
   │                   Infrastructure Layer                  │
   │          (EF Core, PostgreSQL, External APIs)           │
   └─────────────────────────────────────────────────────────┘

### 📐 Diagramas del Sistema

| Diagrama | Descripción | Vista Previa |
| :--- | :--- | :---: |
| **Arquitectura General** | Vista global del ecosistema y de los actores clave. | [Ver Diagrama](./docs/diagramas/01-arquitectura-general.puml.png) |
| **Capas y Dependencias** | Aislamiento de capas bajo principios Clean Architecture. | [Ver Diagrama](./docs/diagramas/02-capas-dependencias.puml.png) |
| **Modelo de Dominio** | Grafo de entidades, agregados y *Value Objects*. | [Ver Diagrama](./docs/diagramas/03-dominio-relaciones.puml.png) |
| **Flujo de Autenticación** | Seguridad basada en JWT y roles. | [Ver Diagrama](./docs/diagramas/04-api-jwt.puml.png) |

---

## 📁 Estructura del Proyecto

```text
CotizacionMVC/
├── src/
│   ├── Controllers/            # Controladores MVC y Endpoints REST
│   │   ├── AutenticacionController.cs
│   │   ├── ClienteController.cs
│   │   ├── CotizacionController.cs
│   │   └── ...
│   ├── Models/                 # Capa de Dominio (Domain Layer)
│   │   ├── Entidades/          # Entidades ricas con encapsulamiento DDD
│   │   │   ├── Cliente.cs
│   │   │   ├── Cotizacion.cs
│   │   │   └── ...
│   │   ├── Valor/              # Value Objects inmutables
│   │   │   ├── Dinero.cs
│   │   │   ├── Contacto.cs
│   │   │   └── Direccion.cs
│   │   ├── Reglas/             # Estrategias y lógica pura de negocio
│   │   └── Enums/              # Enumeradores del sistema
│   ├── Servicios/              # Capa de Aplicación e Infraestructura
│   │   ├── Aplicacion/         # Orquestación de casos de uso (DTOs e Interfaces)
│   │   └── Infraestructura/    # Implementaciones externas y servicios de fondo
│   ├── Data/                   # Persistencia de Datos
│   │   ├── ApplicationDbContext.cs
│   │   ├── Repositorios/       # Patrón Repository & Unit of Work
│   │   └── Importadores/       # Parsers de catálogos e insumos
│   ├── Views/                  # Vistas Razor (.cshtml)
│   ├── Hubs/                   # WebSockets con SignalR
│   └── Program.cs              # Contenedor de IoC y Pipeline HTTP
├── tests/                      # Batería de Pruebas Unitarias e Integración
│   └── CotizacionMVC.Tests/
└── docs/                       # Documentación técnica y diagramas PlantUML
🛠️ Stack TecnológicoCore: .NET 8.0 SDK (C# 12)Web Framework: ASP.NET Core MVCPersistencia: Entity Framework Core 8, PostgreSQLGeneración de Reportes: QuestPDFReal-Time: ASP.NET Core SignalRFrontend: Razor Views, Bootstrap 5, Font Awesome, jQueryTesting: xUnit, Moq, FluentAssertionsDocumentación API: Swagger / OpenAPI💰 Motor de Precios (Strategy Pattern)Para gestionar las complejas variaciones tarifarias de los fabricantes, se implementó el patrón Strategy en el cálculo de costos:                      ┌──────────────────────┐
                      │ ICalculadoraPrecio   │
                      └──────────┬───────────┘
                                 │
         ┌───────────────────────┼───────────────────────┐
         │                       │                       │
┌────────┴─────────┐    ┌────────┴─────────┐    ┌────────┴─────────┐
│ CalculadoraTrane │    │CalculadoraHisense│    │CalculadoraGenerica│
└──────────────────┘    └──────────────────┘    └──────────────────┘
Trane: Precio (USD) = Precio Base × 0.31 × 1.18Hisense / TCL: Precio (MXN) = Precio ListaMarcas Generales: Precio (MXN) = Precio Base × (1 + % Margen Empresa)🔐 Configuración y Roles PredeterminadosEl sistema cuenta con un Seeder que inicializa los roles de seguridad y las credenciales por defecto para entornos de desarrollo:UsuarioContraseñaRol Asignadoadmin@empresa.comAdmin123!Administrator🚀 Despliegue e Instalación LocalRequisitos Previos.NET 8.0 SDKPostgreSQL 15+Pasos de EjecuciónClonar el repositorio:Bashgit clone [https://github.com/BaltaTech/CotizacionMVC-.git](https://github.com/BaltaTech/CotizacionMVC-.git)
cd CotizacionMVC
Restaurar dependencias:Bashdotnet restore
Configurar la base de datos:Actualiza la cadena de conexión en appsettings.json o mediante User Secrets:JSON"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=CotizacionDb;Username=postgres;Password=tu_password"
}
Aplicar migraciones:Bashdotnet ef database update
Iniciar la aplicación:Bashdotnet run
Explorar API (Swagger):Abre tu navegador e ingresa a https://localhost:7271/swagger.🧪 Batería de PruebasPara ejecutar las pruebas unitarias y de integración:Bash# Ejecutar la suite completa de pruebas
dotnet test

# Ejecutar únicamente pruebas unitarias
dotnet test --filter "Category=Unit"
📄 LicenciaEste proyecto está distribuido bajo la Licencia MIT. Consulta el archivo LICENSE para obtener más detalles.
