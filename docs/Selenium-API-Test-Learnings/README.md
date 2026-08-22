# API Testing — Learning Course

> A practical, project-based course built from a **real, passing 7-test suite** that drives the
> Acme.LegalTech ABP API over HTTP. Each module maps to working code in
> `test/Acme.LegalTech.HttpApi.Tests/`.

**Textbook:** [`API-Testing-Course.md`](API-Testing-Course.md) — read it end to end, or follow the
module path below.

---

## Who this is for

- QA / SDETs who know Selenium but want faster, more stable **API-level** tests.
- Developers who must test **auth, permissions, and contracts** without a browser.
- Anyone onboarding to the Acme.LegalTech API test suite.

## What you'll be able to do

- Explain why API tests beat UI tests for contract/permission coverage.
- Authenticate against **OpenIddict / OAuth2** from code (password grant → JWT).
- Understand **anti-forgery, Bearer vs cookie auth, and ABP error wrapping**.
- Build a **reusable HTTP client** and an **xunit fixture** (`IAsyncLifetime`).
- Write **happy + negative** tests (401 / 403 / 404 / full CRUD).
- Debug and harden a **flaky** integration test.

## Syllabus (learning path)

| # | Module | In the textbook | You will learn |
|---|---|---|---|
| 0 | Orientation | §0 | What you'll build mentally |
| 1 | Why API testing (not Selenium) | §1 | Speed/stability trade-offs, when to use each |
| 2 | Know the system under test | §2 | ABP wrappers, endpoints, auth, permissions |
| 3 | Project structure & tooling | §3 | Where things live, xunit + Shouldly conventions |
| 4 | The test client (`ApiClient`) | §4 | One thin client: TLS, login, auth, retries |
| 5 | Fixtures & test lifecycle | §5 | `IAsyncLifetime`, login-once, data hygiene |
| 6 | The test cases | §6 | 7 tests as lessons (auth, 401, list, CRUD, 403) |
| 7 | Running the suite | §7 | Host + `dotnet test` + env overrides |
| 8 | Pitfalls we hit | §8 | 10 real mistakes (400 vs 401, 204, concurrency…) |
| 9 | Exercises | §9 | 8 hands-on tasks to master it |
| 10 | Cheat sheet | §10 | Endpoints + status codes at a glance |

## How to study

1. **Run it first.** Start the host (`dotnet run --project src/Acme.LegalTech.HttpApi.Host`) and
   run `dotnet test test/Acme.LegalTech.HttpApi.Tests/Acme.LegalTech.HttpApi.Tests.csproj`. See 7 green.
2. **Read Module 2** to understand the API (auth is the part everyone gets wrong).
3. **Read Modules 4–6** alongside the actual source files.
4. **Do Module 9 exercises** — learning is in the doing. Break an assertion on purpose and watch it fail.
5. **Attempt the concurrency bug fix** (Exercise 7): it's a real app defect the tests surfaced.

## Files in this course

```
Selenium-API-Test-Learnings/
├── README.md                  # this file — course home & syllabus
└── API-Testing-Course.md      # the full textbook (modules 0–10)
```

> Note on the folder name: the original request called these "Selenium" tests. They are **HTTP API
> tests** — Selenium cannot test an API. The folder keeps the original name for findability; the
> content is pure API testing.
