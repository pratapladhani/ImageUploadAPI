using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;
using ImageUploadAPI.Controllers;
using Swashbuckle.Swagger.Annotations;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ImageUpload.Controllers
{
    /// <summary>
    /// Image Upload API for uploading images to Azure Blob Storage
    /// </summary>
    public class UploadImageController : ApiController
    {
        /// <summary>
        /// Upload an image file to Azure Blob Storage
        /// </summary>
        /// <param name="fileName">FileName for the image. Will use a Guid, if not provided or empty string.</param>
        /// <returns>UploadedFileInfo type</returns>
        [HttpPost]
        [SwaggerResponse(
            HttpStatusCode.OK,
            Description = "Saved successfully",
            Type = typeof(UploadedFileInfo))]
        [SwaggerResponse(
            HttpStatusCode.BadRequest,
            Description = "Could not find file to upload")]
        [SwaggerOperation("UploadImage")]
        public async Task<IHttpActionResult> UploadImage(string fileName = "")
        {
            //Use a GUID in case the fileName is not specified
            if (fileName == "")
            {
                fileName = Guid.NewGuid().ToString();
            }
            //Check if submitted content is of MIME Multi Part Content with Form-data in it?
            if (!Request.Content.IsMimeMultipartContent("form-data"))
            {
                return BadRequest("Could not find file to upload");
            }

            //Read the content in a InMemory Muli-Part Form Data format
            var provider = await Request.Content.ReadAsMultipartAsync(new InMemoryMultipartFormDataStreamProvider());

            //Get the first file
            var files = provider.Files;
            var uploadedFile = files[0];

            //Extract the file extention
            var extension = ExtractExtension(uploadedFile);
            //Get the file's content type
            var contentType = uploadedFile.Headers.ContentType.ToString();

            //create the full name of the image with the fileName and extension
            var imageName = string.Concat(fileName, extension);

            //Get the reference to the Blob Storage and upload the file there
            var storageConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference("images");
            container.CreateIfNotExists();

            var blockBlob = container.GetBlockBlobReference(imageName);
            blockBlob.Properties.ContentType = contentType;

            try
            {
                using (var fileStream = await uploadedFile.ReadAsStreamAsync()) //as Stream is IDisposable
                {
                    blockBlob.UploadFromStream(fileStream);
                }
            }
            catch (StorageException e)
            {
                return BadRequest(e.Message);
            }

            try
            {
                //Generate the shared access signature on the blob.
                string sasBlobToken = blockBlob.GetSharedAccessSignature(null, ConfigurationManager.AppSettings["StoredAccessPolicyName"]);

                var fileInfo = new UploadedFileInfo
                {
                    FileName = fileName,
                    FileExtension = extension,
                    ContentType = contentType,
                    FileURL = blockBlob.Uri.ToString() + sasBlobToken
                };
                return Ok(fileInfo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }
        /// <summary>
        /// Extract the file extension for the file passed
        /// </summary>
        /// <param name="file">File</param>
        /// <returns>Extension of the file</returns>
        public static string ExtractExtension(HttpContent file)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var fileStreamName = file.Headers.ContentDisposition.FileName;
            var fileName = new string(fileStreamName.Where(x => !invalidChars.Contains(x)).ToArray());
            var extension = Path.GetExtension(fileName);
            return extension;
        }
    }
}
