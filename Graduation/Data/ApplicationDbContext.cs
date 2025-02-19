using Graduation.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using System.Reflection;

namespace Graduation.Data
{
    public class ApplicationDbContext :IdentityDbContext<ApplicationUser,IdentityRole<int>,int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options) { }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ImageDetails>().HasOne(i => i.Services).WithMany(s => s.ImageDetails).HasForeignKey(i => i.ServiceId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<ImageDetails>().HasOne(i => i.Properties).WithMany(p => p.ImageDetails).HasForeignKey(i => i.PropertyId).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Review>().HasOne(r=>r.Services).WithMany(s=>s.Reviews).HasForeignKey(r=>r.ServiceId).OnDelete(deleteBehavior: DeleteBehavior.Restrict);
            builder.Entity<Review>().HasOne(r => r.Properties).WithMany(p=>p.Reviews).HasForeignKey(r => r.PropertyId).OnDelete(deleteBehavior: DeleteBehavior.Restrict);
            //    builder.Entity<CustomAttributeData>().HasNoKey();
            //builder.Entity<Type>().Ignore(t => t.CustomAttributes);
            //builder.Entity<Type>().HasNoKey();
        }
        public DbSet<ApplicationUser> users { get; set; }
        public DbSet<AdvertisementProject> advertisements { get; set; }
        public DbSet<Complaint> complaints { get; set; }
        public DbSet<ImageDetails> images { get; set; }
        public DbSet<PropertyProject> properties { get; set; }
        public DbSet<Review> reviews { get; set; }
        public DbSet<TypeProperty> typeProperties { get; set; }
        public DbSet<ServiceProject> services { get; set; }
        public DbSet<TypeService> typeServices { get; set; }


    }
}
