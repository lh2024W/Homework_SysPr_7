using Microsoft.EntityFrameworkCore;

namespace Homework_SysPr_7
{
    class Program
    {
        static async Task Main()
        {
            await InitializeAsync();
            try
            {
                User currentUser = await LoadUserDataAsync();
                if (currentUser != null)
                {
                    Console.WriteLine($"ID: {currentUser.Id}, Name: {currentUser.Name}, Email: {currentUser.Email}");
                }
            }
            catch (OperationCanceledException) 
            {
                Console.WriteLine("Запрос к серверу был отменен из-за длительного ожидания.");
            }

        }

        static async Task<User?> LoadUserDataAsync()
        {
            //таймер для отмены операции через 10 секунд
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Task<User> loadTask = Task.Run(async () =>
            {
                //await Task.Delay(TimeSpan.FromSeconds(11)); //Имитация длительной операции

                using (ApplicationContext db = new ApplicationContext())
                {
                   return db.Users.FirstOrDefault()!;
                }
            });

            //Дожидаемся выполнения задачи с учетом отмены по истечении времени
            if (await Task.WhenAny(loadTask, Task.Delay(TimeSpan.FromSeconds(10), cancellationToken)) == loadTask)
            {
                return await loadTask;
            }
            else
            {
                throw new OperationCanceledException("Запрос к серверу был отменен из-за длительного ожидания.");
            }
        }

        //static async Task<User?> LoadUserDataAsync()
        //{
        //    //таймер для отмены операции через 10 секунд
        //    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        //    CancellationToken cancellationToken = cancellationTokenSource.Token;

        //    using (ApplicationContext context = new ApplicationContext())
        //    {
        //        //Асинхронный запрос к базе данных
        //        User? user = await context.Users.FirstOrDefaultAsync(cancellationToken);
        //        return user;
        //    }
        //}

        static async Task InitializeAsync()
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                db.Users.AddRange
                    (
                    new User { Name = "Tom", Email = "tom@gmail.com" },
                    new User { Name = "Tom", Email = "tom@gmail.com" }
                    );
                db.SaveChanges();
            }
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=WIN-UKQRC56FDU3;Database=testDb;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }
}
