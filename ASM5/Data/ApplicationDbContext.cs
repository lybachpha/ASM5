using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ASM5.Models;
using ASM5.Areas.Data;

namespace ASM5.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ASM5.Models.JobList> JobList { get; set; } = default!;
        public DbSet<ASM5.Models.JobApplication> JobApplication { get; set; } = default!;
       
    }
}
