using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskTracker.DataAccess.Entities;

namespace TaskTracker.DataAccess.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        // Configure the UserEntity properties and relationships in case you need to customize the default behavior of IdentityUser<Guid>
    }
}