using CotizacionMVC.Data;
using CotizacionMVC.Data.Repositorios.Interfaces;
using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Models.Enums;
using Microsoft.EntityFrameworkCore;

public class LeadRepository : ILeadRepository
{
    private readonly ApplicationDbContext _context;

    public LeadRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Lead?> GetByIdAsync(Guid id)
        => await _context.Leads.FindAsync(id);

    public async Task<IReadOnlyList<Lead>> GetAllAsync()
        => await _context.Leads.ToListAsync();

    public async Task AddAsync(Lead lead)
        => await _context.Leads.AddAsync(lead);

    public void Update(Lead lead)
        => _context.Leads.Update(lead);

    public void Delete(Lead lead)
        => _context.Leads.Remove(lead);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public async Task<IReadOnlyList<Lead>> ObtenerConClientesAsync()
        => await _context.Leads
            .Include(l => l.Cliente)
            .Include(l => l.Empresa)
            .OrderByDescending(l => l.FechaCreacion)
            .ToListAsync();

    public async Task<IReadOnlyList<Lead>> ObtenerPorVendedorAsync(Guid vendedorId)
        => await _context.Leads
            .Include(l => l.Cliente)
            .Include(l => l.Empresa)
            .Where(l => l.VendedorAsignadoId == vendedorId)
            .OrderByDescending(l => l.FechaCreacion)
            .ToListAsync();

    public async Task<IReadOnlyList<Lead>> ObtenerPorClientesAsync(List<Guid> clienteIds)
    {
        if (!clienteIds.Any())
            return new List<Lead>();

        return await _context.Leads
            .Include(l => l.Cliente)
            .Where(l => l.ClienteId != null && clienteIds.Contains(l.ClienteId.Value))
            .OrderByDescending(l => l.FechaCreacion)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Lead>> ObtenerSinVendedorPorClienteAsync(Guid clienteId)
        => await _context.Leads
            .Where(l => l.ClienteId == clienteId && l.VendedorAsignadoId == null)
            .ToListAsync();

    public async Task<int> ContarPerdidosDesdeAsync(DateTime fechaInicio)
        => await _context.Leads
            .Where(l => l.Estado == EstadoCliente.Perdido && l.UltimoSeguimiento >= fechaInicio)
            .CountAsync();

    public async Task<IReadOnlyList<Lead>> ObtenerTodosAsync()
    {
        return await _context.Leads
            .Include(l => l.Cliente)
            .Include(l => l.Empresa)
            .OrderByDescending(l => l.FechaCreacion)
            .ToListAsync();
    }

    public IQueryable<Lead> ObtenerQueryable()
    {
        return _context.Leads.AsQueryable();
    }

}