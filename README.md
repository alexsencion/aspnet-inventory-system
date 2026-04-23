# Inventory Management System

A comprehensive, production-ready inventory management system built with ASP.NET Core MVC, Entity Framework Core, and deployed on Azure.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Azure](https://img.shields.io/badge/Azure-Deployed-0078D4?logo=microsoft-azure)](https://azure.microsoft.com/)
[![Tests](https://img.shields.io/badge/Tests-160%20Passing-success)](https://github.com/yourusername/InventoryManagementSystem)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

## 🎯 Project Overview

This Inventory Management System is a full-featured web application designed for small retail businesses to efficiently manage their product inventory, track stock movements, and maintain supplier relationships. Built following industry best practices, clean architecture principles, and comprehensive testing strategies.

### Key Features

- **Supplier Management**: Complete CRUD operations for supplier information with relationship tracking
- **Product Catalog**: Manage products with categories, pricing, stock levels, and low-stock alerts
- **Stock Movement Tracking**: Record stock IN/OUT transactions with full audit trail
- **Dashboard & Reports**: Real-time analytics, low stock alerts, and movement history
- **Transaction Safety**: Atomic stock updates with database transaction support
- **Responsive UI**: Bootstrap 5-based interface optimized for desktop and mobile

## 🏗️ Architecture

### Technology Stack

**Backend:**
- ASP.NET Core 8.0 MVC
- Entity Framework Core 8.0
- SQL Server / Azure SQL Database
- Repository & Service Pattern
- Dependency Injection

**Frontend:**
- Razor Views
- Bootstrap 5
- jQuery (minimal)
- Bootstrap Icons

**Testing:**
- xUnit
- Moq
- FluentAssertions
- In-Memory Database for unit tests
- Integration tests with WebApplicationFactory

**DevOps:**
- GitHub Actions for CI/CD
- Azure App Service
- Azure SQL Database
- Application Insights for monitoring

### Project Structure

InventoryManagementSystem/
├── InventoryManagementSystem.Web/           # ASP.NET MVC Web Application
│   ├── Controllers/                          # MVC Controllers
│   ├── Views/                                # Razor Views
│   ├── Models/                               # View Models
│   └── wwwroot/                              # Static files
├── InventoryManagementSystem.Data/           # Data Access Layer
│   ├── Entities/                             # Domain Models
│   ├── Repositories/                         # Repository Pattern Implementation
│   ├── Migrations/                           # EF Core Migrations
│   └── ApplicationDbContext.cs               # Database Context
├── InventoryManagementSystem.Services/       # Business Logic Layer
│   ├── Services/                             # Service Implementations
│   ├── Interfaces/                           # Service Contracts
│   └── DTOs/                                 # Data Transfer Objects
├── InventoryManagementSystem.Tests.Unit/     # Unit Tests
└── InventoryManagementSystem.Tests.Integration/ # Integration Tests

## 🚀 Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or SQL Server LocalDB
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

### Installation

1. **Clone the repository**

```bash
git clone https://github.com/yourusername/InventoryManagementSystem.git
cd InventoryManagementSystem
```

2. **Restore NuGet packages**

```bash
dotnet restore
```

3. **Update database connection string**

Edit `appsettings.json` in `InventoryManagementSystem.Web`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=InventoryManagementDB;Trusted_Connection=true;"
  }
}
```

4. **Apply database migrations**

```bash
dotnet ef database update --project InventoryManagementSystem.Data --startup-project InventoryManagementSystem.Web
```

5. **Run the application**

```bash
cd InventoryManagementSystem.Web
dotnet run
```

6. **Access the application**

Open your browser and navigate to: `https://localhost:5001`

### Initial Data

The database is seeded with sample data:
- 3 suppliers
- 5 products (including one with low stock)
- Various categories

## 🧪 Testing

The project includes comprehensive test coverage with 160+ tests.

### Run All Tests

```bash
dotnet test
```

### Run Specific Test Projects

```bash
# Unit tests only
dotnet test InventoryManagementSystem.Tests.Unit

# Integration tests only
dotnet test InventoryManagementSystem.Tests.Integration
```

### Run Tests with Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Test Coverage

| Module | Unit Tests | Integration Tests | Total |
|--------|-----------|-------------------|-------|
| Supplier | 25 | 20 | 45 |
| Product | 29 | 31 | 60 |
| Stock Movement | 28 | 27 | 55 |
| **Total** | **82** | **78** | **160** |

## 📊 Features Documentation

### Supplier Management
- Create, read, update, and delete supplier records
- Track contact information and addresses
- View associated products per supplier
- Search and filter capabilities
- Delete protection for suppliers with products

### Product Management
- Complete product catalog with SKU tracking
- Category organization
- Price and stock level management
- Low stock threshold configuration
- Automatic inventory value calculation
- Search by name, SKU, or description
- Filter by category and stock status

### Stock Movement Tracking
- Record stock IN transactions (purchases, returns)
- Record stock OUT transactions (sales, damage, loss)
- Automatic stock level updates
- Transaction-based data integrity
- Movement history with filtering
- Audit trail (no edit/delete of movements)
- Real-time stock validation

### Dashboard & Reports
- Key performance metrics
- Low stock alerts
- Recent activity feed
- Category breakdown
- Stock levels report with CSV export
- Low stock alert report
- Movement history report with date filtering

## 🔄 Git Workflow

This project follows a structured branching strategy:

### Branch Structure

- **`main`**: Production-ready code, deployed to Azure
- **`develop`**: Integration branch for features
- **`feature/*`**: Feature development branches
- **`test/*`**: Testing-specific branches
- **`fix/*`**: Bug fix branches

### Workflow

```bash
# Create a feature branch from develop
git checkout develop
git pull origin develop
git checkout -b feature/your-feature-name

# Make changes and commit
git add .
git commit -m "feat: description of your changes"

# Push to remote
git push origin feature/your-feature-name

# Create pull request to develop
# After review and approval, merge to develop
# Periodically merge develop to main for production releases
```

### Commit Message Convention

Follow [Conventional Commits](https://www.conventionalcommits.org/):

- `feat:` New feature
- `fix:` Bug fix
- `refactor:` Code refactoring
- `test:` Adding or updating tests
- `docs:` Documentation changes
- `chore:` Maintenance tasks

## 🚢 Deployment

### Azure Deployment

The application is configured for deployment to Azure with automated CI/CD.

#### Prerequisites

- Azure account
- Azure CLI installed
- GitHub account

#### Quick Deploy to Azure

1. **Run Azure setup script**

```bash
# For Windows PowerShell
.\azure-setup.ps1

# For Linux/Mac
./azure-setup.sh
```

2. **Deploy database schema**

```bash
.\deploy-database.ps1
```

3. **Deploy application**

```bash
.\deploy-app.ps1
```

#### Automated Deployment (CI/CD)

The project includes GitHub Actions workflow for automated deployment:

1. Push code to `main` branch
2. GitHub Actions automatically:
   - Builds the solution
   - Runs all 160 tests
   - Deploys to Azure (if tests pass)

### Manual Deployment

See [DEPLOYMENT.md](docs/DEPLOYMENT.md) for detailed deployment instructions.

## 📖 API Documentation

### Stock Movement Endpoints

#### Get Product Stock Information

GET /StockMovements/GetProductStock?productId={id}

**Response:**
```json
{
  "productName": "Wireless Mouse",
  "sku": "WM-001",
  "currentStock": 50,
  "lowStockThreshold": 10,
  "isLowStock": false
}
```

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'feat: add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines

- Follow C# coding conventions
- Write unit tests for new features
- Maintain test coverage above 80%
- Update documentation as needed
- Use meaningful commit messages

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Authors

- **Your Name** - *Initial work* - [YourGitHub](https://github.com/yourusername)

## 🙏 Acknowledgments

- Built as a learning project for .NET backend development
- Inspired by real-world inventory management challenges
- Thanks to the .NET community for excellent documentation and resources

## 📞 Support

For questions or issues:
- Open an issue on [GitHub Issues](https://github.com/yourusername/InventoryManagementSystem/issues)
- Contact: your.email@example.com

## 🗺️ Roadmap

### Current Version (v1.0)
- ✅ Core inventory management
- ✅ Stock movement tracking
- ✅ Reporting and analytics
- ✅ Azure deployment
- ✅ Comprehensive testing

### Future Enhancements (v2.0)
- [ ] User authentication and authorization (ASP.NET Identity)
- [ ] Role-based access control (Admin, Manager, User)
- [ ] Multi-location inventory support
- [ ] Barcode scanning integration
- [ ] Automated purchase order generation
- [ ] Advanced analytics and forecasting
- [ ] Email notifications for low stock
- [ ] REST API for mobile apps
- [ ] Export to Excel/PDF
- [ ] Audit log viewer

## 📈 Project Status

**Status**: ✅ Production Ready

- All core features implemented
- Comprehensive test coverage (160+ tests)
- Deployed to Azure
- CI/CD pipeline active
- Documentation complete

## 🔗 Links

- **Live Demo**: (https://inventory-mgmt-app-7176.azurewebsites.net/)
- **Documentation**: [Wiki](https://github.com/alexsencion/InventoryManagementSystem/wiki)
- **Issue Tracker**: [GitHub Issues](https://github.com/alexsencion/InventoryManagementSystem/issues)

---

**Built with ❤️ using ASP.NET Core**
