namespace Pancake.Physics
{
    using System.Collections.Generic;
    using Unity.Collections;
    using Unity.Jobs;
    using UnityEngine;

    [DefaultExecutionOrder(1000)] // ensure this script runs AFTER all user scripts 
    public class RaycastManager : MonoBehaviour
    {
        public static RaycastManager instance;

        private readonly List<RaycastCommand> _ongoingRaycastCommandsUpdate = new();
        private readonly List<RaycastCommand> _ongoingRaycastCommandsFixedUpdate = new();
        private readonly List<RaycastHit> _previousRaycastResultsUpdate = new();
        private readonly List<RaycastHit> _previousRaycastResultsFixedUpdate = new();

        private NativeList<RaycastCommand> _raycastCommandsUpdate;
        private NativeList<RaycastCommand> _raycastCommandsFixedUpdate;
        private NativeList<RaycastHit> _raycastResultsUpdate;
        private NativeList<RaycastHit> _raycastResultsFixedUpdate;

        private readonly List<AsyncRaycast> _requestRaycastsUpdate = new();
        private readonly List<AsyncRaycast> _requestRaycastsFixedUpdate = new();
        private readonly List<AsyncRaycast> _ongoingRaycastsUpdate = new();
        private readonly List<AsyncRaycast> _ongoingRaycastsFixedUpdate = new();

        private JobHandle _handleUpdate;
        private JobHandle _handleFixedUpdate;

        private bool _hasPreviousUpdate;
        private bool _hasPreviousFixedUpdate;
        private bool _allocatedNativeMemory;

        protected void OnEnable()
        {
            if (instance != null && instance != this)
            {
                Debug.LogWarning($"A second RaycastManager has been activated. Please ensure only one is ever active. Disabling this one.");
                enabled = false;
                return;
            }

            instance = this;
            _allocatedNativeMemory = true;

            _raycastCommandsUpdate = new NativeList<RaycastCommand>(256, Allocator.Persistent);
            _raycastCommandsFixedUpdate = new NativeList<RaycastCommand>(256, Allocator.Persistent);
            _raycastResultsUpdate = new NativeList<RaycastHit>(256, Allocator.Persistent);
            _raycastResultsFixedUpdate = new NativeList<RaycastHit>(256, Allocator.Persistent);
        }

        private void OnDisable()
        {
            _handleUpdate.Complete();
            _handleFixedUpdate.Complete();

            _hasPreviousUpdate = false;
            _hasPreviousFixedUpdate = false;

            _ongoingRaycastCommandsUpdate.Clear();
            _raycastCommandsFixedUpdate.Clear();
            _previousRaycastResultsUpdate.Clear();
            _previousRaycastResultsFixedUpdate.Clear();

            _requestRaycastsUpdate.Clear();
            _requestRaycastsFixedUpdate.Clear();
            _ongoingRaycastsUpdate.Clear();
            _ongoingRaycastsFixedUpdate.Clear();

            if (_allocatedNativeMemory)
            {
                _raycastCommandsUpdate.Dispose();
                _raycastCommandsFixedUpdate.Dispose();
                _raycastResultsUpdate.Dispose();
                _raycastResultsFixedUpdate.Dispose();

                _allocatedNativeMemory = false;
            }
        }

        private void Update()
        {
            UpdateRaycastJobs(_ongoingRaycastCommandsUpdate,
                _previousRaycastResultsUpdate,
                _requestRaycastsUpdate,
                _ongoingRaycastsUpdate,
                _raycastCommandsUpdate,
                _raycastResultsUpdate,
                ref _handleUpdate,
                ref _hasPreviousUpdate);
        }

        private void FixedUpdate()
        {
            UpdateRaycastJobs(_ongoingRaycastCommandsFixedUpdate,
                _previousRaycastResultsFixedUpdate,
                _requestRaycastsFixedUpdate,
                _ongoingRaycastsFixedUpdate,
                _raycastCommandsFixedUpdate,
                _raycastResultsFixedUpdate,
                ref _handleFixedUpdate,
                ref _hasPreviousFixedUpdate);
        }

