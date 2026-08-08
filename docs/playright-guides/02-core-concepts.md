# Guide 02: Playwright Core Concepts

This guide covers the fundamental building blocks of Playwright tests: fixtures, locators, actions, and assertions. All examples use the LegalTech Angular app as reference.

## Fixtures

Fixtures are the backbone of Playwright's test runner. They provide a way to set up and tear down resources that your tests depend on.

### Built-in Fixtures

Playwright provides several built-in fixtures (the most common are accessed via destructured parameters):

```typescript
import { test, expect, type Page, type Locator } from '@playwright/test';
```

| Fixture | Type | Description |
|---|---|---|
| `page` | `Page` | A Chromium page (tab). Automatically created and closed per test |
| `context` | `BrowserContext` | An isolated browsing context (like an incognito window). Pages belong to a context |
| `browser` | `Browser` | The browser instance |
| `request` | `APIRequestContext` | HTTP client for making API calls |
| `expect` | `Expect` | Assertion functions |

### How Fixtures Flow

```typescript
test('example with fixtures', async ({ page, context }) => {
  // page fixture is automatically created fresh for each test
  await page.goto('/');
  // context provides additional capabilities like incognito pages, cookies, storage
});
```

### Custom Fixtures

You can create your own fixtures for things like logged-in pages, test data, or configuration:

```typescript
import { test as base } from '@playwright/test';

// Extend the built-in 'test' with a custom fixture
type MyFixtures = {
  authenticatedPage: Page;
};

export const test = base.extend<MyFixtures>({
  authenticatedPage: async ({ page }, use) => {
    // Setup: log in
    await page.goto('/account/login');
    await page.fill('input[name="userName"]', 'admin@abp.io');
    await page.fill('input[name="password"]', '1q2w3E*');
    await page.click('button[type="submit"]');
    await page.waitForURL('http://localhost:4200/');

    // Use the authenticated page in the test
    await use(page);

    // Teardown: automatically cleared when page is closed
  },
});
```

Usage:

```typescript
import { test } from './fixtures';

test('can navigate to contracts', async ({ authenticatedPage }) => {
  await authenticatedPage.click('a[href="/contracts"]');
  await expect(authenticatedPage).toHaveURL(/\/contracts/);
});
```

## Locators

A `Locator` represents a way to find elements in the DOM. It's lazily evaluated (queries at action time, not at creation time) and resilient to DOM changes.

### Creating Locators

```typescript
// By CSS selector
const button = page.locator('.btn-primary');

// By text content
const link = page.locator('text=View All');

// By role (accessibility-based, recommended)
const nav = page.locator('role=navigation');
const heading = page.locator('role=heading[name="Welcome"]');

// By ABP data attributes (the Angular components use these)
const card = page.locator('abp-card');

// Chaining
const firstCard = page.locator('.card').first();
const cardByTitle = page.locator('.card', { hasText: 'Contracts' });

// Filter
const contractRows = page.locater('tr').filter({ has: page.locator('.contract-title') });
```

### Locator Methods

| Method | Description |
|---|---|
| `.first()` / `.last()` | Pick a specific element from the matched set |
| `.nth(n)` | Pick the n-th element (0-indexed) |
| `.filter(options)` | Narrow down a locator by text, has, visible, etc. |
| `.locator(selector)` | Nested locator (find child within parent) |

### Locator vs ElementHandle

```typescript
// Locator — queries lazily, re-queries on every action (preferred)
const button = page.locator('.submit-btn');
await button.click();

// ElementHandle — resolves immediately, holds the element (avoid in most cases)
const buttonHandle = await page.$('.submit-btn');
await buttonHandle?.click();
```

### Waiting for Elements

Playwright automatically waits for elements to be actionable before performing actions. You rarely need explicit waits. But you can configure behavior:

```typescript
// Wait for element to be visible
await page.locator('.card').first().waitFor({ state: 'visible' });

// Wait for specific text
await page.locator('text=Welcome').waitFor();

// Wait with a custom timeout (default 30s for actions, 5s for assertions)
await page.locator('.loading').waitFor({ state: 'detached', timeout: 30_000 });
```

## Actions

Actions simulate user interactions. Playwright automatically waits for the element to be ready before acting.

```typescript
// Clicking
await page.click('a[href="/contracts"]');
await page.click('.btn-submit', { button: 'right' }); // right-click
await page.dblClick('.card');
await page.click('.menu-item', { delay: 100 }); // hold before clicking

// Typing / filling
await page.fill('input[name="userName"]', 'admin@abp.io');
await page.type('#password', '1q2w3E*'); // types character by character
await page.press('input[name="password"]', 'Enter');

// Selection
await page.selectOption('select[name="status"]', 'active');
await page.selectOption('select[name="category"]', { label: 'Legal' });

// Check / uncheck
await page.check('input[type="checkbox"]');
await page.uncheck('input[type="checkbox"]');

// Hover
await page.hover('.nav-link');

// Drag and drop
await page.dragAndDrop('#source', '#target');

// File upload
await page.setInputFiles('input[type="file"]', 'test-data/sample.pdf');
```

### Actionability Checks

Before every action, Playwright verifies:
1. **Visibility** — element is visible (has non-zero size, not `display: none`, not `visibility: hidden`)
2. **Enabled** — element is not disabled
3. **Stability** — element is not animating/moving (checked for 2 animation frames)
4. **Pointer/CSS** — element is not covered by another element; no pointer-events: none
5. **Scrollable** — element can be scrolled into view

If checks fail, Playwright retries up to the `actionTimeout` (default 30s in config, overridable per call).

