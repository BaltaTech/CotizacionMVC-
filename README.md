# Sistema de Cotizaciones HVAC - CotizacionMVC

## 📋 Descripción del Proyecto

Sistema web especializado para la gestión y control de cotizaciones en el sector de climatización (HVAC). La plataforma permite administrar de forma integral clientes, equipos e instalaciones, generando cotizaciones profesionales con cálculos automatizados de precios, márgenes de utilidad e impuestos.

Desarrollado bajo un enfoque de **Domain-Driven Design (DDD)** con modelos enriquecidos que encapsulan la lógica de negocio, aplicando principios SOLID y arquitectura limpia (Clean Architecture).

### 🎯 Características Principales

- **Gestión de Cotizaciones:** Creación, edición estructural y seguimiento de cotizaciones comerciales.
- **Catálogo de Equipos:** Administración del inventario de equipos HVAC con soporte nativo de precios en MXN/USD.
- **Gestión de Clientes:** Registro centralizado con soporte multicontacto y direcciones detalladas.
- **Pipeline de Ventas:** Gestión de Leads, seguimientos y oportunidades comerciales.
- **Múltiples Empresas:** Soporte multiempresa con márgenes de utilidad configurables.
- **Generación de PDF:** Creación automática de documentos profesionales con branding corporativo.
- **Control de Acceso:** Autenticación y autorización basada en roles (Administrador, Vendedor, Recepción).
- **Cálculo Automático:** Motor de precios con estrategias por marca, IVA y sugerencias de carga térmica.
- **Notificaciones en Tiempo Real:** Sistema de alertas mediante SignalR.
- **Dashboards:** Paneles de control para recepción y vendedores.

---

## 🛠️ Tecnologías Utilizadas

### Backend
| Tecnología | Versión | Propósito |
|------------|---------|-----------|
| **.NET** | 8 | Framework principal |
| **ASP.NET Core MVC** | 8 | Arquitectura de presentación |
| **Entity Framework Core** | 8 | ORM para acceso a datos |
| **PostgreSQL** | 15+ | Base de datos relacional |
| **ASP.NET Core Identity** | 8 | Autenticación y autorización |
| **SignalR** | 8 | Notificaciones en tiempo real |

### Frontend
| Tecnología | Propósito |
|------------|-----------|
| **Razor Views** | Motor de plantillas del lado del servidor |
| **Bootstrap 5** | Framework CSS responsivo |
| **jQuery** | Manipulación del DOM e interacciones AJAX |
| **Font Awesome** | Iconografía vectorial |

### Generación de Documentos
| Tecnología | Propósito |
|------------|-----------|
| **QuestPDF** | Generación de PDF profesionales |

### Arquitectura y Patrones
| Patrón | Aplicación |
|--------|------------|
| **Domain-Driven Design (DDD)** | Modelos y entidades ricas con lógica de negocio |
| **Value Objects** | Inmutabilidad en conceptos como `Contacto`, `Dirección`, `Dinero` |
| **Repository Pattern** | Abstracción de la capa de persistencia |
| **Dependency Injection** | Inversión de control nativa |
| **Strategy Pattern** | Cálculo de precios por marca |
| **Open/Closed Principle** | Dominio abierto a extensión, cerrado a modificación |

---

