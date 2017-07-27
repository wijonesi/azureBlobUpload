using System;
using System.Linq;
using System.Web.Mvc;

using Microsoft.WindowsAzure.Storage;

namespace BlobUpload.Controllers
{
    public class HomeController : Controller
    {
        private class AzureConfiguration
        {
            private static string StorageAccountKey = "<YOUR STORAGE ACCOUNT KEY>"; // use this for Azure Commercial (MAC)
            private static string StorageAccountKeyMAG = "<YOUR STORAGE ACCOUNT KEY>"; // use this for Azure Government (MAG)
            public static string StorageAccountName = "<YOUR STORAGE ACCOUNT NAME>";
            public static string StorageContainerName = "<YOUR CONTAINER NAME>";
            private static string StorageEndPoint = "core.usgovcloudapi.net"; // this endpoint is used only for Azure Government (MAG)

            public static string StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=" + StorageAccountName + ";AccountKey=" + StorageAccountKey; // for Azure Commercial only (MAC)

            // Microsoft Azure Commercial (MAC)
            public static CloudStorageAccount Account = CloudStorageAccount.Parse(StorageConnectionString);

            // Microsoft Azure Government (MAG)
            //public static CloudStorageAccount Account = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(StorageAccountName, StorageAccountKeyMAG), StorageEndPoint, useHttps: true);
        }

        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        [HttpPost]
        public JsonResult Commit()
        {
            var error = string.Empty;

            try
            {
                var name = Request.Form["name"];

                var list = Request.Form["list"];

                var ids = list.Split(',').Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => Convert.ToBase64String(BitConverter.GetBytes(int.Parse(id)))).ToArray();

                CloudStorageAccount storageAccount = AzureConfiguration.Account;

                var blobClient = storageAccount.CreateCloudBlobClient();

                var container = blobClient.GetContainerReference(AzureConfiguration.StorageContainerName);

                var blob = container.GetBlockBlobReference(name);

                blob.PutBlockList(ids);
            }
            catch(Exception e)
            {
                error = e.ToString();
            }

            return new JsonResult() { Data = new { success = string.IsNullOrWhiteSpace(error), error = error } };
        }

        [HttpPost]
        public JsonResult UploadInFormData()
        {
            var error = string.Empty;

            try
            {
                var name = Request.Form["name"];

                var index = int.Parse(Request.Form["index"]);

                var file = Request.Files[0];

                var id = Convert.ToBase64String(BitConverter.GetBytes(index));

                CloudStorageAccount storageAccount = AzureConfiguration.Account;

                var blobClient = storageAccount.CreateCloudBlobClient();

                var container = blobClient.GetContainerReference(AzureConfiguration.StorageContainerName);

                var blob = container.GetBlockBlobReference(name);

                blob.PutBlock(id, file.InputStream, null);
            }
            catch(Exception e)
            {
                error = e.ToString();
            }

            return new JsonResult() { Data = new { success = string.IsNullOrWhiteSpace(error), error = error } };
        }
    }
}