## Assertions

Playwright's `expect` API is tightly integrated with locators and auto-retries until the condition is met or the timeout expires.

### Locator Assertions

```typescript
const welcome = page.locator('h3:has-text("Welcome")');

// Visibility
await expect(welcome).toBeVisible();
await expect(welcome).toBeHidden();

// Content
await expect(welcome).toHaveText('Welcome to LegalTech');
await expect(welcome).toContainText('Welcome');
await expect(welcome).toHaveText(/Welcome to.*/);

// Attributes
const link = page.locator('a[href="/contracts"]');
await expect(link).toHaveAttribute('href', '/contracts');

// CSS classes
await expect(link).toHaveClass(/btn/);
await expect(link).toHaveCSS('color', 'rgb(0, 123, 255)');

// Count
await expect(page.locator('.contract-row')).toHaveCount(5);

// Checked / selected
await expect(page.locator('input[type="checkbox"]')).toBeChecked();
await expect(page.locator('option[selected]')).toBeSelected();

// Enabled / disabled
await expect(page.locator('button[type="submit"]')).toBeEnabled();
await expect(page.locator('button[disabled]')).toBeDisabled();

// Editable
await expect(page.locator('input[name="title"]')).toBeEditable();
await expect(page.locator('textarea')).toBeEditable();
```

### Page-Level Assertions

```typescript
// URL
await expect(page).toHaveURL('http://localhost:4200/contracts');
await expect(page).toHaveURL(/\/contracts\/\w+/);
await page.goto('/contracts');
await expect(page).toHaveURL(/\/contracts/);

// Title
await expect(page).toHaveTitle('LegalTech');
await expect(page).toHaveTitle(/LegalTech - .*/);

// URL contains path
await expect(page).toHaveURL(url => url.pathname.includes('/contracts'));
```

### Negating Assertions

```typescript
// Use .not
await expect(page.locator('.error-banner')).not.toBeVisible();
await expect(page.locator('.loading-spinner')).not.toBeInDOM();
```

### Soft Assertions

Use `expect.soft()` for checks that should not stop the test if they fail (useful for collecting multiple failures):

```typescript
await expect.soft(page.locator('.contract-title')).toHaveText('Test Contract');
await expect.soft(page.locator('.contract-status')).toHaveText('Draft');
// Test continues even if the first soft assertion fails
```

### Custom Timeouts

```typescript
// Global in config (default: 5000ms)
use: { actionTimeout: 10_000 }

// Per-assertion
await expect(locator).toBeVisible({ timeout: 15_000 });

// Per-test (overrides both)
test.use({ actionTimeout: 30_000 });
```

## Navigation and Events

```typescript
// Go to a page
await page.goto('/');
await page.waitForLoadState('networkidle');

// Reload / go back / forward
await page.reload({ waitUntil: 'domcontentloaded' });
await page.goBack();
await page.goForward();

// Wait for navigation to complete
await page.waitForLoadState('networkidle'); // all requests settled
await page.waitForLoadState('domcontentloaded'); // DOM parsed
await page.waitForLoadState('load'); // all resources loaded

// Wait for URL change
await page.waitForURL('**/contracts');
```

## Working with Multiple Pages/Contexts

```typescript
// Open a link in a new tab (popup)
const [newPage] = await Promise.all([
  page.context().waitForEvent('page'),
  page.click('a[target="_blank"]'),
]);
await newPage.waitForLoadState();

// Open a new page in the same context
const newTab = await page.context().newPage();
await newTab.goto('/account/login');
```

## Working with Frames and Modals

```typescript
// Frames
const frame = page.frameLocator('iframe[name="docs-frame"]');
await frame.fill('input[name="query"]', 'contract');

// Dialogs (alert, confirm, prompt)
page.on('dialog', dialog => {
  console.log('Dialog message:', dialog.message());
  dialog.dismiss(); // or dialog.accept()
});
await page.click('button[type="button"]', { hasText: 'Delete' });
```

## Practical Example: Testing a Contract List Page

Here is a test that combines all the concepts above against a typical contract list page in LegalTech:

```typescript
import { test, expect } from '@playwright/test';

test.describe('Contracts List Page', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/contracts');
  });

  test('displays contract table headers', async ({ page }) => {
    const headers = page.locator('ngx-datatable .datatable-header-cell');
    await expect(headers).toContainText([
      'Title',
      'Counterparty',
      'Status',
      'Effective Date',
    ]);
  });

  test('filters contracts by title', async ({ page }) => {
    const searchInput = page.locator('input[placeholder*="Search"]');
    await searchInput.fill('NDA-001');
    await page.keyboard.press('Enter');

    // Wait for filtered results
    const rows = page.locator('ngx-datatable .datatable-row');
    await expect(rows).toHaveCount(1);
    await expect(rows.first()).toContainText('NDA-001');
  });

  test('navigates to contract detail', async ({ page }) => {
    const firstLink = page.locator('.contract-link').first();
    const href = await firstLink.getAttribute('href');
    await firstLink.click();

    // URL should change to contract detail page
    await expect(page).toHaveURL(/\/contracts\//);
    await expect(page.locator('.contract-title')).toBeVisible();
  });
});
```

## Next Steps

- [Guide 01: Getting Started](./01-getting-started.md) — If you haven't set up Playwright yet
- [Guide 03: Test Organization](./03-test-organization.md) — Learn Page Object Model and test best practices
- [Guide 04: Authentication](./04-authentication.md) — Handle ABP OAuth login flows
- [Guide 05: Advanced Patterns](./05-advanced-patterns.md) — Network mocking, tracing, and debugging
