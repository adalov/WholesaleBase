using System.Data.Entity;

namespace WholesaleBase.Models
{
    public class WholesaleContext : DbContext
    {
        public WholesaleContext() :  base("ConnectionString") { }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Goods> Goods { get; set; }
        public DbSet<MeasureUnit> MeasureUnits { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<TurnoverMember> TurnoverMembers { get; set; }
        public DbSet<TurnoverNotice> TurnoverNotices { get; set; }
        public DbSet<TurnoverNoticeStatus> TurnoverNoticeStatus { get; set; }
        public DbSet<TurnoverType> TurnoverTypes { get; set; }
        public DbSet<InventoryResult> InventoryResults { get; set; }
        public DbSet<GoodsMovement> GoodsMovements { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<TurnoverMember>()
            //    .HasMany(c => c.ContractsForContractor)
            //    .WithRequired(o => o.Contractor)
            //    .WillCascadeOnDelete(false);
            //modelBuilder.Entity<Warehouse>()
            //    .HasMany(c => c.ContractsForWarehouse)
            //    .WithRequired(o => o.Warehouse)
            //    .WillCascadeOnDelete(false);
            //modelBuilder.Entity<Contract>()
            //    .HasRequired(c => c.Contractor)
            //    .WithMany()
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<Contract>()
            //    .HasRequired(s => s.Warehouse)
            //    .WithMany()
            //    .WillCascadeOnDelete(false);


            modelBuilder.Entity<Contract>().HasRequired(i => i.Contractor).WithMany().HasForeignKey(i => i.ContractorId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Contract>().HasRequired(i => i.Warehouse).WithMany().HasForeignKey(i => i.WarehouseId).WillCascadeOnDelete(false);
            modelBuilder.Entity<GoodsMovement>().HasRequired(i => i.Warehouse).WithMany().HasForeignKey(i => i.WarehouseId).WillCascadeOnDelete(false);
            modelBuilder.Entity<InventoryResult>().HasRequired(i => i.Warehouse).WithMany().HasForeignKey(i => i.WarehouseId).WillCascadeOnDelete(false);
            modelBuilder.Entity<TurnoverNotice>().HasRequired(i => i.Warehouse).WithMany().HasForeignKey(i => i.WarehouseId).WillCascadeOnDelete(false);
            modelBuilder.Entity<TurnoverNotice>().HasRequired(i => i.TurnoverMember).WithMany().HasForeignKey(i => i.TurnoverMemberId).WillCascadeOnDelete(false);
        }
    }
}