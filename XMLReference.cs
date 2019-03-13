using System.Collections.Generic;
using System.Linq;

namespace CSMud
{
    public interface Identifiable
    {
        int Id { get; set; }
    }
    public class XMLReference<T> where T : Identifiable
    {
        public T Actual { get; set; }
        public int Id { get; set; }

        public static void Link(List<XMLReference<T>> refs, List<T> actuals)
        {
            var ActualById = actuals.ToDictionary(t => t.Id, t => t);
            refs.ForEach(r => r.Actual = ActualById[r.Id]);
        }
    }
}
