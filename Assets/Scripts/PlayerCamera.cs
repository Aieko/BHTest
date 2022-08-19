using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
// This sets up the scene camera for the local player

namespace BHTest
{
    public class PlayerCamera : NetworkBehaviour
    {
        public Camera myCam;
        public AudioListener myAudioListener;
        public float rotateSpeed = 5;
        Vector3 offset;

        void Awake()
        {
            offset = transform.position - transform.position;
        }

        public override void OnStartLocalPlayer()
        {
            if (isLocalPlayer && isClient)
            {
                //Only the client that owns this object executes this code
                if (myCam.enabled == false)
                    myCam.enabled = true;

                if (myAudioListener.enabled == false)
                    myAudioListener.enabled = true;
            }

            if (myCam != null)
            {
                    // configure and make camera a child of player with 3rd person offset
                    myCam.orthographic = false;
                    myCam.transform.localPosition = new Vector3(0f, 3f, -8f);
                    myCam.transform.localEulerAngles = new Vector3(10f, 0f, 0f);
            }
        }

        public override void OnStopLocalPlayer()
        {
            if (myCam != null)
            {
                myCam.orthographic = true;
                myCam.transform.localPosition = new Vector3(0f, 70f, 0f);
                myCam.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
            }
        }

        void LateUpdate()
        {
            MouseAimCamera();
        }

        private void MouseAimCamera()
        {
            float horizontal = Input.GetAxis("Mouse X") * rotateSpeed;
            transform.Rotate(0, horizontal, 0);

            float desiredAngle = transform.eulerAngles.y;
            Quaternion rotation = Quaternion.Euler(0, desiredAngle, 0);
            transform.position = transform.position - (rotation * offset);

            transform.LookAt(transform);
        }
    }
}
