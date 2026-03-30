using UnityEngine;

public class DetachFromParent : MonoBehaviour
{
        public bool DetachOnStart = true;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start () {
                if(DetachOnStart)
                        Detach();
        }

        public void Detach () {
                transform.SetParent(null);
        }
}
