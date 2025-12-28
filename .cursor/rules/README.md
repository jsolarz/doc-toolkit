# Cursor Rules Directory

This directory contains structured rule files (`.mdc`) that guide AI behavior in Cursor. Rules are organized by domain and apply based on the `alwaysApply` flag and task context.

---

## Quick Start

1. **Start here**: `general.mdc` - Universal rules for all tasks
2. **For coding**: `coding/` directory - Code-specific rules
3. **For documentation**: `documentation/` directory - Documentation structure and style
4. **For architecture**: `architecture/` directory - Design and architecture patterns
5. **Navigation**: `meta-index.mdc` - Complete index of all rule files

---

## Rule File Structure

Each `.mdc` file follows this format:

```yaml
---
alwaysApply: true  # or false
---
# Rule Title

Rule content...
```

- `alwaysApply: true`: Rule applies to all tasks
- `alwaysApply: false`: Rule applies only when contextually relevant

## Rule File Organization

### Structure Hierarchy

```
.cursor/rules/
├── general.mdc                    # Universal rules (always apply)
├── meta-index.mdc                 # Complete index
├── README.md                      # Navigation guide
├── architecture/                  # Architecture & design
│   ├── ai-behavior.mdc
│   ├── cloud-solution-architecture.mdc
│   ├── code-quality-style.mdc
│   ├── performance.mdc
│   ├── safety-robustness.mdc
│   ├── security.mdc
│   ├── testing.mdc
│   └── version-control.mdc
├── coding/                        # Code-specific rules
│   ├── coding-mode.mdc           # NEW
│   ├── documentation.mdc
│   ├── general-behavior.mdc      # IMPROVED
│   └── project-navigation.mdc
├── documentation/                 # Documentation rules
│   ├── adrs-and-architecture.mdc
│   ├── clarity-and-tone.mdc
│   ├── documentation-structure.mdc
│   ├── prds-and-requirements.mdc
│   ├── quality-and-review.mdc
│   ├── rfps-and-tenders.mdc
│   └── sows-and-contracts.mdc
├── governance/                    # Governance & collaboration
│   ├── accuracy-and-verification.mdc
│   ├── communication-and-collaboration.mdc
│   ├── communication-style.mdc   # IMPROVED
│   ├── consistency-and-reuse.mdc
│   └── strict-architecture-governance.mdc
├── frameworks/                    # Framework alignment
│   ├── iso-standards.mdc
│   ├── itil.mdc
│   ├── safe.mdc
│   └── togaf.mdc
└── industries/                   # Industry-specific
    ├── defense-industry.mdc
    ├── finance.mdc
    ├── government-tenders.mdc
    ├── healthcare.mdc
    └── saas-cloud.mdc
```

---

## Directory Organization

### `/general.mdc`
Universal rules that apply to all tasks: tone, behavior, verification, security, testing.

### `/coding/`
Code-specific rules:
- `coding-mode.mdc`: Language-specific patterns (.NET, Python, TypeScript)
- `general-behavior.mdc`: Code modification principles
- `documentation.mdc`: Code documentation standards
- `project-navigation.mdc`: Codebase exploration guidelines

### `/architecture/`
Architecture and design rules:
- `ai-behavior.mdc`: AI-specific behavior patterns
- `cloud-solution-architecture.mdc`: Cloud-native architecture patterns
- `code-quality-style.mdc`: Code style and quality standards
- `performance.mdc`: Performance optimization principles
- `safety-robustness.mdc`: Code safety and robustness
- `security.mdc`: Security best practices
- `testing.mdc`: Testing standards
- `version-control.mdc`: Git workflow guidelines

### `/documentation/`
Documentation structure and style:
- `adrs-and-architecture.mdc`: ADR format and structure
- `clarity-and-tone.mdc`: Writing style and tone
- `documentation-structure.mdc`: Document structure standards
- `prds-and-requirements.mdc`: Requirements documentation
- `quality-and-review.mdc`: Review and quality standards
- `rfps-and-tenders.mdc`: RFP/tender response guidelines
- `sows-and-contracts.mdc`: SOW and contract documentation

### `/governance/`
Governance and collaboration:
- `accuracy-and-verification.mdc`: Fact-checking and verification
- `communication-and-collaboration.mdc`: Collaboration patterns
- `communication-style.mdc`: Communication guidelines
- `consistency-and-reuse.mdc`: Consistency and template reuse
- `strict-architecture-governance.mdc`: Enterprise architecture governance

### `/frameworks/`
Framework-specific alignment:
- `iso-standards.mdc`: ISO 27001/9001 compliance
- `itil.mdc`: ITIL service management
- `safe.mdc`: SAFe agile framework
- `togaf.mdc`: TOGAF enterprise architecture

### `/industries/`
Industry-specific patterns:
- `defense-industry.mdc`: Defense and mission-critical systems
- `finance.mdc`: Financial services and compliance
- `government-tenders.mdc`: Government procurement
- `healthcare.mdc`: Healthcare and HIPAA compliance
- `saas-cloud.mdc`: SaaS and cloud-native patterns

---

## How Rules Are Applied

1. **Always Apply Rules**: Files with `alwaysApply: true` are loaded for every task
2. **Contextual Rules**: Files with `alwaysApply: false` are loaded when relevant
3. **Conflict Resolution**: General rules override specialized rules for tone, clarity, and safety. Specialized rules override general rules for domain-specific structure or compliance.

---

## Related Files

- **AGENTS.md**: Quick reference protocol summary
- **CLAUDE.md**: Detailed protocol with philosophical foundation and Claude Skills
- **meta-index.mdc**: Complete index of all rule files with descriptions

---

## Maintenance

When adding or modifying rules:

1. Update the relevant `.mdc` file
2. Update `meta-index.mdc` if adding a new file or changing scope
3. Ensure consistency with `general.mdc` principles
4. Add cross-references where rules relate to each other
5. Test that rules don't conflict with existing rules

---

## Rule Design Principles

1. **Clarity**: Rules should be unambiguous and actionable
2. **Consistency**: Related rules should align, not contradict
3. **Completeness**: Cover common scenarios without being exhaustive
4. **Context**: Rules should specify when they apply
5. **Cross-Reference**: Related rules should reference each other
