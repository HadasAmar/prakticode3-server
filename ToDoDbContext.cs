// using System;
// using System.Collections.Generic;
// using Microsoft.EntityFrameworkCore;
// using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

// namespace TodoApi;

// public partial class MyschemaContext : DbContext
// {

//     public DbSet<Item> Items { get; set; }  

//     public DbSet<Customer> Customers {get; set;}


//     public MyschemaContext()
//     {
//     }

//     public MyschemaContext(DbContextOptions<MyschemaContext> options)
//         : base(options)
//     {
//     }

//     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//         => optionsBuilder.UseMySql("name=ToDoDB", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.40-mysql"));

//     protected override void OnModelCreating(ModelBuilder modelBuilder)
//     {
//         modelBuilder
//             .UseCollation("utf8mb4_0900_ai_ci")
//             .HasCharSet("utf8mb4");

//         OnModelCreatingPartial(modelBuilder);
//     }

//     partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
// }
