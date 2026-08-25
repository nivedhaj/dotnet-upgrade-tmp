# Student Service — Domain Knowledge Extraction

## Your task

You are a code archaeologist analysing the `ct.stu` .NET microservice. Your job is NOT
to understand the code as a developer. Your job is to extract **domain knowledge** —
what the business does, not how the code implements it.

The output is a **dialog tree**: a structured question-and-answer document that
captures system behaviour at every level of depth. A domain expert (school administrator,
registrar, finance officer) with no coding knowledge should be able to read it and
recognise their institution in it.

---

## Codebase map (pre-filled for you)

```
ct.stu/                          ← Main service (ASP.NET Core)
  Controllers/
    StudentController.cs         ← ~1200 lines, all student operations
    ReportController.cs          ← Reporting endpoints
  Repo/
    Resource/
      Student.cs                 ← Student + Enrollment domain entities (START HERE)
      Course.cs                  ← Course entity
      BaselineData.cs            ← Baseline assessment entity
      IEP.cs                     ← Individual Education Plan entity
      StudentEssentials.cs       ← Essentials tracking entity
    Db/
      Index/
        Student_IX.cs            ← RavenDB indexes (4 indexes inside)
        Course_IX.cs             ← Course indexes
        AttendanceEvent_IX.cs    ← Attendance index
        StudentEssentials_IX.cs  ← Essentials index
      Command/
        StudentCmd.cs            ← All write operations (~80KB — the core logic)
      Query/
        StudentQry.cs            ← All read/query operations
    Dto/
      StudentDto.cs              ← All request/response shapes
  _Core/
    Policy/                      ← Institution-specific business rules (9 engines)
      IPolicy.cs                 ← Policy interface
      DefaultPolicyEngine.cs     ← Default rules
      AKSHFNPolicyEngine.cs      ← Institution: AKSHFN
      BITLOKPolicyEngine.cs      ← Institution: BITLOK
      BTLCOLPolicyEngine.cs      ← Institution: BTLCOL
      PPECKNPolicyEngine.cs      ← Institution: PPECKN
      VVSGCSPolicyEngine.cs      ← Institution: VVSGCS
      VVSPHSPolicyEngine.cs      ← Institution: VVSPHS
      VVSPUCPolicyEngine.cs      ← Institution: VVSPUC
      VVSSPUPolicyEngine.cs      ← Institution: VVSSPU
    Middleware/
      Auth.cs                    ← Authentication
  Service/
    EtlService.cs                ← ETL/bulk data import
    ReportService.cs             ← Report generation
    StudentService.cs            ← Thin service layer

ct.stu.tests/
  StudentTests.cs                ← ~2400 lines, all student tests (START HERE)
  EtlTests.cs                    ← ETL tests
  ReportTests.cs                 ← Report tests
  IndexTests.cs                  ← Index tests
```

---

## Domain entities already identified

From reading `Student.cs`, these entities exist:

- **Student** — top-level aggregate; has Status (Active / Disabled)
- **Enrollment** — nested inside Student; one per institution; has Status
  (Active / Promoted / TC / Disabled / Unknown)
- **Person** — base type for Student, Father, Mother, Guardian
- **Domicile** — caste, country, mother tongue, state
- **FeesReceivable** — pending fee linked to a student per institution
- **OpeningBalance** — historical fee balance at start of year
- **IndividualEducationPlan (IEP)** — special needs education plan
- **Documents** — uploaded document references (diagnosis, disability cert, birth cert, etc.)
- **TCDto** — Transfer Certificate data (TC No, applied date, issued date, character, conduct, remarks)

**Student states:** Active (1), Disabled (99)

**Enrollment states:** Active (1), Promoted (10), TC (98), Disabled (99), Unknown (-1)

**Key business rule already found in code:**
- A student can have multiple Enrollments — one per institution
- `EnrolInInstitute` will update an existing enrollment if one already exists for that institution; it does NOT create a duplicate
- `SetCourse` requires both CourseId AND TermName — both are mandatory

---

## Rules

**Rule 1 — No implementation language in output**
Never use class names, method names, variable names, or framework terms in the dialog tree.

```
BAD:  "StudentCmd.EnrolInInstitute() calls RavenDB session to patch the Student document"
GOOD: "When a student is enrolled, the system links them to the institution and course,
       recording their date of admission, application number, and admission number"
```

**Rule 2 — Tests are ground truth for intended behaviour**
Read `StudentTests.cs` before reading `StudentCmd.cs`. Tests tell you what the author
thought the system SHOULD do. Implementation tells you what it DOES do. Note divergences.

**Rule 3 — Confidence levels are mandatory**
Every dialog tree node must have a confidence level:
- **HIGH** — confirmed by tests AND implementation
- **MEDIUM** — confirmed in implementation, no test coverage
- **LOW** — inferred or ambiguous
- **UNRESOLVED** — contradiction between sources

**Rule 4 — Gaps are first-class**
Mark gaps explicitly:
`[GAP — no test coverage, code path exists but behaviour unclear]`
`[UNRESOLVED — tests expect X, implementation does Y]`

**Rule 5 — State is the skeleton**
Everything hangs off the Enrollment state machine. Map every operation to the states
it reads, requires, or transitions to.

**Rule 6 — Policy engines are business rules**
There are 9 institution-specific policy engines. Each one encodes rules that DIFFER
from the default. These differences are high-value domain knowledge — they represent
decisions made per institution. Read them carefully and note what each institution does
differently.

---

## How to proceed

### Phase 1 — Orient (already partially done — verify and extend)

