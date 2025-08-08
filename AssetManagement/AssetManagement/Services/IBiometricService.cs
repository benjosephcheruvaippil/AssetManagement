using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Services
{
    public interface IBiometricService
    {
        bool IsBiometricEnrolled();
    }
}
