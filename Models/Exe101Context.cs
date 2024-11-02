using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EXE.Models;

public partial class Exe101Context : DbContext
{
    public Exe101Context()
    {
    }

    public Exe101Context(DbContextOptions<Exe101Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Account { get; set; }
    public virtual DbSet<Interpreters> Interpreters { get; set; }
    public virtual DbSet<Bookings> Bookings { get; set; }
    public virtual DbSet<Blog> Blogs { get; set; }
    public virtual DbSet<Reviews> Reviews { get; set; }
    public virtual DbSet<Points> Points { get; set; }
    public virtual DbSet<CalendarAvailability> CalendarAvailability { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=PHUC\\PHUCTRAN;Database=EXE201;User Id=sa;Password=12345678;TrustServerCertificate=true;Trusted_Connection=SSPI;Encrypt=false;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("Account");

            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.FullName).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.Phone).HasMaxLength(50);

        });
        modelBuilder.Entity<Interpreters>(entity =>
        {
            entity.ToTable("Interpreters");

            entity.HasKey(e => e.InterpreterId); 
        });
        modelBuilder.Entity<Bookings>(entity =>
        {
            entity.ToTable("Bookings");

            entity.HasKey(e => e.BookingId); 

        });        
        modelBuilder.Entity<Blog>(entity =>
        {
            entity.ToTable("Blogs");

            entity.HasKey(e => e.BlogId); 

        });        
        modelBuilder.Entity<Reviews>(entity =>
        {
            entity.ToTable("Reviews");

            entity.HasKey(e => e.ReviewId); 

        });          
        modelBuilder.Entity<Points>(entity =>
        {
            entity.ToTable("Points");

            entity.HasKey(e => e.PointId); 

        });        
        modelBuilder.Entity<CalendarAvailability>(entity =>
        {
            entity.ToTable("CalendarAvailability ");

            entity.HasKey(e => e.AvailabilityId); 

        });
    }


    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
