using UnityEngine;

namespace BallancePhysics.Wapper
{
  [AddComponentMenu("BallancePhysics/PhysicsHinge")]
  [SLua.CustomLuaClass]
  public class PhysicsHinge : PhysicsConstraintBase
  {
    [Tooltip("位置和轴参照")]
    public Transform HingeRef = null;
    [Tooltip("连接到的另外一个物体，如果为空，则连接到世界")]
    public PhysicsObject Other = null;

    protected override void DoCreateConstraint()
    {
      var obj = GetComponent<PhysicsObject>();
      if(!obj.IsPhysicalized)
        return;
      if(Other != null && !Other.IsPhysicalized) {
        Other.AddPendCreateComponent(this);
        return;
      }
      Handle = PhysicsApi.API.set_physics_hinge(obj.Handle, Other != null ? Other.Handle : System.IntPtr.Zero, 
        HingeRef.transform.position, HingeRef.transform.forward.normalized);
    }
  }
}