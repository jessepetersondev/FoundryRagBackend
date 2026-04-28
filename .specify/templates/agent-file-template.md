# [PROJECT NAME] Development Guidelines

Auto-generated from all feature plans. Last updated: [DATE]

## Active Technologies

[EXTRACTED FROM ALL PLAN.MD FILES]

## Project Structure

```text
[ACTUAL STRUCTURE FROM PLANS]
```

## Commands

[ONLY COMMANDS FOR ACTIVE TECHNOLOGIES]

## Code Style

[LANGUAGE-SPECIFIC, ONLY FOR LANGUAGES IN USE]

## Constitution Alignment

All generated guidance MUST preserve the FoundryRagBackend constitution:
controllers handle HTTP only, normal domain answers follow the explicit RAG
pipeline, retrieved context is treated as untrusted data, Azure SDK clients stay
behind interfaces, secrets stay in configuration, structured logging avoids
sensitive data, seed ingestion remains available, and business logic remains
unit-testable without live Azure services.

## Recent Changes

[LAST 3 FEATURES AND WHAT THEY ADDED]

<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->
