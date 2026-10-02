using System;
using System.Collections.Generic;
using System.Text;

namespace OrderFulfillment.Application.Common
{
    public enum ErrorType
    {
        None = 0,
        Failure = 1,
        NotFound = 2,
        Conflict = 3,
    }
}
