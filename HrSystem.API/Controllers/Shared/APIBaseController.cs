using ErrorOr;
using HrSystem.API.Common.Localization;
using HrSystem.API.Common.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace HrSystem.API.Controllers.Shared
{
    [ApiController]
    public class APIBaseController : ControllerBase
    {
        private IApiMessageLocalizer MessageLocalizer => HttpContext.RequestServices.GetRequiredService<IApiMessageLocalizer>();

        protected IActionResult Problem(List<Error> errors)
        {
            if (errors.Count is 0)
            {
                return Problem();
            }
            if (errors.All(error => error.Type == ErrorType.Validation))
            {
                return ValidationProblem(errors);
            }
            HttpContext.Items[HttpContextitemKeys.Errors] = errors;

            return Problem(errors[0]);
        }

        private IActionResult ValidationProblem(List<Error> errors)
        {
            var modelStateDictionary = new ModelStateDictionary();
            foreach (var error in errors)
            {
                modelStateDictionary.AddModelError(error.Code, MessageLocalizer.Localize(error.Description) ?? error.Description);
            }
            return ValidationProblem(modelStateDictionary);
        }

        private IActionResult Problem(Error error)
        {
            var statusCode = error.Type switch
            {
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };
            return Problem(statusCode: statusCode, title: MessageLocalizer.Localize(error.Description) ?? error.Description);
        }
    }
}
