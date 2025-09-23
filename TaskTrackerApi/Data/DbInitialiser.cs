namespace TaskTrackerApi.Data
{
    public static class DbInitialiser
    {
        public static void Initialise(AppDbContext context)
        {
            context.Database.EnsureCreated();

            // Check if database has been seeded
            if (context.Users.Any())
            {
                return; // DB has been seeded
            }

            // Add seeded data here if necessary
            context.SaveChanges();
        }
    }
}