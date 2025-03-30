using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pocket.Data.Models;

namespace Pocket.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IDataProtectionKeyContext
{
    public DbSet<SecureNoteEntity> SecureNotes { get; set; }

    #region IDataProtectionKeyContext

    DbSet<DataProtectionKey> IDataProtectionKeyContext.DataProtectionKeys => Set<DataProtectionKey>();

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DataProtectionKey>(b =>
        {
            b.ToTable("data_protection_keys");
            b.Property(x => x.Xml).HasColumnType("xml");
        });
    }
}