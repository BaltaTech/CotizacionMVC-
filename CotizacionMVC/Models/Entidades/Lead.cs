    // Models/Entidades/Lead.cs
    using CotizacionMVC.Models.Enums;

    namespace CotizacionMVC.Models.Entidades
    {
        public class Lead
        {
            public Guid Id { get; private set; }
            public Guid EmpresaId { get; private set; }
            public virtual Empresa Empresa { get; private set; }
            public Guid? VendedorAsignadoId { get; private set; }
            public virtual Usuario? VendedorAsignado { get; private set; }
            public Guid? ClienteId { get; private set; }
            public virtual Cliente? Cliente { get; private set; }
            public string NombreContacto { get; private set; }
            public string? Telefono { get; private set; }
            public string? CorreoElectronico { get; private set; }
            public string? EmpresaCliente { get; private set; }
            public CategoriaLead Categoria { get; private set; }
            public string Origen { get; private set; }
            public OrigenLead OrigenLead { get; private set; }
            public DateTime FechaCreacion { get; private set; }
            public DateTime? FechaAsignacion { get; private set; }
            public string? ComentariosInternos { get; private set; }
            public string? ProductoBusca { get; private set; }
            public EstadoCliente Estado { get; private set; }
            public MotivoNoCotizable? MotivoNoCotizable { get; private set; }
            public string? ComentarioNoCotizable { get; private set; }
            public DateTime? FechaContacto { get; private set; }
            public DateTime? FechaCotizacion { get; private set; }
            public DateTime? UltimoSeguimiento { get; private set; }
            public EtapaNegociacion? EtapaNegociacion { get; private set; }


            protected Lead()
            {
                Empresa = null!;
                NombreContacto = null!;
                Origen = null!;
                Telefono = null;
                CorreoElectronico = null;
                EmpresaCliente = null;
                ComentariosInternos = null;
                VendedorAsignado = null;
                Cliente = null;
                ProductoBusca = null;
                ComentarioNoCotizable = null;
                EtapaNegociacion = null;

            }

            public Lead(
                Empresa empresa,
                string nombreContacto,
                string? telefono,
                CategoriaLead categoria,
                string origen,
                OrigenLead origenLead = OrigenLead.Prospeccion,
                string? correoElectronico = null)
            {
                if (empresa == null)
                    throw new ArgumentNullException(nameof(empresa));

                if (string.IsNullOrWhiteSpace(nombreContacto))
                    throw new ArgumentException("El nombre de contacto es obligatorio");

                bool tieneTelefono = !string.IsNullOrWhiteSpace(telefono);
                bool tieneCorreo = !string.IsNullOrWhiteSpace(correoElectronico);

                if (!tieneTelefono && !tieneCorreo)
                    throw new ArgumentException("Debe proporcionar al menos teléfono o correo electrónico");

                if (string.IsNullOrWhiteSpace(origen))
                    throw new ArgumentException("El origen del lead es obligatorio");

                Id = Guid.NewGuid();
                Empresa = empresa;
                EmpresaId = empresa.Id;
                NombreContacto = nombreContacto.Trim();
                Telefono = telefono?.Trim();
                CorreoElectronico = correoElectronico?.Trim().ToLower();
                Categoria = categoria;
                Origen = origen.Trim();
                OrigenLead = origenLead;
                FechaCreacion = DateTime.UtcNow;
                Estado = EstadoCliente.SinAsignar;
                EmpresaCliente = null;
                ComentariosInternos = null;
                VendedorAsignado = null;
                Cliente = null;
                ProductoBusca = null;
                ComentarioNoCotizable = null;
            }

            public void AsignarVendedor(Usuario vendedor)
            {
                if (vendedor == null)
                    throw new ArgumentNullException(nameof(vendedor));

                if (VendedorAsignadoId.HasValue)
                    throw new InvalidOperationException("Este lead ya tiene un vendedor asignado");

                VendedorAsignado = vendedor;
                VendedorAsignadoId = vendedor.Id;
                FechaAsignacion = DateTime.UtcNow;
                Estado = EstadoCliente.Asignado;
            }

            public void ActualizarCategoria(CategoriaLead nuevaCategoria)
            {
                Categoria = nuevaCategoria;
            }

            public void AgregarComentario(string? comentario)
            {
                if (!string.IsNullOrWhiteSpace(comentario))
                    ComentariosInternos = comentario.Trim();
            }

            public void ActualizarDatosContacto(string? telefono, string? correo, string? empresaCliente)
            {
                if (!string.IsNullOrWhiteSpace(telefono))
                    Telefono = telefono.Trim();

                if (!string.IsNullOrWhiteSpace(correo))
                    CorreoElectronico = correo.Trim().ToLower();

                if (!string.IsNullOrWhiteSpace(empresaCliente))
                    EmpresaCliente = empresaCliente.Trim();
            }

            public bool TieneVendedorAsignado()
            {
                return VendedorAsignadoId.HasValue && VendedorAsignado != null;
            }

            public string ObtenerMedioContactoPrincipal()
            {
                if (!string.IsNullOrWhiteSpace(Telefono))
                    return $"Tel: {Telefono}";
                if (!string.IsNullOrWhiteSpace(CorreoElectronico))
                    return $"Email: {CorreoElectronico}";
                return "Sin medio de contacto";
            }

            public bool EsLeadCaliente()
            {
                return Categoria == CategoriaLead.Contactado
                    || Categoria == CategoriaLead.Caliente
                    || Categoria == CategoriaLead.Calificado
                    || Categoria == CategoriaLead.Cotizando;
            }

            public void VincularCliente(Cliente cliente)
            {
                if (cliente == null)
                    throw new ArgumentNullException(nameof(cliente));

                Cliente = cliente;
                ClienteId = cliente.Id;
            }

            public void EstablecerProducto(string productoBusca)
            {
                if (string.IsNullOrWhiteSpace(productoBusca))
                    throw new ArgumentException("El producto es obligatorio");

                ProductoBusca = productoBusca.Trim();
            }

            public void MarcarContactado()
            {
                Estado = EstadoCliente.Contactado;
                FechaContacto = DateTime.UtcNow;
            }

            public void MarcarCotizado()
            {
                Estado = EstadoCliente.Cotizado;
                FechaCotizacion = DateTime.UtcNow;
            }

            public void MarcarNoCotizable(MotivoNoCotizable motivo, string? comentario = null)
            {
                Estado = EstadoCliente.NoCotizable;
                MotivoNoCotizable = motivo;
                ComentarioNoCotizable = comentario?.Trim();
            }

            public void MarcarCerrado()
            {
                Estado = EstadoCliente.Cerrado;
            }

            public void MarcarPerdido()
            {
                Estado = EstadoCliente.Perdido;
            }

            public void ReasignarVendedor(Usuario nuevoVendedor)
            {
                if (nuevoVendedor == null)
                    throw new ArgumentNullException(nameof(nuevoVendedor));

                VendedorAsignado = nuevoVendedor;
                VendedorAsignadoId = nuevoVendedor.Id;
                FechaAsignacion = DateTime.UtcNow;
            }

            public void RegistrarActividad(DateTime fecha)
            {
                UltimoSeguimiento = fecha;
            }

            public bool EstaSinActividad(int dias)
            {
                if (!UltimoSeguimiento.HasValue)
                    return (DateTime.UtcNow - FechaCreacion).TotalDays > dias;

                return (DateTime.UtcNow - UltimoSeguimiento.Value).TotalDays > dias;
            }

            public bool EsEstadoTerminal()
            {
                return Categoria == CategoriaLead.NoInteresado
                    || Categoria == CategoriaLead.Convertido
                    || Categoria == CategoriaLead.Incontactable;
            }

            public bool PuedeRecibirSeguimientos()
            {
                return !EsEstadoTerminal();
            }

            public void MarcarComoIncontactable()
            {
                Categoria = CategoriaLead.Incontactable;
                Estado = EstadoCliente.Perdido;
            }

            public void MarcarComoConvertido()
            {
                Categoria = CategoriaLead.Convertido;
                Estado = EstadoCliente.Cerrado;
            }

            public void MarcarComoNoInteresado()
            {
                Categoria = CategoriaLead.NoInteresado;
                Estado = EstadoCliente.Perdido;
            }

            public void EstablecerOrigenLead(OrigenLead origenLead)
            {
                OrigenLead = origenLead;
            }

            public void ActualizarEtapa(EtapaNegociacion nuevaEtapa)
            {
                EtapaNegociacion = nuevaEtapa;
            }

        }
    }