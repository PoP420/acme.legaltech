Here is the complete, structured plan formatted specifically for you to save as a `PLAN.md` file. 

This document is optimized for AI coding agents (like GitHub Copilot Workspace, Kilo Code, or Cursor). It includes explicit context, exact file paths, and complete code blocks so the AI knows exactly what to build and where to put it without hallucinating.

***

**Instructions:** Copy everything below this line and save it as **`PLAN.md`** in the root of your workspace. Then, feed this file to your AI coding agent.

***

```markdown
# Project Plan: Acme.LegalTech (CLM Foundation)

## 1. Context & Architecture Overview
**Objective:** Build the foundational backend and frontend for a Contract Lifecycle Management (CLM) SaaS application. Today's goal is strictly to establish the multi-tenant ABP backend, PostgreSQL database, file upload (Blob Storage), and Angular UI. AI processing will be added in a later phase.

**Tech Stack:**
- **Framework:** ABP Framework (Commercial/Open Source)
- **Backend:** .NET 8, C#, Entity Framework Core
- **Database:** PostgreSQL 15 (running in Docker)
- **Frontend:** Angular (TypeScript, Bootstrap)
- **Storage:** ABP Blob Storage (File System provider for local dev)

**Architectural Rules for AI Agent:**
1. Follow ABP's Domain-Driven Design (DDD) principles.
2. All entities must be multi-tenant ready (ABP handles this via `IMultiTenant` interface on base classes).
3. Never block HTTP requests with long-running tasks (use Background Jobs for future AI tasks).
4. Keep Domain layer free of infrastructure concerns.

---

## 2. Phase 1: Infrastructure (Docker PostgreSQL)

**Action:** Create a Docker Compose file to host the PostgreSQL database.

**File to create:** `docker-compose.yml` (in the root workspace directory)
```yaml
version: '3.8'
services:
  db:
    image: postgres:15
    container_name: legaltech_postgres
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: myPassword123
      POSTGRES_DB: LegalTech
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data:
```

**Terminal Commands:**
```bash
docker-compose up -d
```

---

## 3. Phase 2: Backend Scaffolding

**Action:** Generate the ABP solution using the CLI, explicitly targeting PostgreSQL.

**Terminal Commands:**
```bash
# Remove old folder if it exists
rm -rf Acme.LegalTech 

# Generate new ABP project
abp new Acme.LegalTech -u angular -csf -dbms PostgreSQL

# Open in VS Code
code Acme.LegalTech
```

---

## 4. Phase 3: Domain Layer (Entities & DbContext)

**Action:** Define the core `Contract` aggregate and register it in the DbContext.

### File: `src/Acme.LegalTech.Domain/Contracts/Contract.cs`
```csharp
using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Acme.LegalTech.Contracts;

public class Contract : FullAuditedAggregateRoot<Guid>
{
    public string Title { get; protected set; }
    public string CounterpartyName { get; protected set; }
    public string? DocumentBlobName { get; set; }

    protected Contract() { }

    public Contract(Guid id, string title, string counterpartyName) : base(id)
    {
        Title = title;
        CounterpartyName = counterpartyName;
    }
}
```

### File: `src/Acme.LegalTech.EntityFrameworkCore/EntityFrameworkCore/LegalTechDbContext.cs`
**Action:** Add the `DbSet` for the `Contract` entity.
```csharp
// Add this using statement at the top:
using Acme.LegalTech.Contracts;

// Add this DbSet inside the LegalTechDbContext class:
public DbSet<Contract> Contracts { get; set; }
```

---

## 5. Phase 4: Application Layer (DTOs, Services, Blob Storage)

**Action:** Configure Blob Storage, create DTOs, and implement the Application Service.

### Step 4.1: Install Blob Storage Package
**Terminal Command:** Run this inside the `src/Acme.LegalTech.Domain` folder.
```bash
dotnet add package Volo.Abp.BlobStorage.FileSystem
```

### Step 4.2: Configure Blob Storage
**File:** `src/Acme.LegalTech.Domain/Acme.LegalTech.Domain/LegalTechDomainModule.cs`
**Action:** Add the configuration inside the `ConfigureServices` method.
```csharp
using Volo.Abp.BlobStoring;
using Volo.Abp.BlobStoring.FileSystem;

