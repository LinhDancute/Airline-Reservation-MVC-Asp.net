using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using App.Models.Contacts;
using App.Models;
using App.Models.Blogs;

namespace App.Models
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        private readonly IConfiguration _configuration;
        public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration)
        : base(options)
        {
            _configuration = configuration;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //base.OnConfiguring(optionsBuilder);
            string connectionString = _configuration.GetConnectionString("AirlineReservationDb"); 
            optionsBuilder.UseSqlServer(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName.StartsWith("AspNet"))
                {
                    entityType.SetTableName(tableName.Substring(6));
                }
            }

            modelBuilder.Entity<Category>(e =>
            {
                e.ToTable("Categories");
                e.HasKey(c => c.Id);
                //e.HasIndex(c => c.Slug).IsUnique();


                // e.HasMany(c => c.Product)  // CategoryData has many Products
                // .WithOne(p => p.Category)  // ProductData has one Category
                // .HasForeignKey(p => p.CategoryID)
                // .HasConstraintName("FK_Product_Category");
            });

            modelBuilder.Entity<Post>(e =>
            {
                e.ToTable("Posts");
                e.HasKey(p => p.PostId);
                e.HasIndex(p => p.Slug).IsUnique();

                e.HasMany(c => c.Categories)
                 .WithMany(p => p.Posts)
                 .UsingEntity<PostCategory>(
                    l => l.HasOne<Category>().WithMany().HasForeignKey(e => e.CategoryID),
                    r => r.HasOne<Post>().WithMany().HasForeignKey(e => e.PostID),
                    j =>
                    {
                        j.HasKey(pc => new { pc.PostID, pc.CategoryID });
                        j.ToTable("PostCategories");
                    }
                 );
            });

            modelBuilder.Entity<PostCategory>(e =>
            {
                e.ToTable("PostCategories");
                e.HasKey(pc => new { pc.PostID, pc.CategoryID });
            });
        }

        #region DbSet
        public DbSet<App.Models.Contacts.Contact> Contacts { get; set; }
        public DbSet<App.Models.Blogs.Category> Categories { set; get; }
        public DbSet<App.Models.Blogs.Post> Posts { set; get; }
        public DbSet<App.Models.Blogs.PostCategory> PostCategories { set; get; }
        #endregion 
    }
}