using CotizacionMVC.Models.Entidades;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CotizacionMVC.Data
{
    public class ApplicationDbContext : IdentityDbContext<Usuario, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opciones) : base(opciones) { }
        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Cotizacion> Cotizaciones { get; set; }
        public DbSet<Equipo> Equipos { get; set; }
        public DbSet<ItemCotizacion> ItemsCotizacion { get; set; }
        public DbSet<Instalacion> Instalaciones { get; set; }
        public DbSet<ItemInstalacion> ItemsInstalacion { get; set; }
        public DbSet<Seguimiento> Seguimientos { get; set; }
        public DbSet<Lead> Leads { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder constructorModelos)
        {
            base.OnModelCreating(constructorModelos);
            ConfigurarEmpresa(constructorModelos);
            ConfigurarCliente(constructorModelos);
            ConfigurarCotizacion(constructorModelos);
            ConfigurarEquipo(constructorModelos);
            ConfigurarItemCotizacion(constructorModelos);
            ConfigurarInstalacion(constructorModelos);
            ConfigurarItemInstalacion(constructorModelos);
            ConfigurarSeguimiento(constructorModelos);
            ConfigurarLead(constructorModelos);
        }

        private void ConfigurarEmpresa(ModelBuilder constructorModelos)
        {
            constructorModelos.Entity<Empresa>(entidad =>
            {
                entidad.ToTable("Empresas");
                entidad.HasKey(e => e.Id);

                entidad.Property(e => e.NombreComercial)
                    .IsRequired()
                    .HasMaxLength(200);

                entidad.Property(e => e.Slug)
                    .IsRequired()
                    .HasMaxLength(100);

                entidad.Property(e => e.MonedaBase)
                    .IsRequired()
                    .HasMaxLength(3);

                entidad.Property(e => e.NombreLegal)
                    .HasMaxLength(300);

                entidad.Property(e => e.LogoUrl)
                    .HasMaxLength(500);

                entidad.Property(e => e.TelefonoContacto)
                    .HasMaxLength(20);

                entidad.Property(e => e.CorreoContacto)
                    .HasMaxLength(200);

                entidad.Property(e => e.SitioWeb)
                    .HasMaxLength(300);

                entidad.Property(e => e.Eslogan)
                    .HasMaxLength(500);

                entidad.Property(e => e.ColorPrimario)
                    .HasMaxLength(20);

                entidad.Property(e => e.ColorSecundario)
                    .HasMaxLength(20);

                entidad.Property(e => e.PlantillaPdfNombre)
                    .HasMaxLength(100);


            });
        }

        private void ConfigurarCliente(ModelBuilder constructorModelos)
        {
            constructorModelos.Entity<Cliente>(entidad =>
            {
                entidad.ToTable("Clientes");
                entidad.HasKey(c => c.Id);

                entidad.Property(c => c.Nombre)
                    .IsRequired()
                    .HasMaxLength(300);

                entidad.Property(c => c.Observaciones)
                    .HasMaxLength(1000);

                entidad.OwnsOne(c => c.Contacto, contacto =>
                {
                    contacto.Property(c => c.Telefono)
                        .HasMaxLength(20);

                    contacto.Property(c => c.TelefonoMovil)
                        .HasMaxLength(20);

                    contacto.Property(c => c.Correo)
                        .HasMaxLength(200);

                    contacto.Property(c => c.NombreContacto)
                        .HasMaxLength(200);
                });

                entidad.OwnsOne(c => c.Direccion, direccion =>
                {
                    direccion.Property(d => d.Calle)
                        .HasMaxLength(200);

                    direccion.Property(d => d.NumeroExterior)
                        .HasMaxLength(20);

                    direccion.Property(d => d.NumeroInterior)
                        .HasMaxLength(20);

                    direccion.Property(d => d.Colonia)
                        .HasMaxLength(200);

                    direccion.Property(d => d.Ciudad)
                        .HasMaxLength(200);

                    direccion.Property(d => d.Estado)
                        .HasMaxLength(100);

                    direccion.Property(d => d.CodigoPostal)
                        .HasMaxLength(10);
                });

            });
        }

        private void ConfigurarCotizacion(ModelBuilder constructorModelos)
        {
            constructorModelos.Entity<Cotizacion>(entidad =>
            {
                entidad.ToTable("Cotizaciones");
                entidad.HasKey(c => c.Id);

                entidad.Property(c => c.NumeroCotizacion)
                    .IsRequired()
                    .HasMaxLength(20);

                entidad.HasIndex(c => c.NumeroCotizacion)
                    .IsUnique();

                entidad.Property(c => c.CondicionesPago)
                    .HasMaxLength(500);

                entidad.Property(c => c.AreaMetrosCuadrados)
                    .HasPrecision(10, 2);

                entidad.HasOne(c => c.Cliente)
                    .WithMany(c => c.Cotizaciones)
                    .HasForeignKey(c => c.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);

                entidad.HasOne(c => c.Empresa)
                    .WithMany(e => e.Cotizaciones)
                    .HasForeignKey(c => c.EmpresaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entidad.HasOne(c => c.Vendedor)
                    .WithMany(u => u.Cotizaciones)
                    .HasForeignKey(c => c.VendedorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entidad.OwnsOne(c => c.Subtotal, dinero =>
                {
                    dinero.Property(d => d.Monto).HasPrecision(18, 2);
                    dinero.Property(d => d.Moneda).HasMaxLength(3);
                });

                entidad.OwnsOne(c => c.Iva, dinero =>
                {
                    dinero.Property(d => d.Monto).HasPrecision(18, 2);
                    dinero.Property(d => d.Moneda).HasMaxLength(3);
                });

                entidad.OwnsOne(c => c.Total, dinero =>
                {
                    dinero.Property(d => d.Monto).HasPrecision(18, 2);
                    dinero.Property(d => d.Moneda).HasMaxLength(3);
                });
            });
        }
        private void ConfigurarEquipo(ModelBuilder constructorModelos)
        {
            constructorModelos.Entity<Equipo>(entidad =>
            {
                entidad.ToTable("Equipos");
                entidad.HasKey(e => e.Id);

                entidad.Property(e => e.Modelo)
                    .IsRequired()
                    .HasMaxLength(200);

                entidad.Property(e => e.Tipo)
                    .HasMaxLength(100);

                entidad.Property(e => e.Tension)
                    .HasMaxLength(20);

                entidad.Property(e => e.Tecnologia)
                    .HasMaxLength(100);

                entidad.Property(e => e.CapacidadToneladas)
                    .HasPrecision(10, 2);

                entidad.Property(e => e.PrecioBase)
                    .HasPrecision(18, 2);

                entidad.Property(e => e.MonedaOriginal)
                    .IsRequired()
                    .HasMaxLength(3);
            });
        }
        private void ConfigurarItemCotizacion(ModelBuilder constructorModelos)
        {
            constructorModelos.Entity<ItemCotizacion>(entidad =>
            {
                entidad.ToTable("ItemsCotizacion");
                entidad.HasKey(i => i.Id);

                entidad.Property(i => i.Cantidad)
                    .IsRequired();

                entidad.Property(i => i.DescripcionPersonalizada)
                    .HasMaxLength(500);

                entidad.Property(i => i.UtilidadEmpresaPorcentaje)
                    .HasPrecision(18, 2);

                entidad.Property(i => i.UtilidadVendedorPorcentaje)
                    .HasPrecision(18, 2);

                entidad.HasOne(i => i.Cotizacion)
                    .WithMany(c => c.ItemsEquipos)
                    .HasForeignKey(i => i.CotizacionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entidad.HasOne(i => i.Equipo)
                    .WithMany()
                    .HasForeignKey(i => i.EquipoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entidad.OwnsOne(i => i.PrecioUnitario, dinero =>
                {
                    dinero.Property(d => d.Monto)
                        .HasPrecision(18, 2);

                    dinero.Property(d => d.Moneda)
                        .HasMaxLength(3);
                });

                entidad.OwnsOne(i => i.Subtotal, dinero =>
                {
                    dinero.Property(d => d.Monto)
                        .HasPrecision(18, 2);

                    dinero.Property(d => d.Moneda)
                        .HasMaxLength(3);
                });
            });
        }

        private void ConfigurarInstalacion(ModelBuilder constructorModelos)
        {
            constructorModelos.Entity<Instalacion>(entidad =>
            {
                entidad.ToTable("Instalaciones");
                entidad.HasKey(i => i.Id);

                entidad.Property(i => i.Concepto)
                    .IsRequired()
                    .HasMaxLength(200);

                entidad.Property(i => i.Descripcion)
                    .HasMaxLength(500);

                entidad.Property(i => i.Categoria)
                    .IsRequired()
                    .HasMaxLength(50);

                entidad.Property(i => i.CostoUnitario)
                    .HasPrecision(18, 2);
            });
        }

        private void ConfigurarItemInstalacion(ModelBuilder constructorModelos)
        {
            constructorModelos.Entity<ItemInstalacion>(entidad =>
            {
                entidad.ToTable("ItemsInstalacion");
                entidad.HasKey(i => i.Id);

                entidad.Property(i => i.Concepto)
                    .IsRequired()
                    .HasMaxLength(200);

                entidad.Property(i => i.Descripcion)
                    .HasMaxLength(500);

                entidad.Property(i => i.Cantidad)
                    .IsRequired();

                entidad.HasOne(i => i.Cotizacion)
                    .WithMany(c => c.ItemsInstalacion)
                    .HasForeignKey(i => i.CotizacionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entidad.HasOne(i => i.Instalacion)
                    .WithMany()
                    .HasForeignKey(i => i.InstalacionId)
                    .OnDelete(DeleteBehavior.SetNull);

                entidad.OwnsOne(i => i.CostoUnitario, dinero =>
                {
                    dinero.Property(d => d.Monto)
                        .HasPrecision(18, 2);

                    dinero.Property(d => d.Moneda)
                        .HasMaxLength(3);
                });

                entidad.OwnsOne(i => i.Subtotal, dinero =>
                {
                    dinero.Property(d => d.Monto)
                        .HasPrecision(18, 2);

                    dinero.Property(d => d.Moneda)
                        .HasMaxLength(3);
                });
            });
        }

        private void ConfigurarSeguimiento(ModelBuilder constructorModelos)
        {
            constructorModelos.Entity<Seguimiento>(entidad =>
            {
                entidad.ToTable("Seguimientos");
                entidad.HasKey(s => s.Id);

                entidad.Property(s => s.Comentarios)
                    .HasMaxLength(1000);

                entidad.HasOne(s => s.Cotizacion)
                    .WithMany(c => c.Seguimientos)
                    .HasForeignKey(s => s.CotizacionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entidad.HasOne(s => s.Empresa)
                    .WithMany()
                    .HasForeignKey(s => s.EmpresaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entidad.HasOne(s => s.Vendedor)
                    .WithMany()
                    .HasForeignKey(s => s.VendedorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigurarLead(ModelBuilder constructorModelos)
        {
            constructorModelos.Entity<Lead>(entidad =>
            {
                entidad.ToTable("Leads");
                entidad.HasKey(l => l.Id);

                entidad.Property(l => l.NombreContacto)
                    .IsRequired()
                    .HasMaxLength(200);

                entidad.Property(l => l.Telefono)
                    .HasMaxLength(20);

                entidad.Property(l => l.CorreoElectronico)
                    .HasMaxLength(200);

                entidad.Property(l => l.EmpresaCliente)
                    .HasMaxLength(300);

                entidad.Property(l => l.Origen)
                    .IsRequired()
                    .HasMaxLength(200);

                entidad.Property(l => l.ComentariosInternos)
                    .HasMaxLength(1000);

                entidad.HasOne(l => l.Empresa)
                    .WithMany(e => e.Leads)
                    .HasForeignKey(l => l.EmpresaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entidad.HasOne(l => l.VendedorAsignado)
                    .WithMany()
                    .HasForeignKey(l => l.VendedorAsignadoId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
