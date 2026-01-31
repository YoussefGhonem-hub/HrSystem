using System.Collections.Generic;

namespace HrSystem.Domain.Enums;

public enum EmployeeDocumentType
{
    NationalId = 1,
    EmploymentContract = 2,
    CV = 3,
    Certificates = 4,
    MedicalReport = 5
}

public readonly record struct EmployeeDocumentTypeInfo(
    string NameEn,
    string NameAr,
    DocumentCategory Category);

public static class EmployeeDocumentTypeExtensions
{
    private static readonly IReadOnlyDictionary<EmployeeDocumentType, EmployeeDocumentTypeInfo> _metadata =
        new Dictionary<EmployeeDocumentType, EmployeeDocumentTypeInfo>
        {
            [EmployeeDocumentType.NationalId] = new("National ID", "الهوية الوطنية", DocumentCategory.Personal),
            [EmployeeDocumentType.EmploymentContract] = new("Employment Contract", "عقد العمل", DocumentCategory.Employment),
            [EmployeeDocumentType.CV] = new("Curriculum Vitae", "السيرة الذاتية", DocumentCategory.Employment),
            [EmployeeDocumentType.Certificates] = new("Certificates", "الشهادات", DocumentCategory.Personal),
            [EmployeeDocumentType.MedicalReport] = new("Medical Report", "تقرير طبي", DocumentCategory.Insurance)
        };

    public static EmployeeDocumentTypeInfo GetInfo(this EmployeeDocumentType type) => _metadata[type];

    public static string GetNameEn(this EmployeeDocumentType type) => type.GetInfo().NameEn;

    public static string GetNameAr(this EmployeeDocumentType type) => type.GetInfo().NameAr;

    public static DocumentCategory GetCategory(this EmployeeDocumentType type) => type.GetInfo().Category;
}
