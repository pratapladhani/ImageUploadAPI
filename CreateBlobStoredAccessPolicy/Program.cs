using System;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;


namespace CreateBlobStoredAccessPolicy
{
    class Program
    {
        static void Main(string[] args)
        {
            //Parse the connection string and return a reference to the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);

            //Create the blob client object.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            //Get a reference to a container to use for the sample code, and create it if it does not exist.
            CloudBlobContainer container = blobClient.GetContainerReference("images");
            container.CreateIfNotExists();

            //Insert calls to the methods created below here...
            //Clear any existing access policies on container.
            BlobContainerPermissions perms = container.GetPermissions();
            perms.SharedAccessPolicies.Clear();
            container.SetPermissions(perms);

            //Create a new access policy on the container, which may be optionally used to provide constraints for
            //shared access signatures on the container and the blob.
            string sharedAccessPolicyName = "ImageReadPolicy";
            CreateSharedAccessPolicy(blobClient, container, sharedAccessPolicyName);
            Console.WriteLine("SAS Policy {0} created successfully for the {1}", sharedAccessPolicyName, container.StorageUri.PrimaryUri.ToString());

            //Require user input before closing the console window.
            Console.ReadLine();
        }
        static void CreateSharedAccessPolicy(CloudBlobClient blobClient, CloudBlobContainer container, string policyName)
        {
            //Create a new shared access policy and define its constraints.
            SharedAccessBlobPolicy sharedPolicy = new SharedAccessBlobPolicy()
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddYears(2),
                Permissions = SharedAccessBlobPermissions.Read
            };

            //Get the container's existing permissions.
            BlobContainerPermissions permissions = container.GetPermissions();

            //Add the new policy to the container's permissions, and set the container's permissions.
            permissions.SharedAccessPolicies.Add(policyName, sharedPolicy);
            container.SetPermissions(permissions);
        }
    }
}
