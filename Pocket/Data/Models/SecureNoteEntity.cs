using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Pocket.Data.Models;

[EntityTypeConfiguration(typeof(Configuration))]
public class SecureNoteEntity
{
    public long Id { get; set; }

    [StringLength(100)]
    public required string PublicId { get; set; }

    public required byte[] Salt { get; set; }

    public required byte[] KeyHash { get; set; }

    public required byte[] Content { get; set; }

    public required Instant CreatedAt { get; set; }

    private class Configuration : IEntityTypeConfiguration<SecureNoteEntity>
    {
        public void Configure(EntityTypeBuilder<SecureNoteEntity> builder)
        {
            builder.HasIndex(x => new { x.PublicId }).IsUnique();
        }
    }
}