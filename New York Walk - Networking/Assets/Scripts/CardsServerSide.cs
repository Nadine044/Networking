using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using System.Linq;
public class CardsServerSide : MonoBehaviour
{
    [HideInInspector]
    public List<int> cards_forboth = new List<int>();
    // Start is called before the first frame update
    [HideInInspector]
    public ConcurrentDictionary<int, string> cards_dictionary = new ConcurrentDictionary<int, string>();
    void Start()
    {
        cards_dictionary.TryAdd(1, "Actress");
        cards_dictionary.TryAdd(2, "Anthropologist");
        cards_dictionary.TryAdd(3, "Artist");
        cards_dictionary.TryAdd(4, "Cabbie");
        cards_dictionary.TryAdd(5, "Coach");
        cards_dictionary.TryAdd(6, "Economist");
        cards_dictionary.TryAdd(7,"Grandma" );
        cards_dictionary.TryAdd(8, "Grandpa");
        cards_dictionary.TryAdd(9,"Historian");
        cards_dictionary.TryAdd(10, "Journalist");
        cards_dictionary.TryAdd(11, "Kid");
        cards_dictionary.TryAdd(12, "Pianist");
        cards_dictionary.TryAdd(13, "PizzaGuy");
        cards_dictionary.TryAdd(14, "PoliceMan");//here
        cards_dictionary.TryAdd(15,"Politician");
        cards_dictionary.TryAdd(16,"Priest");
        cards_dictionary.TryAdd(17, "Profressor");
        cards_dictionary.TryAdd(18,"Receptionist");
        cards_dictionary.TryAdd(19, "Robot");
        cards_dictionary.TryAdd(20,"Secretary" );
        cards_dictionary.TryAdd(21, "Student");
        cards_dictionary.TryAdd(22,"Tourist Guide" );
        cards_dictionary.TryAdd(23, "Truck Driver");
        cards_dictionary.TryAdd(24, "Watchmaker");


        for (int i = 0; i < 6; i++)
        {
            cards_forboth.Add(Random.Range(0, 23));
        }

        //checks if all numbers inside the list are different
        //var allUnique = cards_forboth.GroupBy(x => x).All(g => g.Count() == 1);

        //IEnumerable<int> duplicates = cards_forboth.GroupBy(x => x).Where(g => g.Count() > 1).Select(x => x.Key);

        //make a new list with the unique numbers
        List<int> result = cards_forboth.Distinct().ToList(); 

        while(result.Count <6)
        {
            //while the 6 numbers arent different keep making randoms
            cards_forboth.Add(Random.Range(0, 23));
            result = cards_forboth.Distinct().ToList();
        }

        //finally the 6 cards to give the players
        cards_forboth = result;
        Debug.Log(result);

    }
}
