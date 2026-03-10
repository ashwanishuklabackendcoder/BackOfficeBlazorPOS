# BackOfficeBlazor POS

`BackOfficeBlazor` is a .NET 8 Point-of-Sale solution composed of a Blazor WebAssembly frontend, an ASP.NET Core Web API, a shared domain/data layer, and a Windows print-agent service for local printing.

The solution is structured for retail back-office and checkout workflows including product management, stock movement, sales, returns, layaway, staff permissions, system settings, reporting, and printer orchestration.

## Overview

Main projects:

- `POS.UI`: Blazor WebAssembly frontend using MudBlazor
- `POSAPI`: ASP.NET Core Web API with EF Core, JWT auth, SignalR, Stripe, and Cloudinary integration
- `BackOfficeBlazor.Admin`: domain/data layer with entities, repositories, services, and EF Core context
- `BackOfficeBlazor.Shared`: shared DTOs and cross-project contracts
- `LocalPrintAgent.Sample`: Windows background service that executes print jobs locally

Core functional modules:

- Categories
- Brands / Manufacturers
- Suppliers
- Customers
- Products
- Stock Input
- Stock Transfer
- Sales
- Returns
- Layaway
- Staff users
- Permissions / roles
- System options
- Reporting
- Printer configuration

## Architecture

### Frontend

`POS.UI` is a Blazor WebAssembly application. It uses a single `HttpClient` configured in `Program.cs` and reads its API base URL from `wwwroot/appsettings.json`.

Authentication is handled client-side in `AuthService.cs`. JWT tokens are stored in browser local storage. Route protection is enforced in `App.razor`.

The main checkout workflow lives in `Pages/PosSell.razor`, which manages:

- barcode or product-code entry
- cart and line editing
- stock checks
- discounts
- customer selection
- layaway handoff
- till PIN verification

### API

`POSAPI` is the application backend. It exposes controllers for authentication, catalog management, stock, sales, returns, layaway, reports, printers, payment integration, and print jobs.

Key backend capabilities:

- EF Core with SQL Server
- JWT authentication and authorization
- repository + service architecture
- SignalR hub for print dispatching
- Stripe payment integration
- Cloudinary-backed image storage

### Data Layer

`BackOfficeBlazor.Admin` contains:

- `BackOfficeAdminContext`
- entity models
- repositories
- domain services

This project provides the main persistence and domain structure used by `POSAPI`.

### Printing

Printing is split between the API and a local agent:

- `POSAPI` manages printers, terminals, and print-job dispatch
- SignalR hub endpoint: `/hubs/print`
- `LocalPrintAgent.Sample` connects to the hub, registers using terminal/location/shared key, and executes jobs on the local machine

Supported local print modes:

- Windows printer
- raw TCP printer
- file drop
- email

## Repository Structure

Recommended repository layout:

```text
BackOfficeBlazor.sln
src/
  POS.UI/
  POSAPI/
  BackOfficeBlazor.Admin/
  BackOfficeBlazor.Shared/
services/
  LocalPrintAgent.Sample/
docs/
  architecture/
  deployment/
.gitignore
README.md
```

Current structure is still solution-root based. For an initial GitHub upload, that is acceptable. If you later want cleaner separation, move:

- `POS.UI`, `POSAPI`, `BackOfficeBlazor.Admin`, `BackOfficeBlazor.Shared` into `src/`
- `LocalPrintAgent.Sample` into `services/`

Do that in a dedicated refactor commit after the first clean repository import.

## Configuration

Tracked configuration files now use placeholders instead of real secrets.

### POS.UI configuration

`POS.UI/wwwroot/appsettings.json`

```json
{
  "Api": {
    "BaseUrl": "http://localhost:5101/"
  },
  "Stripe": {
    "PublishableKey": "SET_STRIPE_PUBLISHABLE_KEY"
  }
}
```

### POSAPI configuration

Set these values locally using `appsettings.Local.json`, environment variables, or your hosting provider secret store.

Required settings:

