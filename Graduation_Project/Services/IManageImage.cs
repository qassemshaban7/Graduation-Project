using Microsoft.AspNetCore.Mvc;

namespace Graduation_Project.Services
{
    public interface IManageImage
    {
        Task<(byte[], string, string)> DownloadFile(string FileName);
    }
}