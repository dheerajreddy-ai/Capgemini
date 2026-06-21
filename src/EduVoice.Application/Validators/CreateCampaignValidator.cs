using EduVoice.Application.DTOs.Campaigns;
using FluentValidation;

namespace EduVoice.Application.Validators;

public class CreateCampaignValidator : AbstractValidator<CreateCampaignRequest>
{
    public CreateCampaignValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Campaign name is required")
            .MaximumLength(200).WithMessage("Campaign name must not exceed 200 characters");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid campaign type");

        RuleFor(x => x.ScheduledAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Scheduled time must be in the future")
            .When(x => x.ScheduledAt.HasValue);
    }
}
