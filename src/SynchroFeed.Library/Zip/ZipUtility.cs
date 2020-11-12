using SharpCompress.Archives;
using System.IO;

namespace SynchroFeed.Library.Zip
{
    public static class ZipUtility
    {
        /// <summary>
        /// Uncompresses the <see cref="IArchiveEntry"/> into a <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="archiveEntry">The <see cref="IArchiveEntry"/> to uncompress into a <see cref="MemoryStream"/>.</param>
        /// <returns>The <see cref="MemoryStream"/> containing the uncompressed <see cref="IArchiveEntry"/>.</returns>
        public static MemoryStream ExtractToStream(this IArchiveEntry archiveEntry)
        {
            using var compressedStream = archiveEntry.OpenEntryStream();
            var memoryStream = new MemoryStream();
            compressedStream.CopyTo(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }
    }
}