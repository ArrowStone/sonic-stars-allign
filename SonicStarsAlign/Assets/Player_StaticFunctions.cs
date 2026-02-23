using UnityEngine;

public class Player_StaticFunctions : MonoBehaviour
{
        [TextSpace("This component has no active or serialised fields, but core, frequently used functions related to the player are held here to allow for debugging. \n" +
                "For instance, SetTransform is a function here to track whenever the player's transform is manually set. ")]
        public bool DummyText;

        public static int _fixedFrameNumber;

        private void Start () {
                _fixedFrameNumber = 0;
        }

        private void FixedUpdate () {
                _fixedFrameNumber++;       
        }

        //Called every time the player's transform is set manually, to allow for debugging code everytime it happens.
        public static void SetTransform (Transform transform, Vector3 position, Quaternion rotation, string source) {
                //Debug.Log(_fixedFrameNumber+ " " +source+ " has set Player Transform to " + position + " and " + rotation + "!");
                Debug.DrawLine(transform.position, position, Color.red, 10f);

                transform.SetPositionAndRotation(position, rotation);
        }

        public static void MoveRBPosition(Rigidbody rb, Vector3 position, string source) {
                //Debug.Log(_fixedFrameNumber + " " + source + " has moved Player RB Position to " + position + "!");
                //Debug.Log(source + " has moved Player RB Position to  !");

                //Debug.DrawRay(rb.transform.position, Vector3.up, Color.blue, 10f);
                //Debug.DrawRay(position, Vector3.up, Color.blue, 10f);
                //Debug.DrawLine(rb.transform.position, position, Color.blue, 10f);
                rb.MovePosition(position);
        }

        public static void SetRBPosition( Rigidbody rb, Vector3 position, string source ) {
                //Debug.Log(_fixedFrameNumber + " " + source + " has set Player RB Position to " + position + "!");

                Debug.DrawLine(rb.position, position, Color.magenta, 10f);
                rb.transform.position = position;
        }
        

        public static void SetTransformRotation(Transform transform, Quaternion rotation, string source) {
                //Debug.Log(_fixedFrameNumber + " " + source + " has set Player Transform Rotation to " + rotation + "!");
                Debug.DrawRay(transform.position, transform.rotation * Vector3.up, Color.green, 10f);
                Debug.DrawRay(transform.position, rotation * Vector3.up, Color.green, 10f);

                transform.rotation = rotation;
        }
}
