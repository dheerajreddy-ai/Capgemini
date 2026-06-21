using EduVoice.Domain.Entities;

namespace EduVoice.Application.Interfaces;

public interface ILowMarksAlertService
{
    Task CheckAndAlertAsync(Student student, School school);
}
