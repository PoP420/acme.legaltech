# Guide 05: Advanced Patterns

This guide covers Playwright's advanced features: network interception, API mocking, tracing for debugging, parallel execution, and CI integration.

## Network Interception

Playwright gives you full control over network requests. You can observe, modify, or block requests.

### Logging Requests and Responses

```typescript
import { test, expect } from '@playwright/test';

test('logs all network requests', async ({ page }) => {
  // Log all responses
  page.on('response', response => {
    console.log(`${response.status()} ${response.url()}`);
  });

  // Log all requests
  page.on('request', request => {
    console.log(`${request.method()} ${request.url()}`);
  });

  // Log request failures
  page.on('requestfailed', request => {
    console.log(`FAILED: ${request.method()} ${request.url()} - ${request.failure()?.errorText}`);
  });

  await page.goto('/');
  // ... test actions
});
```

### Blocking Requests

Prevent slow or unnecessary resources from loading:

```typescript
test('blocks analytics and fonts for faster tests', async ({ page }) => {
  await page.route('**/analytics/**', route => route.abort());
  await page.route('**/fonts.googleapis.com/**', route => route.abort());
  await page.route('**/fontawesome.com/**', route => route.abort());

  await page.goto('/');
  // ... assertions
});
```

### Modifying Request Headers

```typescript
test('sends custom headers', async ({ page }) => {
  await page.route('https://localhost:44334/**', route => {
    const headers = {
      ...route.request().headers(),
      'x-test-id': 'my-test',
    };
    route.continue({ headers });
  });

  await page.goto('/');
});
```

### Mocking API Responses

You can intercept API calls and return mock data, useful for testing edge cases without modifying the backend.

#### Mocking the ABP Application Configuration

The ABP Angular app loads configuration from `https://localhost:44334/api/abp/application-configuration`. You can mock this to test different permission sets:

```typescript
test('mocks application configuration with limited permissions', async ({ page }) => {
  await page.route('https://localhost:44334/api/abp/application-configuration*', async route => {
    const mockResponse = {
      "authentication": {
        "isAuthenticated": true,
        "userName": "test-user"
      },
      "authorization": {
        "permissions": [
          "LegalTech.Contracts",
          "LegalTech.Contracts.Create"
        ]
      },
      "localization": {
        "currentCulture": {
          "cultureName": "en",
          "uiCultureName": "en",
          "isRightToLeft": false
        }
      },
      "feature": {},
      "currentTenant": {
        "id": "2c7a03d3-8a9e-4a25-d7f8-3b8e9c012345"
      },
      "multiTenancy": {
        "isEnabled": true
      }
    };

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(mockResponse),
    });
  });

  await page.goto('/');
  // The app will think the user only has Contract permissions
  // Navigation to /clauses should show a permission error
});
```

#### Mocking Contract API Responses

Mock the contract list endpoint for deterministic data:

```typescript
test('mocks contract list with test data', async ({ page }) => {
  const mockContracts = {
    items: [
      {
        id: '11111111-1111-1111-1111-111111111111',
        title: 'Master Service Agreement',
        counterpartyName: 'ACME Corp',
        status: 1,
        effectiveDate: '2024-01-15T00:00:00',
        expirationDate: '2025-01-15T00:00:00',
        contractValue: 75000,
      },
      {
        id: '22222222-2222-2222-2222-222222222222',
        title: 'NDA - Confidential',
        counterpartyName: 'Globex Inc',
        status: 0,
        effectiveDate: '2024-03-01T00:00:00',
        expirationDate: null,
        contractValue: null,
      },
    ],
    totalCount: 2,
  };

  await page.route('https://localhost:44334/api/abp/application-services/contracts*', route => {
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(mockContracts),
    });
  });

  await page.goto('/contracts');
  await expect(page.locator('ngx-datatable .datatable-row')).toHaveCount(2);
});
```

#### Simulating API Errors

Test error handling by returning error responses:

