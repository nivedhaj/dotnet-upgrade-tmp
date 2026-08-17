# PostgreSQL views and triggers

RavenDB indexes/ETL have been replaced with PostgreSQL views and triggers.

- `campus.v_student_inst_index` replaces `Student_Inst_Index`.
- `campus.v_student_coursewise_count` replaces `Student_Coursewise_Count`.
- `campus.v_inst_student_count` replaces `Inst_Student_Count`.
- `campus.v_attendance_event_index` replaces `AttendanceEvent_IX`.
- `campus.v_student_essentials_index` replaces `Student_Essentials_IX`.
- `campus.v_ping_index` replaces `Ping_Index`.
- `campus.v_student_fee_summary` is the fee read model.
- `trg_audit_documents` stamps created/modified audit fields.
- `trg_refresh_student_fee_summary` refreshes the fee summary cache after fee writes.

The SQL definitions live in `Database/schema.sql`.
