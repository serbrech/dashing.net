using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dashing
{
    public interface IFileSystem
    {
        bool FileExists(string path);
    }
}
