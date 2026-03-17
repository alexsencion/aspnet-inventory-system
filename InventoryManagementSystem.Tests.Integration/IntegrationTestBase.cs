using InventoryManagementSystem.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Tests.Integration
{
    public class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory<Program>>, IDisposable
    {
        protected readonly HttpClient Client;
        protected readonly CustomWebApplicationFactory<Program> Factory;
        protected readonly IServiceScope Scope;
        protected readonly ApplicationDbContext Context;

        public IntegrationTestBase(CustomWebApplicationFactory<Program> factory)
        {
            Factory = factory;

            // Create client that doesn't follow redirects automatically
            Client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true
            });

            Scope = factory.Services.CreateScope();
            Context = Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        }

        protected void ClearDatabase()
        {
            Context.StockMovements.RemoveRange(Context.StockMovements);
            Context.Products.RemoveRange(Context.Products);
            Context.Suppliers.RemoveRange(Context.Suppliers);
            Context.SaveChanges();
        }

        /// <summary>
        /// Refreshes the context to get the latest data from the in-memory database
        /// </summary>
        protected void RefreshContext()
        {
            Context.ChangeTracker.Clear();
        }

        /// <summary>
        /// Gets a fresh instance of an entity from the database
        /// </summary>
        protected T? GetFreshEntity<T>(params object[] keyValues) where T : class
        {
            RefreshContext();
            return Context.Set<T>().Find(keyValues);
        }

        public void Dispose()
        {
            ClearDatabase();
            Scope?.Dispose();
            Context?.Dispose();
        }
    }
}
