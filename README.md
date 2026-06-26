# Sistema de Cotizaciones HVAC 

##  Descripción del Proyecto
Sistema web especializado para la gestión y control de cotizaciones en el sector de climatización (HVAC). La plataforma permite administrar de forma integral clientes, equipos e instalaciones, generando cotizaciones profesionales con cálculos automatizados de precios, márgenes de utilidad e impuestos. Desarrollado bajo un enfoque de **Domain-Driven Design (DDD)** utilizando modelos enriquecidos con lógica de negocio interna.

### 🎯 Características Principales
* **Gestión de Cotizaciones:** Creación, edición estructural y seguimiento de cotizaciones comerciales.
* **Catálogo de Equipos:** Administración del inventario de equipos HVAC con soporte nativo de precios en MXN/USD.
* **Gestión de Clientes:** Registro centralizado con soporte multcontacto y direcciones detalladas.
* **Múltiples Empresas:** Soporte multiempresa para diferentes razones sociales con márgenes de utilidad configurables.
* **Generación de PDF:** Creación automática de documentos profesionales con branding corporativo mediante QuestPDF.
* **Control de Acceso:** Esquema seguro de autenticación y autorización basado en roles (Administrador/Vendedor) vía ASP.NET Core Identity.
* **Cálculo Automático:** Motor de precios que procesa utilidades encadenadas, IVA y sugerencias de carga térmica.

---

## 🛠️ Tecnologías Utilizadas

### Backend
* **.NET 8** - Framework principal del ecosistema.
* **ASP.NET Core MVC** - Arquitectura de presentación basada en el patrón Modelo-Vista-Controlador.
* **Entity Framework Core** - ORM para el mapeo y acceso eficiente a datos.
* **PostgreSQL** - Motor de base de datos relacional para entornos de producción.
* **ASP.NET Core Identity** - Sistema de gestión de usuarios, sesiones y políticas de seguridad.

### Frontend
* **Razor Views** - Motor de plantillas dinámicas del lado del servidor.
* **Bootstrap 5** - Framework CSS para maquetación responsiva.
* **jQuery** - Manipulación del DOM e interacciones asíncronas en el cliente.
* **Font Awesome** - Set de iconografía vectorial.

### Generación de Documentos
* **QuestPDF** - Librería avanzada para el diseño y maquetación de archivos PDF profesionales.

### Arquitectura y Patrones
* **Domain-Driven Design (DDD):** Modelos y entidades ricas que encapsulan el comportamiento y las reglas de negocio.
* **Value Objects:** Inmutabilidad aplicada a conceptos del dominio como `Contacto`, `Dirección` y `Dinero`.
* **Repository Pattern:** Capa de abstracción para el desacoplamiento de la persistencia de datos.
* **Dependency Injection:** Inversión de control nativa para la gestión de ciclo de vida de los servicios.

---

## 📁 Estructura del Proyecto

