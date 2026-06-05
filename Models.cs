// Models.cs

namespace ClubArm.Models
{
    public class Client
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        public decimal Balance { get; set; }
        public decimal Discount { get; set; } // процент скидки 0-100
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }

    public class Computer
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Status { get; set; } = "Free"; // Free, Occupied, Blocked, Maintenance
        public string? Configuration { get; set; }   // описание железа
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }

    public class Tariff
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Type { get; set; } = "Minute"; // Minute, Package, Night
        public decimal PricePerMinute { get; set; }  // для Minute и Package (пакетные - цена за минуту внутри пакета)
        public int? PackageMinutes { get; set; }     // для пакетных тарифов: фиксированное количество минут
        public decimal? PackagePrice { get; set; }   // фиксированная стоимость пакета
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }

    public class Session
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;
        public int ComputerId { get; set; }
        public Computer Computer { get; set; } = null!;
        public int TariffId { get; set; }
        public Tariff Tariff { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsActive { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class Transaction
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;
        public decimal Amount { get; set; } // положительное – пополнение, отрицательное – списание
        public string Description { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public ICollection<Sale> Sales { get; set; } = new List<Sale>(); // связь с продажами товаров
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }

    public class Sale
    {
        public int Id { get; set; }
        public int TransactionId { get; set; }
        public Transaction Transaction { get; set; } = null!;
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public int Quantity { get; set; }
    }
}