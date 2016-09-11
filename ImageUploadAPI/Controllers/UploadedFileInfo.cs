namespace ImageUploadAPI.Controllers
{
    /// <summary>
    /// File Information for the Uploaded file
    /// </summary>
    public class UploadedFileInfo
    {
        /// <summary>
        /// Name of the File
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// File extension
        /// </summary>
        public string FileExtension { get; set; }
        /// <summary>
        /// URL of the file
        /// </summary>
        public string FileURL { get; set; }
        /// <summary>
        /// Content Type of the uploaded file
        /// </summary>
        public string ContentType { get; set; }
    }
}