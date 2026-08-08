# Guide 04: Authentication & ABP OAuth Testing

The LegalTech Angular app uses ABP Framework's OAuth/OpenID Connect flow with the backend serving as the identity provider. This guide covers how to test authenticated flows in Playwright.

## Understanding the Auth Flow

1. When an unauthenticated user accesses a protected route (e.g., `/contracts`), the ABP `permissionGuard` redirects to `/account/login`.
2. The ABP `LoginComponent` renders a form with username and password fields.
3. On submit, the ABP OAuth module calls the backend's identity endpoint (`https://localhost:44334`) to authenticate.
4. On success, an access token is stored in `localStorage` / `sessionStorage` and the user is redirected back to the app.

## Default Credentials

The ABP data seeder creates a default admin user. These are defined in `src/Acme.LegalTech.Domain.Shared/LegalTechConsts.cs:8`:

| Field | Value |
|---|---|
| Email / Username | `admin@abp.io` |
| Password | `1q2w3E*` |
| Role | `admin` (full permissions) |

## Login Page Selectors

The ABP `@abp/ng.account` `LoginComponent` template uses these elements (extracted from the compiled component in `node_modules/@abp/ng.account/fesm2022/abp-ng.account.mjs`):

```html
<!-- Username / email field -->
<input id="login-input-user-name-or-email-address" formControlName="username" />

<!-- Password field -->
<input id="login-input-password" formControlName="password" type="password" />

<!-- Remember me checkbox -->
<input id="login-input-remember-me" formControlName="rememberMe" type="checkbox" />

<!-- Submit button -->
<abp-button buttonType="submit">Login</abp-button>
```

## Strategy 1: Authenticate in `beforeEach`

The simplest approach is to log in at the start of each test:

```typescript
import { test, expect } from '@playwright/test';

test.describe('Authenticated Flow', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/account/login');

    await page.fill('#login-input-user-name-or-email-address', 'admin@abp.io');
    await page.fill('#login-input-password', '1q2w3E*');
    await page.click('abp-button[buttontype="submit"]');

    // Wait for redirect back to app
    await page.waitForURL(/\/account\/login/, { timeout: 5000 }).catch(() => {});
    await page.waitForURL('http://localhost:4200/', { timeout: 30000 });
  });

  test('can access contracts page', async ({ page }) => {
    await page.getByRole('link', { name: /contracts/i }).click();
    await expect(page).toHaveURL(/\/contracts/);
  });
});
```

## Strategy 2: Reusable Login Helper

Create a helper function to avoid repeating login code:

```typescript
// e2e/support/auth.ts
import type { Page } from '@playwright/test';

export async function loginAsAdmin(page: Page) {
  const ADMIN_EMAIL = process.env.PLAYWRIGHT_ADMIN_EMAIL || 'admin@abp.io';
  const ADMIN_PASSWORD = process.env.PLAYWRIGHT_ADMIN_PASSWORD || '1q2w3E*';

  await page.goto('/account/login');
  await page.fill('#login-input-user-name-or-email-address', ADMIN_EMAIL);
  await page.fill('#login-input-password', ADMIN_PASSWORD);
  await page.click('abp-button[buttontype="submit"]');

  // Wait for successful authentication
  await page.waitForURL('http://localhost:4200/', { timeout: 30000 });
}
```

Then in your tests:

```typescript
import { test } from '@playwright/test';
import { loginAsAdmin } from '../support/auth';

test('authenticated test', async ({ page }) => {
  await loginAsAdmin(page);
  await page.goto('/contracts');
  // ... assertions
});
```

## Strategy 3: Custom Fixture (Recommended)

For test suites that require authentication across many tests, create a custom fixture that handles login once:

```typescript
// e2e/fixtures/authenticated.fixture.ts
import { test as base, type Page } from '@playwright/test';

export type AuthenticatedPage = Page & {
  // You can extend the Page type with custom methods if needed
};

export const test = base.extend<{
  authenticatedPage: AuthenticatedPage;
}>({
  authenticatedPage: async ({ page }, use) => {
    // Login
    await page.goto('/account/login');
    await page.fill('#login-input-user-name-or-email-address', 'admin@abp.io');
    await page.fill('#login-input-password', '1q2w3E*');
    await page.click('abp-button[buttontype="submit"]');
    await page.waitForURL('http://localhost:4200/', { timeout: 30000 });

    // Provide the authenticated page to the test
    await use(page as AuthenticatedPage);
  },
});

export { expect } from '@playwright/test';
```

Usage:

