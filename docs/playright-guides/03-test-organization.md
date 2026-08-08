# Guide 03: Test Organization & Best Practices

As your test suite grows, organizing tests becomes critical. This guide covers the Page Object Model, custom fixtures, test data management, and best practices for maintainable Playwright tests against the LegalTech app.

## Project Structure

Organize your `e2e/` folder for scalability:

```
angular/e2e/
  playwright.config.ts          (optional per-project overrides)
  specs/
    home.spec.ts
    contracts.spec.ts
    auth.spec.ts
  pages/
    base.page.ts
    login.page.ts
    contracts-list.page.ts
    contract-detail.page.ts
    home.page.ts
  fixtures/
    authenticated-page.fixture.ts
  support/
    db-seeder.ts
    test-helpers.ts
  test-data/
    sample-contract.json
```

## Page Object Model (POM)

The Page Object Model encapsulates UI interactions into reusable classes. Each class represents a page (or a reusable component) and exposes methods that tests call.

### Base Page

```typescript
// e2e/pages/base.page.ts
import type { Page } from '@playwright/test';

export class BasePage {
  readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async navigate(path: string) {
    await this.page.goto(path);
    await this.page.waitForLoadState('networkidle');
  }

  async waitForLoad() {
    await this.page.locator('.page-loading, .spinner-border')
      .waitFor({ state: 'detached', timeout: 15000 })
      .catch(() => {}); // ignore timeout if no loading element
  }
}
```

### Login Page

```typescript
// e2e/pages/login.page.ts
import { BasePage, type Page } from './base.page';

export class LoginPage extends BasePage {
  async open() {
    await this.navigate('/account/login');
  }

  async fillCredentials(email: string, password: string) {
    await this.page.fill('input[name="userName"]', email);
    await this.page.fill('input[name="password"]', password);
  }

  async submit() {
    await Promise.all([
      this.page.waitForURL(/\/(?!account)/), // wait until we leave the login page
      this.page.click('button[type="submit"]'),
    ]);
  }

  async login(email: string, password: string) {
    await this.open();
    await this.fillCredentials(email, password);
    await this.submit();
    await this.waitForLoad();
    return this;
  }
}
```

### Contracts List Page

```typescript
// e2e/pages/contracts-list.page.ts
import { BasePage, type Page } from './base.page';

export class ContractsListPage extends BasePage {
  async goto() {
    await this.navigate('/contracts');
  }

  get tableRows() {
    return this.page.locator('ngx-datatable .datatable-row');
  }

  get searchBox() {
    return this.page.locator('input[placeholder*="Search"], input[name="filter"]');
  }

  async search(query: string) {
    await this.searchBox.fill(query);
    await this.page.keyboard.press('Enter');
    await this.waitForLoad();
  }

  async openContract(title: string) {
    const row = this.tableRows.filter({ hasText: title });
    await row.click();
  }

  async getContractTitles(): Promise<string[]> {
    return this.tableRows.locator('.contract-title, td').allTextContents();
  }

  async expectContractExists(title: string) {
    await expect(this.tableRows.filter({ hasText: title }))
      .toBeVisible();
  }
}
```

### Home Page

```typescript
// e2e/pages/home.page.ts
import { BasePage } from './base.page';

export class HomePage extends BasePage {
  async open() {
    await this.navigate('/');
  }

  async expectWelcomeMessage() {
    await expect(this.page.locator('h3:has-text("Welcome")')).toBeVisible();
  }

  async clickMenuItem(menuName: string) {
    await this.page.click(`.lx-sidebar-menu .menu-text:has-text("${menuName}")`);
  }
}
```

## Using Page Objects in Tests

```typescript
// e2e/specs/contracts.spec.ts
import { test, expect } from '@playwright/test';
import { HomePage } from '../pages/home.page';
import { ContractsListPage } from '../pages/contracts-list.page';
import { LoginPage } from '../pages/login.page';

test.describe('Contracts Feature', () => {
  test('unauthenticated user is redirected to login', async ({ page }) => {
    const home = new HomePage(page);
    await page.goto('/contracts');
    await expect(page).toHaveURL(/\/account\/login/);
  });

  test('authenticated user can view contracts list', async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.login('admin@abp.io', '1q2w3E*');

    const home = new HomePage(page);
    await home.open();
    await home.expectWelcomeMessage();
    await home.clickMenuItem('Contracts');

    const contractsPage = new ContractsListPage(page);
    await expect(page).toHaveURL(/\/contracts/);
    await contractsPage.search('NDA');
    await expect(contractsPage.tableRows).toHaveCountGreaterThan(0);
  });
});
```

## Custom Fixtures with Page Objects

For tests that need authentication, create a fixture that handles login once and provides a page with an authenticated session:

```typescript
// e2e/fixtures/authenticated-page.fixture.ts
import { test as base } from '@playwright/test';
import { LoginPage } from '../pages/login.page';
import { HomePage } from '../pages/home.page';

type AuthenticatedFixtures = {
  authenticatedPage: HomePage;
};

export const test = base.extend<AuthenticatedFixtures>({
  authenticatedPage: async ({ page }, use) => {
    const login = new LoginPage(page);
    await login.login('admin@abp.io', '1q2w3E*');
    const home = new HomePage(page);
    await use(home);
  },
});
```

Then in your test:

```typescript
import { expect } from '@playwright/test';
import { test } from '../fixtures/authenticated-page.fixture';

test('authenticated user sees home page', async ({ authenticatedPage }) => {
  await authenticatedPage.expectWelcomeMessage();
});
```

## Test Data Management

### Inline Data

For simple tests, define test data inside the test:

