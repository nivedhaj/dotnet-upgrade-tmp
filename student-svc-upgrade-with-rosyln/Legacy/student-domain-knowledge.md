# Student Domain Knowledge

This document captures student-domain behavior in plain business language. Confidence means:

- HIGH: confirmed by both tests or operational scripts and the service behavior.
- MEDIUM: confirmed by observed system behavior, but test coverage is missing or weak.
- LOW: inferred from data shapes, reports, or operational scripts.
- UNRESOLVED: sources conflict or the behavior appears incorrect.

## Orientation Summary

### Public Operation Inventory

The student area supports these business operations:

- List students for the current institution, list all students, and fetch a student profile. [MEDIUM]
- Create a student, update core identity details, upload or set a student photo, and assign an institution student number. [MEDIUM]
- Enable or disable a student's enrollment in the current institution. [MEDIUM]
- Issue a transfer certificate. [MEDIUM]
- Promote a student from one course/institution/year to another. [HIGH]
- Set address, home phone, student email, student mobile, gender, tags, roll number, course, section, branch, domicile, and timetable-related data. [MEDIUM]
- Update father, mother, and guardian details, email addresses, and mobile numbers. [MEDIUM]
- Enable or disable father, mother, and guardian email/mobile contacts without deleting them. [MEDIUM]
- Build recipient lists for SMS/email by student, parent, course, section, tags, and absence filters. [MEDIUM]
- Send absence messages and parent messages. [MEDIUM]
- Query filters, facets, exam-facet students, flat student data, attendance widgets, gender counts, institution counts, tagged students, and students with outstanding fees. [MEDIUM]
- Add, update, enable, disable, and retrieve student essentials. [MEDIUM]
- Add and retrieve Individual Education Plans. [MEDIUM]
- Store document references for diagnosis report, disability report, birth certificate, identification proof, address proof, therapy report, psychological assessment report, and photo. [MEDIUM]
- Create and retrieve baseline assessment forms and submitted baseline assessment records. [MEDIUM]

The reporting area supports:

- Marks cards, fee receipts, transfer certificate view, certificates, hall tickets, timetable reports, student detail reports, parent detail reports, coursewise and sectionwise breakup reports, new admissions, absentee reports, TC-student reports, and IEP reports. [MEDIUM]
- CSV outputs exist for student details and parent details. [MEDIUM]

### Institution-Specific Decisions

The institution policy layer controls:

- How many attendance sessions exist in a day.
- Which SMS gateway is used.
- The default absence message.
- How student-number prefixes are generated from course names.
- Course display/order sequence.
- Print pagination settings.
- Whether admission numbers are generated automatically.
- The admission-number prefix when automatic generation is enabled.
- Baseline assessment picklists for physical/occupational therapy, speech/language, and special education.

### What Reporting Indexes Reveal

- Student reports are centered on institution, course, term, section, enrollment state, roll number, gender, date of birth, date of admission, admission number, student number, tags, parent names, parent PAN, student/parent contact lists, virtual login identity, and pending fee records. [MEDIUM]
- Search is designed around parent and guardian email/mobile contact lookup. [MEDIUM]
- Coursewise and institution-level counts are expected by gender and include only active student records with active enrollments. [MEDIUM]
- Attendance summaries are keyed by institution, course, student, and date. [MEDIUM]
- Student essentials are retrieved by student, separate from the core student record. [MEDIUM]

## Policy Comparison

| Institution | Attendance sessions | Student number prefix | Course order | Print records per page | Admission number behavior | Baseline assessment lists |
|---|---:|---|---|---:|---|---|
| Default | 1 | First letter of course, or Z when blank | Playhome through VII | 3 | Manual | Empty |
| AKSHFN | 2 | Same as default | Playhome through VII | 3 | Auto-generate with `AK/yyyy-yy/0001` pattern | Detailed special-needs picklists |
| BITLOK | 1 | Same as default | Playhome through VII | 3 | Manual | Mostly empty, but some special-education picklists exist |
| BTLCOL | 8 | Same as default | Playhome through VII | 3 | Manual | Detailed special-needs picklists |
| PPECKN | 1 | Same as default | Playhome through VII | 3 | Manual | Empty |
| VVSGCS | 1 | Mapped by course: Playhome=P, KG I=L, KG II=U, I=1, II=2, III=3, IV=4, V=5, VI=6, VII=7U | Playhome through VII | 10 | Manual | Empty |
| VVSPHS | 1 | Mapped by course: 8th=8, 9th=9, Xth=X | 8th, 9th, Xth | 3 | Manual | Empty |
| VVSPUC | 2 | Same as default | PUC I, PUC II | 3 | Manual | Empty |
| VVSSPU | 2 | Same as default | PUC I, PUC II | 3 | Manual | Empty |

