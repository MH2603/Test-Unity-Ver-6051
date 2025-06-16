using System.Linq;
using UnityEngine;

namespace MH.Git_Amend
{
    public class ToListExam : MonoBehaviour
    {
        private ILogger logger = new UnityLogger();
        
        void Start()
        {
            // Example usage of ToList
            int[] numbers = { 1, 2, 3, 4, 5 };

            var filter = numbers.Where(x =>
            {
                logger.Log($"check: {x}");
                return x > 4;
            });
            
            logger.Log($" Filter result: {string.Join(", ", filter)}");
            logger.Log($" Filter result: {string.Join(", ", filter)}");

        }
    }
}