- `ConnectionStrings__Default`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__SecretKey`
- `Stripe__SecretKey`
- `Cloudinary__CloudName`
- `Cloudinary__ApiKey`
- `Cloudinary__ApiSecret`
- `PrintAgent__SharedKey`

Optional bootstrap settings:

- `AuthBootstrap__Username`
- `AuthBootstrap__Password`
- `AuthBootstrap__FullName`
- `AuthBootstrap__StaffCode`

### LocalPrintAgent configuration

`LocalPrintAgent.Sample/appsettings.json` contains non-secret placeholders. Configure:

- `ApiBaseUrl`
- `AgentKey`
- `TerminalCode`
- `LocationCode`
- `FileOutputDirectory`
- SMTP settings if email printing is used

## Recommended Environment Variables

API:

```powershell
$env:ConnectionStrings__Default="Server=localhost;Database=BackOfficeBlazor;Trusted_Connection=True;TrustServerCertificate=True"
$env:Jwt__Issuer="POSAPI"
$env:Jwt__Audience="POS.UI"
$env:Jwt__SecretKey="replace-with-a-long-random-32-plus-character-secret"
$env:Stripe__SecretKey="sk_test_or_live_value"
$env:Cloudinary__CloudName="your-cloudinary-cloud-name"
$env:Cloudinary__ApiKey="your-cloudinary-api-key"
$env:Cloudinary__ApiSecret="your-cloudinary-api-secret"
$env:PrintAgent__SharedKey="replace-with-a-shared-print-agent-key"
```

Optional bootstrap admin:

```powershell
$env:AuthBootstrap__Username="admin"
$env:AuthBootstrap__Password="ChangeThisNow!"
$env:AuthBootstrap__FullName="System Administrator"
$env:AuthBootstrap__StaffCode="ADMIN"
```

## Database Setup

1. Provision a SQL Server database.
2. Set `ConnectionStrings__Default` or update your local config.
3. Start the API once so the startup schema checks can run.

The application currently performs schema creation/update checks in `POSAPI/Program.cs` for some security, printer, and shortcut tables. If you later formalize migrations, keep them in `BackOfficeBlazor.Admin`.

## Running Locally

### Prerequisites

- .NET 8 SDK
- SQL Server
- Visual Studio 2022 or newer, or the .NET CLI
- Windows machine for `LocalPrintAgent.Sample`

### 1. Run the API

From the solution root:

```powershell
dotnet restore BackOfficeBlazor.sln
dotnet run --project .\POSAPI\POSAPI.csproj
```

Default local API URL from launch settings:

- `http://localhost:5101`

### 2. Run the Blazor frontend

Make sure `POS.UI/wwwroot/appsettings.json` points to your local API URL.

```powershell
dotnet run --project .\POS.UI\POS.UI.csproj --launch-profile http
```

Recommended local UI URL:

- `http://localhost:5256`

Use the `http` launch profile if your API is running on plain HTTP to avoid browser mixed-content issues.

### 3. Run the LocalPrintAgent

Update `LocalPrintAgent.Sample/appsettings.json` with:

- local API base URL
- print-agent shared key
- terminal code
- location code

Then run:

```powershell
dotnet run --project .\LocalPrintAgent.Sample\LocalPrintAgent.Sample.csproj
```

## Building

Build the full solution:

```powershell
dotnet build .\BackOfficeBlazor.sln
```

Publish API:

```powershell
dotnet publish .\POSAPI\POSAPI.csproj -c Release
```

Publish UI:

```powershell
dotnet publish .\POS.UI\POS.UI.csproj -c Release
```

## Building and Publishing LocalPrintAgent

Build:

```powershell
dotnet build .\LocalPrintAgent.Sample\LocalPrintAgent.Sample.csproj -c Release
```

Publish as a self-contained Windows x64 service binary:

```powershell
dotnet publish .\LocalPrintAgent.Sample\LocalPrintAgent.Sample.csproj -c Release -r win-x64 --self-contained true -o .\publish\LocalPrintAgent
```

Install as a Windows service from an elevated PowerShell session:

```powershell
sc.exe create LocalPrintAgent binPath= "C:\Path\To\LocalPrintAgent.Sample.exe"
sc.exe start LocalPrintAgent
```

Remove the service:

```powershell
sc.exe stop LocalPrintAgent
sc.exe delete LocalPrintAgent
```

## GitHub Preparation Notes

Before pushing this repository:

- verify all real secrets have been rotated if they were previously committed anywhere else
- keep deployment-specific `.pubxml.user` files out of source control
- avoid committing `bin/`, `obj/`, `.vs/`, uploads, and generated publish output
- prefer secret injection through environment variables or untracked local config

## Initialize Git and Push to GitHub

From the solution root:

```powershell
git init
git add .
git commit -m "Initial open-source cleanup for BackOfficeBlazor POS"
git branch -M main
git remote add origin https://github.com/<your-user-or-org>/<your-repo>.git
git push -u origin main
```

If the repository already exists and uses SSH:

```powershell
git remote add origin git@github.com:<your-user-or-org>/<your-repo>.git
git push -u origin main
```

## Security

This repository should not contain live credentials. If any real secrets were previously stored in local files, rotate them before making the repository public.

