using FanHubPlus.Areas.Admin.Controllers;
using FanHubPlus.Controllers;
using FanHubPlus.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace FanHubPlus.Tests.Security;

public class AuthorizationContractTests
{
    public static TheoryData<Type, string> AdministrativeControllers => new()
    {
        { typeof(ArticlesController), "Admin" },
        { typeof(FanHubPlus.Areas.Admin.Controllers.CharactersController), "Admin" },
        { typeof(CategoriesController), "Admin" },
        { typeof(ContentsController), "Admin" },
        { typeof(DashboardController), "Admin" },
        { typeof(FanHubPlus.Areas.Admin.Controllers.EventsController), "Admin" },
        { typeof(FeedbackController), "Admin" },
        { typeof(FanHubPlus.Areas.Admin.Controllers.MerchController), "Admin" },
        { typeof(SubmissionsController), "Admin" },
        { typeof(UsersController), "Admin" }
    };

    [Theory]
    [MemberData(nameof(AdministrativeControllers))]
    public void Every_admin_controller_requires_admin_role(Type controller, string role)
    {
        var authorize = controller.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .SingleOrDefault();

        Assert.NotNull(authorize);
        Assert.Equal(role, authorize!.Roles);
        var area = controller.GetCustomAttributes(inherit: true)
            .OfType<AreaAttribute>().Single();
        Assert.Equal("Admin", area.RouteValue?.ToString());
    }

    [Theory]
    [InlineData(typeof(AccountController), "Logout")]
    [InlineData(typeof(AccountController), "ChangePassword")]
    public void Authenticated_state_changing_actions_are_protected(Type controller, string action)
    {
        var method = controller.GetMethod(action)!;

        Assert.Contains(method.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true),
            attribute => attribute is not null);
        Assert.Contains(method.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), inherit: true),
            attribute => attribute is not null);
    }

    [Fact]
    public void Application_registers_global_antiforgery_validation()
    {
        // The global MVC filter is configured in Program. A controller-level test
        // makes regressions visible without booting the entire application.
        var unsafePost = typeof(AccountController).GetMethod(nameof(AccountController.Login),
            new[] { typeof(LoginViewModel) })!;

        Assert.Contains(unsafePost.GetCustomAttributes(
                typeof(ValidateAntiForgeryTokenAttribute), inherit: true),
            attribute => attribute is not null);
    }
}