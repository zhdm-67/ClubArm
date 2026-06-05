using System.Windows;
using ClubArm.Data;
using ClubArm.Models;
using System.Linq;

namespace ClubArm
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            using (var context = new ClubDbContext())
            {
                context.Database.EnsureCreated();
                if (!context.Computers.Any())
                {
                    context.Computers.AddRange(
                        new Computer { Name = "PC-1", Status = "Free" },
                        new Computer { Name = "PC-2", Status = "Free" },
                        new Computer { Name = "PC-3", Status = "Free" }
                    );
                    context.Tariffs.Add(new Tariff { Name = "Стандарт", PricePerMinute = 0.05m });
                    context.Clients.Add(new Client { FullName = "Тестовый клиент", Phone = "+375290000000", Balance = 5.0m });
                    context.SaveChanges();
                }
            }
        }
    }
}