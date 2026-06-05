// Services/ClubService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Microsoft.EntityFrameworkCore;
using ClubArm.Data;
using ClubArm.Models;

namespace ClubArm.Services
{
    public class ComputerWithSession
    {
        public Computer Computer { get; set; }
        public Session? ActiveSession { get; set; }
    }

    public class ClubService : IDisposable
    {
        private readonly ClubDbContext _context;
        private readonly System.Timers.Timer _billingTimer;

        public ClubService()
        {
            _context = new ClubDbContext();
            _billingTimer = new System.Timers.Timer(60000); // 1 минута
            _billingTimer.Elapsed += ProcessBilling;
            _billingTimer.AutoReset = true;
            _billingTimer.Start();
        }

        // --------------------- Клиенты ---------------------
        public List<Client> GetAllClients() => _context.Clients.ToList();
        public Client? GetClient(int id) => _context.Clients.Find(id);
        public void AddClient(Client client)
        {
            _context.Clients.Add(client);
            _context.SaveChanges();
        }
        public void UpdateClient(Client client)
        {
            _context.Entry(client).State = EntityState.Modified;
            _context.SaveChanges();
        }
        public void DeleteClient(int id)
        {
            var client = _context.Clients.Find(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
                _context.SaveChanges();
            }
        }

        public void TopUpBalance(int clientId, decimal amount)
        {
            var client = _context.Clients.Find(clientId);
            if (client == null) throw new Exception("Клиент не найден");
            client.Balance += amount;
            _context.Transactions.Add(new Transaction
            {
                ClientId = clientId,
                Amount = amount,
                Description = "Пополнение баланса",
                CreatedAt = DateTime.Now
            });
            _context.SaveChanges();
        }

        // --------------------- Компьютеры ---------------------
        public List<Computer> GetAllComputers() => _context.Computers.ToList();
        public void AddComputer(Computer computer)
        {
            _context.Computers.Add(computer);
            _context.SaveChanges();
        }
        public void UpdateComputer(Computer computer)
        {
            _context.Entry(computer).State = EntityState.Modified;
            _context.SaveChanges();
        }
        public void DeleteComputer(int id)
        {
            var comp = _context.Computers.Find(id);
            if (comp != null) _context.Computers.Remove(comp);
            _context.SaveChanges();
        }

        // --------------------- Тарифы ---------------------
        public List<Tariff> GetAllTariffs() => _context.Tariffs.ToList();
        public void AddTariff(Tariff tariff)
        {
            _context.Tariffs.Add(tariff);
            _context.SaveChanges();
        }
        public void UpdateTariff(Tariff tariff)
        {
            _context.Entry(tariff).State = EntityState.Modified;
            _context.SaveChanges();
        }
        public void DeleteTariff(int id)
        {
            var tariff = _context.Tariffs.Find(id);
            if (tariff != null) _context.Tariffs.Remove(tariff);
            _context.SaveChanges();
        }

        // --------------------- Товары (Products) ---------------------
        public List<Product> GetAllProducts() => _context.Products.ToList();
        public void AddProduct(Product product)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
        }
        public void UpdateProduct(Product product)
        {
            _context.Entry(product).State = EntityState.Modified;
            _context.SaveChanges();
        }
        public void DeleteProduct(int id)
        {
            var prod = _context.Products.Find(id);
            if (prod != null) _context.Products.Remove(prod);
            _context.SaveChanges();
        }

        // Продажа товара клиенту (списание с баланса, уменьшение остатка)
        public void SellProduct(int clientId, int productId, int quantity)
        {
            var client = _context.Clients.Find(clientId);
            var product = _context.Products.Find(productId);
            if (client == null || product == null) throw new Exception("Клиент или товар не найден");
            if (product.Stock < quantity) throw new Exception("Недостаточно товара на складе");
            decimal total = product.Price * quantity;
            if (client.Balance < total) throw new Exception("Недостаточно средств на балансе");

            // Списание
            client.Balance -= total;
            product.Stock -= quantity;

            // Транзакция и продажа
            var transaction = new Transaction
            {
                ClientId = clientId,
                Amount = -total,
                Description = $"Покупка: {product.Name} x{quantity}",
                CreatedAt = DateTime.Now
            };
            _context.Transactions.Add(transaction);
            _context.SaveChanges(); // чтобы получить Id транзакции

            var sale = new Sale
            {
                TransactionId = transaction.Id,
                ProductId = productId,
                Quantity = quantity
            };
            _context.Sales.Add(sale);
            _context.SaveChanges();
        }

        // --------------------- Сессии ---------------------
        public Session StartSession(int clientId, int computerId, int tariffId)
        {
            var client = _context.Clients.Find(clientId);
            var computer = _context.Computers.Find(computerId);
            var tariff = _context.Tariffs.Find(tariffId);

            if (computer.Status != "Free") throw new Exception("Компьютер занят или недоступен");
            if (client.Balance <= 0) throw new Exception("Недостаточно средств на балансе");
            if (tariff == null) throw new Exception("Тариф не найден");

            // Для пакетных тарифов можно добавить особую логику, здесь опущено
            var session = new Session
            {
                ClientId = clientId,
                ComputerId = computerId,
                TariffId = tariffId,
                StartTime = DateTime.Now,
                IsActive = true,
                TotalCost = 0
            };
            computer.Status = "Occupied";
            _context.Sessions.Add(session);
            _context.SaveChanges();
            return session;
        }

