using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace caveCache
{
    interface IMediaCache
    {
        Stream GetMediaDataStream(ObjectId mediaId);
        bool RemoveMedia(ObjectId mediaId);
        bool SetMediaDataStream(ObjectId mediaId, Stream stream);
    }

    class MediaCache : IMediaCache
    {        
        private IConfiguration _config;
        public MediaCache(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

    private string BuildFilePathFromIntId(int mediaId)
    {
      string subDir = (mediaId / 1000 * 1000 + 1).ToString();
      string dir = Path.Combine(_config.ImageDirectory, subDir);
      if (!Directory.Exists(dir))
        Directory.CreateDirectory(dir);

      return Path.Combine(dir, $"{mediaId:0000}.bin");
    }

    private string BuildFilePath(ObjectId mediaId)
        {
            string subDir = mediaId.ToString().Substring(4);
            string dir = Path.Combine(_config.ImageDirectory, subDir);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return Path.Combine(dir, $"{mediaId:0000}.bin");
        }

        public Stream GetMediaDataStream(ObjectId mediaId)
        {
            string path = BuildFilePath(mediaId);
            if (File.Exists(path))
                return new FileStream(path, FileMode.Open, FileAccess.Read);
            else
                throw new FileNotFoundException($"File for media {mediaId} not found at expected location '{path}'");
        }

        public bool RemoveMedia(ObjectId mediaId)
        {
            string path = BuildFilePath(mediaId);
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unable to removed file. Error {ex.GetType().Name} '{ex.Message}'");
                    return false;
                }
            }
            else
                return true;
        }

        public bool SetMediaDataStream(ObjectId mediaId, Stream stream)
        {
            try
            {
                string path = BuildFilePath(mediaId);
                using (var fout = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    var buffer = new byte[4096];
                    int readCount = 0;
                    while (0 < (readCount = stream.Read(buffer, 0, buffer.Length)))
                    {
                        fout.Write(buffer, 0, readCount);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing media: {ex.GetType().Name} : {ex.Message}");
                Console.WriteLine($"{ex.StackTrace}");
                return false;
            }
        }
    }
}
