namespace Triggered
{
    // To use this data class, reference I.O.Data
    using System.Collections.Generic;
    public sealed class I
    {
        private static I o = null;
        private static readonly object padlock = new object();
        private Dictionary<string, object> data;

        private I()
        {
            data = new Dictionary<string, object>();
        }

        public static I O
        {
            get
            {
                lock (padlock)
                {
                    if (o == null)
                    {
                        o = new I();
                    }
                    return o;
                }
            }
        }

        public Dictionary<string, object> Data
        {
            get { return data; }
            set { data = value; }
        }
    }
}