## 📁 Estructura del Proyecto
CotizacionMVC/
├── Controllers/ # Controladores MVC
│ ├── AutenticacionController.cs
│ ├── ClienteController.cs
│ ├── CotizacionController.cs
│ ├── EmpresaController.cs
│ ├── EquipoController.cs
│ ├── HomeController.cs
│ ├── InstalacionController.cs
│ ├── NotificacionController.cs
│ ├── RecepcionController.cs
│ ├── RecepcionDashboardController.cs
│ ├── SeguimientoController.cs
│ └── UsuariosController.cs
│
├── Models/ # Capa de Dominio
│ ├── Entidades/ # Entidades Ricas (DDD)
│ │ ├── Cliente.cs 
│ │ ├── Cotizacion.cs 
│ │ ├── Empresa.cs 
│ │ ├── Equipo.cs 
│ │ ├── Instalacion.cs
│ │ ├── ItemCotizacion.cs
│ │ ├── ItemInstalacion.cs
│ │ ├── Lead.cs
│ │ ├── Notificacion.cs
│ │ ├── Seguimiento.cs
│ │ └── Usuario.cs
│ │
│ ├── Enums/ # Enumeradores del sistema
│ │ ├── CategoriaLead.cs
│ │ ├── EstadoCliente.cs
│ │ ├── EstadoCotizacion.cs
│ │ ├── EstadoSeguimiento.cs
│ │ ├── MedioContacto.cs
│ │ ├── MotivoNoCotizable.cs
│ │ ├── OrigenCliente.cs
│ │ ├── OrigenLead.cs
│ │ ├── ResultadoSeguimiento.cs
│ │ ├── RolUsuario.cs
│ │ ├── TipoEspacio.cs
│ │ └── TipoMarca.cs
│ │
│ ├── Valor/ # Value Objects (Inmutables)
│ │ ├── Contacto.cs
│ │ ├── Dinero.cs
│ │ └── Direccion.cs
│ │
│ └── Reglas/ # Reglas de Negocio
│ ├── ReglasNegocio.cs
│ ├── ICalculadoraPrecio.cs
│ ├── CalculadoraPrecioTrane.cs
│ ├── CalculadoraPrecioYork.cs
│ ├── CalculadoraPrecioHisense.cs
│ ├── CalculadoraPrecioTCL.cs
│ └── CalculadoraPrecioEstandar.cs
│
├── ViewModels/ # ViewModels y DTOs
│ ├── Cliente/
│ ├── Cotizacion/
│ ├── Empresa/
│ ├── Equipo/
│ ├── Instalacion/
│ ├── Recepcion/
│ ├── Seguimientos/
│ └── Usuarios/ 
│ └── CrearUsuarioViewModel.cs
│
├── Servicios/ # Capa de Aplicación
│ ├── Aplicacion/
│ │ ├── Interfaces/ # Contratos de servicios
│ │ │ ├── IAutorizacionServicio.cs
│ │ │ ├── IClienteServicio.cs
│ │ │ ├── ICotizacionServicio.cs
│ │ │ ├── IEmpresaServicio.cs
│ │ │ ├── IEquipoServicio.cs
│ │ │ ├── IInstalacionServicio.cs
│ │ │ ├── IRecepcionServicio.cs
│ │ │ └── ISeguimientoServicio.cs
│ │ │
│ │ ├── AutorizacionServicio.cs
│ │ ├── ClienteServicio.cs
│ │ ├── CotizacionServicio.cs 
│ │ ├── EmpresaServicio.cs
│ │ ├── EquipoServicio.cs 
│ │ ├── InstalacionServicio.cs
│ │ ├── RecepcionServicio.cs
│ │ └── SeguimientoServicio.cs
│ │
│ ├── Infraestructura/
│ │ ├── NotificacionServicio.cs
│ │ └── RecordatorioBackgroundService.cs
│ │
│ └── IDocumento.cs
│
├── Data/ # Capa de Infraestructura
│ ├── ApplicationDbContext.cs
│ ├── Repositorios/
│ │ ├── Interfaces/ # Contratos de repositorios
│ │ │ ├── IClienteRepository.cs
│ │ │ ├── ICotizacionRepository.cs
│ │ │ ├── IEmpresaRepository.cs
│ │ │ ├── IEquipoRepository.cs
│ │ │ ├── IInstalacionRepository.cs
│ │ │ └── ISeguimientoRepository.cs
│ │ │
│ │ └── Implementaciones/ # Implementaciones concretas
│ │ ├── BaseRepository.cs
│ │ ├── ClienteRepository.cs
│ │ ├── CotizacionRepository.cs
│ │ ├── EmpresaRepository.cs
│ │ ├── EquipoRepository.cs
│ │ ├── InstalacionRepository.cs
│ │ └── SeguimientoRepository.cs
│ │
│ ├── CargaDatos/
│ │ └── CargadorDatosIniciales.cs
│ │
│ └── Importadores/
│ ├── ImportadorEquipos.cs
│ └── ImportadorInstalaciones.cs
│
├── Hubs/ # SignalR Hubs
│ └── NotificacionHub.cs
│
├── Views/ # Vistas Razor (.cshtml)
│ ├── Autenticacion/
│ ├── Cliente/
│ ├── Cotizacion/
│ ├── Empresa/
│ ├── Equipo/
│ ├── Home/
│ ├── Instalacion/
│ ├── Recepcion/
│ ├── Seguimiento/
│ ├── Shared/
│ └── Usuarios/
│
├── Tests/
│ ├── Pruebas Unitarias/
│ │ ├── Entidades/
│ │ │ ├── CotizacionTests.cs
│ │ │ ├── CotizacionTotalesTests.cs
│ │ │ └── SeguimientoTests.cs
│ │ └── ValueObjects/
│ │ ├── ContactoTests.cs
│ │ ├── DineroTests.cs
│ │ └── DireccionTests.cs
│ │
│ └── Pruebas Integracion/
│ └── DetectarErrorCotizacionTests.cs
│
├── Program.cs # Punto de entrada
├── appsettings.json
└── README.md

text

---

## 🏗️ Arquitectura