```typescript
test('creates a contract with minimum fields', async ({ page }) => {
  const contractData = {
    title: `Test Contract ${Date.now()}`,
    counterparty: 'ACME Corp',
    status: 'Draft',
  };

  // ... fill form and submit
});
```

### External Test Data Files

For complex data, use JSON or TypeScript files:

```json
// e2e/test-data/sample-contract.json
{
  "title": "Annual Service Agreement",
  "counterpartyName": "Global Tech Solutions",
  "category": "Service Agreement",
  "riskBaseline": "Medium",
  "contractValue": 50000,
  "effectiveDate": "2024-01-15",
  "expirationDate": "2025-01-14",
  "documentNumber": "CON-2024-001"
}
```

```typescript
// e2e/test-data/sample-contract.ts
export const sampleContract = {
  title: 'Annual Service Agreement',
  counterpartyName: 'Global Tech Solutions',
  category: 'Service Agreement',
  riskBaseline: 'Medium',
  contractValue: 50000,
  effectiveDate: '2024-01-15',
  expirationDate: '2025-01-14',
  documentNumber: 'CON-2024-001',
};
```

### API-Level Test Data Setup

For tests that need database state (e.g., a contract that already exists), use the ABP HTTP API directly:

```typescript
import { request } from '@playwright/test';

async function seedContract(contract: any) {
  const apiContext = await request.newContext({
    baseURL: 'https://localhost:44334',
    ignoreHTTPSErrors: true,
  });

  const response = await apiContext.post('/api/abp/application-services', {
    data: contract,
    headers: { 'Content-Type': 'application/json' },
  });

  const result = await response.json();
  return result;
}

test('tests contract detail page with seeded data', async ({ page }) => {
  const contractId = await seedContract(sampleContract);
  // ... navigate to /contracts/{id}
});
```

## Test Lifecycle Hooks

Playwright provides hooks for setup and teardown at different levels:

```typescript
test.describe('Contract Management', () => {
  // Runs once before all tests in this describe block
  test.beforeAll(async ({ browser }) => {
    const context = await browser.newContext();
    // shared setup (e.g., create a test user via API)
    await context.close();
  });

  // Runs before each test
  test.beforeEach(async ({ page }) => {
    // navigate to a starting page
    await page.goto('/');
  });

  // Runs after each test
  test.afterEach(async ({ page }, testInfo) => {
    // capture screenshot on failure
    if (testInfo.status === 'failed') {
      await page.screenshot({
        path: `screenshots/${testInfo.title.replace(/\s+/g, '-')}.png`,
        fullPage: true,
      });
    }
  });

  // Runs once after all tests
  test.afterAll(async () => {
    // cleanup (e.g., delete test data via API)
  });

  test('test case 1', async ({ page }) => { /* ... */ });
  test('test case 2', async ({ page }) => { /* ... */ });
});
```

## Best Practices

### 1. Use Role-Based Selectors (Accessible Selectors)

Prefer selectors that are tied to the element's purpose, not its CSS class:

```typescript
// Good — by role and name
await page.getByRole('button', { name: 'Save Contract' }).click();

// Good — by label
await page.getByLabel('Contract Title').fill('My Contract');

// Good — by placeholder
const search = page.getByPlaceholder('Search contracts...');

// Avoid — brittle CSS selectors
await page.click('.card > div:nth-child(2) button'); // breaks easily
```

### 2. Avoid `page.goto()` in Page Objects; Use `open()` Methods

```typescript
// Good — explicit navigation
class ContractsListPage {
  async open() {
    await this.page.goto('/contracts');
  }
}

// Test
await contractsPage.open();
```

### 3. Use `data-testid` Attributes for Test-Only Selectors

In the Angular templates, you can add `data-testid` attributes:

```html
<!-- In a component template -->
<button class="btn btn-primary" data-testid="save-contract-btn">Save</button>
```

Then select in Playwright:

```typescript
await page.locator('[data-testid="save-contract-btn"]').click();
```

This decouples tests from CSS class names that might change during UI refactoring.

### 4. Avoid Hardcoded Waits

```typescript
// Bad
await page.waitForTimeout(2000);

// Good — wait for specific conditions
await page.locator('.contract-card').first().waitFor({ state: 'visible' });
await expect(page.locator('.toast-success')).toBeVisible();
```

### 5. Use Page Object Methods Instead of Inline Playwright Calls

```typescript
// Good — intent is clear
await contractsPage.createContract(sampleContract);

// Bad — test knows too much about the UI
await page.click('.fab');
await page.fill('#mat-input-0', sampleContract.title);
await page.click('button.mat-primary');
```

### 6. Test IDs for Parallel Tests

If running tests in parallel, ensure each test uses unique data to avoid conflicts:

```typescript
const uniqueTitle = `Contract_${test.info().workerIndex}_${Date.now()}`;
```

### 7. Leverage `testId` and Tracing

```typescript
// In config
use: {
  trace: 'on-first-retry',
}

// After a failure, inspect the trace:
// npx playwright show-trace path/to/trace.zip
```

## Running and Filtering Tests

```bash
# Run all tests
npx playwright test

# Run a single file
npx playwright test e2e/specs/contracts.spec.ts

# Run tests matching a name
npx playwright test -g "creates a contract"

# Run with a specific project
npx playwright test --project=chromium

# Run only failed tests
npx playwright test --project=chromium --last

# Run in headed mode
npx playwright test --headed

# Run with debug output
npx playwright test --debug
```

## Next Steps

- [Guide 02: Core Concepts](./02-core-concepts.md) — Review the fundamentals
- [Guide 04: Authentication](./04-authentication.md) — Master ABP OAuth login flows
- [Guide 05: Advanced Patterns](./05-advanced-patterns.md) — Network interception, mocking, tracing
