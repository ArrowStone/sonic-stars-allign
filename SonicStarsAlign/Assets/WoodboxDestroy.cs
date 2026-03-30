using UnityEngine;

public class WoodboxDestroy : MonoBehaviour
{
        public GameObject CompleteBoxVersion;
        public GameObject PiecesBoxVersion;
        public GameObject ParentToDeactivate;

        public MonoBehaviour[] SetInactive;

        public Rigidbody[] PiecesRBs;

        public void DestroyBox () {
                CompleteBoxVersion.SetActive(false);
                PiecesBoxVersion.SetActive(true);

                foreach(MonoBehaviour m in SetInactive)
                {
                        m.enabled = false;
                }

                foreach (Rigidbody b in PiecesRBs)
                {
                        b.transform.parent = null;
                        b.AddExplosionForce(60, CompleteBoxVersion.transform.position, 5);
                }

                ParentToDeactivate.SetActive(false);

        }
}
