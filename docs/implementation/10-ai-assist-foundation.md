# 10 AI Assist Foundation

## Goal and MVP scope

Implement controlled AI-assisted workflows for extraction, risk scoring, and retrieval support with mandatory human review.

## Current state delta

No AI service orchestration currently exists in repository modules.

## Domain model and ownership

Primary entities:

- IngestionJob
- ExtractionSuggestion
- RiskAssessmentSuggestion
- SuggestionDecision

All entities tenant-owned and fully auditable.

## Vertical slices

### Slice 1: ingestion and orchestration

- Add asynchronous ingestion pipeline for uploaded contracts.
- Add OCR and text extraction adapter boundaries.

### Slice 2: suggestion workflows

- Generate clause extraction suggestions with confidence and source spans.
- Generate risk suggestions against playbook rules.

### Slice 3: human review console

- Add accept/reject/correct actions.
- Persist final approved values as authoritative domain data.

### Slice 4: retrieval assist

- Add controlled retrieval pipeline over approved content.
- Return sources and confidence context in answers.

## Permissions and role checks

- AIAssist.Default
- AIAssist.RunJobs
- AIAssist.ReviewSuggestions
- AIAssist.ConfigureProviders

## Data rules and failure modes

- Block direct promotion of AI outputs without human action.
- Prevent provider errors from breaking core CLM operations.

## Test and acceptance

- Validate async job lifecycle and retries.
- Validate suggestion acceptance/correction persistence.
- Validate retrieval responses include source references.

## Risk register

- Risk: false-positive risk suggestions.
- Mitigation: confidence thresholds and reviewer verification.

## Observability

- Track job queue health, provider failures, and suggestion acceptance rates.

## Definition of done

- Ingestion and suggestion APIs complete.
- Human-in-the-loop review UI complete.
- AI outputs fully auditable and traceable.
