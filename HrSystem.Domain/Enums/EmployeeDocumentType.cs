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

    private static readonly EmployeeDocumentTypeInfo _unknown = new("Unknown", "غير معروف", DocumentCategory.Other);

    public static EmployeeDocumentTypeInfo GetInfo(this EmployeeDocumentType type)
    {
        if (_metadata.TryGetValue(type, out var info))
        {
            return info;
        }

        return _unknown;
    }

    public static bool TryGetInfo(this EmployeeDocumentType type, out EmployeeDocumentTypeInfo info)
    {
        if (_metadata.TryGetValue(type, out info))
        {
            return true;
        }

        info = _unknown;
        return false;
    }

    public static string GetNameEn(this EmployeeDocumentType type) => type.GetInfo().NameEn;

    public static string GetNameAr(this EmployeeDocumentType type) => type.GetInfo().NameAr;

    public static DocumentCategory GetCategory(this EmployeeDocumentType type) => type.GetInfo().Category;
}
