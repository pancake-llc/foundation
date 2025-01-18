using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pancake.Component
{
    /// <summary>
    /// Add this class to an object (usually a sprite) and it'll face the camera at all times
    /// </summary>
    public class BillboardComponent : GameComponent
    {
        public virtual Camera MainCamera { get; set; }
        public bool grabMainCameraOnStart = true; // whether or not this object should automatically grab a camera on start
        public bool nestObject = true; // whether or not to nest this object below a parent container
        public Vector3 offsetDirection = Vector3.back;
        public Vector3 up = Vector3.up; // the Vector3 to consider as "world up"

        protected GameObject parentContainer;
        private Transform _transform;

        /// <summary>
        /// On awake we grab a camera if needed, and nest our object
        /// </summary>
        protected virtual void Awake()
        {
            _transform = transform;

            if (grabMainCameraOnStart) GrabMainCamera();
        }

        private void Start()
        {
            if (nestObject) NestThisObject();
        }

        /// <summary>
        /// Nests this object below a parent container
        /// </summary>
        protected virtual void NestThisObject()
        {
            parentContainer = new GameObject();
            SceneManager.MoveGameObjectToScene(parentContainer, this.gameObject.scene);
            parentContainer.name = "Parent" + transform.gameObject.name;
            parentContainer.transform.position = transform.position;
            transform.SetParent(parentContainer.transform);
        }

        /// <summary>
        /// Grabs the main camera.
        /// </summary>
        protected virtual void GrabMainCamera() { MainCamera = Camera.main; }

        /// <summary>
        /// On update, we change our parent container's rotation to face the camera
        /// </summary>
        protected virtual void Update()
        {
            if (nestObject)
            {
                parentContainer.transform.LookAt(parentContainer.transform.position + MainCamera.transform.rotation * offsetDirection,
                    MainCamera.transform.rotation * up);
            }
            else
            {
                _transform.LookAt(_transform.position + MainCamera.transform.rotation * offsetDirection, MainCamera.transform.rotation * up);
            }
        }
    }
}