The codebase map above is pre-filled from initial inspection. Your job in Phase 1:

1. Read `StudentController.cs` and list ALL operations by their HTTP route — translate
   each to plain business language (e.g. `POST /tc` → "Issue Transfer Certificate")
2. Read `IPolicy.cs` to understand what decisions are policy-driven vs fixed
3. Scan `StudentDto.cs` to understand the full shape of data flowing in and out
4. Note any operations in the controller that have NO corresponding test

Produce an **orientation summary**:
- Complete list of operations (business language, not route names)
- What decisions are institution-specific (from IPolicy)
- What the 4 RavenDB indexes reveal about reporting needs

### Phase 2 — Test mining (read StudentTests.cs first)

Key test methods to mine (already identified):

| Test name | Business operation |
|---|---|
| `can_create_student` | Creating a new student record |
| `can_promote_student` | Promoting a student to next grade/year |
| `can_promote_sppu_students` | University-specific promotion (SPPU) |
| `can_change_student_section` | Moving a student to a different section |
| `can_change_enrol` / `can_change_enrol1` | Modifying enrollment details |
| `can_set_fee_receivable` | Setting pending fees on a student |
| `can_get_students_fees_receivable` | Querying students with outstanding fees |
| `can_disable_student_job` | Disabling a student record |
| `can_enable_student_job` | Re-enabling a student record |
| `can_get_disabled_students` | Querying disabled/alumni students |
| `can_get_all_passes_out_students` | Querying passed-out students |
| `can_set_course` | Updating a student's course assignment |
| `can_assign_students_to_section_with_list` | Bulk section assignment |
| `can_get_enrl_studentId` | StudentId vs GUID handling in enrollment |
| `assign_sec` | Section assignment logic |

For each test:
1. What inputs does it use?
2. What outcome does it assert?
3. Does it test a rejection/error path?

Produce a **test coverage map** with confidence ratings.

### Phase 3 — Implementation reading

Read `StudentCmd.cs` (the largest file, ~80KB) and `StudentQry.cs`.

Focus on:
- **Enrollment rules**: what prevents a duplicate enrollment? What happens if you
  re-enroll a student already enrolled at an institution?
- **Promotion logic**: what carries over from old enrollment to new? What is reset?
  What happens if the target institution already has an enrollment?
- **TC (Transfer Certificate) logic**: what state does this put the enrollment in?
  Can a TC enrollment be reversed?
- **Fee receivable logic**: how are fees linked to students? What triggers an update?
  What is the "spanning fee" variant?
- **StudentId assignment**: the code has special handling for GUIDs vs real student IDs
  — what is the business rule here?
- **Policy engine branching**: where does the code select a policy engine, and what
  decisions does it delegate?
- **ETL service**: what bulk operations are supported? What data is imported and from where?

### Phase 4 — Policy engine deep-dive

Read each policy engine and produce a **policy comparison table**:
- For each institution (AKSHFN, BITLOK, BTLCOL, PPECKN, VVSGCS, VVSPHS, VVSPUC, VVSSPU)
- For each policy decision point
- What does the Default engine do vs what does this institution's engine do?

This is likely the highest-value section of the document — institution-specific rules
are where business decisions live that aren't written down anywhere else.

### Phase 5 — Dialog tree construction

Build the dialog tree bottom-up. Structure:

```
Q: [Top-level operation or concept in plain business language]
A: [What happens, in plain business language] [CONFIDENCE]

  Q: [Edge case or condition]
  A: [What happens in this case] [CONFIDENCE]

    Q: [Further detail]
    A: [What happens] [CONFIDENCE]

  Q: [Another condition]
  A: [UNRESOLVED — description of contradiction]
```

**Mandatory sections in the dialog tree:**

1. Student lifecycle (create → enroll → promote → TC / disable)
2. Enrollment rules (one per institution, what's mandatory, what's optional)
3. Promotion (cross-institution, same-institution, SPPU-specific)
4. Transfer Certificate (TC) issuance
5. Section and course changes
6. Fees receivable (how fees are attached, updated, queried)
7. Contact management (student, father, mother, guardian — emails and mobiles)
8. IEP (Individual Education Plan) — what it tracks, who has it
9. Student Essentials — what it tracks
10. Documents — what documents are stored per student
11. ETL / bulk operations — what bulk imports are supported
12. Reports — what reports exist and what they show
13. Institution-specific rules (from policy engines)

### Phase 6 — Gap and contradiction report

List:
- All UNRESOLVED nodes
- All GAP nodes (untested code paths)
- Top 10 SME interview questions prioritised by business risk

---

## Output format

Produce a single markdown file: `student-domain-knowledge.md`

Sections:
1. **Domain Entity Overview** — entities, states, relationships (no code terms)
2. **Operations Catalogue** — every operation the system supports, in plain language
3. **Reporting Model** — what the indexes reveal about reporting needs
4. **Institution Policy Comparison** — what each institution does differently
5. **Dialog Tree** — the main artifact
6. **Gap and Contradiction Report**
7. **SME Interview Questions** — top 10, prioritised by business risk

---

## What good looks like

A school registrar reads `student-domain-knowledge.md` and says:
- "Yes, that's exactly how our enrollment process works"
- "Wait, we changed that promotion rule in 2022" ← most valuable outcome
- "I didn't know the system tracked that"

A developer reads it and can redesign the service without ever looking at the original code.

---

## Quick-start command

```bash
# From the repo root (where this CLAUDE.md lives)
claude -p "Follow the instructions in CLAUDE.md and produce student-domain-knowledge.md"
```
