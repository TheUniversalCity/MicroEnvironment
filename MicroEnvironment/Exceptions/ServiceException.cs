using System;
using System.Collections.Generic;
using System.Text;

namespace MicroEnvironment.Exceptions
{
    public class ServiceException: ApplicationException
    {
        public ServiceException(string message): base(message)
        {

        }
    }
}
