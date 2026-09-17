namespace Ct.Modules.Admissions.Domain;

// All numeric values below are preserved verbatim from the legacy RavenDB-era
// enums (spec section 8). Do not renumber — persisted data and any surviving
// legacy clients depend on these exact values.

public enum Gender
{
    Female = 0,
    Male = 1,
    NoInfo = 90
}

public enum StudentStatus
{
    Unknown = -1,
    Active = 1,
    Disabled = 99
}

/// <summary>
/// Legacy enrollment status values were not fully recoverable from the
/// standalone specification (spec section 8: "If exact enum values are
/// unknown, use a nullable/integer-compatible representation and document it
/// rather than inventing semantic values"). We therefore keep a minimal,
/// clearly-documented set and store the column as a plain smallint so unknown
/// legacy values round-trip without data loss. See docs/migration-matrix.md.
/// </summary>
public enum EnrollmentStatus
{
    Unknown = -1,
    Active = 1,
    Disabled = 99
}

public enum CourseStatus
{
    Unknown = 0,
    Active = 1,
    Disabled = 99
}

public enum ContactStatus
{
    Unknown = -1,
    Active = 1,
    Archived = 99,
    Disabled = 90
}

public enum ContactType
{
    Unknown = -1,
    Email = 10,
    Mobile = 20,
    Phone = 30,
    Website = 40,
    Social = 50
}

public enum LocationType
{
    Unknown = -1,
    Personal = 10,
    Home = 20,
    Office = 30,
    Other = 40,
    Institute = 50
}

public enum AddressStatus
{
    Unknown = -1,
    Active = 1,
    Archived = 99
}

public enum StudentEssentialItemStatus
{
    Unknown = 0,
    Active = 1,
    Disable = 99
}

/// <summary>Spec section 10 — the only relation types the legacy model allowed on a student.</summary>
public enum RelationType
{
    Father = 1,
    Mother = 2,
    Guardian = 3
}

/// <summary>Spec section 26 — document slots recognised on a student record.</summary>
public enum StudentDocumentType
{
    DiagnosisReport = 1,
    DisabilityCertificate = 2,
    BirthCertificate = 3,
    IdentificationProof = 4,
    AddressProof = 5,
    TherapyReport = 6,
    /// <summary>Intentionally misspelled to match the legacy API contract (spec section 26 / 55).</summary>
    PsycologicalAssessmentReport = 7,
    Photo = 8
}
