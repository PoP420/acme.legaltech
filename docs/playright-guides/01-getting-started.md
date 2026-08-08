# Guide 01: Playwright Getting Started

This guide walks through installing Playwright, creating the configuration, and writing your first test against the LegalTech Angular application.

## Prerequisites

- Node.js 18+ (already installed for Angular)
- .NET 10 SDK (already installed for the backend)
- PostgreSQL database (already configured for the backend)

## Step 1: Install Playwright

```bash
cd angular
npm install --save-dev @playwright/test
npx playwright install --with-deps chromium
```

- `@playwright/test` is the test runner and library (goes in `devDependencies`).
- `npx playwright install` downloads the browser binaries (Chromium, Firefox, WebKit) and their system dependencies.

## Step 2: Create the Configuration File

Create `angular/playwright.config.ts`:

```typescript
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  reporter: 'list',
  use: {
    baseURL: 'http://localhost:4200',
    trace: 'on-first-retry',
    ignoreHTTPSErrors: true,
  },
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
  ],
  webServer: {
    command: 'npm start',
    url: 'http://localhost:4200',
    reuseExistingServer: !process.env.CI,
    timeout: 120 * 1000,
  },
});
```

### Configuration field guide

| Field | Purpose |
|---|---|
| `testDir` | Directory containing your test files |
| `baseURL` | Base URL prepended to `page.goto()` calls that start with `/` |
| `trace` | `on-first-retry` captures a trace on the first retry of a failed test, useful for debugging |
| `ignoreHTTPSErrors` | Skips HTTPS certificate validation. Required here because the ABP backend uses a self-signed dev cert |
| `projects` | Runs the same tests in different browsers. Each entry can have its own settings |
| `webServer` | Tells Playwright to start `npm start`, wait until `url` responds, then run tests. Use `reuseExistingServer` to avoid restarting if one is already running |

> **Important:** Playwright only manages the Angular dev server. The .NET backend (`https://localhost:44334`) must be running separately. If it is not, the Angular app will show "An error has occurred!" because the ABP OAuth and API calls will fail.

## Step 3: Update tsconfig for e2e

Replace `e2e/tsconfig.json`:

```json
{
  "extends": "../tsconfig.json",
  "compilerOptions": {
    "outDir": "../out-tsc/e2e",
    "module": "commonjs",
    "target": "es2018",
    "types": ["@playwright/test"]
  },
  "include": ["**/*.spec.ts", "**/*.ts"]
}
```

This removes the legacy Protractor types (`jasmine`, `jasminewd2`) and adds Playwright's type definitions.

## Step 4: Write Your First Test

Create `e2e/home.spec.ts`:

```typescript
import { test, expect } from '@playwright/test';

test.describe('Home Page', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    // ABP loads config and OAuth data from the backend — wait for all requests
    await page.waitForLoadState('networkidle');
  });

  test('has title LegalTech', async ({ page }) => {
    await expect(page).toHaveTitle(/LegalTech/);
  });

  test('shows welcome message', async ({ page }) => {
    const heading = page.locator('h3:has-text("Welcome")');
    await expect(heading).toBeVisible({ timeout: 15000 });
  });

  test('shows feature cards for contracts, clauses, playbooks, reviews, and obligations', async ({ page }) => {
    const featureHeadings = page.locator('.home-feature-card h5');
    await expect(featureHeadings).toContainText([
      'Contracts',
      'Clauses',
      'Playbooks',
      'Reviews',
      'Obligations',
    ]);
  });

  test('shows login link when unauthenticated', async ({ page }) => {
    const loginLink = page.getByRole('link', { name: /login/i });
    await expect(loginLink).toBeVisible();
  });
});
```

### Why `waitForLoadState('networkidle')`?

The ABP Angular app makes several asynchronous API calls during initialization (OpenID configuration, application config, localization resources). `page.goto('/')` resolves as soon as the initial HTML loads, but the component content renders only after those API calls complete. `waitForLoadState('networkidle')` waits until there are no network requests for 500ms, ensuring the app is fully initialized.
```
```

### Key concepts in this test

- **`test.describe(title, fn)`** — Groups related tests under a logical name.
- **`test.beforeEach(fn)`** — Hook that runs before every test in the block. Receives a fixture object; `page` is one of Playwright's built-in fixtures.
- **`test(title, fn)`** — Defines a test case.
- **`page.goto('/')`** — Navigates to the base URL + `/` path.
- **`page.locator(selector)`** — Finds elements. Returns a `Locator` object that is lazily evaluated (queries the DOM at assertion time, not at creation).
- **`expect(locator).toBeVisible()`** — Assertion that waits up to the default timeout (5s) for the element to appear in the rendered DOM.

## Step 5: Add an npm Script

Add to `package.json` scripts:

```json
{
  "scripts": {
    "test:e2e": "playwright test"
  }
}
```

## Step 6: Run the Tests

```bash
npm run test:e2e
```

This will:
1. Start the Angular dev server (`npm start` → `ng serve`) automatically via `webServer`.
2. Wait for `http://localhost:4200` to respond.
3. Launch Chromium and run each test.
4. Print a summary.

## Running Specific Tests

```bash
# Run a single test file
npx playwright test e2e/home.spec.ts

# Run a single test by name (partial match)
npx playwright test "has title LegalTech"

# Run in headed mode (see the browser)
npx playwright test --headed

# Run with a specific worker count
npx playwright test --workers=1
```

## Viewing the HTML Report

Playwright generates an HTML report by default (when `reporter: 'html'` is set). After tests run:

```bash
npx playwright show-report
```

This opens a browser window showing pass/fail status, screenshots, and traces for each test.

## Troubleshooting

### Tests hang or show "An error has occurred!"

The ABP Angular app makes several API calls during initialization:
- `https://localhost:44334/.well-known/openid-configuration` (OAuth)
- `https://localhost:44334/api/abp/application-configuration` (permissions, settings)
- Localization and feature flag endpoints

If any of these fail, the app shows an error page. Make sure:
1. The .NET backend is running (`dotnet run` in `src/Acme.LegalTech.HttpApi.Host`)
2. `ignoreHTTPSErrors: true` is set in the config (for the self-signed HTTPS cert)
3. CORS is configured on the backend (`http://localhost:4200` is in `CorsOrigins` in `appsettings.json`)
4. Use `await page.waitForLoadState('networkidle')` after `page.goto()` to wait for ABP's async init to complete
5. Increase assertion timeouts (e.g., `{ timeout: 15000 }`) for elements that render after API calls resolve

### Certificate errors in browser

The ABP backend uses a development HTTPS certificate. `ignoreHTTPSErrors: true` in the config handles this for Playwright's browser. If you also need the certificate trusted for other reasons:

```bash
dotnet dev-certs https --trust
```

### Port already in use

If port 4200 is in use by another process, either stop that process or override the webServer URL:

```typescript
webServer: {
  command: 'npm start -- --port 4201',
  url: 'http://localhost:4201',
  ...
}
```

## Next Steps

- [Guide 02: Core Concepts](./02-core-concepts.md) — Learn the building blocks of Playwright tests
- [Guide 03: Test Organization](./03-test-organization.md) — Organize tests with Page Objects
- [Guide 04: Authentication](./04-authentication.md) — Handle ABP OAuth login flows
- [Guide 05: Advanced Patterns](./05-advanced-patterns.md) — Network mocking, tracing, and debugging
