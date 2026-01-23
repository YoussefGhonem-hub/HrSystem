using System;
using System.Globalization;
using System.Linq;
using HrSystem.Domain.Enums;

namespace HrSystem.Application.Features.Employees.Queries.GetMyDocuments;

public static class DocumentCategoryHelper
{
    private static readonly string[] IdentityDocumentKeywords =
    [
        "passport",
        "id",
        "identity",
        "iqama",
        "residence",
        "national"
    ];

    private static readonly string[] EmploymentDocumentKeywords =
    [
        "contract",
        "offer",
        "employment",
        "appointment",
        "job"
    ];

    private static readonly string[] InsuranceDocumentKeywords =
    [
        "insurance",
        "medical",
        "health",
        "policy"
    ];

    public static DocumentCategory ResolveCategory(string? typeName, string? documentName)
    {
        var name = string.Join(" ", typeName, documentName).ToLower(CultureInfo.InvariantCulture);

        if (ContainsAny(name, IdentityDocumentKeywords))
        {
            return DocumentCategory.Personal;
        }

        if (ContainsAny(name, EmploymentDocumentKeywords))
        {
            return DocumentCategory.Employment;
        }

        if (ContainsAny(name, InsuranceDocumentKeywords))
        {
            return DocumentCategory.Insurance;
        }

        return DocumentCategory.Other;
    }

    public static string GetCategoryNameEn(DocumentCategory categoryKey)
    {
        return categoryKey switch
        {
            DocumentCategory.Personal => "Personal",
            DocumentCategory.Employment => "Employment",
            DocumentCategory.Insurance => "Insurance",
            _ => "Other"
        };
    }

    public static string GetCategoryNameAr(DocumentCategory categoryKey)
    {
        return categoryKey switch
        {
            DocumentCategory.Personal => "شخصي",
            DocumentCategory.Employment => "العمل",
            DocumentCategory.Insurance => "التأمين",
            _ => "أخرى"
        };
    }

    private static bool ContainsAny(string? value, string[] keywords)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return keywords.Any(keyword => value.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
}
