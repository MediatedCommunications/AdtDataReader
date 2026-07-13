namespace AdtDataReader {
    internal static class StringExtensions {
        public static string TrimEndNulls(this string This) {
            var ret = This.TrimEnd('\0');
            return ret;
        }

    }
}
