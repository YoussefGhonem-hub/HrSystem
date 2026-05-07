using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lookups.Queries.GetRequestMasterLookups;

public record GetRequestTypesLookupQuery(bool IncludeInactive = false) : IRequest<ErrorOr<GenericResponse<List<RequestTypeDto>>>>;

public class GetRequestTypesLookupQueryHandler : IRequestHandler<GetRequestTypesLookupQuery, ErrorOr<GenericResponse<List<RequestTypeDto>>>>
{
	private readonly ApplicationDbContext _context;

	public GetRequestTypesLookupQueryHandler(ApplicationDbContext context)
	{
		_context = context;
	}

	public async Task<ErrorOr<GenericResponse<List<RequestTypeDto>>>> Handle(
		GetRequestTypesLookupQuery request,
		CancellationToken cancellationToken)
	{
		var typesQuery = _context.RequestTypes
			.AsNoTracking();

		if (!request.IncludeInactive)
		{
			typesQuery = typesQuery.Where(t => t.IsActive);
		}

		var types = await typesQuery
			.OrderBy(t => t.SortOrder)
			.ThenBy(t => t.NameEn)
			.Select(t => new RequestTypeDto
			{
				Id = t.Id,
				Code = t.Code,
				NameAr = t.NameAr,
				NameEn = t.NameEn,
				Description = t.Description,
				IsActive = t.IsActive,
				SortOrder = t.SortOrder,
				CreatedDate = t.CreatedDate,
				ModifiedDate = t.ModifiedDate
			})
			.ToListAsync(cancellationToken);

		return GenericResponse<List<RequestTypeDto>>.SuccessResult(types, "Request types retrieved successfully");
	}
}

public record GetVacationTypesLookupQuery(bool IncludeInactive = false) : IRequest<ErrorOr<GenericResponse<List<VacationTypeDto>>>>;

public class GetVacationTypesLookupQueryHandler : IRequestHandler<GetVacationTypesLookupQuery, ErrorOr<GenericResponse<List<VacationTypeDto>>>>
{
	private readonly ApplicationDbContext _context;

	public GetVacationTypesLookupQueryHandler(ApplicationDbContext context)
	{
		_context = context;
	}

	public async Task<ErrorOr<GenericResponse<List<VacationTypeDto>>>> Handle(
		GetVacationTypesLookupQuery request,
		CancellationToken cancellationToken)
	{
		var query = _context.VacationTypes.AsNoTracking();

		if (!request.IncludeInactive)
		{
			query = query.Where(v => v.IsActive);
		}

		var vacations = await query
			.OrderBy(v => v.SortOrder)
			.ThenBy(v => v.NameEn)
			.Select(v => new VacationTypeDto
			{
				Id = v.Id,
				NameAr = v.NameAr,
				NameEn = v.NameEn,
				Description = v.Description,
				IsPaid = v.IsPaid,
				RequiresManagerApproval = v.RequiresManagerApproval,
				SortOrder = v.SortOrder
			})
			.ToListAsync(cancellationToken);

		return GenericResponse<List<VacationTypeDto>>.SuccessResult(vacations, "Vacation types retrieved successfully");
	}
}

public record GetTrainingTypesLookupQuery(bool IncludeInactive = false) : IRequest<ErrorOr<GenericResponse<List<TrainingTypeDto>>>>;

public class GetTrainingTypesLookupQueryHandler : IRequestHandler<GetTrainingTypesLookupQuery, ErrorOr<GenericResponse<List<TrainingTypeDto>>>>
{
	private readonly ApplicationDbContext _context;

	public GetTrainingTypesLookupQueryHandler(ApplicationDbContext context)
	{
		_context = context;
	}

	public async Task<ErrorOr<GenericResponse<List<TrainingTypeDto>>>> Handle(
		GetTrainingTypesLookupQuery request,
		CancellationToken cancellationToken)
	{
		var query = _context.TrainingTypes.AsNoTracking();

		if (!request.IncludeInactive)
		{
			query = query.Where(t => t.IsActive);
		}

		var trainingTypes = await query
			.OrderBy(t => t.SortOrder)
			.ThenBy(t => t.NameEn)
			.Select(t => new TrainingTypeDto
			{
				Id = t.Id,
				NameAr = t.NameAr,
				NameEn = t.NameEn,
				Description = t.Description,
				RequiresManagerApproval = t.RequiresManagerApproval,
				SortOrder = t.SortOrder
			})
			.ToListAsync(cancellationToken);

		return GenericResponse<List<TrainingTypeDto>>.SuccessResult(trainingTypes, "Training types retrieved successfully");
	}
}

