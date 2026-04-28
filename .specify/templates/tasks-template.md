---

description: "Task list template for feature implementation"
---

# Tasks: [FEATURE NAME]

**Input**: Design documents from `/specs/[###-feature-name]/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Unit tests are required for constitution-governed business logic,
including prompt construction, query validation, failure handling, and response
shaping. Manual Azure integration checks are required when live Azure resources
are configured.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **API project**: `src/FoundryRagBackend.Api/`
- **Tests**: `tests/FoundryRagBackend.Tests/`
- **Seed data**: `data/seed/`
- **Documentation**: `README.md` and feature `quickstart.md`
- Adjust paths only when `plan.md` documents a different real structure

<!-- 
  ============================================================================
  IMPORTANT: The tasks below are SAMPLE TASKS for illustration purposes only.
  
  The /speckit.tasks command MUST replace these with actual tasks based on:
  - User stories from spec.md (with their priorities P1, P2, P3...)
  - Feature requirements from plan.md
  - Entities from data-model.md
  - Endpoints from contracts/
  - FoundryRagBackend constitution gates for RAG, grounding, Azure boundaries,
    observability, tests, seed ingestion, and local setup
  
  Tasks MUST be organized by user story so each story can be:
  - Implemented independently
  - Tested independently
  - Delivered as an MVP increment
  
  DO NOT keep these sample tasks in the generated tasks.md file.
  ============================================================================
-->

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [ ] T001 Create .NET Web API and test project structure per implementation plan
- [ ] T002 Initialize ASP.NET Core dependencies, Azure SDK dependencies, and test dependencies
- [ ] T003 [P] Configure nullable reference types, formatting, and local user-secret placeholders

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

Examples of foundational tasks (adjust based on your project):

- [ ] T004 Define strongly typed Azure OpenAI, Azure AI Search, and RAG options in src/FoundryRagBackend.Api/Options/
- [ ] T005 [P] Define interfaces for embedding generation, vector search, chat completion, prompt building, and document ingestion
- [ ] T006 [P] Setup API routing, dependency injection, correlation/request ID middleware, and consistent error responses
- [ ] T007 Create shared contracts for question requests, grounded answers, source metadata, and retrieval metadata
- [ ] T008 Configure structured logging and basic transient Azure retry handling without logging secrets
- [ ] T009 Add seed data files and local ingestion entry point or endpoint

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - [Title] (Priority: P1) 🎯 MVP

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 1 (constitution-required where behavior is touched)

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T010 [P] [US1] Unit test for prompt construction in tests/FoundryRagBackend.Tests/Prompts/[test-name].cs
- [ ] T011 [P] [US1] Unit test for query validation, insufficiency handling, or response shaping in tests/FoundryRagBackend.Tests/Application/[test-name].cs

### Implementation for User Story 1

- [ ] T012 [P] [US1] Create API DTOs or records in src/FoundryRagBackend.Api/Contracts/
- [ ] T013 [P] [US1] Create domain or retrieval models in src/FoundryRagBackend.Api/Models/
- [ ] T014 [US1] Implement application workflow service in src/FoundryRagBackend.Api/Application/ (depends on T012, T013)
- [ ] T015 [US1] Implement HTTP endpoint in src/FoundryRagBackend.Api/Controllers/ without direct Azure SDK calls
- [ ] T016 [US1] Add validation, insufficiency response behavior, and consistent error handling
- [ ] T017 [US1] Add structured logging for received query, retrieval count, top scores, prompt path, and model call status

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - [Title] (Priority: P2)

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 2 (constitution-required where behavior is touched)

- [ ] T018 [P] [US2] Unit test for [behavior] in tests/FoundryRagBackend.Tests/[area]/[test-name].cs
- [ ] T019 [P] [US2] Manual integration check documented in specs/[###-feature-name]/quickstart.md when Azure resources are required

### Implementation for User Story 2

- [ ] T020 [P] [US2] Create [Entity] model in src/FoundryRagBackend.Api/Models/[entity].cs
- [ ] T021 [US2] Implement [Service] in src/FoundryRagBackend.Api/[area]/[service].cs
- [ ] T022 [US2] Implement [endpoint/feature] in src/FoundryRagBackend.Api/Controllers/[controller].cs
- [ ] T023 [US2] Integrate with User Story 1 components (if needed)

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - [Title] (Priority: P3)

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 3 (constitution-required where behavior is touched)

- [ ] T024 [P] [US3] Unit test for [behavior] in tests/FoundryRagBackend.Tests/[area]/[test-name].cs
- [ ] T025 [P] [US3] Manual integration check documented in specs/[###-feature-name]/quickstart.md when Azure resources are required

### Implementation for User Story 3

- [ ] T026 [P] [US3] Create [Entity] model in src/FoundryRagBackend.Api/Models/[entity].cs
- [ ] T027 [US3] Implement [Service] in src/FoundryRagBackend.Api/[area]/[service].cs
- [ ] T028 [US3] Implement [endpoint/feature] in src/FoundryRagBackend.Api/Controllers/[controller].cs

**Checkpoint**: All user stories should now be independently functional

---

[Add more user story phases as needed, following the same pattern]

---

## Phase N: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] TXXX [P] README updates for architecture, RAG flow, setup, seed ingestion, and tradeoffs
- [ ] TXXX Code cleanup and refactoring
- [ ] TXXX Performance optimization across all stories
- [ ] TXXX [P] Additional unit tests for prompt safety, failure handling, and source metadata
- [ ] TXXX Prompt-injection and secret-handling hardening
- [ ] TXXX Run quickstart.md validation

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - May integrate with US1 but should be independently testable
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - May integrate with US1/US2 but should be independently testable

### Within Each User Story

- Constitution-required tests MUST be written and FAIL before implementation
- Models before services
- Services before endpoints
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, all user stories can start in parallel (if team capacity allows)
- All tests for a user story marked [P] can run in parallel
- Models within a story marked [P] can run in parallel
- Different user stories can be worked on in parallel by different team members

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together (if tests requested):
Task: "Contract test for [endpoint] in tests/contract/test_[name].py"
Task: "Integration test for [user journey] in tests/integration/test_[name].py"

# Launch all models for User Story 1 together:
Task: "Create [Entity1] model in src/models/[entity1].py"
Task: "Create [Entity2] model in src/models/[entity2].py"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test User Story 1 independently
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 → Test independently → Deploy/Demo
4. Add User Story 3 → Test independently → Deploy/Demo
5. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1
   - Developer B: User Story 2
   - Developer C: User Story 3
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
