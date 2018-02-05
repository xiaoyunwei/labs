using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AmazonS3WebLab.Models;

using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace AmazonS3WebLab.Controllers
{
    public class HomeController : Controller
    {
        const string BUCKET_NAME = "sanofi-op-uat";

        private readonly IAmazonS3 _awsS3Client;

        public HomeController(IAmazonS3 s3Client)
        {
            _awsS3Client = s3Client;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult GetUploadUri(string key)
        {
            GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
            {
                BucketName = BUCKET_NAME,
                Key = key,
                Verb = HttpVerb.PUT,
                Expires = DateTime.Now.AddMinutes(30)
            };

            string uri = _awsS3Client.GetPreSignedURL(request);
            return Content(uri);
        }

        public IActionResult UploadFile()
        {
            return View();
        }

        // 参考资料
        // https://docs.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads
        [HttpPost]
        public async Task<IActionResult> Post(List<IFormFile> files)
        {
            long size = files.Sum(f => f.Length);

            // full path to file in temp location
            var filePath = Path.GetTempFileName();

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                        stream.Flush();
                        stream.Close();
                        int pos = formFile.FileName.LastIndexOf("\\");
                        string fileName = formFile.FileName.Substring(pos + 1);

                        PutObjectRequest request = new PutObjectRequest()
                        {
                            BucketName = BUCKET_NAME,                            
                            Key = $"demo/{fileName}",   // AWS S3上的Key
                            FilePath = filePath         // 本地文件路径
                        };
                        PutObjectResponse resp = await _awsS3Client.PutObjectAsync(request);
                    }
                }
            }

            // process uploaded files
            // Don't rely on or trust the FileName property without validation.

            return Ok(new { count = files.Count, size });
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
