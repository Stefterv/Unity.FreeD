using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace FreeD
{
    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode]
    public class FreeDLink : MonoBehaviour
    {
        public int id;
        FreeDServer server;
        Packet packet;
        void Awake()
        {
            server = FreeDServer.Get(40_000);
            server.received += (packet) =>
            {
                if (packet.Id != id) return;

                this.packet = packet;
            };
        }
        void Update() {
            if(packet == null) { return; }
            transform.localPosition = new Vector3(packet.PosX / 1000f, packet.PosY / 1000f, -packet.PosZ / 1000f);
            transform.localRotation = Quaternion.identity;
            transform.Rotate(Vector3.up, packet.Pan);
            transform.Rotate(Vector3.right, -packet.Tilt);
            transform.Rotate(Vector3.forward, -packet.Roll);

            var camera = GetComponent<Camera>();
            if(camera != null)
            {
                //camera.fieldOfView = 60 * packet.Zoom;
                camera.focusDistance = packet.Focus;
            }

            packet = null;
        }

    }
}