```typescript
test('shows error message when contract API fails', async ({ page }) => {
  await page.route('https://localhost:44334/api/abp/application-services/contracts*', route => {
    route.fulfill({
      status: 500,
      contentType: 'application/json',
      body: JSON.stringify({
        error: {
          code: 'InternalError',
          message: 'Database is temporarily unavailable',
          details: 'Connection timeout',
        },
      }),
    });
  });

  await page.goto('/contracts');
  await expect(page.locator('.toast-error, .alert-danger')).toBeVisible();
  await expect(page.locator('text=Database is temporarily unavailable')).toBeVisible();
});
```

## Tracing

Traces are a powerful debugging tool that capture everything happening during a test: DOM snapshots, network activity, console logs, and step-by-step actions.

### Enabling Traces

In `playwright.config.ts`:

```typescript
use: {
  baseURL: 'http://localhost:4200',
  trace: 'on-first-retry', // Captures trace on the first retry of a failed test
  screenshot: 'only-on-failure',
  video: 'retain-on-failure',
}
```

Modes:
- `'on-first-retry'` — Captures when a test is retried (after first failure)
- `'on'` — Captures for every test (slow, but comprehensive)
- `'off'` — No tracing (default)

### Viewing Traces

After a failed test, open the trace:

```bash
npx playwright show-trace path/to/trace.zip
```

Or find traces in `test-results/` folder after a failed run.

### Programmatic Tracing

```typescript
test('manual tracing example', async ({ page }, testInfo) => {
  await testInfo.trace.start({
    screenshots: true,
    snapshots: true,
    sources: true,
  });

  await page.goto('/');
  await page.click('a[href="/contracts"]');

  await testInfo.trace.stop({ path: 'my-trace.zip' });
});
```

## Debugging Tests

### Playwright Inspector

Run tests with the `--debug` flag to open the Playwright Inspector, which lets you step through tests interactively:

```bash
npx playwright test --debug
```

This pauses at the first line of the first test and opens a browser window where you can inspect elements and step through code.

### Headed Mode

Run tests with the browser visible:

```bash
npx playwright test --headed
```

### Slow Motion

Slow down actions for debugging:

```bash
npx playwright test --headed --slow-mo 1000
```

### Browser Context

Preserve the browser state between tests for debugging:

```bash
npx playwright test --headed --browser-context "context-name"
```

Or in the config:

```typescript
use: {
  headless: false, // always run headed (useful for development)
}
```

## Screenshots and Videos

### Full-Page Screenshots

```typescript
test('captures full page screenshot', async ({ page }) => {
  await page.goto('/');
  await page.screenshot({
    path: 'screenshots/home-full.png',
    fullPage: true,
  });
});
```

### Element Screenshots

```typescript
await page.locator('.contract-card').screenshot({
  path: 'screenshots/contract-card.png',
});
```

### Auto-Capturing on Failure

```typescript
use: {
  screenshot: 'only-on-failure',
  video: 'retain-on-failure',
}
```

### Element Screenshots in Traces

```typescript
await page.screenshot({
  path: 'error-state.png',
  fullPage: true,
  animations: 'disabled', // hide animations in the screenshot
});
```

## API Testing

Playwright includes a built-in `request` fixture for making HTTP API calls. This is useful for setup and teardown (seeding test data, cleaning up).

```typescript
import { test, expect, request } from '@playwright/test';

test('creates a contract via API then verifies in UI', async ({ page, request }) => {
  // Create a contract via the backend API
  const createResponse = await request.post('https://localhost:44334/api/abp/application-services/contracts', {
    ignoreHTTPSErrors: true,
    data: {
      title: 'API-Created Contract',
      counterpartyName: 'API Test Corp',
      status: 0,
    },
  });

  expect(createResponse.ok()).toBeTruthy();
  const createdContract = await createResponse.json();

  // Log in and verify the contract appears in the UI
  await page.goto('/account/login');
  // ... login steps
  await page.goto('/contracts');
  await expect(page.locator(`text=${createdContract.title}`)).toBeVisible();

  // Cleanup
  await request.delete(`https://localhost:44334/api/abp/application-services/contracts/${createdContract.id}`, {
    ignoreHTTPSErrors: true,
  });
});
```

## Parallel and Cross-Browser Testing

### Multiple Browsers

```typescript
// playwright.config.ts
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
    { name: 'firefox', use: { ...devices['Desktop Firefox'] } },
    { name: 'webkit', use: { ...devices['Desktop Safari'] } },
    { name: 'Mobile Chrome', use: { ...devices['Pixel 5'] } },
    { name: 'Mobile Safari', use: { ...devices['iPhone 12'] } },
  ],
});
```

```bash
# Run all browsers
npx playwright test