public record GetPersonalTypesLookupQuery(bool IncludeInactive = false) : IRequest<ErrorOr<GenericResponse<List<PersonalTypeDto>>>>;

public class GetPersonalTypesLookupQueryHandler : IRequestHandler<GetPersonalTypesLookupQuery, ErrorOr<GenericResponse<List<PersonalTypeDto>>>>
{
	private readonly ApplicationDbContext _context;

	public GetPersonalTypesLookupQueryHandler(ApplicationDbContext context)
	{
		_context = context;
	}

	public async Task<ErrorOr<GenericResponse<List<PersonalTypeDto>>>> Handle(
		GetPersonalTypesLookupQuery request,
		CancellationToken cancellationToken)
	{
		var query = _context.PersonalTypes.AsNoTracking();

		if (!request.IncludeInactive)
		{
			query = query.Where(p => p.IsActive);
		}

		var personalTypes = await query
			.OrderBy(p => p.SortOrder)
			.ThenBy(p => p.NameEn)
			.Select(p => new PersonalTypeDto
			{
				Id = p.Id,
				NameAr = p.NameAr,
				NameEn = p.NameEn,
				Description = p.Description,
				RequiresManagerApproval = p.RequiresManagerApproval,
				SortOrder = p.SortOrder
			})
			.ToListAsync(cancellationToken);

		return GenericResponse<List<PersonalTypeDto>>.SuccessResult(personalTypes, "Personal types retrieved successfully");
	}
}

public record GetPermissionTypesLookupQuery(bool IncludeInactive = false) : IRequest<ErrorOr<GenericResponse<List<PermissionTypeDto>>>>;

public class GetPermissionTypesLookupQueryHandler : IRequestHandler<GetPermissionTypesLookupQuery, ErrorOr<GenericResponse<List<PermissionTypeDto>>>>
{
	private readonly ApplicationDbContext _context;

	public GetPermissionTypesLookupQueryHandler(ApplicationDbContext context)
	{
		_context = context;
	}

	public async Task<ErrorOr<GenericResponse<List<PermissionTypeDto>>>> Handle(
		GetPermissionTypesLookupQuery request,
		CancellationToken cancellationToken)
	{
		var query = _context.PermissionTypes.AsNoTracking();

		if (!request.IncludeInactive)
		{
			query = query.Where(p => p.IsActive);
		}

		var permissionTypes = await query
			.OrderBy(p => p.SortOrder)
			.ThenBy(p => p.NameEn)
			.Select(p => new PermissionTypeDto
			{
				Id = p.Id,
				NameAr = p.NameAr,
				NameEn = p.NameEn,
				Description = p.Description,
				RequiresManagerApproval = p.RequiresManagerApproval,
				SortOrder = p.SortOrder
			})
			.ToListAsync(cancellationToken);

		return GenericResponse<List<PermissionTypeDto>>.SuccessResult(permissionTypes, "Permission types retrieved successfully");
	}
}

public record GetOvertimeTypesLookupQuery(bool IncludeInactive = false) : IRequest<ErrorOr<GenericResponse<List<OvertimeTypeDto>>>>;

public class GetOvertimeTypesLookupQueryHandler : IRequestHandler<GetOvertimeTypesLookupQuery, ErrorOr<GenericResponse<List<OvertimeTypeDto>>>>
{
	private readonly ApplicationDbContext _context;

	public GetOvertimeTypesLookupQueryHandler(ApplicationDbContext context)
	{
		_context = context;
	}

	public async Task<ErrorOr<GenericResponse<List<OvertimeTypeDto>>>> Handle(
		GetOvertimeTypesLookupQuery request,
		CancellationToken cancellationToken)
	{
		var query = _context.OvertimeTypes.AsNoTracking();

		if (!request.IncludeInactive)
		{
			query = query.Where(o => o.IsActive);
		}

		var overtimeTypes = await query
			.OrderBy(o => o.SortOrder)
			.ThenBy(o => o.NameEn)
			.Select(o => new OvertimeTypeDto
			{
				Id = o.Id,
				NameAr = o.NameAr,
				NameEn = o.NameEn,
				Description = o.Description,
				DefaultMultiplier = o.DefaultMultiplier,
				RequiresManagerApproval = o.RequiresManagerApproval,
				SortOrder = o.SortOrder
			})
			.ToListAsync(cancellationToken);

		return GenericResponse<List<OvertimeTypeDto>>.SuccessResult(overtimeTypes, "Overtime types retrieved successfully");
	}
}

public record GetMiscellaneousTypesLookupQuery(bool IncludeInactive = false) : IRequest<ErrorOr<GenericResponse<List<MiscellaneousTypeDto>>>>;

