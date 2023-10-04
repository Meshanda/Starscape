using UnityEngine;

namespace ScriptableObjects.Variables
{
    public class GenericVariableSO<T> : ScriptableObject
    {
        public T value; 
    }
}