```text
CotizacionMVC/
├── Controllers/              # Controladores de la arquitectura MVC
│   ├── AutenticacionController.cs
│   ├── ClienteController.cs
│   ├── CotizacionController.cs
│   ├── EmpresaController.cs
│   ├── EquipoController.cs
│   └── UsuariosController.cs
├── Models/                   # Modelos y lógica de Dominio
│   ├── Entidades/            # Entidades ricas enriquecidas con lógica interna
│   │   ├── Cliente.cs
│   │   ├── Cotizacion.cs
│   │   ├── Empresa.cs
│   │   ├── Equipo.cs
│   │   ├── Instalacion.cs
│   │   ├── ItemCotizacion.cs
│   │   ├── ItemInstalacion.cs
│   │   ├── Lead.cs
│   │   ├── Seguimiento.cs
│   │   └── Usuario.cs
│   ├── Enums/                # Enumeradores globales del sistema
│   ├── Valor/                # Objetos de Valor (Value Objects Inmutables)
│   │   ├── Contacto.cs
│   │   ├── Dinero.cs
│   │   └── Direccion.cs
│   └── Reglas/               # Validaciones y especificaciones de negocio
├── Data/                     # Capa de infraestructura de persistencia
│   └── ApplicationDbContext.cs
├── Servicios/                # Servicios de aplicación e infraestructura externa
│   ├── IDocumento.cs
│   └── PdfCotizacion.cs
├── Views/                    # Vistas e interfaces Razor (.cshtml)
│   ├── Autenticacion/
│   ├── Cliente/
│   ├── Cotizacion/
│   ├── Empresa/
│   ├── Equipo/
│   └── Usuarios/
└── wwwroot/                  # Recursos estáticos de la aplicación
    ├── css/
    ├── js/
    ├── lib/
    └── pdf/cotizaciones/     # Directorio de almacenamiento temporal de PDFs

 Instalación y ConfiguraciónPrerrequisitos.NET 8 SDKPostgreSQL (Configurable a SQL Server si se prefiere)JetBrains Rider o Visual Studio 2022 / VS CodePasos de InstalaciónClonar el repositorio:Bashgit clone [https://github.com/BaltaTech/Ventas.git](https://github.com/BaltaTech/Ventas.git)
cd Ventas
Configurar la base de datos:Modifica el archivo appsettings.json dentro de la raíz con tu cadena de conexión local:JSON{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=CotizacionDB;Username=postgres;Password=tu-password"
  }
}
Restaurar dependencias y aplicar migraciones:Bash# Restaurar paquetes de NuGet
dotnet restore

# Crear y aplicar la estructura inicial en la base de datos
dotnet ef migrations add InitialCreate
dotnet ef database update
Ejecutar el proyecto:Bashdotnet run
La aplicación se levantará localmente en los puertos correspondientes:HTTP: http://localhost:5000HTTPS: https://localhost:5001🔐 Autenticación y RolesCredenciales por Defecto (Seed Data)CorreoContraseñaRol Asignadoadmin@empresa.comAdmin123!AdministradorPolíticas de AutorizaciónAdministrador:
Acceso irrestricto a configuraciones globales, catálogos, auditorías y control de usuarios.Vendedor:
Permisos enfocados a la gestión operativa: visualización de equipos, creación de clientes y emisión de cotizaciones.Usuario Anónimo: Acceso restringido exclusivamente al portal de Login.
📊 Flujo de Trabajo y Reglas de Negocio.
 Ciclo de una CotizaciónSelección del Cliente: Vinculación directa desde el catálogo maestro.Carga de Equipos: Selección de ítems técnicos configurados previamente.Servicios de Instalación: Inclusión opcional de mano de obra y viáticos.Cálculo de Precios:
El núcleo del dominio procesa en cascada:$$\text{Precio Final} = \text{Precio Base} \times (1 + \text{Utilidad Empresa}\%) \times (1 + \text{Utilidad Vendedor}\%)$$Impuestos y Totales:
 Aplicación automatizada del IVA (16%) y conversión a la moneda base.Emisión: Compilación limpia del archivo PDF listo para su envío comercial.2. Matriz de Reglas TécnicasEntidad /
FlujoRegla de NegocioDescripción TécnicaEquiposMoneda restringida por fabricanteEquipos marcas Trane y York se cotizan estrictamente en USD.
Otras marcas en MXN.ClientesContacto MandatorioNo se permite el alta de clientes sin al menos un medio de contacto válido (Email/Teléfono).
FinanzasValidaciones numéricasTodos los costos base y precios de lista deben ser estrictamente mayores a cero ($>0$).EvoluciónMáquina de EstadosLos estados de las cotizaciones siguen un flujo lineal hacia adelante; no se permite el retroceso de fases comerciales.
🔧 Configuración AvanzadaTipo de Cambio FinancieroEl sistema centraliza las tasas de conversión dentro de la entidad de cotización para asegurar consistencia transaccional histórica:C#// Ubicado en: Models/Entidades/Cotizacion.cs
public decimal ObtenerTipoCambioActual()
{
    return 20.50m; // Centralizado y configurable según necesidades del negocio
}
Márgenes por Defecto: Utilidad Corporativa: 20% | Comisión de Venta: 10%.🧪 Pruebas de SoftwareEl proyecto incluye un conjunto de pruebas automatizadas destinadas a asegurar que el núcleo de cálculo no sufra regresiones:Bash# Ejecutar la suite completa de pruebas
dotnet test

# Filtrar ejecuciones exclusivas del dominio (Unit Tests)
dotnet test --filter "Category=Unit"
 DesplieguePublicación Local / IISBashdotnet publish --configuration Release --output ./publish
Implementación en Azure CloudBash# Empaquetar artefactos de producción
dotnet publish --configuration Release

# Despliegue directo mediante Azure CLI
az webapp deployment source config-zip --resource-group <tu-grupo-recursos> --name <nombre-app> --src publish.zip
 Contribución y Buenas PrácticasSi deseas colaborar en el desarrollo del módulo de cotizaciones, por favor sigue el flujo estructurado de Git:Realiza un Fork del repositorio.Crea una rama de desarrollo limpia (git checkout -b feature/nueva-funcionalidad).Guarda tus cambios bajo convenios descriptivos (git commit -m 'feat: Agrega nueva regla de cálculo').Sube tu rama al origen remoto (git push origin feature/nueva-funcionalidad).Abre un Pull Request apuntando hacia la rama main del repositorio principal.Guía de Estilo de Código InternoPresentación: Las vistas Razor y controladores web se nombran en español descriptivo para mantener cohesión con el lenguaje de negocio.Documentación: Todo método público en servicios o repositorios debe incluir comentarios estructurados en XML.DDD: Queda estrictamente prohibido colocar lógica transaccional o de negocio dentro de los Controladores o ViewModels; esta debe vivir encapsulada en las entidades de dominio.📄 LicenciaEste proyecto está distribuido bajo la licencia MIT. Consulta el archivo LICENSE para mayores detalles.
