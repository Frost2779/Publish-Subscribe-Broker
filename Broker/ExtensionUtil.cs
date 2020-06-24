using System;
using System.Collections.Generic;
using System.Text;

namespace Broker {
    public static class ExtensionUtil {

        public static bool EqualsIgnoreCase(this string caller, string toCompare) {
            return caller.ToLower().Equals(toCompare.ToLower());
        }    
    }
}
