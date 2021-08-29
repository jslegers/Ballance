using Ballance2.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace PhysicsRT
{
    [AddComponentMenu("PhysicsRT/Physics World")]
    [DefaultExecutionOrder(190)]
    [DisallowMultipleComponent]
    [SLua.CustomLuaClass]
    public class PhysicsWorld : MonoBehaviour
    {
        [Tooltip("如果需要任何连续模拟，请使用此模拟。 (模拟开始后更改此值无效)")]
        [SerializeField]
        private bool Continuous = false;
        [SerializeField]
        [Tooltip("世界的引力。默认值是 (0, -9.8, 0). (模拟开始后更改此值无效，请使用 SetGravity 更改)")]
        private Vector3 Gravity = new Vector3(0, -9.81f, 0);
        [SerializeField]
        [Tooltip("指定物理引擎将执行的解算器迭代次数。值越高，稳定性越高，但性能也越差。 (模拟开始后更改此值无效)")]
        private int SolverIterationCount = 4;
        [SerializeField]
        [Tooltip("设置物理边界为边长原点为中心的立方体。 (模拟开始后更改此值无效)")]
        private float BroadPhaseWorldSize = 1000;
        [SerializeField]
        [Tooltip("碰撞容限。这用于创建和保持接触点，即使对于非穿透性对象也是如此。这大大提高了系统的稳定性。默认碰撞容限为0.1f。 (模拟开始后更改此值无效)")]
        private float CollisionTolerance = 0.1f;
        [SerializeField]
        [Tooltip("是否启用VisualDebugger (模拟开始后更改此值无效)")]
        private bool VisualDebugger = true;
        [SerializeField]
        [Tooltip("是否启用 StableSolver (模拟开始后更改此值无效)")]
        private bool StableSolverOn = true;
        [Tooltip("是否启用物理模拟")]
        public bool Simulating = true;
        [Tooltip("是否自动更新物理物体的变换数据")]
        public bool AutoSyncTransforms = true;
        [Tooltip("是否启用Unity Profiler ")]
        public bool EnableProfiler = true;

        /// <summary>
        /// 所有物理场景
        /// </summary>
        /// <typeparam name="int">Unity场景的buildIndex</typeparam>
        /// <typeparam name="PhysicsWorld"></typeparam>
        /// <returns></returns>
        public static Dictionary<int, PhysicsWorld> PhysicsWorlds { get; } = new Dictionary<int, PhysicsWorld>();
        /// <summary>
        /// 获取当前场景的物理场景
        /// </summary>
        /// <returns></returns>
        public static PhysicsWorld GetCurrentScensePhysicsWorld() {
            int currentScenseIndex = SceneManager.GetActiveScene().buildIndex;
            if(PhysicsWorlds.TryGetValue(currentScenseIndex, out var a))
                return a;
            return null;
        }

        private Dictionary<int, PhysicsBody> bodysDict = new Dictionary<int, PhysicsBody>();
        private Dictionary<int, PhysicsPhantom> phantomsList = new Dictionary<int, PhysicsPhantom>();
        private Dictionary<int, PhysicsConstraint> constraintDict = new Dictionary<int, PhysicsConstraint>();
        private Dictionary<int, PhysicsBody> bodysDictAddContactListener = new Dictionary<int, PhysicsBody>();
        private IntPtr physicsWorldPtr = IntPtr.Zero;

        private void Awake() {
            _OnBodyContactEventCallback = OnBodyPointContactCallback;
            _OnConstraintBreakingCallback = OnConstraintBreakingCallback;
            _OnBodyTriggerEventCallback = OnBodyTiggerEventCallback;
            _OnPhantomOverlapCallback = OnPhantomOverlapCallback;
            _OnMaxPositionExceededCallback = OnMaxPositionExceededCallback;

            PhysicsSystemInit.Worlds.Add(this);

            var layerNames = Resources.Load<PhysicsLayerNames>("PhysicsLayerNames");
            Debug.Assert(layerNames != null);

            int currentScenseIndex = SceneManager.GetActiveScene().buildIndex;
            if(PhysicsWorlds.ContainsKey(currentScenseIndex)) 
                Debug.LogError("There can only one PhysicsWorld instance in a scense.");
            else {
                PhysicsWorlds.Add(currentScenseIndex, this);
                physicsWorldPtr = PhysicsApi.API.CreatePhysicsWorld(
                    Gravity,
                    SolverIterationCount,
                    BroadPhaseWorldSize,
                    CollisionTolerance,
                    Continuous,
                    VisualDebugger,
                    0xffffffff,
                    layerNames.GetGroupFilterMasks(), 
                    StableSolverOn,
                    _OnConstraintBreakingCallback,
                    _OnBodyTriggerEventCallback,
                    _OnBodyContactEventCallback,
                    _OnPhantomOverlapCallback,
                    _OnMaxPositionExceededCallback);
            }
        }
        private void OnDestroy() {
            int currentScenseIndex = SceneManager.GetActiveScene().buildIndex;
            Simulating = false;

            var list3 = new List<PhysicsConstraint>(constraintDict.Values);
            for(int i = list3.Count - 1; i >= 0; i--)
                DestroyImmediate(list3[i]);
            constraintDict.Clear();

            var list2 = new List<PhysicsPhantom>(phantomsList.Values);
            for(int i = list2.Count - 1; i >= 0; i--) 
                DestroyImmediate(list2[i]);
            phantomsList.Clear();

            var list = new List<PhysicsBody>(bodysDict.Values);
            for(int i = list.Count - 1; i >= 0; i--)
                DestroyImmediate(list[i]);
            bodysDict.Clear();
            bodysDictAddContactListener.Clear();

            dictSystemGroup.Clear();

            if(PhysicsWorlds.ContainsKey(currentScenseIndex)) 
                PhysicsWorlds.Remove(currentScenseIndex);
            if (physicsWorldPtr != IntPtr.Zero)
            {
                PhysicsApi.API.DestroyPhysicsWorld(physicsWorldPtr);
                physicsWorldPtr = IntPtr.Zero;
            }
        }
        private void FixedUpdate() {
            if(Simulating) {

                if(EnableProfiler)
                    Profiler.BeginSample("StepPhysicsWorld");

                //StepWorld
                StepPhysicsWorld();

                if(EnableProfiler) 
                    Profiler.EndSample();
   
                //Update all bodys position
                if(EnableProfiler) 
                    Profiler.BeginSample("UpdateAllBodys");

                if(AutoSyncTransforms) 
                    UpdateAllBodys();
                UpdateContactListenerState();

                if(EnableProfiler)
                    Profiler.EndSample();
            }
        }
    

        /// <summary>
        /// 执行物理事件模拟
        /// </summary>
        public void StepPhysicsWorld() {
            PhysicsApi.API.StepPhysicsWorld(physicsWorldPtr, Time.fixedDeltaTime);
        }
        /// <summary>
        /// 更新所有刚体位置旋转信息
        /// </summary>
        public void UpdateAllBodys()
        {
            PhysicsApi.API.UpdateAllPhysicsWorldBodys(physicsWorldPtr);

            float[] dat = new float[4];
            foreach(var bodyCurrent in bodysDict.Values) 
            {
                var t = bodyCurrent.gameObject.transform;
                IntPtr ptr = bodyCurrent.GetPtr(); //pos 0
                Marshal.Copy(ptr, dat, 0, 3);      //float[3]

                var p = new Vector3(dat[0], dat[1], dat[2]);
                if(CommonUtils.IsValid(p))
                    t.position = p;

                ptr = new IntPtr(ptr.ToInt64() + Marshal.SizeOf<float>() * 3); //pos 3
                Marshal.Copy(ptr, dat, 0, 4);      //float[4]

                t.rotation = new Quaternion(dat[0], dat[1], dat[2], dat[3]);
            }
        }

        //更新ContactListener的状态
        private void UpdateContactListenerState() {
            foreach(var body in  bodysDictAddContactListener.Values)
                body.FlushPhysicsBodyContactDataTick();
        }

        /// <summary>
        /// [由PhysicsPhantom自动调用，请勿手动调用]
        /// </summary>
        /// <param name="id"></param>
        /// <param name="body"></param>
        internal void AddPhantom(int id, PhysicsPhantom phantom) {
            phantomsList.Add(id, phantom);
            
        }
        /// <summary>
        /// [由PhysicsPhantom自动调用，请勿手动调用]
        /// </summary>
        /// <param name="body"></param>
        internal void RemovePhantom(PhysicsPhantom phantom) {
            phantomsList.Remove(phantom.Id);
        }      
        /// <summary>
        /// [由PhysicsBody自动调用，请勿手动调用]
        /// </summary>
        /// <param name="id"></param>
        /// <param name="body"></param>
        internal void AddBody(int id, PhysicsBody body) {
            bodysDict.Add(id, body);
            if(body.AddContactListener) 
                bodysDictAddContactListener.Add(id, body);
        }
        /// <summary>
        /// [由PhysicsBody自动调用，请勿手动调用]
        /// </summary>
        /// <param name="body"></param>
        internal void RemoveBody(PhysicsBody body) {
            bodysDict.Remove(body.Id);
            if(body.AddContactListener) 
                bodysDictAddContactListener.Remove(body.Id);
        }      
        /// <summary>
        /// [由PhysicsConstraint自动调用，请勿手动调用]
        /// </summary>
        /// <param name="id"></param>
        /// <param name="body"></param>
        internal void AddConstraint(int id, PhysicsConstraint constraint) {
            constraintDict.Add(id, constraint);
        }
        /// <summary>
        /// [由PhysicsBody自动调用，请勿手动调用]
        /// </summary>
        /// <param name="body"></param>
        internal bool RemoveConstraint(PhysicsConstraint constraint) {
            bool rs = constraintDict.ContainsKey(constraint.Id);
            constraintDict.Remove(constraint.Id);
            return rs;
        }

        /// <summary>
        /// 通过ID查找世界中的约束
        /// </summary>
        /// <param name="bodyId">ID</param>
        /// <returns>如果未找到则返回null</returns>
        public PhysicsConstraint GetConstraintById(int id) {
            if(constraintDict.TryGetValue(id, out var r))
                return r;
            return null;
        }
        /// <summary>
        /// 通过ID查找世界中的幻影
        /// </summary>
        /// <param name="phantomId">ID</param>
        /// <returns>如果未找到则返回null</returns>
        public PhysicsPhantom GetPhantomById(int phantomId) {
            if(phantomsList.TryGetValue(phantomId, out var r))
                return r;
            return null;
        }
        /// <summary>
        /// 通过ID查找世界中的刚体
        /// </summary>
        /// <param name="bodyId">ID</param>
        /// <returns>如果未找到则返回null</returns>
        public PhysicsBody GetBodyById(int bodyId) {
            if(bodysDict.TryGetValue(bodyId, out var r))
                return r;
            return null;
        }
        /// <summary>
        /// 获取C++层句柄
        /// </summary>
        /// <returns></returns>
        public IntPtr GetPtr() { return physicsWorldPtr; }
        /// <summary>
        /// 设置世界重力
        /// </summary>
        /// <param name="gravity"></param>
        public void SetGravity(Vector3 gravity) {
            Gravity = gravity;
            if(physicsWorldPtr != IntPtr.Zero)
                PhysicsApi.API.SetPhysicsWorldGravity(physicsWorldPtr, gravity);
        }
        /// <summary>
        /// 获取世界重力
        /// </summary>
        /// <returns></returns>
        public Vector3 GetGravity() {
            return Gravity;
        }
        /// <summary>
        /// 通过名称获取碰撞组信息
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetSystemGroup(string name) {
            if(physicsWorldPtr == IntPtr.Zero || string.IsNullOrEmpty(name))
                return 0;
            if(dictSystemGroup.TryGetValue(name, out var i))
                return i;
            i = PhysicsApi.API.GetNewSystemGroup(physicsWorldPtr);
            dictSystemGroup[name] = i;
            return i;
        }

        private Dictionary<string, int> dictSystemGroup = new Dictionary<string, int>();

        public bool CastRay(Vector3 from, Vector3 to, out PhysicsRayCastResult result) {
            return CastRay(from, to, out result);
        }
        public bool CastRay(Vector3 from, Vector3 to, int rayLayout, out PhysicsRayCastResult result) {
            if(physicsWorldPtr == IntPtr.Zero) 
                throw new Exception("World not create or destroyed");

            int count = PhysicsApi.API.PhysicsWorldRayCastBody(physicsWorldPtr, from, to, rayLayout, out var r);
            if(count > 0) {
                result = new PhysicsRayCastResult();
                result.bodyId = r.bodyId;
                result.normal = new Vector3(r.normal[0], r.normal[1], r.normal[2]);
                result.pos = new Vector3(r.pos[0], r.pos[1], r.pos[2]);
                result.body = GetBodyById(result.bodyId);
            } else {
                result = null;
            }
            return count > 0;
        }
        public PhysicsRayCastResult[] CastRayAll(Vector3 from, Vector3 to, int rayLayout) {
            if(physicsWorldPtr == IntPtr.Zero) 
                throw new Exception("World not create or destroyed");

            int count = PhysicsApi.API.PhysicsWorldRayCastHit(physicsWorldPtr, from, to, rayLayout, true, out var r);
            if(count > 0) {
                var rs = new PhysicsRayCastResult[r.Length];
                var ix = 0;
                foreach(var i in r) {
                    var result = new PhysicsRayCastResult();
                    result.bodyId = i.bodyId;
                    result.normal = new Vector3(i.normal[0], i.normal[1], i.normal[2]);
                    result.pos = new Vector3(i.pos[0], i.pos[1], i.pos[2]);
                    result.body = GetBodyById(result.bodyId);
                    rs[ix++] = result;
                }
            }

            return new PhysicsRayCastResult[0];
        }
        public PhysicsRayCastResult[] CastRayAll(Vector3 from, Vector3 to) {
            return CastRayAll(from, to);
        }

        public class PhysicsRayCastResult {
            public Vector3 normal;
            public Vector3 pos;
            public float hitFraction;
            public int bodyId;
            public PhysicsBody body;
        }

        private fnOnBodyContactEventCallback _OnBodyContactEventCallback;
        private fnOnConstraintBreakingCallback _OnConstraintBreakingCallback;
        private fnOnBodyTriggerEventCallback _OnBodyTriggerEventCallback;
        private fnOnPhantomOverlapCallback _OnPhantomOverlapCallback;
        private fnOnMaxPositionExceededCallback _OnMaxPositionExceededCallback;

        private void OnMaxPositionExceededCallback(IntPtr body, int id) {
            var sbody = GetBodyById(id);
            if(sbody != null) {
                sbody.ForceDePhysics();
                if(sbody.DeactiveWhenLeaveBroadphase)
                    sbody.gameObject.SetActive(false);
                sbody.onLeaveBroadphase?.Invoke(sbody);
            }
        }
        private void OnConstraintBreakingCallback(IntPtr constraint, int id, float forceMagnitude, int removed) {
            var c = GetConstraintById(id);
            if(c != null && c.onBreaking != null) 
                c.onBreaking(c, forceMagnitude, removed);
        }   
        private void OnBodyTiggerEventCallback(IntPtr body, IntPtr bodyOther, int id, int otherId, int ty) {
            if(ty == 1) {
                var sbody = GetBodyById(id);
                var sbodyOther = GetBodyById(otherId);
                if(sbody != null) 
                    sbody.onTiggerEnter?.Invoke(sbody, sbodyOther);
            } else {
                var sbody = GetBodyById(id);
                var sbodyOther = GetBodyById(otherId);
                if(sbody != null) 
                    sbody.onTiggerLeave?.Invoke(sbody, sbodyOther);
            }
        }
        private void OnBodyPointContactCallback(IntPtr body, IntPtr bodyOther, int id, int otherId, IntPtr dataPtr) {
            PhysicsBody sbody = GetBodyById(id), sbodyOther = GetBodyById(otherId);
            if(sbody != null && sbodyOther != null && sbodyOther != sbody)
                sbody.OnBodyPointContactCallback(sbodyOther, Marshal.PtrToStructure<sPhysicsBodyContactData>(dataPtr));
        }
        private void OnPhantomOverlapCallback(IntPtr phantom, IntPtr bodyOther, int id, int otherId, int ty) {
            PhysicsPhantom sphantom = GetPhantomById(id);
            PhysicsBody sbodyOther = GetBodyById(otherId);
            if(sphantom != null && sbodyOther != null)
                sphantom.OnPhantomOverlapCallback(sbodyOther, ty);
        }
    }
}