        private void UpdateRaycastJobs(
            List<RaycastCommand> ongoingCommands,
            List<RaycastHit> previousResults,
            List<AsyncRaycast> asyncRequests,
            List<AsyncRaycast> asyncResults,
            NativeList<RaycastCommand> nativeCommands,
            NativeList<RaycastHit> raycastResults,
            ref JobHandle handle,
            ref bool hasPrevious)
        {
            // parse previous batch 
            if (hasPrevious)
            {
                handle.Complete();
                hasPrevious = false;

                previousResults.Clear();

                // if there's a size mismatch, something went very wrong.
                if (nativeCommands.Length == raycastResults.Length)
                {
                    var newResultsLength = raycastResults.Length;
                    for (var i = 0; i < newResultsLength; ++i)
                    {
                        previousResults.Add(raycastResults[i]);
                    }
                }

                // complete ongoing async requests
                for (var i = 0; i < asyncResults.Count; ++i)
                {
                    var asyncResult = asyncResults[i];
                    var asyncTicket = asyncResult.ticket;

                    var asyncHitSuccess = GetRaycastResult(asyncTicket, out var asyncHitInfo);
                    if (asyncResult.onComplete != null)
                    {
                        var asyncCallbackInfo = new AsyncRaycastCallbackResult() {hit = asyncHitSuccess, hitInfo = asyncHitInfo, context = asyncResult.context,};

                        asyncResult.onComplete.Invoke(asyncCallbackInfo);
                    }
                }

                asyncResults.Clear();
            }

            // early exit 
            var ongoingCount = ongoingCommands.Count;
            if (ongoingCount == 0)
            {
                return;
            }

            // reset and resize if necessary 
            nativeCommands.Clear();
            raycastResults.Clear();

            if (ongoingCount > nativeCommands.Capacity)
            {
                nativeCommands.SetCapacity(ongoingCount);
                raycastResults.SetCapacity(ongoingCount);
            }

            // copy over commands 
            for (var i = 0; i < ongoingCount; ++i)
            {
                nativeCommands.Add(ongoingCommands[i]);
                raycastResults.Add(default);
            }

            // reset the ongoing request list 
            ongoingCommands.Clear();

            // schedule next batch
            handle = RaycastCommand.ScheduleBatch(nativeCommands.AsArray(), raycastResults.AsArray(), 64);
            hasPrevious = true;

            // copy over ongoing async requests 
            asyncResults.Clear();

            for (var i = 0; i < asyncRequests.Count; ++i)
            {
                asyncResults.Add(asyncRequests[i]);
            }

            asyncRequests.Clear();
        }

#if UNITY_2022_2_OR_NEWER
        /// <summary>
        /// Requests a raycast be executed at the end of the current update loop. Returns a RaycastTicket. 
        /// During the same calling update loop, you can later do GetRaycastResult() with that ticket to get the result. 
        /// Note: You MUST call RequestRaycast() and GetRaycastResult() from within the same update loop. 
        /// This means that if you call RequestRaycast() in FixedUpdate, you must also call GetRaycastResult() in the next FixedUpdate. 
        /// If you call RequestRaycast() from Update, you must call GetRaycastResult() in the next Update. 
        /// If you do not want to keep track of RaycastTickets, please use RequestAsyncRaycast() intead. 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        /// <param name="maxDistance"></param>
        /// <param name="layerMask"></param>
        /// <param name="queryTriggerInteraction"></param>
        /// <param name="hitBackfaces"></param>
        /// <returns></returns>
        public RaycastTicket RequestRaycast(
            Vector3 origin,
            Vector3 direction,
            float maxDistance,
            int layerMask = -5,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
            bool hitBackfaces = false)
        {
            var query = new QueryParameters(layerMask: layerMask, hitMultipleFaces: false, hitTriggers: queryTriggerInteraction, hitBackfaces: hitBackfaces);
            var command = new RaycastCommand(origin, direction, query, maxDistance);

            var raycastTicket = new RaycastTicket();
            raycastTicket.fixedUpdateLoop = Time.inFixedTimeStep;

            if (raycastTicket.fixedUpdateLoop)
            {
                raycastTicket.value = _ongoingRaycastCommandsFixedUpdate.Count;
                _ongoingRaycastCommandsFixedUpdate.Add(command);
            }
            else
            {
                raycastTicket.value = _ongoingRaycastCommandsUpdate.Count;
                _ongoingRaycastCommandsUpdate.Add(command);
            }

            return raycastTicket;
        }
#else
        /// <summary>
        /// Requests a raycast be executed at the end of the current update loop. Returns a RaycastTicket. 
        /// During the same calling update loop, you can later do GetRaycastResult() with that ticket to get the result. 
        /// Note: You MUST call RequestRaycast() and GetRaycastResult() from within the same update loop. 
        /// This means that if you call RequestRaycast() in FixedUpdate, you must also call GetRaycastResult() in the next FixedUpdate. 
        /// If you call RequestRaycast() from Update, you must call GetRaycastResult() in the next Update. 
        /// If you do not want to keep track of RaycastTickets, please use RequestAsyncRaycast() intead. 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        /// <param name="maxDistance"></param>
        /// <param name="layerMask"></param>
        /// <param name="hitBackfaces"></param>
        /// <returns></returns>
        public RaycastTicket RequestRaycast(Vector3 origin, Vector3 direction, float maxDistance, int layerMask = -5, bool hitBackfaces = false)
        {
            var command = new RaycastCommand(origin, direction, distance: maxDistance, layerMask: layerMask, maxHits: 1);

            var raycastTicket = new RaycastTicket();
            raycastTicket.fixedUpdateLoop = Time.inFixedTimeStep;

            if (raycastTicket.fixedUpdateLoop)
            {
                raycastTicket.value = _ongoingRaycastCommandsFixedUpdate.Count;
                _ongoingRaycastCommandsFixedUpdate.Add(command);
            }
            else
            {
                raycastTicket.value = _ongoingRaycastCommandsUpdate.Count;
                _ongoingRaycastCommandsUpdate.Add(command);
            }

            return raycastTicket;
        }
#endif

