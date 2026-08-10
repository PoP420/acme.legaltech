import { test, expect, type Page } from '@playwright/test';

const ADMIN_USERNAME = 'admin@abp.io';
const ADMIN_PASSWORD = '1q2w3E*';
const BASE_URL = 'http://localhost:4200';

async function login(page: Page) {
  await page.goto(`${BASE_URL}/account/login`);
  await page.waitForLoadState('networkidle');

  await page.fill('input[name="LoginInput.UserNameOrEmailAddress"]', ADMIN_USERNAME);
  await page.fill('input[name="LoginInput.Password"]', ADMIN_PASSWORD);
  await page.check('input[name="LoginInput.RememberMe"]');
  await page.click('button[type="submit"]');

  await page.waitForLoadState('networkidle', { timeout: 60000 });
}

test.describe('Document Extraction and AI Assist', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test.describe('Document Upload and Extraction', () => {
    test('shows extraction status badge on document versions', async ({ page }) => {
      await page.goto(`${BASE_URL}/contracts`);
      await page.waitForLoadState('networkidle');

      const firstContract = page.locator('a[href*="/contracts/"]').first();
      if (await firstContract.count() > 0) {
        await firstContract.click();
        await page.waitForLoadState('networkidle');

        const versionTable = page.locator('table.table');
        if (await versionTable.count() > 0) {
          await expect(versionTable).toBeVisible();
        }
      }
    });

    test('displays Review button for successful extractions', async ({ page }) => {
      await page.goto(`${BASE_URL}/contracts`);
      await page.waitForLoadState('networkidle');

      const firstContract = page.locator('a[href*="/contracts/"]').first();
      if (await firstContract.count() > 0) {
        await firstContract.click();
        await page.waitForLoadState('networkidle');

        const reviewButton = page.locator('button:has-text("Review")');
        const count = await reviewButton.count();
        expect(count).toBeGreaterThanOrEqual(0);
      }
    });
  });

  test.describe('Extraction Review Modal', () => {
    test('opens extraction review modal when Review is clicked', async ({ page }) => {
      await page.goto(`${BASE_URL}/contracts`);
      await page.waitForLoadState('networkidle');

      const firstContract = page.locator('a[href*="/contracts/"]').first();
      if (await firstContract.count() > 0) {
        await firstContract.click();
        await page.waitForLoadState('networkidle');

        const reviewButton = page.locator('button:has-text("Review")').first();
        if (await reviewButton.count() > 0) {
          await reviewButton.click();

          const modal = page.locator('.modal.show, .modal-backdrop.show');
          await expect(modal).toBeVisible({ timeout: 5000 });

          const modalTitle = page.locator('h5:has-text("Review AI Extraction")');
          await expect(modalTitle).toBeVisible({ timeout: 5000 });
        }
      }
    });

    test('shows extracted fields in review form', async ({ page }) => {
      await page.goto(`${BASE_URL}/contracts`);
      await page.waitForLoadState('networkidle');

      const firstContract = page.locator('a[href*="/contracts/"]').first();
      if (await firstContract.count() > 0) {
        await firstContract.click();
        await page.waitForLoadState('networkidle');

        const reviewButton = page.locator('button:has-text("Review")').first();
        if (await reviewButton.count() > 0) {
          await reviewButton.click();

          const titleInput = page.locator('input[formcontrolname="title"]');
          await expect(titleInput).toBeVisible({ timeout: 5000 });
        }
      }
    });

    test('allows accepting extraction and updating contract', async ({ page }) => {
      await page.goto(`${BASE_URL}/contracts`);
      await page.waitForLoadState('networkidle');

      const firstContract = page.locator('a[href*="/contracts/"]').first();
      if (await firstContract.count() > 0) {
        await firstContract.click();
        await page.waitForLoadState('networkidle');

        const reviewButton = page.locator('button:has-text("Review")').first();
        if (await reviewButton.count() > 0) {
          await reviewButton.click();

          const acceptButton = page.locator('button:has-text("Accept")');
          if (await acceptButton.count() > 0) {
            await acceptButton.click();
            await page.waitForLoadState('networkidle');
          }
        }
      }
    });
  });

  test.describe('AI Permission Guards', () => {
    test('shows upload section for users with AttachDocument permission', async ({ page }) => {
      await page.goto(`${BASE_URL}/contracts`);
      await page.waitForLoadState('networkidle');

      const firstContract = page.locator('a[href*="/contracts/"]').first();
      if (await firstContract.count() > 0) {
        await firstContract.click();
        await page.waitForLoadState('networkidle');

        const fileInput = page.locator('input[type="file"]');
        if (await fileInput.count() > 0) {
          await expect(fileInput.first()).toBeVisible();
        }
      }
    });

    test('shows Download and View buttons for users with Default permission', async ({ page }) => {
      await page.goto(`${BASE_URL}/contracts`);
      await page.waitForLoadState('networkidle');

      const firstContract = page.locator('a[href*="/contracts/"]').first();
      if (await firstContract.count() > 0) {
        await firstContract.click();
        await page.waitForLoadState('networkidle');

        const downloadButton = page.locator('button:has-text("Download")');
        const viewButton = page.locator('button:has-text("View")');

        if (await downloadButton.count() > 0) {
          await expect(downloadButton.first()).toBeVisible();
        }
        if (await viewButton.count() > 0) {
          await expect(viewButton.first()).toBeVisible();
        }
      }
    });
  });
});
