# Implementation Plan: Multi-User Reading Data and Authentication

**Branch**: `db-init-and-auth-tummalga` | **Date**: 2026-11-01 | **Spec**: [spec.md](spec.md)

## Summary

Establish the BN-8 SQLite persistence foundation for `Books`, `ReadingList`, and `ReadingGoal`, then use ASP.NET Core Identity for account and role management, enforce user ownership through relational constraints and authorization checks, and support normalized reviews, comments, likes, and moderation metadata. The existing project already contains the core models, migrations, Identity registration, and startup migration application; Admin role provisioning is deferred to deployment, while remaining implementation work is concentrated in protected Razor Page workflows, ownership filtering, moderation actions, and automated coverage.

## Technical Context

**Language/Version**: C# on .NET 10

**Primary Dependencies**: ASP.NET Core Razor Pages, ASP.NET Core Identity, Entity Framework Core SQLite 10.0.14

**Storage**: `BookNest.db` SQLite database configured through `ConnectionStrings:DefaultConnection`; EF Core migrations create and update the schema.

**Testing**: `dotnet build` for compilation; add a focused automated test project for model validation, ownership authorization, uniqueness constraints, authentication, and moderation behavior.

**Target Platform**: ASP.NET Core web application running locally or on a supported .NET hosting environment.

**Project Type**: Server-rendered Razor Pages web application.

**Performance Goals**: Personal list, goal, and review queries should use indexed ownership and relationship columns; normal user actions should complete without avoidable repeated database calls.

**Constraints**: User input must be validated server-side; secrets must not be committed; personal queries must be scoped to the authenticated user; only Admin-role users may moderate content; the application must build without new compiler warnings.

**Scale/Scope**: Initial public multi-user MVP with a shared book catalog, personal reading data, one review per user/book, comments, likes, and role-based moderation.

## Constitution Check

- **Code Quality and Maintainability**: Pass. The plan follows the existing Razor Pages structure, uses focused entities and a single EF context, and preserves explicit relationships and constraints.
- **Testing Standards**: Pass with implementation gate. Automated coverage must be added for normal, validation, authorization, and boundary behavior before the feature is considered complete.
- **Consistent User Experience**: Pass with implementation gate. Account, validation, unauthorized, forbidden, and moderation feedback must use the shared Razor layout and server-side validation patterns.
- **Performance Requirements**: Pass. Indexed ownership and unique-key queries are planned; asynchronous EF operations should be used in page handlers and services.
- **Technology and Security Constraints**: Pass. The plan uses the configured .NET version, Razor Pages, Identity password hashing, server-side validation, and no source-controlled secrets.
- **Development Workflow**: Pass with implementation gate. Build, tests, and warning checks are required before completion.

## Project Structure

### Documentation (this feature)

```text
specs/001-db-init-auth/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── checklists/requirements.md
└── tasks.md
```

### Source Code (repository root)

```text
Data/
├── ApplicationDbContext.cs
└── Migrations/
Models/
├── Book.cs
├── BookReview.cs
├── ReadingGoal.cs
├── ReadingList.cs
├── ReadingStatus.cs
├── ReviewComment.cs
├── ReviewLike.cs
├── ReviewModerationStatus.cs
└── User.cs
Pages/
├── Account/              # Identity UI and account flows
├── Shared/
└── ...                   # Razor Pages with ownership checks
Services/
Program.cs
appsettings.json
IS7024_GroupProject.csproj
```

**Structure Decision**: Keep the existing single ASP.NET Core Razor Pages project. Database entities remain under `Models`, EF configuration and migrations under `Data`, startup/role seeding under `Program.cs` and `Services`, and user-facing account and protected workflows under `Pages`.

## Implementation Phases

1. **Foundation**: Packages, Identity context, SQLite connection, model constraints, and migrations. Admin role provisioning is deferred to deployment.
2. **Authentication**: Account registration, login/logout, unique email validation, and authenticated session behavior.
3. **Ownership**: Protect personal pages and scope all reading-list, goal, review, comment, and like queries to the current user.
4. **Review community features**: Add review submission, aggregate ratings, comments, likes, and moderation workflows.
5. **Verification and polish**: Automated tests, migration/quickstart validation, user feedback, documentation, and warning-free build.

## Complexity Tracking

No constitution violations are planned. An application-level Admin role is intentionally used instead of unrestricted database access to preserve least privilege.
