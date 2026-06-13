using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;
using Microsoft.EntityFrameworkCore;

namespace Entities
{
    public class ShopDbContext : DbContext
    {
        public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options)
        {


        }

        public DbSet<Product> products { get; set; }
        public DbSet<Branch> branches { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Product>().ToTable("products");
            modelBuilder.Entity<Branch>().ToTable("branches");

            modelBuilder.Entity<Product>().HasData(



            new Product { Name = "لپ تاب", Description = "core i7", BranchId = 1, HasDiscount = true, Discount = 200000, Price = 2000000, DisconType = DisconTypeEnum.price, Id = 1, },
            new Product { Name = "گوشی", Description = "redmi note 14", BranchId = 2, HasDiscount = true, Discount = 200000, Price = 2000000, DisconType = DisconTypeEnum.price, Id = 2, },
            new Product { Name = "موس", Description = "good", BranchId = 1, HasDiscount = true, Discount = 200000, Price = 700000, DisconType = DisconTypeEnum.price, Id = 3, },
            new Product { Name = "مودم", Description = "همراه اول ", BranchId = 3, HasDiscount = true, Discount = 10, Price = 10000000, DisconType = DisconTypeEnum.percent, Id = 4, },
            new Product { Name = "آیفون", Description = "17 pro max ", BranchId = 1, HasDiscount = true, Discount = 200000, Price = 2000000, DisconType = DisconTypeEnum.price, Id = 5, },
            new Product { Name = "ps5", Description = "پرو", BranchId = 2, HasDiscount = true, Discount = 15, Price = 100000000, DisconType = DisconTypeEnum.percent, Id = 6, }

                );

            modelBuilder.Entity<Branch>().HasData(


                new Branch { Id = 1, Name = "شهر آرا" },
                new Branch { Id = 2, Name = "ونک" },
                new Branch { Id = 3, Name = "سعادت آباد" }

                );



            modelBuilder.Entity<Branch>().Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("ونک");


            modelBuilder.Entity<Product>()
                .HasOne(x => x.Branch)
                .WithMany(y => y.products)
                .HasForeignKey(t => t.BranchId)
                .OnDelete(DeleteBehavior.Restrict);


        }



    }
}
