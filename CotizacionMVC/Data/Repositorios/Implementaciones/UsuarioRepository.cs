using CotizacionMVC.Data;
using CotizacionMVC.Data.Repositorios.Interfaces;
using CotizacionMVC.Models.Entidades;
using Microsoft.EntityFrameworkCore;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly ApplicationDbContext _context;

    public UsuarioRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> GetByIdAsync(Guid id)
        => await _context.Users.FindAsync(id);

    public async Task<IReadOnlyList<Usuario>> GetAllAsync()
        => await _context.Users.ToListAsync();

    public async Task AddAsync(Usuario usuario)
        => await _context.Users.AddAsync(usuario);

    public void Update(Usuario usuario)
        => _context.Users.Update(usuario);

    public void Delete(Usuario usuario)
        => _context.Users.Remove(usuario);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public async Task<IReadOnlyList<Usuario>> ObtenerActivosAsync()
        => await _context.Users
            .Where(u => u.Activo)
            .OrderBy(u => u.NombreCompleto)
            .ToListAsync();

    public async Task<IReadOnlyList<Usuario>> ObtenerPorRolAsync(string rol)
    {
        var roleId = await _context.Roles
            .Where(r => r.Name == rol)
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        if (roleId == Guid.Empty)
            return new List<Usuario>();

        var userIds = await _context.UserRoles
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.UserId)
            .ToListAsync();

        return await _context.Users
            .Where(u => userIds.Contains(u.Id) && u.Activo)
            .OrderBy(u => u.NombreCompleto)
            .ToListAsync();
    }

    public async Task<Usuario?> ObtenerPorEmailAsync(string email)
        => await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

    public async Task<IReadOnlyList<Usuario>> ObtenerVendedoresActivosAsync()
    {
        var roleId = await _context.Roles
            .Where(r => r.Name == "Vendedor")
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        if (roleId == Guid.Empty)
            return new List<Usuario>();

        var userIds = await _context.UserRoles
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.UserId)
            .ToListAsync();

        return await _context.Users
            .Where(u => userIds.Contains(u.Id) && u.Activo)
            .OrderBy(u => u.NombreCompleto)
            .ToListAsync();
    }
}