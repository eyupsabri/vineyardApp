using Microsoft.EntityFrameworkCore;

namespace Entities
{

    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<IoTDevice> IoTDevices { get; set; }
        public DbSet<UserDevice> UserDevices { get; set; }
        public DbSet<Pump> Pumps { get; set; }
        public DbSet<PumpSession> PumpSessions { get; set; }
        public DbSet<TemperatureSensor> TemperatureSensors { get; set; }
        public DbSet<TemperatureReading> TemperatureReadings { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Composite key for UserDevice
            modelBuilder.Entity<UserDevice>()
                .HasKey(ud => new { ud.UserId, ud.IoTDeviceId });

            modelBuilder.Entity<UserDevice>()
                .HasOne(ud => ud.User)
                .WithMany(u => u.UserDevices)
                .HasForeignKey(ud => ud.UserId);

            modelBuilder.Entity<UserDevice>()
                .HasOne(ud => ud.IoTDevice)
                .WithMany(d => d.UserDevices)
                .HasForeignKey(ud => ud.IoTDeviceId);

            modelBuilder.Entity<IoTDevice>()
                .HasOne(d => d.Pump)
                .WithOne(p => p.IoTDevice)
                .HasForeignKey<Pump>(p => p.IoTDeviceId);

            modelBuilder.Entity<IoTDevice>()
                .HasOne(d => d.TemperatureSensor)
                .WithOne(t => t.IoTDevice)
                .HasForeignKey<TemperatureSensor>(t => t.IoTDeviceId);


            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                Username = "testuser",
                CurrentJwtId = null,
                RefreshJwtId = null,
                PasswordHash = "password123" // Example hash for "password123"

            });

            modelBuilder.Entity<IoTDevice>().HasData(
              new IoTDevice
              {
                  Id = 1,
                  Name = "Device 1",
                  DeviceIdentifier = Guid.Parse("ee2f5493-968b-48bd-8663-1b1a0fa3c3df")
              },
              new IoTDevice
              {
                  Id = 2,
                  Name = "Device 2",
                  DeviceIdentifier = Guid.Parse("36c71071-37c0-4749-ac75-9ba72eed4e3e")
              }
            );

            // 3. Pumps (linked to IoT Devices)
            modelBuilder.Entity<Pump>().HasData(
                new Pump
                {
                    Id = 1,
                    IsOn = false,
                    IoTDeviceId = 1
                },
                new Pump
                {
                    Id = 2,
                    IsOn = false,
                    IoTDeviceId = 2
                }
            );

            modelBuilder.Entity<UserDevice>().HasData(
                new UserDevice
                {
                    UserId = 1,
                    IoTDeviceId = 1
                },
                new UserDevice
                {
                    UserId = 1,
                    IoTDeviceId = 2
                }
            );

        }
    }

}