        /// <summary>
        /// When you use RequestRaycast() you will be given a RaycastTicket. 
        /// Use that ticket with this function on the next update loop to get the result. 
        /// Note: You must use both functions on the same update loop, with result happening one update later. 
        /// If you use Update, get the result on the next Update. If you use FixedUpdate, get the result on the next FixedUpdate. 
        /// If you do not want to keep track of RaycastTickets, please use RequestAsyncRaycast(). 
        /// </summary>
        /// <param name="raycastTicket"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool GetRaycastResult(RaycastTicket raycastTicket, out RaycastHit info)
        {
            if (!raycastTicket.IsValid())
            {
                info = default;
                return false;
            }

            if (raycastTicket.fixedUpdateLoop != Time.inFixedTimeStep)
            {
                info = default;
                return false;
            }

            var raycastResultsList = raycastTicket.fixedUpdateLoop ? _previousRaycastResultsFixedUpdate : _previousRaycastResultsUpdate;

            if (raycastTicket.value < 0 || raycastTicket.value >= raycastResultsList.Count)
            {
                info = default;
                return false;
            }

            info = raycastResultsList[raycastTicket.value];
            return info.colliderInstanceID != 0 && info.collider != null;
        }


#if UNITY_2022_2_OR_NEWER
        /// <summary>
        /// Requests that a raycast be executed at the end of the current update loop. 
        /// The callback onComplete will be executed as soon as the raycast results are available. 
        /// If you need, you can provide an arbitrary context object to associated callbacks with their caller. 
        /// If you need more performance, consider using RequestRaycast() and GetRaycastResult() directly. 
        /// This function simply wraps around those and provides comfy callback functionality.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        /// <param name="maxDistance"></param>
        /// <param name="layerMask"></param>
        /// <param name="queryTriggerInteraction"></param>
        /// <param name="hitBackfaces"></param>
        /// <param name="context"></param>
        /// <param name="onComplete"></param>
        public void RequestAsyncRaycast(
            Vector3 origin,
            Vector3 direction,
            float maxDistance,
            int layerMask = -5,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
            bool hitBackfaces = false,
            object context = null,
            System.Action<AsyncRaycastCallbackResult> onComplete = null)
        {
            var raycastTicket = RequestRaycast(origin,
                direction,
                maxDistance,
                layerMask,
                queryTriggerInteraction,
                hitBackfaces);

            var asyncRaycastList = raycastTicket.fixedUpdateLoop ? _requestRaycastsFixedUpdate : _requestRaycastsUpdate;

            asyncRaycastList.Add(new AsyncRaycast() {ticket = raycastTicket, context = context, onComplete = onComplete,});
        }
#else
        /// <summary>
        /// Requests that a raycast be executed at the end of the current update loop. 
        /// The callback onComplete will be executed as soon as the raycast results are available. 
        /// If you need, you can provide an arbitrary context object to associated callbacks with their caller. 
        /// If you need more performance, consider using RequestRaycast() and GetRaycastResult() directly. 
        /// This function simply wraps around those and provides comfy callback functionality.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        /// <param name="maxDistance"></param>
        /// <param name="layerMask"></param>
        /// <param name="hitBackfaces"></param>
        /// <param name="context"></param>
        /// <param name="onComplete"></param>
        public void RequestAsyncRaycast(Vector3 origin, Vector3 direction, float maxDistance, int layerMask = -5,
            bool hitBackfaces = false, object context = null, System.Action<AsyncRaycastCallbackResult> onComplete = null)
        {
            var raycastTicket = RequestRaycast(origin, direction, maxDistance, layerMask: layerMask, hitBackfaces: hitBackfaces);

            var asyncRaycastList = raycastTicket.fixedUpdateLoop
                ? _requestRaycastsFixedUpdate
                : _requestRaycastsUpdate;

            asyncRaycastList.Add(new AsyncRaycast()
            {
                ticket = raycastTicket,
                context = context,
                onComplete = onComplete,
            });
        }
#endif
    }
}