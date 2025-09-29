namespace TaskTrackerApi.DTOs.Errors
{
    public class ValidationErrorResponse : ErrorResponse
    {
        public Dictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();
    }
}