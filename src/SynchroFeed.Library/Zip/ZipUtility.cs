using ICSharpCode.SharpZipLib.Zip;
using System.IO;

namespace SynchroFeed.Library.Zip
{
    public static class ZipUtility
    {
        /// <summary>
        /// Loads the <see cref="ZipEntry"/> into a <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="zipFile">The <see cref="ZipFile"/> containing the entry.</param>
        /// <param name="zipEntry">The <see cref="ZipEntry"/> to push into a <see cref="MemoryStream"/>.</param>
        /// <returns>The <see cref="MemoryStream"/> containing the entry.</returns>
        public static MemoryStream ReadFromZip(ZipFile zipFile, ZipEntry zipEntry)
        {
            using (var inputStream = zipFile.GetInputStream(zipEntry))
            {
                var memoryStream = new MemoryStream();
                inputStream.CopyTo(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                return memoryStream;
            }
        }
    }
}