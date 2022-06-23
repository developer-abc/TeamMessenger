namespace TeamMessenger.Server.Data
{
    public static class DbInitializer
    {
        public static void Initialize(MessagesContext context)
        {
            context.Database.EnsureCreated();
            context.SaveChanges();
        }
    }
}
