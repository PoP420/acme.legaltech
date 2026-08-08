# Playwright Guides for LegalTech

A practical, project-specific guide to learning Playwright by testing the LegalTech Angular application.

## What You'll Learn

This guide series walks you through progressively more advanced Playwright concepts, all using real UI elements and workflows from the LegalTech app.

## Guides

### [Guide 01: Getting Started](./01-getting-started.md)
Install Playwright, create the configuration, clean up legacy Protractor files, and write your first test. Learn how Playwright's `webServer` auto-starts the Angular dev server and how to handle the ABP backend's HTTPS dev certs.

### [Guide 02: Core Concepts](./02-core-concepts.md)
The building blocks of every Playwright test: fixtures (built-in and custom), locators (CSS, text, role), actions (click, fill, select), and assertions (visibility, content, count, URL). Includes auto-waiting, actionability checks, and best practices for resilient selectors.

### [Guide 03: Test Organization](./03-test-organization.md)
Move from ad-hoc tests to a maintainable test suite using the Page Object Model. Learn how to structure your `e2e/` folder, create page classes (Login, Home, ContractsList), use custom fixtures for authenticated sessions, manage test data, and follow best practices like `data-testid` attributes and soft assertions.

### [Guide 04: Authentication & ABP OAuth](./04-authentication.md)
Test the ABP OAuth login flow end-to-end. Learn four strategies: inline `beforeEach` login, reusable helper functions, custom authenticated fixtures, and storage state for fast parallel test execution. Includes selectors extracted from the actual ABP `@abp/ng.account` LoginComponent, permission guard testing, and logout flows.

### [Guide 05: Advanced Patterns](./05-advanced-patterns.md)
Level up your tests: network interception (logging, blocking, header modification), API mocking (mock ABP config responses, simulate 500 errors), tracing and debugging (Playwright Inspector, `--debug` mode), screenshots and videos, API testing with the `request` fixture, parallel and cross-browser execution, and CI/CD integration (GitHub Actions workflow).

## Quick Start

```bash
# 1. Backend must be running (terminal 1)
cd src/Acme.LegalTech.HttpApi.Host
dotnet run

# 2. Run Playwright tests (terminal 2) — this auto-starts the Angular dev server
cd angular
npm run test:e2e

# 3. View the HTML report
npx playwright show-report

# 4. Run a specific test in headed mode for debugging
npx playwright test home.spec.ts --headed --debug
```

## Project-Specific Details

| Item | Details |
|---|---|
| **Angular app URL** | `http://localhost:4200` |
| **Backend API URL** | `https://localhost:44334` (HTTPS, self-signed cert) |
| **Admin credentials** | `admin@abp.io` / `1q2w3E*` |

| Route | Protection | Permission |
|---|---|---|
| `/` | None | — |
| `/account/login` | None | — |
| `/contracts` | `permissionGuard` | `LegalTech.Contracts` |
| `/clauses` | `permissionGuard` | `LegalTech.Clauses` |
| `/playbooks` | `permissionGuard` | `LegalTech.Clauses.Playbooks` |
| `/reviews` | `permissionGuard` | `LegalTech.Reviews` |
| `/obligations` | `permissionGuard` | `LegalTech.Obligations` |
| `/reports` | `permissionGuard` | `LegalTech.Reports` |

| Config File | Location |
|---|---|
| Playwright config | `angular/playwright.config.ts` |
| Tests | `angular/e2e/` |
| HTML report | `angular/playwright-report/` |
| Test results | `angular/test-results/` |
| Screenshots | `angular/test-results/` |
| Traces | `angular/test-results/` (`.zip` files on failure) |
