using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HrSystem.API.Common.Localization;

public sealed class LocalizeApiResponseFilter : IAsyncResultFilter
{
    private readonly IApiMessageLocalizer _messageLocalizer;

    public LocalizeApiResponseFilter(IApiMessageLocalizer messageLocalizer)
    {
        _messageLocalizer = messageLocalizer;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult && objectResult.Value is not null)
        {
            _messageLocalizer.LocalizeObject(objectResult.Value);
        }

        await next();
    }
}