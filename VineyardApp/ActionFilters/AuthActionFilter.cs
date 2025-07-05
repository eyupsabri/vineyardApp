using Business.Services;
using DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VineyardApp.ActionFilters
{
    public class AuthActionFilter : IAsyncActionFilter
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;


        public AuthActionFilter(IUnitOfWork unitOfWork, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {

            var authorizationHeader = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (authorizationHeader != null && authorizationHeader.StartsWith("Bearer "))
            {
                var token = authorizationHeader.Substring("Bearer ".Length).Trim();


                var response = _tokenService.ValidateToken(token);
                if (!response.IsValidToken)
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }
                if (response.IsExpired)
                {
                    if (response.TokenType == "access")
                    {
                        var user = await _unitOfWork.UserRepo.GetUserByEmailAsync(response.Email);
                        if (user == null)
                        {
                            context.Result = new UnauthorizedResult();
                            return;
                        }
                        if (user.CurrentJwtId != token)
                        {
                            context.Result = new UnauthorizedResult();
                            return;
                        }
                        //unauthorized ve refresh token iste
                        context.Result = new ContentResult
                        {
                            StatusCode = 401,
                            Content = "Refresh Token",
                            ContentType = "text/plain",
                        };
                        return;
                    }

                }
                else
                {
                    if (response.TokenType == "access")
                    {
                        var user = await _unitOfWork.UserRepo.GetUserByEmailAsync(response.Email);
                        if (user == null || user.CurrentJwtId != token)
                        {
                            context.Result = new UnauthorizedResult();
                            return;
                        }
                        context.HttpContext.Items["Email"] = response.Email;
                        await next();
                        return;
                    }

                }
            }

            context.Result = new UnauthorizedResult();
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
