using Minio;
using Minio.DataModel.Args;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SMO.Service.CM
{
    public class FileService
    {
        private readonly IMinioClient _minioClient;
        private readonly string _bucketName;

        public FileService()
        {
            var endpoint = ConfigurationManager.AppSettings["MinIO:Endpoint"] ?? "localhost";
            var port = int.Parse(ConfigurationManager.AppSettings["MinIO:Port"] ?? "9000");
            var accessKey = ConfigurationManager.AppSettings["MinIO:AccessKey"] ?? "minioadmin";
            var secretKey = ConfigurationManager.AppSettings["MinIO:SecretKey"] ?? "minioadmin";
            var useSSL = bool.Parse(ConfigurationManager.AppSettings["MinIO:UseSSL"] ?? "true");
            _bucketName = ConfigurationManager.AppSettings["MinIO:BucketName"] ?? "ebid-vtb";

            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;

            _minioClient = new MinioClient()
                .WithEndpoint(endpoint, port)
                .WithCredentials(accessKey, secretKey)
                .WithSSL(useSSL)
                .Build();
        }

        /// <summary>
        /// Kiểm tra và tạo bucket nếu chưa tồn tại
        /// </summary>
        public async Task EnsureBucketExistsAsync()
        {
            try
            {
                var beArgs = new BucketExistsArgs()
                    .WithBucket(_bucketName);

                bool found = await _minioClient.BucketExistsAsync(beArgs);

                if (!found)
                {
                    var mbArgs = new MakeBucketArgs()
                        .WithBucket(_bucketName);
                    await _minioClient.MakeBucketAsync(mbArgs);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi kiểm tra bucket: {ex.Message}", ex);
            }
        }


        /// <summary>
        /// Upload file lên MinIO
        /// </summary>
        /// <param name="fileStream">Stream của file</param>
        /// <param name="fileName">Tên file gốc</param>
        /// <param name="contentType">Content type của file</param>
        /// <param name="folder">Thư mục con (optional)</param>
        /// <returns>Object name trên MinIO</returns>
        public async Task UploadFileAsync(byte[] data, string objectName)
        {
            try
            {
                var contentType = "application/octet-stream";
                await EnsureBucketExistsAsync();

                using (var ms = new MemoryStream(data))
                {
                    var putObjectArgs = new PutObjectArgs()
                        .WithBucket(_bucketName)
                        .WithObject(objectName)
                        .WithStreamData(ms)
                        .WithObjectSize(data.Length)
                        .WithContentType(contentType);

                    await _minioClient.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi upload vật lý lên MinIO: {ex.Message}", ex);
            }
        }


        /// <summary>
        /// Upload nhiều file cùng lúc
        /// </summary>
        public async Task<string[]> UploadMultipleFilesAsync(byte[][] datas, string[] fileNames)
        {
            var objectNames = new string[datas.Length];

            for (int i = 0; i < datas.Length; i++)
            {
                await UploadFileAsync(
                    datas[i],
                    fileNames[i]
                );
                objectNames[i] = fileNames[i];
            }

            return objectNames;
        }

        /// <summary>
        /// Download file từ MinIO
        /// </summary>
        /// <param name="objectName">Object name trên MinIO</param>
        /// <returns>MemoryStream chứa dữ liệu file</returns>
        public async Task<MemoryStream> DownloadFileAsync(string objectName)
        {
            try
            {
                var memoryStream = new MemoryStream();

                var getObjectArgs = new GetObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(objectName)
                    .WithCallbackStream((stream) =>
                    {
                        stream.CopyTo(memoryStream);
                    });

                await _minioClient.GetObjectAsync(getObjectArgs);

                memoryStream.Position = 0;
                return memoryStream;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi download file: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Xóa file trên MinIO
        /// </summary>
        /// <param name="objectName">Object name cần xóa</param>
        public async Task DeleteFileAsync(string objectName)
        {
            try
            {
                var removeObjectArgs = new RemoveObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(objectName);

                await _minioClient.RemoveObjectAsync(removeObjectArgs);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi xóa file: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Xóa nhiều file cùng lúc
        /// </summary>
        public async Task DeleteMultipleFilesAsync(string[] objectNames)
        {
            foreach (var objectName in objectNames)
            {
                await DeleteFileAsync(objectName);
            }
        }

        /// <summary>
        /// Lấy presigned URL để download file (link tạm thời)
        /// </summary>
        /// <param name="objectName">Object name</param>
        /// <param name="expiryInSeconds">Thời gian hết hạn (giây), mặc định 1 giờ</param>
        /// <returns>URL có thể truy cập trực tiếp</returns>
        public async Task<string> GetPresignedUrlAsync(string objectName, int expiryInSeconds = 3600)
        {
            try
            {
                var presignedGetObjectArgs = new PresignedGetObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(objectName)
                    .WithExpiry(expiryInSeconds);

                return await _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tạo presigned URL: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Lấy presigned URL để upload file trực tiếp (từ client)
        /// </summary>
        public async Task<string> GetPresignedUploadUrlAsync(string objectName, int expiryInSeconds = 3600)
        {
            try
            {
                var presignedPutObjectArgs = new PresignedPutObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(objectName)
                    .WithExpiry(expiryInSeconds);

                return await _minioClient.PresignedPutObjectAsync(presignedPutObjectArgs);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tạo presigned upload URL: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Kiểm tra file có tồn tại không
        /// </summary>
        public async Task<bool> FileExistsAsync(string objectName)
        {
            try
            {
                var statObjectArgs = new StatObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(objectName);

                await _minioClient.StatObjectAsync(statObjectArgs);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Lấy thông tin file (metadata)
        /// </summary>
        public async Task<FileMetadata> GetFileMetadataAsync(string objectName)
        {
            try
            {
                var statObjectArgs = new StatObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(objectName);

                var statObject = await _minioClient.StatObjectAsync(statObjectArgs);

                return new FileMetadata
                {
                    ObjectName = statObject.ObjectName,
                    Size = statObject.Size,
                    ContentType = statObject.ContentType,
                    LastModified = statObject.LastModified,
                    ETag = statObject.ETag
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy metadata: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Copy file trong MinIO
        /// </summary>
        public async Task<string> CopyFileAsync(string sourceObjectName, string destinationObjectName)
        {
            try
            {
                var cpSrcArgs = new CopySourceObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(sourceObjectName);

                var copyObjectArgs = new CopyObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(destinationObjectName)
                    .WithCopyObjectSource(cpSrcArgs);

                await _minioClient.CopyObjectAsync(copyObjectArgs);

                return destinationObjectName;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi copy file: {ex.Message}", ex);
            }
        }
    }


    /// <summary>
    /// Class chứa metadata của file
    /// </summary>
    public class FileMetadata
    {
        public string ObjectName { get; set; }
        public long Size { get; set; }
        public string ContentType { get; set; }
        public DateTime LastModified { get; set; }
        public string ETag { get; set; }
    }
}