// Inside ConfigureServices method:
Configure<AbpBlobStorageOptions>(options =>
{
    options.Containers.ConfigureDefault(container =>
    {
        container.UseFileSystem(fileSystem =>
        {
            fileSystem.BasePath = "my-blobs";
        });
    });
});
```

### Step 4.3: Create Contracts DTOs
**File:** `src/Acme.LegalTech.Application.Contracts/Contracts/CreateContractDto.cs`
```csharp
namespace Acme.LegalTech.Contracts;

public class CreateContractDto
{
    public string Title { get; set; }
    public string CounterpartyName { get; set; }
}
```

**File:** `src/Acme.LegalTech.Application.Contracts/Contracts/ContractDto.cs`
```csharp
using System;

namespace Acme.LegalTech.Contracts;

public class ContractDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string CounterpartyName { get; set; }
    public string? DocumentBlobName { get; set; }
}
```

### Step 4.4: Create App Service Interface
**File:** `src/Acme.LegalTech.Application.Contracts/Contracts/IContractAppService.cs`
```csharp
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace Acme.LegalTech.Contracts;

public interface IContractAppService : IApplicationService
{
    Task<ContractDto> CreateAsync(CreateContractDto input);
    Task UploadDocumentAsync(Guid contractId, IRemoteStreamContent file);
}
```

### Step 4.5: Implement App Service
**File:** `src/Acme.LegalTech.Application/Contracts/ContractAppService.cs`
```csharp
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;

namespace Acme.LegalTech.Contracts;

public class ContractAppService : ApplicationService, IContractAppService
{
    private readonly IRepository<Contract, Guid> _contractRepository;
    private readonly IBlobContainer _blobContainer;

    public ContractAppService(
        IRepository<Contract, Guid> contractRepository,
        IBlobContainer blobContainer)
    {
        _contractRepository = contractRepository;
        _blobContainer = blobContainer;
    }

    public async Task<ContractDto> CreateAsync(CreateContractDto input)
    {
        var contract = new Contract(GuidGenerator.Create(), input.Title, input.CounterpartyName);
        await _contractRepository.InsertAsync(contract);
        return ObjectMapper.Map<Contract, ContractDto>(contract);
    }

    public async Task UploadDocumentAsync(Guid contractId, IRemoteStreamContent file)
    {
        var contract = await _contractRepository.GetAsync(contractId);
        
        using var memoryStream = new MemoryStream();
        await file.GetStream().CopyToAsync(memoryStream);
        var bytes = memoryStream.ToArray();

        var blobName = $"{contractId}_{file.FileName}";
        await _blobContainer.SaveAsync(blobName, bytes, overrideExisting: true);

        contract.DocumentBlobName = blobName;
        await _contractRepository.UpdateAsync(contract);
    }
}
```

### Step 4.6: Configure AutoMapper
**File:** `src/Acme.LegalTech.Application/Acme.LegalTech.Application/LegalTechAutoMapperProfile.cs`
**Action:** Add the mapping inside the constructor.
```csharp
using Acme.LegalTech.Contracts;

// Inside the constructor:
CreateMap<Contract, ContractDto>();
```

---

## 6. Phase 5: Database Migration

**Action:** Push the domain model to the PostgreSQL database.

**Terminal Commands:** Run these inside the `src/Acme.LegalTech.HttpApi.Host` folder.
```bash
dotnet ef migrations add "Initial_Contracts"
dotnet ef database update
```

---

## 7. Phase 6: Frontend (Angular)

**Action:** Generate API proxies, create the upload component, and wire up the UI.

### Step 6.1: Generate Proxies
**Terminal Commands:** 
First build the backend, then generate proxies in the `angular` folder.
```bash
cd ../src/Acme.LegalTech.HttpApi.Host
dotnet build
cd ../../angular
abp generate-proxy -t ng
# Select 'app' for module and 'Default' for endpoint if prompted.
```

### Step 6.2: Generate Component
**Terminal Command:**
```bash
ng generate component contract-upload
```

### Step 6.3: Component TypeScript
**File:** `src/app/contract-upload/contract-upload.component.ts`
```typescript
import { Component } from '@angular/core';
import { ContractService, CreateContractDto } from '@proxy/contracts';

