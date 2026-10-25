using FanHubPlus.Application.DTOs;
using FanHubPlus.Application.Interfaces;
using FanHubPlus.Core.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FanHubPlus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionsController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [HttpGet("plans")]
    public ActionResult<List<SubscriptionPlanDto>> GetPlans()
    {
        var plans = _subscriptionService.GetSubscriptionPlansAsync().Result;
        return Ok(plans.Select(p => new SubscriptionPlanDto
        {
            SubscriptionPlanId = p.SubscriptionPlanId,
            Name = p.Name,
            Description = p.Description,
            MonthlyPrice = p.MonthlyPrice,
            YearlyPrice = p.YearlyPrice,
            LifetimePrice = p.LifetimePrice,
            MaxStreamingQuality = p.MaxStreamingQuality,
            ConcurrentStreams = p.ConcurrentStreams
        }));
    }

    [HttpPost("subscribe")]
    public async Task<ActionResult> Subscribe(string userId, int planId)
    {
        await _subscriptionService.SubscribeAsync(userId, planId);
        return Ok(new { UserId = userId, PlanId = planId });
    }

    [HttpPost("cancel")]
    public async Task<ActionResult> Cancel(string userId)
    {
        await _subscriptionService.CancelSubscriptionAsync(userId);
        return Ok(new { UserId = userId });
    }
}