public class GetMiscellaneousTypesLookupQueryHandler : IRequestHandler<GetMiscellaneousTypesLookupQuery, ErrorOr<GenericResponse<List<MiscellaneousTypeDto>>>>
{
	private readonly ApplicationDbContext _context;

	public GetMiscellaneousTypesLookupQueryHandler(ApplicationDbContext context)
	{
		_context = context;
	}

	public async Task<ErrorOr<GenericResponse<List<MiscellaneousTypeDto>>>> Handle(
		GetMiscellaneousTypesLookupQuery request,
		CancellationToken cancellationToken)
	{
		var query = _context.MiscellaneousTypes.AsNoTracking();

		if (!request.IncludeInactive)
		{
			query = query.Where(m => m.IsActive);
		}

		var miscTypes = await query
			.OrderBy(m => m.SortOrder)
			.ThenBy(m => m.NameEn)
			.Select(m => new MiscellaneousTypeDto
			{
				Id = m.Id,
				NameAr = m.NameAr,
				NameEn = m.NameEn,
				Description = m.Description,
				RequiresManagerApproval = m.RequiresManagerApproval,
				SortOrder = m.SortOrder
			})
			.ToListAsync(cancellationToken);

		return GenericResponse<List<MiscellaneousTypeDto>>.SuccessResult(miscTypes, "Miscellaneous types retrieved successfully");
	}
}

public record GetFeedbackTypesLookupQuery(bool IncludeInactive = false) : IRequest<ErrorOr<GenericResponse<List<FeedbackTypeDto>>>>;

public class GetFeedbackTypesLookupQueryHandler : IRequestHandler<GetFeedbackTypesLookupQuery, ErrorOr<GenericResponse<List<FeedbackTypeDto>>>>
{
	private readonly ApplicationDbContext _context;

	public GetFeedbackTypesLookupQueryHandler(ApplicationDbContext context)
	{
		_context = context;
	}

	public async Task<ErrorOr<GenericResponse<List<FeedbackTypeDto>>>> Handle(
		GetFeedbackTypesLookupQuery request,
		CancellationToken cancellationToken)
	{
		var query = _context.FeedbackTypes.AsNoTracking();

		if (!request.IncludeInactive)
		{
			query = query.Where(f => f.IsActive);
		}

		var feedbackTypes = await query
			.OrderBy(f => f.SortOrder)
			.ThenBy(f => f.NameEn)
			.Select(f => new FeedbackTypeDto
			{
				Id = f.Id,
				NameAr = f.NameAr,
				NameEn = f.NameEn,
				Description = f.Description,
				IsAnonymousAllowed = f.IsAnonymousAllowed,
				RequiresManagerApproval = f.RequiresManagerApproval,
				SortOrder = f.SortOrder
			})
			.ToListAsync(cancellationToken);

		return GenericResponse<List<FeedbackTypeDto>>.SuccessResult(feedbackTypes, "Feedback types retrieved successfully");
	}
}

public record GetAttendanceCorrectionTypesLookupQuery(bool IncludeInactive = false) : IRequest<ErrorOr<GenericResponse<List<AttendanceCorrectionTypeDto>>>>;

public class GetAttendanceCorrectionTypesLookupQueryHandler : IRequestHandler<GetAttendanceCorrectionTypesLookupQuery, ErrorOr<GenericResponse<List<AttendanceCorrectionTypeDto>>>>
{
	private readonly ApplicationDbContext _context;

	public GetAttendanceCorrectionTypesLookupQueryHandler(ApplicationDbContext context)
	{
		_context = context;
	}

	public async Task<ErrorOr<GenericResponse<List<AttendanceCorrectionTypeDto>>>> Handle(
		GetAttendanceCorrectionTypesLookupQuery request,
		CancellationToken cancellationToken)
	{
		var query = _context.AttendanceCorrectionTypes.AsNoTracking();

		if (!request.IncludeInactive)
		{
			query = query.Where(a => a.IsActive);
		}

		var types = await query
			.OrderBy(a => a.SortOrder)
			.ThenBy(a => a.NameEn)
			.Select(a => new AttendanceCorrectionTypeDto
			{
				Id = a.Id,
				NameAr = a.NameAr,
				NameEn = a.NameEn,
				Description = a.Description,
				RequiresManagerApproval = a.RequiresManagerApproval,
				SortOrder = a.SortOrder
			})
			.ToListAsync(cancellationToken);

		return GenericResponse<List<AttendanceCorrectionTypeDto>>.SuccessResult(types, "Attendance correction types retrieved successfully");
	}
}