@Component({
  selector: 'app-contract-upload',
  templateUrl: './contract-upload.component.html',
  styleUrls: ['./contract-upload.component.scss']
})
export class ContractUploadComponent {
  title = '';
  counterparty = '';
  selectedFile: File | null = null;
  isLoading = false;

  constructor(private contractService: ContractService) {}

  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0];
  }

  async submit() {
    if (!this.title || !this.counterparty || !this.selectedFile) return;

    this.isLoading = true;
    try {
      const dto: CreateContractDto = { title: this.title, counterpartyName: this.counterparty };
      const contract = await this.contractService.create(dto).toPromise();

      await this.contractService.uploadDocument(contract.id, this.selectedFile).toPromise();
      
      alert('Contract and PDF uploaded successfully!');
      this.title = '';
      this.counterparty = '';
      this.selectedFile = null;
    } catch (error) {
      console.error(error);
      alert('Error uploading');
    } finally {
      this.isLoading = false;
    }
  }
}
```

### Step 6.4: Component HTML
**File:** `src/app/contract-upload/contract-upload.component.html`
```html
<div class="container mt-4">
  <h2>Upload New Contract</h2>
  <form (ngSubmit)="submit()">
    <div class="mb-3">
      <label class="form-label">Contract Title</label>
      <input type="text" class="form-control" [(ngModel)]="title" name="title" required>
    </div>
    <div class="mb-3">
      <label class="form-label">Counterparty</label>
      <input type="text" class="form-control" [(ngModel)]="counterparty" name="counterparty" required>
    </div>
    <div class="mb-3">
      <label class="form-label">Upload PDF</label>
      <input type="file" class="form-control" (change)="onFileSelected($event)" accept=".pdf" required>
    </div>
    <button type="submit" class="btn btn-primary" [disabled]="isLoading">
      {{ isLoading ? 'Uploading...' : 'Upload Contract' }}
    </button>
  </form>
</div>
```

### Step 6.5: Add Routing
**File:** `src/app/app-routing.module.ts`
**Action:** Import the component and add it to the `routes` array.
```typescript
import { ContractUploadComponent } from './contract-upload/contract-upload.component';

// Add this object to the routes array:
{ path: 'upload-contract', component: ContractUploadComponent }
```

---

## 8. Phase 7: Execution & Verification

**Action:** Run both backend and frontend to verify the end-to-end flow.

**Terminal 1 (Backend):**
```bash
cd src/Acme.LegalTech.HttpApi.Host
dotnet run
```

**Terminal 2 (Frontend):**
```bash
cd angular
npm start
```

**Verification Steps:**
1. Open browser to `http://localhost:4200`.
2. Log in (Username: `admin`, Password: `1q2w3E*`).
3. Navigate to `http://localhost:4200/upload-contract`.
4. Fill out the form, select a PDF, and click Upload.
5. **Verify File:** Check the `src/Acme.LegalTech.HttpApi.Host/my-blobs` folder. The PDF should be saved there.
6. **Verify Database:** Run this command in terminal to check Postgres:
   ```bash
   docker exec -it legaltech_postgres psql -U postgres -d LegalTech -c "SELECT \"Title\", \"DocumentBlobName\" FROM \"Contracts\";"
   ```
```

***

### How to use this with AI Agents:
1. Save the text above as `PLAN.md`.
2. Open **GitHub Copilot Chat**, **Cursor**, or **Kilo Code**.
3. Type: *"Read PLAN.md and execute Phase 1 and Phase 2."* (Don't ask it to do everything at once. Feed it phase by phase to ensure it doesn't lose context or make mistakes).
4. Once an AI agent finishes a phase, verify it works, then prompt it: *"Phase X is complete. Proceed to Phase Y."*