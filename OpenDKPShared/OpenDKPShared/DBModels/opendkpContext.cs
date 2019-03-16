using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace OpenDKPShared.DBModels
{
    public partial class opendkpContext : DbContext
    {
        public virtual DbSet<Adjustments> Adjustments { get; set; }
        public virtual DbSet<AdminSettings> AdminSettings { get; set; }
        public virtual DbSet<Audit> Audit { get; set; }
        public virtual DbSet<Cache> Cache { get; set; }
        public virtual DbSet<Characters> Characters { get; set; }
        public virtual DbSet<Clients> Clients { get; set; }
        public virtual DbSet<Items> Items { get; set; }
        public virtual DbSet<ItemsXCharacters> ItemsXCharacters { get; set; }
        public virtual DbSet<Pools> Pools { get; set; }
        public virtual DbSet<Raids> Raids { get; set; }
        public virtual DbSet<Ticks> Ticks { get; set; }
        public virtual DbSet<TicksXCharacters> TicksXCharacters { get; set; }
        public virtual DbSet<UserRequests> UserRequests { get; set; }
        public virtual DbSet<UserXCharacter> UserXCharacter { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql("server=#server#;port=3306;user=#username#;password=#password#;database=#dbname#;Convert Zero Datetime=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Adjustments>(entity =>
            {
                entity.HasKey(e => new { e.IdAdjustment, e.ClientId });

                entity.ToTable("adjustments");

                entity.HasIndex(e => e.ClientId)
                    .HasName("fkey_adjust_clientid_idx");

                entity.HasIndex(e => e.IdAdjustment)
                    .HasName("id_adjustment_UNIQUE")
                    .IsUnique();

                entity.HasIndex(e => e.IdCharacter)
                    .HasName("fk_adjust_char_id_idx");

                entity.Property(e => e.IdAdjustment)
                    .HasColumnName("id_adjustment")
                    .HasColumnType("int(11)")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ClientId)
                    .HasColumnName("clientId")
                    .HasMaxLength(255);

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasMaxLength(45);

                entity.Property(e => e.IdCharacter)
                    .HasColumnName("id_character")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(45);

                entity.Property(e => e.Timestamp)
                    .HasColumnName("timestamp")
                    .HasColumnType("datetime");

                entity.Property(e => e.Value)
                    .HasColumnName("value")
                    .HasDefaultValueSql("'0'");

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.Adjustments)
                    .HasForeignKey(d => d.ClientId)
                    .HasConstraintName("fkey_adjust_clientid");

                entity.HasOne(d => d.IdCharacterNavigation)
                    .WithMany(p => p.Adjustments)
                    .HasPrincipalKey(p => p.IdCharacter)
                    .HasForeignKey(d => d.IdCharacter)
                    .HasConstraintName("fkey_adjust_char_id");
            });

            modelBuilder.Entity<AdminSettings>(entity =>
            {
                entity.HasKey(e => new { e.SettingName, e.ClientId });

                entity.ToTable("admin_settings");

                entity.HasIndex(e => e.ClientId)
                    .HasName("fkey_admin_clientId_idx");

                entity.HasIndex(e => e.SettingName)
                    .HasName("setting_name_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.SettingName)
                    .HasColumnName("setting_name")
                    .HasMaxLength(45);

                entity.Property(e => e.ClientId)
                    .HasColumnName("clientId")
                    .HasMaxLength(255);

                entity.Property(e => e.SettingValue)
                    .IsRequired()
                    .HasColumnName("setting_value");

                entity.Property(e => e.UpdatedBy)
                    .IsRequired()
                    .HasColumnName("updated_by")
                    .HasMaxLength(45);

                entity.Property(e => e.UpdatedTimestamp)
                    .HasColumnName("updated_timestamp")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.AdminSettings)
                    .HasForeignKey(d => d.ClientId)
                    .HasConstraintName("fkey_admin_clientId");
            });

            modelBuilder.Entity<Audit>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.ClientId });

                entity.ToTable("audit");

                entity.HasIndex(e => e.ClientId)
                    .HasName("fkey_audit_clientid_idx");

                entity.HasIndex(e => e.Id)
                    .HasName("id_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ClientId)
                    .HasColumnName("clientId")
                    .HasMaxLength(255);

                entity.Property(e => e.Action)
                    .IsRequired()
                    .HasMaxLength(45);

                entity.Property(e => e.CognitoUser)
                    .IsRequired()
                    .HasMaxLength(45);

                entity.Property(e => e.Timestamp).HasColumnType("datetime");

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.Audit)
                    .HasForeignKey(d => d.ClientId)
                    .HasConstraintName("fkey_audit_clientid");
            });

            modelBuilder.Entity<Cache>(entity =>
            {
                entity.ToTable("cache");

                entity.HasIndex(e => e.Id)
                    .HasName("id_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CacheExpires)
                    .HasColumnName("cache_expires")
                    .HasColumnType("datetime");

                entity.Property(e => e.CacheName)
                    .IsRequired()
                    .HasColumnName("cache_name")
                    .HasMaxLength(45);

                entity.Property(e => e.CacheUpdated)
                    .HasColumnName("cache_updated")
                    .HasColumnType("datetime");

                entity.Property(e => e.CacheValue)
                    .IsRequired()
                    .HasColumnName("cache_value");

                entity.Property(e => e.ClientId)
                    .IsRequired()
                    .HasColumnName("clientId")
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<Characters>(entity =>
            {
                entity.HasKey(e => new { e.IdCharacter, e.ClientId });

                entity.ToTable("characters");

                entity.HasIndex(e => e.ClientId)
                    .HasName("fkey_clientid_idx");

                entity.HasIndex(e => e.IdCharacter)
                    .HasName("id_UNIQUE")
                    .IsUnique();

                entity.HasIndex(e => e.Name)
                    .HasName("name_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.IdCharacter)
                    .HasColumnName("id_character")
                    .HasColumnType("int(11)")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ClientId)
                    .HasColumnName("clientId")
                    .HasMaxLength(256);

                entity.Property(e => e.Active)
                    .HasColumnName("active")
                    .HasColumnType("tinyint(4)")
                    .HasDefaultValueSql("'1'");

                entity.Property(e => e.Class)
                    .HasColumnName("class")
                    .HasMaxLength(45);

                entity.Property(e => e.Gender)
                    .HasColumnName("gender")
                    .HasMaxLength(45);

                entity.Property(e => e.Guild)
                    .HasColumnName("guild")
                    .HasMaxLength(45);

                entity.Property(e => e.IdAssociated)
                    .HasColumnName("id_associated")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'-1'");

                entity.Property(e => e.Level)
                    .HasColumnName("level")
                    .HasColumnType("int(11)");

                entity.Property(e => e.MainChange)
                    .HasColumnName("main_change")
                    .HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(45);

                entity.Property(e => e.Race)
                    .HasColumnName("race")
                    .HasMaxLength(45);

                entity.Property(e => e.Rank)
                    .HasColumnName("rank")
                    .HasMaxLength(45);

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.Characters)
                    .HasForeignKey(d => d.ClientId)
                    .HasConstraintName("fkey_clientid");
            });

            modelBuilder.Entity<Clients>(entity =>
            {
                entity.HasKey(e => e.ClientId);

                entity.ToTable("clients");

                entity.HasIndex(e => e.ClientId)
                    .HasName("clientId_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.ClientId)
                    .HasColumnName("clientId")
                    .HasMaxLength(256);

                entity.Property(e => e.AssumedRole)
                    .HasColumnName("assumedRole")
                    .HasMaxLength(256);

                entity.Property(e => e.Forums)
                    .HasColumnName("forums")
                    .HasMaxLength(256);

                entity.Property(e => e.Identity)
                    .IsRequired()
                    .HasColumnName("identity")
                    .HasMaxLength(256);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(256);

                entity.Property(e => e.Subdomain)
                    .IsRequired()
                    .HasColumnName("subdomain")
                    .HasMaxLength(256);

                entity.Property(e => e.UserPool)
                    .IsRequired()
                    .HasColumnName("userPool")
                    .HasMaxLength(256);

                entity.Property(e => e.WebClientId)
                    .IsRequired()
                    .HasColumnName("webClientId")
                    .HasMaxLength(256);

                entity.Property(e => e.Website)
                    .HasColumnName("website")
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<Items>(entity =>
            {
                entity.HasKey(e => e.IdItem);

                entity.ToTable("items");

                entity.Property(e => e.IdItem)
                    .HasColumnName("id_item")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Lucylink)
                    .HasColumnName("lucylink")
                    .HasMaxLength(45);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(45);
            });

            modelBuilder.Entity<ItemsXCharacters>(entity =>
            {
                entity.HasKey(e => new { e.TransactionId, e.ClientId, e.CharacterId, e.ItemId, e.RaidId });

                entity.ToTable("items_x_characters");

                entity.HasIndex(e => e.CharacterId)
                    .HasName("fkey_items_character_idx");

                entity.HasIndex(e => e.ClientId)
                    .HasName("fkey_items_clientId_idx");

                entity.HasIndex(e => e.ItemId)
                    .HasName("fkey_items_item_idx");

                entity.HasIndex(e => e.RaidId)
                    .HasName("fkey_items_raid_id_idx");

                entity.HasIndex(e => e.TransactionId)
                    .HasName("transaction_id_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.TransactionId)
                    .HasColumnName("transaction_id")
                    .HasColumnType("int(11)")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ClientId)
                    .HasColumnName("clientId")
                    .HasMaxLength(255);

                entity.Property(e => e.CharacterId)
                    .HasColumnName("character_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ItemId)
                    .HasColumnName("item_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.RaidId)
                    .HasColumnName("raid_id")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'-1'");

                entity.Property(e => e.Dkp)
                    .HasColumnName("dkp")
                    .HasDefaultValueSql("'0'");

                entity.HasOne(d => d.Character)
                    .WithMany(p => p.ItemsXCharacters)
                    .HasPrincipalKey(p => p.IdCharacter)
                    .HasForeignKey(d => d.CharacterId)
                    .HasConstraintName("fkey_items_character");

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.ItemsXCharacters)
                    .HasForeignKey(d => d.ClientId)
                    .HasConstraintName("fkey_items_clientId");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.ItemsXCharacters)
                    .HasForeignKey(d => d.ItemId)
                    .HasConstraintName("fkey_items_item");

                entity.HasOne(d => d.Raid)
                    .WithMany(p => p.ItemsXCharacters)
                    .HasPrincipalKey(p => p.IdRaid)
                    .HasForeignKey(d => d.RaidId)
                    .HasConstraintName("fkey_items_raid_id");
            });

            modelBuilder.Entity<Pools>(entity =>
            {
                entity.HasKey(e => e.IdPool);

                entity.ToTable("pools");

                entity.HasIndex(e => e.IdPool)
                    .HasName("id_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.IdPool)
                    .HasColumnName("id_pool")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasMaxLength(45);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(45);

                entity.Property(e => e.Order)
                    .HasColumnName("order")
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'99'");
            });

            modelBuilder.Entity<Raids>(entity =>
            {
                entity.HasKey(e => new { e.IdRaid, e.ClientId });

                entity.ToTable("raids");

                entity.HasIndex(e => e.ClientId)
                    .HasName("fkey_raids_clientid_idx");

                entity.HasIndex(e => e.IdPool)
                    .HasName("fkey_raids_pool_id_idx");

                entity.HasIndex(e => e.IdRaid)
                    .HasName("id_raid_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.IdRaid)
                    .HasColumnName("id_raid")
                    .HasColumnType("int(11)")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ClientId)
                    .HasColumnName("clientId")
                    .HasMaxLength(255);

                entity.Property(e => e.IdPool)
                    .HasColumnName("id_pool")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(45);

                entity.Property(e => e.Timestamp)
                    .HasColumnName("timestamp")
                    .HasColumnType("datetime");

                entity.Property(e => e.UpdatedBy)
                    .HasColumnName("updated_by")
                    .HasMaxLength(45)
                    .HasDefaultValueSql("'system'");

                entity.Property(e => e.UpdatedTimestamp)
                    .HasColumnName("updated_timestamp")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.Raids)
                    .HasForeignKey(d => d.ClientId)
                    .HasConstraintName("fkey_raids_clientid");

                entity.HasOne(d => d.IdPoolNavigation)
                    .WithMany(p => p.Raids)
                    .HasForeignKey(d => d.IdPool)
                    .HasConstraintName("fkey_raids_pool_id");
            });

            modelBuilder.Entity<Ticks>(entity =>
            {
                entity.HasKey(e => new { e.TickId, e.ClientId });

                entity.ToTable("ticks");

                entity.HasIndex(e => e.ClientId)
                    .HasName("fkey_entries_clientId_idx");

                entity.HasIndex(e => e.RaidId)
                    .HasName("fkey_entries_raids_id_idx");

                entity.HasIndex(e => e.TickId)
                    .HasName("tick_id_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.TickId)
                    .HasColumnName("tick_id")
                    .HasColumnType("int(11)")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ClientId)
                    .HasColumnName("clientId")
                    .HasMaxLength(255);

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasMaxLength(45);

                entity.Property(e => e.RaidId)
                    .HasColumnName("raid_id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Value)
                    .HasColumnName("value")
                    .HasDefaultValueSql("'0'");

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.Ticks)
                    .HasForeignKey(d => d.ClientId)
                    .HasConstraintName("fkey_entries_clientId");

                entity.HasOne(d => d.Raid)
                    .WithMany(p => p.Ticks)
                    .HasPrincipalKey(p => p.IdRaid)
                    .HasForeignKey(d => d.RaidId)
                    .HasConstraintName("fkey_entries_raids_id");
            });

            modelBuilder.Entity<TicksXCharacters>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.ClientId });

                entity.ToTable("ticks_x_characters");

                entity.HasIndex(e => e.ClientId)
                    .HasName("fkey_tick_clientId_idx");

                entity.HasIndex(e => e.Id)
                    .HasName("id_UNIQUE")
                    .IsUnique();

                entity.HasIndex(e => e.IdCharacter)
                    .HasName("fkey_tick_charId_idx");

                entity.HasIndex(e => e.IdTick)
                    .HasName("fkey_tick_tickId_idx");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ClientId)
                    .HasColumnName("clientId")
                    .HasMaxLength(255);

                entity.Property(e => e.IdCharacter)
                    .HasColumnName("id_character")
                    .HasColumnType("int(11)");

                entity.Property(e => e.IdTick)
                    .HasColumnName("id_tick")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.TicksXCharacters)
                    .HasForeignKey(d => d.ClientId)
                    .HasConstraintName("fkey_tick_clientId");

                entity.HasOne(d => d.IdCharacterNavigation)
                    .WithMany(p => p.TicksXCharacters)
                    .HasPrincipalKey(p => p.IdCharacter)
                    .HasForeignKey(d => d.IdCharacter)
                    .HasConstraintName("fkey_tick_charId");

                entity.HasOne(d => d.IdTickNavigation)
                    .WithMany(p => p.TicksXCharacters)
                    .HasPrincipalKey(p => p.TickId)
                    .HasForeignKey(d => d.IdTick)
                    .HasConstraintName("fkey_tick_tickId");
            });

            modelBuilder.Entity<UserRequests>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.ClientId });

                entity.ToTable("user_requests");

                entity.HasIndex(e => e.ClientId)
                    .HasName("fkey_userrequests_clientId_idx");

                entity.HasIndex(e => e.Id)
                    .HasName("id_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ClientId)
                    .HasColumnName("clientId")
                    .HasMaxLength(255);

                entity.Property(e => e.RequestApprover)
                    .IsRequired()
                    .HasColumnName("request_approver")
                    .HasMaxLength(45);

                entity.Property(e => e.RequestDetails)
                    .IsRequired()
                    .HasColumnName("request_details");

                entity.Property(e => e.RequestStatus)
                    .HasColumnName("request_status")
                    .HasColumnType("int(11)");

                entity.Property(e => e.RequestTimestamp)
                    .HasColumnName("request_timestamp")
                    .HasColumnType("datetime");

                entity.Property(e => e.RequestType)
                    .HasColumnName("request_type")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Requestor)
                    .IsRequired()
                    .HasColumnName("requestor")
                    .HasMaxLength(45);

                entity.Property(e => e.ReviewedTimestamp)
                    .HasColumnName("reviewed_timestamp")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.UserRequests)
                    .HasForeignKey(d => d.ClientId)
                    .HasConstraintName("fkey_userrequests_clientId");
            });

            modelBuilder.Entity<UserXCharacter>(entity =>
            {
                entity.HasKey(e => new { e.User, e.IdCharacter, e.ClientId });

                entity.ToTable("user_x_character");

                entity.HasIndex(e => e.ClientId)
                    .HasName("fkey_clientId_idx");

                entity.HasIndex(e => e.IdCharacter)
                    .HasName("userCharFkey_idx");

                entity.Property(e => e.User)
                    .HasColumnName("user")
                    .HasMaxLength(45);

                entity.Property(e => e.IdCharacter)
                    .HasColumnName("id_character")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ClientId)
                    .HasColumnName("clientId")
                    .HasMaxLength(255);

                entity.Property(e => e.ApprovedBy)
                    .IsRequired()
                    .HasColumnName("approved_by")
                    .HasMaxLength(45);

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.UserXCharacter)
                    .HasForeignKey(d => d.ClientId)
                    .HasConstraintName("fkey_userxchar_clientId");

                entity.HasOne(d => d.IdCharacterNavigation)
                    .WithMany(p => p.UserXCharacter)
                    .HasPrincipalKey(p => p.IdCharacter)
                    .HasForeignKey(d => d.IdCharacter)
                    .HasConstraintName("userCharFkey");
            });
        }
    }
}
