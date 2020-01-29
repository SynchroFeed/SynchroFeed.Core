using SharpCompress.Archives;
using SynchroFeed.Library.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SynchroFeed.Library.Reflection
{
    /// <summary>
    /// An assembly resolver that inspects a zip file for every assembly that may be loaded.
    /// The file name is expected to be the same as the assembly's simple name.
    /// Multiple assemblies can exist in the zip file with the same name but in different directories.
    /// A single instance of ZipAssemblyResolver can be used with multiple MetadataAssemblyResolver instances.
    /// </summary>
    /// <remarks>
    /// In order for an AssemblyName to match to a loaded assembly, AssemblyName.Name must be equal (casing ignored).
    /// - If AssemblyName.PublicKeyToken is specified, it must be equal.
    /// - If AssemblyName.PublicKeyToken is not specified, assemblies with no PublicKeyToken are selected over those with a PublicKeyToken.
    /// - If more than one assembly matches, the assembly with the highest Version is returned.
    /// - CultureName is ignored.
    /// </remarks>
    public class ZipAssemblyResolver : MetadataAssemblyResolver
    {
        private readonly Dictionary<string, List<IArchiveEntry>> _zipEntries = new Dictionary<string, List<IArchiveEntry>>(StringComparer.OrdinalIgnoreCase);
        private readonly IArchive _archive;
        private readonly string _coreAssemblyPath;
        private readonly string _coreAssemblyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="System.Reflection.PathAssemblyResolver"/> class.
        /// </summary>
        /// <param name="zipFile">The <see cref="ZipFile" /> to load assemblies from.</param>
        /// <param name="coreAssembly">The core <see cref="Assembly"/>.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="zipFile"/> or <paramref name="coreAssembly"/> is null.</exception>
        public ZipAssemblyResolver(IArchive archive, Assembly coreAssembly)
        {
            if (archive == null)
                throw new ArgumentNullException(nameof(archive));

            _archive = archive;
            _coreAssemblyName = coreAssembly.GetName().Name;
            _coreAssemblyPath = coreAssembly.Location;

            foreach (var archiveEntry in _archive.Entries)
            {
                if (archiveEntry.IsDirectory)
                    continue;

                var extension = Path.GetExtension(archiveEntry.Key);

                if (extension.Equals(".dll", StringComparison.InvariantCultureIgnoreCase)
                    || extension.Equals(".exe", StringComparison.InvariantCultureIgnoreCase))
                {
                    var file = Path.GetFileNameWithoutExtension(archiveEntry.Key);

                    List<IArchiveEntry> archiveEntries;

                    if (!_zipEntries.TryGetValue(file, out archiveEntries))
                        _zipEntries.Add(file, archiveEntries = new List<IArchiveEntry>());

                    archiveEntries.Add(archiveEntry);
                }
            }
        }

        /// <inheritdoc />
        public override Assembly Resolve(MetadataLoadContext context, AssemblyName assemblyName)
        {
            Assembly candidateWithSamePkt = null;
            Assembly candidateIgnoringPkt = null;

            if (_zipEntries.TryGetValue(assemblyName.Name, out List<IArchiveEntry> archiveEntries))
            {
                ReadOnlySpan<byte> pktFromName = assemblyName.GetPublicKeyToken();

                foreach (var archiveEntry in archiveEntries)
                {
                    using (var entryStream = archiveEntry.ExtractToStream())
                    {
                        var assemblyFromZip = context.LoadFromStream(entryStream);
                        var assemblyNameFromZip = assemblyFromZip.GetName();

                        if (assemblyName.Name.Equals(assemblyNameFromZip.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            ReadOnlySpan<byte> pktFromAssembly = assemblyNameFromZip.GetPublicKeyToken();

                            // Find exact match on PublicKeyToken including treating no PublicKeyToken as its own entry.
                            if (pktFromName.SequenceEqual(pktFromAssembly))
                            {
                                // Pick the highest version.
                                if (candidateWithSamePkt == null || assemblyNameFromZip.Version > candidateWithSamePkt.GetName().Version)
                                {
                                    candidateWithSamePkt = assemblyFromZip;
                                }
                            }
                            // If assemblyName does not specify a PublicKeyToken, then still consider those with a PublicKeyToken.
                            else if (candidateWithSamePkt == null && pktFromName.IsEmpty)
                            {
                                // Pick the highest version.
                                if (candidateIgnoringPkt == null || assemblyNameFromZip.Version > candidateIgnoringPkt.GetName().Version)
                                {
                                    candidateIgnoringPkt = assemblyFromZip;
                                }
                            }
                        }
                    }
                }
            }

            var assembly = candidateWithSamePkt ?? candidateIgnoringPkt;

            if ((assembly == null) && (assemblyName.Name.Equals(_coreAssemblyName, StringComparison.InvariantCultureIgnoreCase)))
                assembly = context.LoadFromAssemblyPath(_coreAssemblyPath);

            return assembly;
        }
    }
}