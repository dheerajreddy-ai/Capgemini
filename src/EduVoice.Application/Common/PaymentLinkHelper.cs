namespace EduVoice.Application.Common;

public static class PaymentLinkHelper
{
    public static string GenerateUpiLink(string upiId, string schoolName, decimal amount, string studentName)
    {
        var encoded = Uri.EscapeDataString($"Fee payment for {studentName} - {schoolName}");
        return $"upi://pay?pa={upiId}&pn={Uri.EscapeDataString(schoolName)}&am={amount:F2}&tn={encoded}&cu=INR";
    }

    public static string FeeReminderWhatsApp(string parentName, string studentName, string schoolName,
        decimal pending, DateTime? dueDate, string paymentLink)
    {
        var due = dueDate.HasValue ? dueDate.Value.ToString("dd MMM yyyy") : "వీలైనంత త్వరగా";
        return $"""
            నమస్కారం {parentName} గారు! 🙏

            *{schoolName}* నుండి ఫీజు రిమైండర్:

            👤 విద్యార్థి: {studentName}
            💰 పెండింగ్ ఫీజు: ₹{pending:N0}
            📅 చెల్లించాల్సిన తేదీ: {due}

            💳 UPI ద్వారా చెల్లించండి:
            {paymentLink}

            ధన్యవాదాలు 🙏
            """;
    }

    public static string PaymentConfirmedWhatsApp(string parentName, string studentName, string schoolName, decimal amount)
    {
        return $"""
            నమస్కారం {parentName} గారు! 🙏

            ✅ ₹{amount:N0} ఫీజు అందుకున్నాము.
            విద్యార్థి: {studentName}
            పాఠశాల: {schoolName}

            మీ సహకారానికి ధన్యవాదాలు! 🎉
            """;
    }

    public static string ExtensionGrantedWhatsApp(string parentName, string studentName, string schoolName, DateTime newDeadline)
    {
        return $"""
            నమస్కారం {parentName} గారు! 🙏

            📋 ఫీజు గడువు పొడిగింపు మంజూరు అయింది.
            విద్యార్థి: {studentName}
            పాఠశాల: {schoolName}
            కొత్త గడువు: {newDeadline:dd MMM yyyy}

            సకాలంలో చెల్లించాలని కోరుకుంటున్నాము. 🙏
            """;
    }
}
