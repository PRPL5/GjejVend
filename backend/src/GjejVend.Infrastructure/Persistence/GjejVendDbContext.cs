using Microsoft.EntityFrameworkCore;

namespace GjejVend.Infrastructure.Persistence;

public sealed class GjejVendDbContext(DbContextOptions<GjejVendDbContext> options) : DbContext(options);
