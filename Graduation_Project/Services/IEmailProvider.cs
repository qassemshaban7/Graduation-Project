namespace Graduation_Project.Services
{
    public interface IEmailProvider
    {
        public Task<int> SendResetCode(string to);
    }
}
