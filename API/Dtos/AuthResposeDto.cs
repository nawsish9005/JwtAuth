namespace API.Dtos
{
    public class AuthResposeDto
    {
        public string? Token { get; set; }=string.Empty;
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
    }
}