[GAP - no tests directly assert most institution-policy differences.]

## Test Coverage Map

- Creating a student appears in tests, but the main creation test has no active assertion and much of its original call is commented out. Observed system behavior confirms the flow. [MEDIUM]
- Promotion appears in tests and observed system behavior. The tests exercise the workflow but do not assert the final enrollment state. [MEDIUM]
- SPPU-style promotion is represented as a bulk script-generation workflow using course maps and carrying term/section forward. It is operational-script evidence, not an assertion test. [MEDIUM]
- Section change, bulk section assignment, and enrollment cleanup are represented by operational scripts against existing data. Observed system behavior supports direct section and course changes. [MEDIUM]
- Fee receivable creation and fee receivable query appear in operational scripts and observed system behavior. Assertions are absent. [MEDIUM]
- Disabling and enabling student jobs are tested as queued events; observed system behavior changes the enrollment state and sends update jobs. [MEDIUM]
- Disabled, TC, and passed-out student queries appear as data extraction scripts rather than assertion tests. [LOW]
- Student number behavior for stored GUIDs is explicitly tested as a small rule check and reflected in enrollment identity display. [HIGH]
- Contact extraction and recipient workflows have several operational tests but little assertion coverage. [MEDIUM]
- IEP, baseline assessment, essentials, and document operations have service and report support, but little or no student-domain assertion coverage. [MEDIUM]

## Dialog Tree

### Student Lifecycle

Q: What is a student record?
A: A student record represents one learner and holds identity details, contact details, parent/guardian details, domicile details, documents, fee links, special education planning, and one or more institution enrollments. [MEDIUM]

Q: Can one learner belong to more than one institution?
A: Yes. A learner can have multiple enrollments, but only one enrollment per institution. Re-enrolling the same learner in the same institution updates that institution's enrollment instead of creating a duplicate. [HIGH]

Q: What happens when a student is created?
A: The student is activated, given personal details, email and mobile contacts when supplied, assigned a student number if one is not supplied, assigned an admission number if required by institution policy, and enrolled into an institution/course/term/section/branch. [MEDIUM]

Q: What must be present for enrollment placement?
A: Institution and course placement require an institution, course, and term. Section and branch may be blank. A missing course or term can prevent a valid course placement. [HIGH]

Q: What is the difference between disabling a student and issuing a transfer certificate?
A: Disabling changes the current institution enrollment to disabled. A transfer certificate changes the current institution enrollment to TC and records transfer-certificate details. The overall student record remains available. [MEDIUM]

Q: Does disabling the enrollment disable the whole student?
A: The active workflow disables the enrollment for the current institution, not the top-level learner record. The student record itself has an active/disabled status, but current enable/disable operations act on the institution enrollment. [MEDIUM]

Q: What happens when a disabled enrollment is enabled?
A: The current institution enrollment is moved back to active and downstream update notifications are sent. [MEDIUM]

### Enrollment Rules

Q: What states can an enrollment be in?
A: Active, promoted, TC, disabled, or unknown. Active enrollments are counted in most current-student reports. [MEDIUM]

Q: What information belongs to an enrollment?
A: Institution, course, term, section, branch, roll number, institution student number, date of admission, application number, admission number, optional subjects, transfer-certificate details, and attributes. [MEDIUM]

Q: How is duplicate enrollment prevented?
A: When the same learner is enrolled in an institution they already have, the existing enrollment for that institution is reused and updated. [HIGH]

Q: What happens if a target institution enrollment already exists during promotion?
A: The target institution enrollment is removed before the promoted enrollment is added. This prevents duplicate institution enrollments, but it can overwrite existing target-year placement. [MEDIUM]

Q: What happens if a student number is actually a system-generated GUID?
A: It is treated as not being a real institution student number and is displayed as blank for that enrollment. [HIGH]

Q: What is the format for automatically generated student numbers?
A: The number is made from two-digit year, institution-specific course prefix, and a three-digit sequence. The year comes from date of admission, or the academic year start when the date of admission is blank. [MEDIUM]

Q: What if a student number is supplied manually?
A: The supplied value is used instead of auto-generation. [MEDIUM]

### Promotion

Q: What does promotion do?
A: Promotion copies the learner's base enrollment into a target institution and target course, updates target course name, term, and section, and saves the learner with the new target placement. [HIGH]

Q: What must be valid before promotion?
A: The base course must belong to the base institution, the target course must belong to the target institution, and the student must exist. [MEDIUM]

Q: What carries forward during promotion?
A: The promotion copies much of the base enrollment. Term and section carry forward unless target values are supplied. The target course name comes from the target course. [MEDIUM]

