using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace HideezMiddleware.Localize
{
    public class ResourceManagersProvider
    {
        /// <summary>
        /// Creates list of ResourceManagers for TranslationSource. 
        /// Your params will be saved by "first in - last out" principe.
        /// </summary>
        /// <param name="types">Type of auto-generated resource class.</param>
        public static void SetResources(params Type[] types)
        {
            List<ResourceManager> resourceManagers = new List<ResourceManager>();
            foreach(var type in types)
            {
                resourceManagers.Add(new ResourceManager(type));
            }
            TranslationSource.SetResourceManagers(resourceManagers);
        }

        /// <summary>
        /// Creates list of ResourceManagers for TranslationSource. 
        /// Your params will be saved by "first in - last out" principe.
        /// </summary>
        /// <param name="types">Type of auto-generated resource class.</param>
        public static void SetErrorCodesResources(params Type[] types)
        {
            List<ResourceManager> resourceManagers = new List<ResourceManager>();
            foreach (var type in types)
            {
                resourceManagers.Add(new ResourceManager(type));
            }
            HideezExceptionLocalization.SetResourceManagers(resourceManagers);
        }
    }
}
