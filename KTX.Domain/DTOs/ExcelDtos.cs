namespace KTX.Domain.DTOs;

public class ExcelImportResultDto
{
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<ExcelImportRowError> Errors { get; set; } = new();
}

public class ExcelImportRowError
{
    public int RowIndex { get; set; }
    public string? Identifier { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
