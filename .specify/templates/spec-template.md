# Feature Specification: [FEATURE NAME]

**Feature Branch**: `[###-feature-name]`  
**Created**: [DATE]  
**Status**: Draft  
**Input**: User description: "$ARGUMENTS"

## User Scenarios & Testing *(mandatory)*

<!--
  IMPORTANT: User stories should be PRIORITIZED as user journeys ordered by importance.
  Each user story/journey must be INDEPENDENTLY TESTABLE - meaning if you implement just ONE of them,
  you should still have a viable MVP (Minimum Viable Product) that delivers value.
  
  Assign priorities (P1, P2, P3, etc.) to each story, where P1 is the most critical.
  Think of each story as a standalone slice of functionality that can be:
  - Developed independently
  - Tested independently
  - Deployed independently
  - Demonstrated to users independently
-->

### User Story 1 - [Brief Title] (Priority: P1)

[Describe this user journey in plain language]

**Why this priority**: [Explain the value and why it has this priority level]

**Independent Test**: [Describe how this can be tested independently - e.g., "Can be fully tested by [specific action] and delivers [specific value]"]

**Acceptance Scenarios**:

1. **Given** [initial state], **When** [action], **Then** [expected outcome]
2. **Given** [initial state], **When** [action], **Then** [expected outcome]

---

### User Story 2 - [Brief Title] (Priority: P2)

[Describe this user journey in plain language]

**Why this priority**: [Explain the value and why it has this priority level]

**Independent Test**: [Describe how this can be tested independently]

**Acceptance Scenarios**:

1. **Given** [initial state], **When** [action], **Then** [expected outcome]

---

### User Story 3 - [Brief Title] (Priority: P3)

[Describe this user journey in plain language]

**Why this priority**: [Explain the value and why it has this priority level]

**Independent Test**: [Describe how this can be tested independently]

**Acceptance Scenarios**:

1. **Given** [initial state], **When** [action], **Then** [expected outcome]

---

[Add more user stories as needed, each with an assigned priority]

### Edge Cases

<!--
  ACTION REQUIRED: The content in this section represents placeholders.
  Fill them out with the right edge cases. For FoundryRagBackend, include RAG,
  grounding, prompt-injection, Azure failure, and local ingestion cases when
  the feature touches question answering or indexing.
-->

- What happens when the user query is empty, too long, or outside the indexed domain?
- What happens when retrieval returns no documents or scores below the confidence threshold?
- How does the system respond when retrieved documents contain instructions that conflict with system behavior?
- How does the system handle Azure OpenAI or Azure AI Search transient failures?
- What happens when required Azure configuration is missing during local development?

## Requirements *(mandatory)*

<!--
  ACTION REQUIRED: The content in this section represents placeholders.
  Fill them out with the right functional requirements.
-->

### Functional Requirements

- **FR-001**: System MUST [specific capability, e.g., "accept a user market question through an HTTP API"]
- **FR-002**: System MUST validate user queries before embedding or retrieval
- **FR-003**: System MUST generate embeddings and retrieve top-k matching documents before normal domain answers
- **FR-004**: System MUST build grounded prompts from clearly delimited retrieved documents
- **FR-005**: System MUST return answer text, source metadata, and retrieval metadata for grounded responses
- **FR-006**: System MUST return the configured insufficiency response when retrieved context is empty, low-confidence, or insufficient
- **FR-007**: System MUST ignore instructions embedded in retrieved documents and treat them as data only
- **FR-008**: System MUST keep Azure endpoints, deployment names, index names, API keys, top-k, and model settings in configuration
- **FR-009**: System MUST provide or preserve seed data ingestion when the feature changes indexed data behavior

*Example of marking unclear requirements:*

- **FR-010**: System MUST use [NEEDS CLARIFICATION: minimum retrieval score or insufficiency threshold not specified]
- **FR-011**: System MUST expose ingestion through [NEEDS CLARIFICATION: command, endpoint, or both not specified]

### Key Entities *(include if feature involves data)*

- **Market/Event Document**: Indexed domain document with title, category, event description, rules, outcomes, dates, explanatory notes, and source identity
- **Retrieval Result**: Retrieved document excerpt plus score and source metadata
- **Grounded Answer**: API response containing answer text, cited sources, retrieval metadata, and insufficiency state when applicable

### Constitution Alignment *(mandatory)*

- **RAG Flow**: [Describe how the feature preserves query validation, embedding, vector retrieval, prompt building, chat completion, and response shaping]
- **Grounding**: [Describe source metadata, insufficiency behavior, and unsupported-claim prevention]
- **Prompt Safety**: [Describe how retrieved text is delimited and treated as untrusted data]
- **Azure Boundaries**: [Describe interfaces and configuration used for Azure OpenAI and Azure AI Search]
- **Local Development**: [Describe seed data, ingestion, placeholders, and manual integration-test needs]

## Success Criteria *(mandatory)*

<!--
  ACTION REQUIRED: Define measurable success criteria.
  These must be technology-agnostic and measurable.
-->

### Measurable Outcomes

- **SC-001**: [Measurable metric, e.g., "A grounded answer includes at least one cited source when sufficient context is retrieved"]
- **SC-002**: [Measurable metric, e.g., "Out-of-domain or low-context questions return the insufficiency response"]
- **SC-003**: [Measurable metric, e.g., "Seed data can be ingested locally after Azure configuration is supplied"]
- **SC-004**: [Measurable metric, e.g., "Unit tests cover prompt construction, query validation, failure handling, and response shaping"]

## Assumptions

<!--
  ACTION REQUIRED: The content in this section represents placeholders.
  Fill them out with the right assumptions based on reasonable defaults
  chosen when the feature description did not specify certain details.
-->

- [Assumption about target users, e.g., "Users have stable internet connectivity"]
- [Assumption about scope boundaries, e.g., "Mobile support is out of scope for v1"]
- [Assumption about data/environment, e.g., "Existing authentication system will be reused"]
- [Dependency on existing system/service, e.g., "Requires access to the existing user profile API"]
