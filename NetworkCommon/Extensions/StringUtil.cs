namespace NetworkCommon.Extensions {
    public static class StringUtil {
        public static bool EqualsIgnoreCase(this string caller, string toCompare) {
            return caller.ToLower().Equals(toCompare.ToLower());
        }
    }
}
