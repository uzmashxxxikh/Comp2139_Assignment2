using WebApplication1.Models;
using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // Check if there are any categories
            if (context.Categories.Any())
            {
                return; // DB has been seeded
            }

            // Add Categories
            var categories = new Category[]
            {
                new Category { Name = "Electronics", Description = "Electronic devices and accessories" },
                new Category { Name = "Clothing", Description = "Apparel and fashion items" },
                new Category { Name = "Books", Description = "Books and publications" },
                new Category { Name = "Home & Garden", Description = "Home improvement and garden supplies" }
            };

            foreach (var category in categories)
            {
                context.Categories.Add(category);
            }
            context.SaveChanges();

            // Add Products
            var products = new Product[]
            {
                new Product {
                    Name = "Laptop",
                    Description = "High-performance laptop",
                    Price = 999.99M,
                    QuantityInStock = 15,
                    LowStockThreshold = 5,
                    CategoryId = categories[0].Id
                },
                new Product {
                    Name = "Smartphone",
                    Description = "Latest model smartphone",
                    Price = 699.99M,
                    QuantityInStock = 25,
                    LowStockThreshold = 8,
                    CategoryId = categories[0].Id
                },
                new Product {
                    Name = "T-Shirt",
                    Description = "Cotton t-shirt",
                    Price = 19.99M,
                    QuantityInStock = 100,
                    LowStockThreshold = 20,
                    CategoryId = categories[1].Id
                },
                new Product {
                    Name = "Jeans",
                    Description = "Denim jeans",
                    Price = 49.99M,
                    QuantityInStock = 50,
                    LowStockThreshold = 10,
                    CategoryId = categories[1].Id
                },
                new Product {
                    Name = "Programming Book",
                    Description = "Learn programming",
                    Price = 39.99M,
                    QuantityInStock = 30,
                    LowStockThreshold = 5,
                    CategoryId = categories[2].Id
                },
                new Product {
                    Name = "Garden Tools Set",
                    Description = "Essential garden tools",
                    Price = 79.99M,
                    QuantityInStock = 20,
                    LowStockThreshold = 4,
                    CategoryId = categories[3].Id
                }
            };

            foreach (var product in products)
            {
                context.Products.Add(product);
            }
            context.SaveChanges();
        }

        public static async Task InitializeAdminUser(UserManager<ApplicationUser> userManager)
        {
            // Check if admin user exists
            var adminEmail = "admin@example.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                // Create admin user
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "Admin User",
                    ContactInformation = "admin@example.com"
                };

                var result = await userManager.CreateAsync(admin, "Admin123!");

                if (result.Succeeded)
                {
                    // Assign admin role
                    await userManager.AddToRoleAsync(admin, "Admin");
                    Console.WriteLine("Admin user created successfully.");
                }
                else
                {
                    Console.WriteLine("Failed to create admin user:");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"- {error.Description}");
                    }
                }
            }
            else
            {
                // Ensure admin role is assigned
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine("Admin role assigned to existing admin user.");
                }
                else
                {
                    Console.WriteLine("Admin user already exists with Admin role.");
                }
            }
        }
    }
} 