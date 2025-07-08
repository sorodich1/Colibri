using System.Collections.Generic;

namespace Colibri.WebApi.Services.Abstract
{
    /// <summary>
    /// 
    /// </summary>
    public interface IJwtGenerator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        string Seed(string userName, List<string> roles);
    }
}
