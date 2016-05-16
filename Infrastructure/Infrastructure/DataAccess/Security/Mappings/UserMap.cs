using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using AFT.RegoV2.Core.Security.Data;

namespace AFT.RegoV2.Core.Services.Security.Repository.Mappings
{
    public class UserMap : EntityTypeConfiguration<User>
    {
        public UserMap(string schema)
        {
            ToTable("Users", schema);
            HasKey(u => u.Id);
            Property(u => u.Username).IsRequired().HasMaxLength(255)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("IX_Username") { IsUnique = true })); // making it unique
            Property(u => u.PasswordEncrypted).IsRequired().HasMaxLength(255);
            HasRequired(u => u.Role);
        }
    }
}