Q: Is the base enrollment marked as promoted?
A: [GAP - observed behavior adds the target enrollment but does not visibly mark the base enrollment as promoted in the path reviewed. Tests do not assert this.]

Q: How are school-section colors handled in bulk promotion scripts?
A: A bulk promotion script maps Yellow to A, Blue to B, Red to C, and Green to D; other sections carry forward unchanged. [MEDIUM]

Q: How are course names advanced in bulk promotion scripts?
A: Playhome advances to KG I, KG I to KG II, KG II to I, and then each grade advances through VII. [MEDIUM]

Q: What is special about SPPU/PUC-style promotion?
A: Operational scripts use a course-map file to map base courses and institutions to target courses and institutions, carrying term and section forward. The core promotion behavior is otherwise the same. [MEDIUM]

### Transfer Certificate

Q: What happens when a transfer certificate is issued?
A: The current institution enrollment stores TC number, applied date, issued date, character, conduct, remarks, and additional attributes, then moves the enrollment into the TC state. [MEDIUM]

Q: Can a TC enrollment be reversed?
A: There is no explicit reversal operation in the reviewed behavior. Re-enabling an enrollment may move a current institution enrollment to active, but no test confirms TC reversal as a business-approved workflow. [UNRESOLVED]

Q: Do TC students appear in ordinary active-student lists?
A: Active-student lists generally filter for active enrollment state, so TC enrollments are excluded from normal current-student views. Separate TC-student reports exist. [MEDIUM]

### Section And Course Changes

Q: How is a student's section changed?
A: The section value on the current institution enrollment is replaced. Bulk scripts also update section by matching a list of student names within a course. [MEDIUM]

Q: How is a student's course changed?
A: The current institution enrollment can be updated with a new course name, course, section, and branch. [MEDIUM]

Q: Are term changes included in course updates?
A: The general course-update operation reviewed changes course, section, and branch but not term. Enrollment creation and promotion include term. [MEDIUM]

Q: How is roll number stored?
A: Roll number belongs to the institution enrollment and can be updated independently. Reports sort or display it as a number where possible. [MEDIUM]

### Fees Receivable

Q: What is a fee receivable?
A: A fee receivable is a pending fee item linked to a student for an institution, with fee id, fee name, transaction id, amount, and transaction status. [MEDIUM]

Q: What happens when a new fee receivable is attached to a student?
A: If the same transaction id already exists for that student, nothing new is added. Otherwise a new active fee receivable is added. [MEDIUM]

Q: What happens when a transaction is cancelled or replaced?
A: The existing fee receivable with that transaction id is marked disabled, and a new active fee receivable is added with the same fee details but no transaction id yet. [MEDIUM]

Q: What is the spanning-fee variant?
A: A spanning fee can be applied to a list of students before individual transactions exist. If a student already has that fee for the institution, amount and status are updated; otherwise a new active fee receivable is added with no transaction id. [MEDIUM]

Q: Can pending fee status be changed before transaction creation?
A: Yes. For a list of students, an existing institution/fee match can be marked active or disabled. [MEDIUM]

Q: How are students with outstanding fees queried?
A: Reports and queries look for student records whose fee-receivable list contains a matching fee and, in some operational scripts, a non-empty transaction id. [MEDIUM]

Q: What about opening balance?
A: Opening-balance behavior is unclear. The service prepares an opening-balance record when none exists, but the reviewed path does not add it to the student's fee list. [UNRESOLVED - business flow exists but appears incomplete; no assertion test confirms intended behavior.]

### Contact Management

Q: Whose contacts are tracked?
A: The student, father, mother, and guardian can each have email and mobile contacts. Home phone and home address are tracked separately. [MEDIUM]

Q: Which contacts are used for parent communication?
A: Parent communication pulls father, mother, and guardian contacts. Student communication can optionally include the student's own email/mobile and institute email. [MEDIUM]

Q: Can a contact be disabled without deleting it?
A: Yes. Father, mother, and guardian email/mobile contacts can be disabled or re-enabled. [MEDIUM]

Q: What details are stored for parents?
A: Father and mother store name, email, mobile, and PAN. Guardian stores name, gender, email, and mobile. [MEDIUM]

Q: How are recipient lists selected?
A: Recipients can be built from facets such as parent/student, course, section, tags, and selected students. Absence-based lists use attendance records for a date and period/session. [MEDIUM]

Q: Is there a bug in parental contact validity?
A: [UNRESOLVED - the reviewed behavior appears to overwrite father validity with mother validity when calculating whether parental contacts are valid. Tests do not cover this.]

### Individual Education Plan

Q: What does an Individual Education Plan track?
A: It tracks one or more terms for a student, each tied to an institution and containing title, start/end dates, working days, days present, domains, educators, goals, performance, evaluations, and remarks. [MEDIUM]

