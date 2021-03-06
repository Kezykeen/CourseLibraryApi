using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibraryApi.Services
{
    public class PropertyMappingValue
    {
        public IEnumerable<string> DestinationProperties { get; }

        public bool Revert { get; }

        public PropertyMappingValue(IEnumerable<string> destinationProperties, bool revert = false)
        {
            DestinationProperties = destinationProperties
                                    ?? throw new ArgumentNullException(nameof(destinationProperties));
            Revert = revert;
        }
    }
}
