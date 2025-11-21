using BookManagementAPI.Features;
using Microsoft.EntityFrameworkCore;

namespace Data;

public class ApplicationContext : DbContext
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options) 
        : base(options)
    {
    }
    
    public DbSet<Book> Books { get; set; }
}