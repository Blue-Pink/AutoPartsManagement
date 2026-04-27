using APM.DbEntities;
using APM.DbEntities.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace APM.ConTaxi
{
    internal class APMDbContext : DbContext
    {

        public DbSet<User> User { get; set; }

        public DbSet<Role> Role { get; set; }

        public DbSet<UserRole> UserRole { get; set; }

        public DbSet<EntityRecord> EntityRecord { get; set; }

        public DbSet<RolePermission> RolePermission { get; set; }

        public DbSet<Category> Category { get; set; }

        public DbSet<Unit> Unit { get; set; }

        public DbSet<Part> Part { get; set; }

        public DbSet<UserRoleView> UserRoleView { get; set; }

        public DbSet<PartView> PartView { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var now = new DateTime(2026, 3, 25, 6, 20, 21);

            base.OnModelCreating(modelBuilder);

            #region 索引

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<UserRole>()
                .HasIndex(u => new { u.UserId, u.RoleId })
                .IsUnique();

            modelBuilder.Entity<EntityRecord>()
                .HasIndex(er => er.FullName)
                .IsUnique();

            modelBuilder.Entity<RolePermission>()
                .HasIndex(rp => new { rp.RoleId, rp.EntityId })
                .IsUnique();

            modelBuilder.Entity<Part>(entity =>
            {
                // OE码唯一索引，防止重复录入
                entity.HasIndex(p => p.OECode).IsUnique();

                // 设置价格精度
                entity.Property(p => p.CostPrice).HasPrecision(18, 2);
                entity.Property(p => p.SellingPrice).HasPrecision(18, 2);
            });

            #endregion

            #region 关系

            modelBuilder.Entity<UserRole>()
                .HasOne(user => user.User)
                .WithMany(userRole => userRole.UserRoles)
                .HasForeignKey(user => user.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(user => user.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(user => user.RoleId);

            #endregion

            #region 种子数据

            var basicRoles = new List<Role> {
                new() {
                    Id = new Guid("7035810c-ede3-4ffc-ab72-14cf85061a04"),
                    RoleName = "Administrator",
                    Description = "系统管理员",
                    CreatedAt = now,
                    ModifiedAt = now
                },
                new() {
                    Id = new Guid("a4ee65f8-ebd9-47aa-ad77-7ecad0cf9db6"),
                    RoleName = "Warehouse Manager",
                    Description = "仓库管理员",
                    CreatedAt = now,
                    ModifiedAt = now
                }
            };
            var basicUsers = new List<User> {
                new() {
                    Id = new Guid("f1a89d52-1c0f-4070-a6dd-761a04fcf7f4"),
                    Username = "Administrator",
                    PasswordHash = "$2a$11$ToqAlthCo6lbu4j6kAb8m.7XIP9gCOUgQRCBsSorupnn88xK9S5ee",
                    Realname = "Administrator",
                    CreatedAt = now,
                    ModifiedAt = now
                },
            };
            var basicUserRole = new List<UserRole> {
                new() {
                    Id= new Guid("8b006bae-2330-4b7e-a6b5-e3defe6e92ef"),
                    UserId = basicUsers.First().Id,
                    RoleId = basicRoles.First().Id,
                    CreatedAt = now,
                    ModifiedAt = now,
                    AssignedAt = now,
                }
            };

            #endregion

            modelBuilder.Entity<Role>().HasData(basicRoles);
            modelBuilder.Entity<User>().HasData(basicUsers);
            modelBuilder.Entity<UserRole>().HasData(basicUserRole);

            #region 映射视图

            modelBuilder.Entity<UserRoleView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_UserRoleView");
            });

            modelBuilder.Entity<PartView>(eb =>
            {
                eb.HasNoKey(); // 视图无主键
                eb.ToView("vw_PartView"); // 映射到数据库视图名称
            });

            #endregion

            #region Migration Sql
            //            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_UserRoleView");
            //            migrationBuilder.Sql(@"CREATE VIEW vw_UserRoleView AS
            //SELECT 
            //    u.Id AS UserId,
            //    r.Id AS RoleId,
            //    u.Username, 
            //    u.Realname, 
            //    r.RoleName, 
            //    r.Description AS RoleDescription,
            //    ur.AssignedAt
            //FROM [User] u
            //JOIN UserRole ur ON u.Id = ur.UserId
            //JOIN Role r ON r.Id = ur.RoleId");
            #endregion
        }

        public APMDbContext(DbContextOptions options) : base(options)
        {

        }
    }
}
