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
  
    void Start()
    {


        for (int i = 0; i < 6; i++)
        {
            cards_forboth.Add(Random.Range(0, 25));
        }

        //checks if all numbers inside the list are different
        //var allUnique = cards_forboth.GroupBy(x => x).All(g => g.Count() == 1);

        //IEnumerable<int> duplicates = cards_forboth.GroupBy(x => x).Where(g => g.Count() > 1).Select(x => x.Key);

        //make a new list with the unique numbers
        List<int> result = cards_forboth.Distinct().ToList(); 

        while(result.Count <6)
        {
            //while the 6 numbers arent different keep making randoms
            cards_forboth.Add(Random.Range(0, 25));
            result = cards_forboth.Distinct().ToList();
        }

        //finally the 6 cards to give the players
        cards_forboth = result;
        Debug.Log(result);

    }
}
