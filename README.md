## How to Run

The app was implemented on MacOS, where I cannot run LocalDB, that's why I choose sql server in Docker instead. I hope that this is not an issue.

1. **Start SQL Server in docker**

   ```bash
   docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=MyPassword123" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
   ```

2. **Setup Database**

   ```bash
   cd src
   dotnet ef database update --project NotesApp.Data --startup-project NotesApp.WebApi
   ```

3. **Start Backend**

   ```bash
   cd src
   dotnet run --project NotesApp.WebApi
   ```

4. **Start Frontend**
   ```bash
   cd spa
   npm install
   npm run dev
   ```

Open http://localhost:5173

5. **Run Tests**

   ```bash
   cd src
   dotnet test
   ```

The app can use Entra ID auth (in prod) and local (based on VITE_USE_DEV_AUTH at .env and "UseDevAuthentication" at appsettings.Development.json) for local usage and development purposes. As soon as I didn't setup pipelines for this app, I left **Placeholder** for all Azure secrets, but I'll be happy to show how Entra ID works at my machine.

## What's Included

**Backend:**

- ASP.NET Core Web API with Clean Architecture
- Entity Framework Core with SQL Server
- Advanced pagination, search, and sorting
- Swagger API documentation
- Unit tests for business logic

**Frontend:**

- React + TypeScript + Vite
- Fluent UI components
- Search with pagination
- CSS modules for styling
