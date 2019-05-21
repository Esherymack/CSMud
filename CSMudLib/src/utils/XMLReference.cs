using System.Collections.Generic;
using System.Linq;

/* A foray into generics
 * Basically tried to smarten up the deserializer to function more like a 
 * SQL "INNER JOIN" between the Thing, Entity, and Room objects.
 */
namespace CSMud.Utils
{
    public interface Identifiable
    {
        int Id { get; set; }
    }
    public class XMLReference<T> where T : Identifiable
    {
        public T Actual { get; set; }
        public int Id { get; set; }

        /* Link is the function that actually performs the 'join'
         */      
        public static void Link(List<XMLReference<T>> refs, List<T> actuals)
        {
            var ActualById = actuals.ToDictionary(t => t.Id, t => t);
            refs.ForEach(r => r.Actual = ActualById[r.Id]);
        }
    }
}