Q: What happens when an IEP is saved?
A: The supplied terms replace the student's existing plan terms. Each term is stamped with the current institution, and empty goals are removed while remaining goals are renumbered. [MEDIUM]

Q: Can an IEP report be generated?
A: Yes. There is an IEP report and a student-level IEP retrieval operation. [MEDIUM]

Q: Are IEP terms additive?
A: No. The reviewed save operation replaces the term list rather than appending to it. [MEDIUM]

### Student Essentials

Q: What are student essentials?
A: Student essentials are student-linked item records with item name, date, amount, institution, and active/disabled status. They appear suited to tracking issued materials, required items, or charged essentials. [LOW]

Q: How are essentials added?
A: The first item creates an essentials record for the student. Later items are appended to that student's essentials record and receive the next item number. [MEDIUM]

Q: Can an essential item be edited?
A: Yes. Item name, date, and amount can be updated. [MEDIUM]

Q: Can an essential item be removed?
A: It is disabled, not deleted. It can be re-enabled. [MEDIUM]

### Documents And Photo

Q: What student documents are stored?
A: Diagnosis report, disability report, birth certificate, identification proof, address proof, therapy report, psychological assessment report, and photo path are stored as document references. [MEDIUM]

Q: Are documents stored as files inside the student record?
A: No. The student record stores paths/references to documents. [MEDIUM]

Q: Is the disability report stored separately from the diagnosis report?
A: [UNRESOLVED - the reviewed disability-report operation writes to the diagnosis-report field, which appears inconsistent with the document model.]

### Baseline Assessment

Q: What is baseline assessment for?
A: It records a child's observed baseline across physical/occupational therapy, speech/language, and special education domains. [MEDIUM]

Q: What information is captured?
A: Name, birth date, age at observation, gender, therapist, date of observation, language, consultation flags, and domain-specific picklist responses or text observations. [MEDIUM]

Q: What PT/OT areas are tracked?
A: Condition, speech, response to name call, focus/attention, eye contact, sitting tolerance, instruction following, body build, posture, muscle tone, muscle tightness, grasp, precision, proprioception, static balance, dynamic balance, coordination, gait, sensory profile, and behavior. [MEDIUM]

Q: What speech/language areas are tracked?
A: Activity/topic focus, speech, condition, eye contact, sitting tolerance, activity compliance, receptive language, expressive language, social communication, pragmatics, intelligibility, behavior, literacy, and recommendation. [MEDIUM]

Q: What special-education areas are tracked?
A: Condition, speech, focus/attention, eye contact, sitting tolerance, instructions comprehension, activity compliance, behavior, barriers to learning, writing, literacy, number concept, conversation skill, cognition, environmental studies, group-session behavior, basic life skills, and additional comments. [MEDIUM]

Q: Are baseline picklists institution-specific?
A: Yes. Some institutions have detailed baseline lists; many use empty default lists. [MEDIUM]

### ETL And Bulk Operations

Q: What bulk operations are supported?
A: Bulk scripts can create promoted students, create new students, generate promotion requests from course maps, import new students from a student-data template, update student mobile numbers, update virtual ids, set up demo students, produce opening-balance import scripts, and generate fee-related student exports. [MEDIUM]

Q: What data is imported for new students?
A: New-student imports include student identity, date of birth, gender, course, term, section, institution, admission/application/student numbers, and parent contact details when supplied. [MEDIUM]

Q: How are promotion imports driven?
A: A course-map file maps base courses/institutions to target courses/institutions. The script then creates promotion requests for matching current students. [MEDIUM]

Q: Are bulk scripts automatically applied?
A: They are generated as API-script lines, often stored locally or uploaded for an ETL worker. Tests show script generation more than direct execution. [MEDIUM]

Q: What is a known risk in bulk operations?
A: Several tests are operational scripts against live data with hard-coded institution/course ids, temporary files, and minimal assertions. They document real workflows but are not robust verification. [HIGH]

## Gaps And Unresolved Items

- [GAP - no strong assertion tests for create, promotion final state, TC issuance, IEP save, essentials, documents, baseline assessment, and most reports.]
- [GAP - many tests are data-repair or export scripts rather than automated business assertions.]
- [UNRESOLVED - base enrollment does not appear to be marked promoted during promotion, despite a promoted enrollment state existing.]
- [UNRESOLVED - opening-balance creation appears incomplete in the reviewed behavior.]
- [UNRESOLVED - disability-report upload appears to overwrite the diagnosis-report field.]
- [UNRESOLVED - TC reversal is not defined as a clear business workflow.]
- [UNRESOLVED - parental contact validity appears to ignore father validity when mother details are present.]
