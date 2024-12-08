using Graduation_Project.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using static Azure.Core.HttpHeader;

namespace Graduation_Project.Services
{
    public class ManageImage : IManageImage
    {
        //public async Task<(byte[], string, string)> DownloadFile(string FileName)
        //{
        //    try
        //    {
        //        var _GetFilePath = CommonA.GetFilePath(FileName);
        //        var provider = new FileExtensionContentTypeProvider();
        //        if (!provider.TryGetContentType(_GetFilePath, out var _ContentType))
        //        {
        //            _ContentType = "application/octet-stream";
        //        }
        //        var _ReadAllBytesAsync = await File.ReadAllBytesAsync(_GetFilePath);
        //        return (_ReadAllBytesAsync, _ContentType, FileName);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public async Task<(byte[], string, string)> DownloadFile(string FileName)
        {
            try
            {
                var _GetFilePath = CommonA.GetFilePath(FileName);
                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(_GetFilePath, out var _ContentType))
                {
                    _ContentType = "application/octet-stream";
                }
                var _ReadAllBytesAsync = await File.ReadAllBytesAsync(_GetFilePath);
                return (_ReadAllBytesAsync, _ContentType, FileName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
