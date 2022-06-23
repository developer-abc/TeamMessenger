using TeamMessenger.Shared.Messages;
using Microsoft.EntityFrameworkCore;

namespace TeamMessenger.Server.Data
{
    public class MessagesContext : DbContext
    {
        public MessagesContext(DbContextOptions<MessagesContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Установка ConnectionId+Signature+LastSignature как уникального ключа
            modelBuilder.Entity<Message>()
            .HasKey(p => new { p.ConnectionId, p.Signature, p.LastSignature });
        }

        public DbSet<Message> Messages { get; set; }
    }
}
