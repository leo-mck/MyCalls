using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MyCalls.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Call> Calls { get; set; }
        public DbSet<Person> Callers { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<CustomFilter> CustomFilters { get; set; }

        public AppDbContext():base("DefaultConnection")
        {
            
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Call>().HasRequired(x => x.Caller).WithMany().WillCascadeOnDelete(false);
            modelBuilder.Entity<Call>().HasRequired(x => x.Callee).WithMany().WillCascadeOnDelete(false);
            modelBuilder.Entity<Person>().HasMany(x => x.Tags).WithMany();
            base.OnModelCreating(modelBuilder);
        }
    }




    public class Call : BaseEntity
    {
        public Person Caller { get; set; }
        public int CallerId { get; set; }
        public Person Callee { get; set; }
        public int CalleeId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime EndedAt { get; set; }
        public int DurationSeconds { get; set; }


        public override string ToString()
        {
            return $"At {StartedAt} From {Caller.Name} ({Caller.Number}) with Duration: {new TimeSpan(0, 0, DurationSeconds).ToString("g")}";
        }
    }




    public class Person : BaseEntity
    {
        public string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public ICollection<Tag> Tags { get; set; }
        public CallerPriority CallerPriority { get; set; }
        public string Number { get; internal set; }
    }

    public enum CallerPriority
    {
        NotDefined,
        Low,
        Normal,
        High,
        Urgent
    }

    public enum TagType
    {
        Global,
        Caller,
        Call
    }

    public class Tag : BaseEntity
    {
        public string Name { get; set; }
        public string Color { get; set; }
    }

    public class CustomFilter : BaseEntity
    {
        public string Name { get; set; }
        public Person User { get; set; }
        public int? UserId { get; set; }
        public string FilterJson { get; set; }
    }

    public class BaseEntity
    {
        public int Id { get; set; }
    }

}
