using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class CardsServerSide : MonoBehaviour
{
    List<int> cards_forboth = new List<int>();
    // Start is called before the first frame update

    public Dictionary<int, string> cards_dictionary = new Dictionary<int, string>()
    {
        {1, "Actress"},
        {2, "Anthropologist"},
        {3, "Artist"},
        {4,"Cabbie" },
        {5,"Coach"},
        {6,"Economist"},
        {2,"Grandma" },
        {3, "Grandpa"},
        {4,"Historian" },
        {5,"Journalist"},
        {6,"Kid" },
        {7,"Pianist" },
        {8,"PizzaGuy" },
        {9,"PoliceMan" },
        {10,"Politician" },
        {11,"Priest" },
        {12,"Profressor" },
        {13,"Receptionist" },
        {14,"Robot" },
        {15,"Secretary" },
        {16,"Student" },
        {17,"Tourist Guide" },
        {18,"Truck Driver"},
        {19,"Watchmaker"}
    };
    void Start()
    {
        for (int i = 0; i < 6; i++)
        {
            cards_forboth.Add(Random.Range(1, 19));
        }

        //checks if all numbers inside the list are different
        //var allUnique = cards_forboth.GroupBy(x => x).All(g => g.Count() == 1);

        //IEnumerable<int> duplicates = cards_forboth.GroupBy(x => x).Where(g => g.Count() > 1).Select(x => x.Key);

        //make a new list with the unique numbers
        List<int> result = cards_forboth.Distinct().ToList(); 

        while(result.Count <6)
        {
            //while the 6 numbers arent different keep making randoms
            cards_forboth.Add(Random.Range(1, 19));
            result = cards_forboth.Distinct().ToList();
        }

        //finally the 6 cards to give the players
        cards_forboth = result;
        Debug.Log(result);

    }
}