### Clean Architecture + DDD
┌─────────────────────────────────────────────────────────────┐
│ PRESENTATION LAYER │
│ (Controllers, Views, ViewModels) │
│ │
│ ✅ MVC Controllers ✅ Razor Views ✅ SignalR Hubs │
└─────────────────────────────────────────────────────────────┘
│
▼
┌─────────────────────────────────────────────────────────────┐
│ APPLICATION LAYER │
│ (Services, DTOs, Interfaces) │
│ │
│ ✅ Servicios de Aplicación ✅ Dependency Injection │
└─────────────────────────────────────────────────────────────┘
│
▼
┌─────────────────────────────────────────────────────────────┐
│ DOMAIN LAYER │
│ (Entities, Value Objects, Rules) │
│ │
│ ✅ Entidades Ricas ✅ Value Objects ✅ Reglas Negocio │
│ ✅ OCP Cumplido ✅ Validaciones encapsuladas │
└─────────────────────────────────────────────────────────────┘
│
▼
┌─────────────────────────────────────────────────────────────┐
│ INFRASTRUCTURE LAYER │
│ (Repositories, Data, External Services) │
│ │
│ ✅ Repository Pattern ✅ EF Core ✅ PostgreSQL │
│ ✅ Importadores ✅ Background Services │
└─────────────────────────────────────────────────────────────┘

text

---

## 🔐 Autenticación y Roles

### Credenciales por Defecto

| Correo | Contraseña | Rol |
|--------|------------|-----|
| admin@empresa.com | Admin123! | Administrador |

### Políticas de Autorización

| Rol | Permisos |
|-----|----------|
| **Administrador** | Acceso total a todas las funcionalidades del sistema |
| **Vendedor** | Gestión de cotizaciones, clientes y seguimientos |
| **Recepción** | Registro de clientes, asignación de vendedores y dashboard |
| **Usuario Anónimo** | Acceso exclusivo a la pantalla de login |

---

## 📊 Flujo de Trabajo - Cotización

### Ciclo de Vida
Registro de Cliente
│
▼
Selección de Equipos
│
▼
Cálculo de Precios
├── Precio Base × Utilidad Empresa%
├── Subtotal + Utilidad Vendedor%
└── + IVA (16%)
│
▼
Generación de PDF
│
▼
Envío al Cliente


### Reglas de Negocio

| Entidad | Regla | Descripción Técnica |
|---------|-------|---------------------|
| **Equipo** | Moneda restringida por fabricante | Trane/York → USD, Hisense/TCL → MXN |
| **Equipo** | Capacidad requerida | No se permite equipo con CapacidadToneladas = 0 |
| **Equipo** | Detalles completos | Tipo, Tensión y Tecnología son obligatorios |
| **Cliente** | Contacto obligatorio | Debe tener al menos un medio de contacto |
| **Cliente** | Dirección obligatoria | Debe tener dirección registrada |
| **Cotización** | Área válida | ÁreaMetrosCuadrados > 0 |
| **Cotización** | Recargo ciudad | Solo aplica a equipos Trane |
| **Cotización** | Estado lineal | No se permite retroceso de estados |

### Métodos de Dominio Implementados

| Método | Ubicación | Estado | Implementado en |
|--------|-----------|--------|-----------------|
| `ActualizarDescripcion()` | `Equipo` | ✅ | `EquipoServicio.ActualizarAsync()` |
| `TieneDetallesCompletos()` | `Equipo` | ✅ | `EquipoServicio.CrearAsync()` |
| `TieneCapacidad()` | `Equipo` | ✅ | `EquipoServicio.CrearAsync()` + `Cotizacion.AgregarEquipo()` |
| `Activar()` | `Equipo` | ✅ | `EquipoServicio.ActivarAsync()` |
| `AgregarUsuarioAcceso()` | `Empresa` | ✅ | `UsuariosController.Crear()` |
| `ActualizarRecargoCiudad()` | `Cotizacion` | ⏳ | Pendiente de análisis de negocio |

---

## 💰 Cálculo de Precios

### Motor de Precios (Strategy Pattern)

```csharp
Precio Final = Precio Base × FactorPrecio × FactorUtilidad

// Para Trane:
PrecioUSD = PrecioBase × 0.31 × 1.18
PrecioMXN = PrecioUSD × TipoCambio

// Para Hisense/TCL:
PrecioMXN = PrecioBase (precio de lista)
PrecioUSD = PrecioMXN / TipoCambio

// Para otras marcas:
PrecioMXN = PrecioBase × (1 + UtilidadEmpresa%)
PrecioUSD = PrecioMXN / TipoCambio
Totales
text
SubtotalEquipos (USD) → + RecargoCiudad% → TotalEquipos (USD)
TotalEquipos (USD) → Convertir a MXN
+ Instalaciones (MXN)
= Subtotal (MXN)
+ IVA 16%
= Total Final (MXN)


🤝 Contribución
bash
# 1. Fork del repositorio
# 2. Crear rama de desarrollo
git checkout -b feature/nueva-funcionalidad

# 3. Commit con mensaje descriptivo
git commit -m "feat: Agrega nueva regla de cálculo"

# 4. Push y Pull Request
git push origin feature/nueva-funcionalidad
