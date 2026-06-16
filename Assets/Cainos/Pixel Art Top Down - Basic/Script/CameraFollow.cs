using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cainos.PixelArtTopDown_Basic
{
    //let camera follow target
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public float lerpSpeed = 1.0f;

        private Vector3 offset;

        private void Start()
        {
            if (target == null)
            {
                PlayerController player = FindAnyObjectByType<PlayerController>();
                if (player != null)
                {
                    target = player.transform;
                }
            }
        }

        private void Update()
        {
            if (target == null) return;

            Vector3 mouseOffset = Vector3.zero;
            // Only apply mouse parallax in the Menu phase
            if (target.name == "MenuCameraTarget")
            {
                float mouseX = (Input.mousePosition.x / Screen.width) - 0.5f;
                float mouseY = (Input.mousePosition.y / Screen.height) - 0.5f;
                mouseOffset = new Vector3(mouseX * 4.5f, mouseY * 2.5f, 0f); // Gentle drift!
            }

            // Follow the target's X and Y, but keep the camera's original Z depth
            Vector3 targetPos = new Vector3(target.position.x, target.position.y, transform.position.z) + mouseOffset;
            transform.position = Vector3.Lerp(transform.position, targetPos, lerpSpeed * Time.unscaledDeltaTime);
        }

    }
}
