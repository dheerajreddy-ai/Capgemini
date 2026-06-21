using EduVoice.Application.DTOs.Auth;
using FluentValidation;

namespace EduVoice.Application.Validators;

public class RegisterSchoolValidator : AbstractValidator<RegisterSchoolRequest>
{
    public RegisterSchoolValidator()
    {
        RuleFor(x => x.SchoolName)
            .NotEmpty().WithMessage("School name is required")
            .MaximumLength(200).WithMessage("School name must not exceed 200 characters");

        RuleFor(x => x.SubDomain)
            .NotEmpty().WithMessage("SubDomain is required")
            .Matches("^[a-z0-9-]+$").WithMessage("SubDomain can only contain lowercase letters, numbers, and hyphens")
            .MinimumLength(3).WithMessage("SubDomain must be at least 3 characters")
            .MaximumLength(50).WithMessage("SubDomain must not exceed 50 characters");

        RuleFor(x => x.ContactEmail)
            .NotEmpty().WithMessage("Contact email is required")
            .EmailAddress().WithMessage("Invalid contact email format");

        RuleFor(x => x.ContactPhone)
            .NotEmpty().WithMessage("Contact phone is required")
            .Matches(@"^\+?[\d\s\-\(\)]{7,15}$").WithMessage("Invalid phone number");

        RuleFor(x => x.AdminFirstName)
            .NotEmpty().WithMessage("Admin first name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.AdminLastName)
            .NotEmpty().WithMessage("Admin last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.AdminEmail)
            .NotEmpty().WithMessage("Admin email is required")
            .EmailAddress().WithMessage("Invalid admin email format");

        RuleFor(x => x.AdminPassword)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");
    }
}
