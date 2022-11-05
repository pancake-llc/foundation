namespace Pancake.GameService
{
    using UnityEngine;

    public class DemoSumitScore : MonoBehaviour
    {
        // Start is called before the first frame update
        private void Start()
        {
            GetComponent<ButtonLeaderboard>().valueExpression += () => Random.Range(0, 1000);
        }
    }
}