        public void StopSession(int sessionId)
        {
            var session = _context.Sessions
                .Include(s => s.Computer)
                .FirstOrDefault(s => s.Id == sessionId);
            if (session == null || !session.IsActive) throw new Exception("Сессия не активна");

            session.EndTime = DateTime.Now;
            session.IsActive = false;
            session.Computer.Status = "Free";
            _context.SaveChanges();
        }

        public void StopSessionByComputer(int computerId)
        {
            var session = _context.Sessions
                .FirstOrDefault(s => s.ComputerId == computerId && s.IsActive);
            if (session != null)
            {
                StopSession(session.Id);
            }
        }

        // Получить активную сессию по компьютеру
        public Session? GetActiveSessionForComputer(int computerId)
        {
            return _context.Sessions
                .Include(s => s.Client)
                .Include(s => s.Tariff)
                .FirstOrDefault(s => s.ComputerId == computerId && s.IsActive);
        }

        public List<ComputerWithSession> GetComputersWithActiveSessions()
        {
            var computers = _context.Computers.ToList();
            var activeSessions = _context.Sessions
                .Include(s => s.Client)
                .Include(s => s.Tariff)
                .Where(s => s.IsActive)
                .ToList();

            var result = new List<ComputerWithSession>();
            foreach (var comp in computers)
            {
                var session = activeSessions.FirstOrDefault(s => s.ComputerId == comp.Id);
                result.Add(new ComputerWithSession
                {
                    Computer = comp,
                    ActiveSession = session
                });
            }
            return result;
        }

        // Автоматическое списание за каждую минуту
        private void ProcessBilling(object sender, ElapsedEventArgs e)
        {
            try
            {
                var activeSessions = _context.Sessions
                    .Include(s => s.Client)
                    .Include(s => s.Tariff)
                    .Include(s => s.Computer)
                    .Where(s => s.IsActive)
                    .ToList();

                foreach (var session in activeSessions)
                {
                    decimal costPerMinute = session.Tariff.PricePerMinute;
                    decimal discountFactor = (100 - session.Client.Discount) / 100.0m;
                    decimal charge = costPerMinute * discountFactor;

                    if (session.Client.Balance >= charge)
                    {
                        session.Client.Balance -= charge;
                        session.TotalCost += charge;
                        _context.Transactions.Add(new Transaction
                        {
                            ClientId = session.ClientId,
                            Amount = -charge,
                            Description = $"Списание за минуту (ПК {session.Computer.Name})",
                            CreatedAt = DateTime.Now
                        });
                    }
                    else
                    {
                        // Недостаточно средств - блокируем и завершаем сессию
                        session.IsActive = false;
                        session.EndTime = DateTime.Now;
                        session.Computer.Status = "Blocked";
                    }
                }
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка биллинга: {ex.Message}");
            }
        }

        public int GetSessionsCount(int clientId)
        {
            return _context.Sessions.Count(s => s.ClientId == clientId);
        }

        public decimal GetTotalSpent(int clientId)
        {
            // Сумма всех отрицательных транзакций (списаний) для клиента
            return _context.Transactions
                .Where(t => t.ClientId == clientId && t.Amount < 0)
                .Sum(t => -t.Amount);
        }

        // --------------------- Отчёты ---------------------
        public decimal GetTotalRevenue(DateTime from, DateTime to)
        {
            return _context.Transactions
                .Where(t => t.CreatedAt >= from && t.CreatedAt <= to && t.Amount < 0)
                .Sum(t => -t.Amount);
        }

        public Dictionary<string, int> GetComputerLoad(DateTime from, DateTime to)
        {
            var sessions = _context.Sessions
                .Where(s => s.StartTime >= from && s.StartTime <= to)
                .ToList();
            var load = new Dictionary<string, int>();
            foreach (var comp in _context.Computers.ToList())
            {
                var totalMinutes = sessions
                    .Where(s => s.ComputerId == comp.Id && s.EndTime.HasValue)
                    .Sum(s => (s.EndTime.Value - s.StartTime).TotalMinutes);
                load[comp.Name] = (int)totalMinutes;
            }
            return load;
        }

        public Dictionary<string, int> GetPopularTariffs(DateTime from, DateTime to)
        {
            return _context.Sessions
                .Where(s => s.StartTime >= from && s.StartTime <= to)
                .GroupBy(s => s.Tariff.Name)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .ToDictionary(x => x.Name, x => x.Count);
        }

        public void Dispose()
        {
            _billingTimer?.Stop();
            _billingTimer?.Dispose();
            _context?.Dispose();
        }
    }
}