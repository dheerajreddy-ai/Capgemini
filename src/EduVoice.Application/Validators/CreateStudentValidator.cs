using EduVoice.Application.DTOs.Students;
using FluentValidation;

namespace EduVoice.Application.Validators;

public class CreateStudentValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Student ID is required")
            .MaximumLength(50).WithMessage("Student ID must not exceed 50 characters");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.ParentName)
            .NotEmpty().WithMessage("Parent name is required")
            .MaximumLength(200).WithMessage("Parent name must not exceed 200 characters");

        RuleFor(x => x.ParentPhone)
            .NotEmpty().WithMessage("Parent phone is required")
            .Matches(@"^\+?[\d\s\-\(\)]{7,15}$").WithMessage("Invalid phone number");

        RuleFor(x => x.TotalFees)
            .GreaterThanOrEqualTo(0).WithMessage("Total fees must be non-negative");

        RuleFor(x => x.PaidFees)
            .GreaterThanOrEqualTo(0).WithMessage("Paid fees must be non-negative")
            .LessThanOrEqualTo(x => x.TotalFees).WithMessage("Paid fees cannot exceed total fees");
    }
}
