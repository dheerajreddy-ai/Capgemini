using EduVoice.Domain.Enums;

namespace EduVoice.Application.Common;

public static class CarrierHealthHelper
{
    public static CarrierHealth Classify(int totalCalls, int completedCalls)
    {
        if (totalCalls == 0) return CarrierHealth.Healthy;
        var rate = (double)completedCalls / totalCalls;
        return rate switch
        {
            >= 0.60 => CarrierHealth.Healthy,
            >= 0.40 => CarrierHealth.Degraded,
            _ => CarrierHealth.Flagged
        };
    }

    public static string AlertMessage(string schoolName, string phone, CarrierHealth health, double rate)
        => $"EduVoice Carrier Alert — {schoolName}\n" +
           $"Number: {phone}\n" +
           $"Status: {health} (delivery rate {rate:P0} over last 7 days)\n" +
           $"Action: {health switch { CarrierHealth.Flagged => "Stop campaigns, rotate number.", CarrierHealth.Degraded => "Reduce wave size.", _ => "No action needed." }}";
}
