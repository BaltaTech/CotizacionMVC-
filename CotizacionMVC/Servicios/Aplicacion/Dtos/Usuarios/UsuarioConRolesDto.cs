namespace CotizacionMVC.Servicios.Aplicacion.Dtos.Usuarios
{
    public class UsuarioConRolesDto
    {
        public Guid Id { get; set; }
        public string NombreCompleto { get; set; } = "";
        public string Email { get; set; } = "";
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public List<string> Roles { get; set; } = new();
        public string RolesDisplay => string.Join(", ", Roles);
    }
}