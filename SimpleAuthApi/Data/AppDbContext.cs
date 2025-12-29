using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SimpleAuthApi.Models;

namespace SimpleAuthApi.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser, IdentityRole,string>
    {
        //constructor
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<TaskAssignement> TaskAssignements { get; set; }
        public DbSet<GlobalSettings> GlobalSettings { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Ici, vous pouvez configurer les relations complexes ou la base de données.
            builder.Entity<TaskAssignement>()
                .HasKey(ta => new { ta.TaskId, ta.UserId });//indiquer la clé composite
            builder.Entity<TaskAssignement>()
                .HasOne(ta => ta.Task) // prend une instance de taskassignement ,ta,et utilise sa prop Task
                .WithMany(t => t.Assignements)
                .HasForeignKey(ta => ta.TaskId);

            /*builder.Entity<TaskAssignment>()
    .HasOne<IdentityUser>() // Spécifie la classe cible
    .WithMany() // Ne spécifie rien du côté utilisateur, car on n'a pas besoin d'une liste 'Assignments' dans IdentityUser
    .HasForeignKey(ta => ta.UserId)
    .OnDelete(DeleteBehavior.Cascade); // Exemple de règle explicite */
        }
    }
}
