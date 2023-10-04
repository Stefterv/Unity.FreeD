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
        [Header("Listening on port 40000")]
        [Tooltip("Run the server in edit mode")]
        public new bool runInEditMode = true;
        [Tooltip("Apply the values received to the gameObject")]
        public bool apply = true;
        public int id;
        FreeDServer server;
        Packet packet;
        public Packet lastPacket;
        void OnEnable()
        {
            if (!Application.isPlaying && !runInEditMode) return;
            server = FreeDServer.Get(40_000);
            server.received += (packet) =>
            {
                if (packet.Id != id) return;

                this.packet = packet;
            };
        }
        void Update() {
            if (!Application.isPlaying && !runInEditMode) return;
            if (packet == null) return;
            lastPacket = packet;
            if (!apply) return;
            

            transform.localPosition = new Vector3(packet.PosX / 1000f, packet.PosY / 1000f, -packet.PosZ / 1000f);
            transform.localRotation = Quaternion.identity;
            transform.Rotate(Vector3.up, packet.Pan);
            transform.Rotate(Vector3.up, 90f);
            transform.Rotate(Vector3.right, -packet.Tilt);
            transform.Rotate(Vector3.forward, -packet.Roll);

            var camera = GetComponent<Camera>();
            if (camera != null)
            {
                if(packet.Zoom > 0) camera.fieldOfView = 60 / (packet.Zoom / 1000f);
                camera.focusDistance = -packet.Focus / 1000f;
            }
                      
           
            packet = null;
        }
        void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
                UnityEditor.SceneView.RepaintAll();
            }
#endif
        }
        void OnDisable()
        {
            if (server == null) return;

            server.Stop();

        }

    }

}