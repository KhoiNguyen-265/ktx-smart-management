using KTX.Domain.DTOs;

namespace KTX.Services.Allocation;

public static class PriorityCalculator
{
    public static decimal CalculatePriority(
        int priorityPolicyPoints,
        int studentCourse,
        int latestCourse,
        int distanceGroup,
        DateTime submittedAt,
        DateTime batchStart,
        DateTime batchEnd)
    {
        decimal score = priorityPolicyPoints;

        // 1. Khóa học (Ưu tiên tân sinh viên)
        if (studentCourse == latestCourse)
        {
            score += 10;
        }

        // 2. Điểm khoảng cách quê quán
        score += distanceGroup switch
        {
            3 => 20, // > 300km
            2 => 10, // 100 - 300km
            _ => 0   // < 100km
        };

        // 3. Hàm tie-breaker dựa trên thời gian nộp đơn g(ThoiGianNop) tối đa 5 điểm
        var totalDuration = (batchEnd - batchStart).TotalMinutes;
        if (totalDuration > 0)
        {
            var elapsed = (submittedAt - batchStart).TotalMinutes;
            if (elapsed < 0) elapsed = 0;
            if (elapsed > totalDuration) elapsed = totalDuration;

            // Nộp càng sớm điểm thưởng tie-break càng cao (tối đa 5.0 điểm)
            var tieBreak = 5.0 * (1.0 - (elapsed / totalDuration));
            score += (decimal)Math.Round(tieBreak, 2);
        }

        return score;
    }
}
