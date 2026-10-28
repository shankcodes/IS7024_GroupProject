<!--
Sync Impact Report
- Version change: unversioned scaffold -> 1.0.0
- Modified principles: placeholder principles -> four project principles
- Added sections: Technology and Security Constraints; Development Workflow
- Removed sections: none
- Follow-up TODOs: confirm the original ratification date
-->

# IS7024 Group Project Constitution

## Core Principles

### I. Code Quality and Maintainability

Code MUST be clear, focused, and maintainable. Public types, methods, and non-obvious business rules MUST use meaningful names and documentation where needed. Features MUST follow the existing Razor Pages and .NET project structure. Duplication, unnecessary complexity, dead code, compiler warnings, and unhandled nullable-reference warnings MUST be addressed before completion.

### II. Testing Standards

New behavior MUST include automated tests appropriate to its risk and scope. Tests MUST cover normal behavior, validation failures, and relevant error or boundary conditions. Tests MUST be deterministic, isolated, and repeatable. A change MUST NOT be considered complete when relevant tests fail or existing behavior regresses.

### III. Consistent User Experience

User-facing Razor Pages MUST provide consistent navigation, layout, labeling, validation, feedback, and accessibility behavior. Forms MUST use server-side validation and display actionable validation messages. Pages MUST support responsive layouts and use shared site styling.

### IV. Performance Requirements

Application code MUST avoid unnecessary allocations, repeated I/O, blocking calls, unbounded resource usage, and inefficient data access. I/O-bound operations MUST use asynchronous APIs when available. Performance-sensitive changes MUST be validated with measurements or representative tests.

## Technology and Security Constraints

The application MUST target the repository's configured .NET version and use Razor Pages for page-based user experiences. Dependencies MUST be limited to those required by the feature. User input MUST be validated server-side and encoded when rendered. Secrets MUST NOT be committed to source control.

## Development Workflow

Each change MUST be reviewed against these principles. The solution MUST build successfully, relevant automated tests MUST pass, and new compiler warnings MUST NOT be introduced. Reviews MUST identify affected principles and explain exceptions.

## Governance

This constitution supersedes conflicting project practices. Amendments MUST be documented, reviewed, and reflected in the version number. MAJOR changes remove or weaken a principle; MINOR changes add or materially expand a principle; PATCH changes clarify wording. Exceptions MUST be explicit, technically justified, narrowly scoped, and approved during review.

**Version**: 1.0.0 | **Ratified**: TODO(RATIFICATION_DATE): Confirm original adoption date | **Last Amended**: 2026-10-27
