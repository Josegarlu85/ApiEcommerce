using Microsoft.EntityFrameworkCore;
using ApiEcommerce.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }

// Configuración avanzada del modelo (relaciones, restricciones, seeds, etc)
// Identity también usa este método para crear sus propias tablas.
// Si no necesitas configuraciones adicionales, simplemente llama a base
protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Category> Categories {get;set;}

    public DbSet<Product> Products {get;set;}

    //public DbSet<User> Users {get;set;}
//atributo añadido para identity
    public DbSet<ApplicationUser> ApplicationUsers {get;set;}


}