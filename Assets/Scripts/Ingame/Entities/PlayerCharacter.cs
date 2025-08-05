using Cardevil.Utils.Directions;
using System;
using UnityEngine;

namespace Cardevil.Ingame.Entities
{
    public class PlayerCharacter : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] protected bool _isDebugMode = false;
        [Header("References")]
        [SerializeField] protected Entity _entity;


        private void Awake()
        {
            if(_entity == null)
            {
                _entity = GetComponent<Entity>();
            }
        }

        public void Update()
        {
            if (_isDebugMode)
            {
                int horizontal = (int)Input.GetAxis("Horizontal");
                int vertical = (int)Input.GetAxis("Vertical");
                
            }
        }
    }
}