```typescript
// e2e/specs/contracts.spec.ts
import { test, expect } from '../fixtures/authenticated.fixture';

test.describe('Contracts Page (Authenticated)', () => {
  test('displays the contracts list', async ({ authenticatedPage }) => {
    await authenticatedPage.goto('/contracts');
    await expect(authenticatedPage).toHaveURL(/\/contracts/);
    await expect(authenticatedPage.locator('h3')).toContainText('Contracts');
  });
});
```

## Strategy 4: Storage State (For Speed)

Playwright can persist cookies and localStorage to a file, then reload it in future test runs. This avoids repeating the login on every test.

### Step 1: Create a global setup that logs in and saves state

```typescript
// e2e/global-setup.ts
import { type FullConfig, type Page } from '@playwright/test';

export default async function globalSetup(config: FullConfig) {
  // The browser context is not set up here; use the project-level globalSetup instead
}
```

### Step 2: Create a setup test file

```typescript
// e2e/auth.setup.ts
import { test as setup, expect } from '@playwright/test';

const ADMIN_EMAIL = 'admin@abp.io';
const ADMIN_PASSWORD = '1q2w3E*';

setup('authenticate', async ({ page }) => {
  await page.goto('/account/login');
  await page.fill('#login-input-user-name-or-email-address', ADMIN_EMAIL);
  await page.fill('#login-input-password', ADMIN_PASSWORD);
  await page.click('abp-button[buttontype="submit"]');
  await page.waitForURL('http://localhost:4200/', { timeout: 30000 });

  // Save the authenticated state (cookies, localStorage) to a file
  await page.context().storageState({ path: 'e2e/auth/admin-state.json' });
  console.log('Authentication state saved to e2e/auth/admin-state.json');
});
```

### Step 3: Configure Playwright to use the storage state

```typescript
// playwright.config.ts
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  // ... existing config
  projects: [
    {
      name: 'setup',
      testDir: './e2e',
      testMatch: /.*\auth\.setup\.ts/,
      // Run setup first, in its own project
    },
    {
      name: 'chromium',
      use: {
        ...devices['Desktop Chrome'],
        storageState: 'e2e/auth/admin-state.json',
      },
      // Depend on the setup project completing
      dependencies: ['setup'],
    },
  ],
});
```

Now tests in the `chromium` project start with the session already authenticated, skipping the login form entirely. This is the fastest approach for large test suites.

## Handling Logout

```typescript
async function logout(page: Page) {
  // Click the user menu (ABP theme shows this when logged in)
  await page.click('.lx-user-info .dropdown-toggle, [role="button"][aria-label*="user"]');

  // Click logout link
  await page.click('a:has-text("Logout"), a:has-text("Log Out")');

  // Wait for redirect to login
  await page.waitForURL('/account/login', { timeout: 10000 });
}
```

## Testing Permission-Guarded Routes

The app uses `permissionGuard` on several routes:

| Route | Permission Required |
|---|---|
| `/contracts` | `LegalTech.Contracts` |
| `/clauses` | `LegalTech.Clauses` |
| `/playbooks` | `LegalTech.Clauses.Playbooks` |
| `/reviews` | `LegalTech.Reviews` |
| `/obligations` | `LegalTech.Obligations` |
| `/reports` | `LegalTech.Reports` |

Test that unauthenticated users are redirected:

```typescript
test('unauthenticated user cannot access contracts', async ({ page }) => {
  await page.goto('/contracts');
  await expect(page).toHaveURL(/\/account\/login/);
});
```

Test that authenticated users with proper permissions can access:

```typescript
test('authenticated admin can access contracts', async ({ authenticatedPage }) => {
  await authenticatedPage.goto('/contracts');
  await expect(authenticatedPage).toHaveURL(/\/contracts/);
  await expect(authenticatedPage.locator('ngx-datatable')).toBeVisible();
});
```

## Environment Variables

Store credentials in Playwright config so you don't hardcode them:

```typescript
// playwright.config.ts
use: {
  baseURL: process.env.PLAYWRIGHT_BASE_URL || 'http://localhost:4200',
  ignoreHTTPSErrors: true,
}

// In tests:
const ADMIN_EMAIL = process.env.PLAYWRIGHT_ADMIN_EMAIL || 'admin@abp.io';
const ADMIN_PASSWORD = process.env.PLAYWRIGHT_ADMIN_PASSWORD || '1q2w3E*';
```

Then run with custom credentials:

```bash
PLAYWRIGHT_ADMIN_EMAIL=test@example.com PLAYWRIGHT_ADMIN_PASSWORD=custom npm run test:e2e
```

## Next Steps

- [Guide 02: Core Concepts](./02-core-concepts.md) — Review locators and assertions
- [Guide 05: Advanced Patterns](./05-advanced-patterns.md) — Network mocking, tracing, and debugging
