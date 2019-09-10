using SynchroFeed.Library.DomainLoader;

namespace SynchroFeed.Command.Catalog
{
    /// <summary>
    /// Extracts information about the assembly and its references.
    /// </summary>
    public class GetReferencesOperation : IAssemblyOperation
    {
        /// <summary>
        /// Operates on the assembly and, optionally, returning an object.
        /// </summary>
        /// <param name="assembly">The assembly to operate on.</param>
        /// <returns>The resulting serializable object returned by the operation.</returns>
        public object Operation(System.Reflection.Assembly assembly)
        {
            var assemblyName = assembly.GetName();
            var assemblyInfo = new AssemblyInfo
            {
                AssemblyName = new AssemblyName()
                {
                    FullName = assemblyName.FullName,
                    Name = assemblyName.Name,
                    Version = assemblyName.Version
                }
            };

            foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
            {
                assemblyInfo.ReferencedAssemblies.Add(new AssemblyName()
                {
                    FullName = referencedAssembly.FullName,
                    Name = referencedAssembly.Name,
                    Version = referencedAssembly.Version
                });
            }

            return assemblyInfo;
        }
    }
}
