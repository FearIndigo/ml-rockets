using UnityEngine;

namespace FearIndigo.Managers
{
    public class SubManager : MonoBehaviour
    {
        private GameManager _gameManager;

        protected GameManager GameManager
        {
            get
            {
                if (!_gameManager)
                {
                    _gameManager = GetComponent<GameManager>();
                }
                    
                return _gameManager;
            }
        }
    }
}