# Run specific browser
npx playwright test --project=chromium

# Run all projects in parallel
npx playwright test --workers=4
```

### Sharding

Split test files across multiple workers or machines:

```bash
# Run on 4 workers, each gets a subset of test files
npx playwright test --workers=4

# Run a specific shard (CI use case)
npx playwright test --shard=1/4  # first of 4 shards
npx playwright test --shard=2/4  # second of 4 shards
```

## Worker Isolation

Each worker runs in its own browser context, providing isolated cookies, localStorage, and sessions. This means tests running in parallel don't interfere with each other.

```typescript
// This test is fully parallelized - each worker has its own browser
test.describe.parallel('Parallel tests', () => {
  test('user 1 can log in', async ({ page }) => {
    // Worker 1
  });
  test('user 2 can log in', async ({ page }) => {
    // Worker 2 - completely isolated
  });
});
```

## CI Integration

### GitHub Actions

Create `.github/workflows/e2e.yml`:

```yaml
name: E2E Tests
on: [push, pull_request]

jobs:
  test:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Install Playwright
        run: |
          cd angular
          npm install --legacy-peer-deps
          npx playwright install --with-deps --system-deps chromium

      - name: Start backend
        run: |
          cd src/Acme.LegalTech.HttpApi.Host
          dotnet run &
        env:
          ASPNETCORE_ENVIRONMENT: Development

      - name: Run Playwright tests
        run: |
          cd angular
          npm run test:e2e

      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: playwright-report
          path: angular/playwright-report/
        if-no-files-found: ignore

      - name: Upload traces
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: test-results
          path: angular/test-results/
        if-no-files-found: ignore
```

### Environment Configuration

In CI, you may need to configure different settings:

```typescript
// playwright.config.ts
const isCI = !!process.env.CI;

export default defineConfig({
  use: {
    baseURL: process.env.PLAYWRIGHT_BASE_URL || 'http://localhost:4200',
    ignoreHTTPSErrors: true,
    trace: 'on-first-retry',
    screenshot: isCI ? 'only-on-failure' : 'on',
    video: isCI ? 'off' : 'off',
    actionTimeout: isCI ? 15_000 : 10_000,
    navigationTimeout: isCI ? 15_000 : 10_000,
  },
  projects: isCI
    ? [
        { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
      ]
    : [
        { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
        { name: 'firefox', use: { ...devices['Desktop Firefox'] } },
      ],
});
```

## Summary: Advanced Features at a Glance

| Feature | How to Use |
|---|---|
| **Network log** | `page.on('response', ...)` |
| **Block requests** | `page.route(url, route => route.abort())` |
| **Mock API** | `page.route(url, route => route.fulfill({...}))` |
| **Modify headers** | `page.route(url, route => route.continue({headers}))` |
| **Tracing** | `trace: 'on-first-retry'` in config |
| **Debug** | `npx playwright test --debug` |
| **Headless vs Headed** | `--headed` flag |
| **Slow motion** | `--slow-mo 1000` |
| **Screenshots** | `page.screenshot({ path: '...' })` |
| **Videos** | `video: 'retain-on-failure'` |
| **API calls** | Built-in `request` fixture |
| **Cross-browser** | Multiple `projects` entries |
| **Parallelization** | `--workers=N` |
| **Sharding** | `--shard=N/M` |
| **CI integration** | `webServer` + env vars |
