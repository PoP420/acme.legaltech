import { test, expect } from '@playwright/test';

test.describe('Home Page', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
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
    // Unauthenticated users should see a login option in the header/menu
    const loginLink = page.getByRole('link', { name: /login/i });
    await expect(loginLink).toBeVisible();
  });
});
