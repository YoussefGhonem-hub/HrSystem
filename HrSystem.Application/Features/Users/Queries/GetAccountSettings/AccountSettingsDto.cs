using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HrSystem.Application.Features.Users.Queries.GetAccountSettings
{
    public record AccountSettingsDto
    {
        /// <summary>
        /// User ID
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Username for login
        /// </summary>
        public string? UserName { get; init; }

        /// <summary>
        /// Account status (Active/Inactive)
        /// </summary>
        public string AccountStatus { get; init; } = string.Empty;

        /// <summary>
        /// Is account active
        /// </summary>
        public bool IsActive { get; init; }

        /// <summary>
        /// Work email used for login
        /// </summary>
        public string? WorkEmail { get; init; }

        /// <summary>
        /// Last login date and time
        /// </summary>
        public DateTimeOffset? LastLogin { get; init; }

        /// <summary>
        /// Full name of the user
        /// </summary>
        public string? FullName { get; init; }

        /// <summary>
        /// Employee ID if linked
        /// </summary>
        public Guid? EmployeeId { get; init; }

        /// <summary>
        /// Employee name if linked
        /// </summary>
        public string? EmployeeName { get; init; }
    }

}
